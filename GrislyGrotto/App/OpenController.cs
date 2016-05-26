using System;
using GrislyGrotto.App.Data;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;
using System.Data.Entity;
using System.Security.Claims;
using System.Web;
using GrislyGrotto.App.Shared;
using System.Text.RegularExpressions;

namespace GrislyGrotto.App
{
	public class OpenController : Controller
	{
        private const int _latestCount = 5;
        private const int _maxComments = 20;
        private const int _maxSearchResults = 50;

        private readonly string[] _months = 
            { "", "january", "february", "march", "april",
            "may", "june", "july", "august", "september",
            "october", "november", "december" };

        private readonly GrottoContext _db = new GrottoContext();

        [Route("")]
        [Route("page/{pageNum?}")]
        public async Task<ActionResult> Latest(int? pageNum)
		{
			var page = pageNum ?? 1;
			ViewBag.Page = page;

            var model = await _db.Posts.OrderByDescending(o => o.Date)
                .Include(o => o.Author).Include(o => o.Comments)
                .Skip((page - 1) * _latestCount)
                .Take(_latestCount).ToArrayAsync();
			return View(model);
		}

        [Route("archives")]
        public async Task<ActionResult> Archives()
        {
            Func<string, string> uppercaseFirst = text => char.ToUpper(text[0]) + text.Substring(1);
            var months = (await _db.Posts.Select(o => new { o.Date.Month, o.Date.Year })
                .GroupBy(o => o.Year)
                .ToArrayAsync())
                .Select(o => Tuple.Create(
                    o.Key,
                    o.GroupBy(m => m.Month).Select(m => Tuple.Create(
                        uppercaseFirst(_months[m.Key]),
                        m.Count()
                    )).ToArray()
                )).ToArray();

            var stories = (await _db.Posts.Where(o => o.IsStory).Select(o => new { o.Title, o.Author, o.WordCount, o.Date, o.Key })
                .OrderByDescending(o => o.Date).ToArrayAsync())
                .Select(o => new Post
                {
                    Title = o.Title,
                    Author = o.Author,
                    WordCount = o.WordCount,
                    Date = o.Date,
                    Key = o.Key
                }).ToArray();

            return View(Tuple.Create(months, stories));
        }

        [Route("m/{month}/{year}")]
        public async Task<ActionResult> Month(string month, int year)
        {
            if (month == null)
                return new HttpNotFoundResult();
            var monthNum = Array.IndexOf(_months, month.ToLower());
            if (monthNum == -1)
                return new HttpNotFoundResult();

            var model = await _db.Posts.OrderBy(o => o.Date)
                .Where(o => o.Date.Year == year && o.Date.Month == monthNum)
                .Include(o => o.Author).Include(o => o.Comments)
                .ToArrayAsync();
            return View("Latest", model);
        }

        [Route("search")]
        public async Task<ActionResult> Search(string searchTerm)
        {
            if(string.IsNullOrWhiteSpace(searchTerm))
                return View();

            var model = await _db.Posts.Where(o =>
                o.Title.Contains(searchTerm) || o.Content.Contains(searchTerm))
                .OrderByDescending(o => o.Date)
                .Take(_maxSearchResults)
                .ToListAsync();
            model.ForEach(o => o.Content = TrimToSearchTerm(searchTerm, o.Content));

            return View(model);
        }

        private string TrimToSearchTerm(string searchTerm, string content)
        {
            var stripped = Regex.Replace(content, "<[^>]*>", string.Empty);
            var firstInstance = stripped.ToLower().IndexOf(searchTerm.ToLower(), StringComparison.Ordinal);
            if (firstInstance < 0)
                return "";

            var firstEnd = firstInstance < 50 ? firstInstance : 50;
            var secondEnd = Math.Min(stripped.Length - (firstInstance + searchTerm.Length), 50);

            return "..." + stripped.Substring(Math.Max(firstInstance - 50, 0), firstEnd)
                + "<span class=\"search-term\">" + stripped.Substring(firstInstance, searchTerm.Length) + "</span>"
                + stripped.Substring(firstInstance + searchTerm.Length, secondEnd) + "...";
        }

        [Route("p/{key}")]
        public async Task<ActionResult> Single(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return HttpNotFound();

            var post = await _db.Posts.Where(o => o.Key == key)
                .Include(o => o.Comments).SingleAsync();
            return View(post);
        }

	    [Route("next/{currentKey}")]
	    public async Task<ActionResult> Next(string currentKey)
	    {
            if (string.IsNullOrWhiteSpace(currentKey))
                return HttpNotFound();

	        var currentDate = await _db.Posts.Where(o => o.Key == currentKey)
                .Select(o => o.Date).SingleAsync();
            var nextKey = await _db.Posts.OrderBy(o => o.Date).Where(o => o.Date > currentDate)
                .Select(o => o.Key).FirstOrDefaultAsync();

	        return RedirectToAction("Single", new { key = nextKey ?? currentKey });
	    }

	    [Route("previous/{currentKey}")]
	    public async Task<ActionResult> Previous(string currentKey)
	    {
	        if (string.IsNullOrWhiteSpace(currentKey))
	            return HttpNotFound();

	        var currentDate = await _db.Posts.Where(o => o.Key == currentKey)
	            .Select(o => o.Date).SingleAsync();
	        var previousKey = await _db.Posts.OrderByDescending(o => o.Date).Where(o => o.Date < currentDate)
	            .Select(o => o.Key).FirstOrDefaultAsync();

	        return RedirectToAction("Single", new {key = previousKey ?? currentKey});
	    }

	    [HttpPost]
        [Route("p/{key}")]
        public async Task<ActionResult> PostComment(string key, Comment model)
        {
            if (string.IsNullOrWhiteSpace(key))
                return HttpNotFound();
            if (!ModelState.IsValid)
                return Redirect("/p/" + key + "#commentsend");

            var invalidTokens = new[] { "http:", "https:", "www." };
            if (invalidTokens.Any(o => model.Content.ToLower().Contains(o)))
                return HttpNotFound(); // fuck off spammers

            var post = await _db.Posts.Where(o => o.Key == key)
                .Include(o => o.Comments).SingleAsync();

            if (post.Comments.Count >= _maxComments)
            {
                ModelState.AddModelError("Content", $"Sorry, The max of {_maxComments} comments has been reached.");
                return Redirect("/p/" + key + "#commentsend");
            }

            model.Post = post;
            model.Date = DateTime.Now.ToUniversalTime();
            post.Comments.Add(model);
            await _db.SaveChangesAsync();

            Events.Add($"{model.Author} posted a comment on '{post.Title}'");

            return Redirect("/p/" + key + "#commentsend");
        }

        [Route("login")]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login(Author model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _db.Authors.SingleOrDefaultAsync(o => o.Username == model.Username && o.Password == model.Password);
            if (user == null)
            {
                ModelState.AddModelError("Username", "Username and Password not recognised");
                return View(model);
            }

            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user.Username)
            }, "Cookie");
            Request.GetOwinContext().Authentication.SignIn(identity);

            var returnUrl = Request.QueryString["ReturnUrl"] ?? "/";
            return Redirect(returnUrl);
        }

        [Route("About")]
        public ActionResult About()
        {
            return View();
        }
    }
}