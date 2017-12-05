using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;

namespace GrislyGrotto
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

        private readonly GrislyGrottoDbContext _db;

        public OpenController(GrislyGrottoDbContext dbContext)
        {
            _db = dbContext;
        }

        [HttpGet("")]
        [HttpGet("page/{pageNum?}")]
        public async Task<IActionResult> Latest(int? pageNum)
        {
            var page = pageNum ?? 1;
			ViewBag.Page = page;

            var posts = await _db.Posts.OrderByDescending(o => o.Date)
                .Include(o => o.Author).Include(o => o.Comments)
                .Skip((page - 1) * _latestCount)
                .Take(_latestCount).ToArrayAsync();

			return View(new LatestViewModel(posts, pageNum ?? 1));
        }

        [HttpGet("archives")]
        public async Task<IActionResult> Archives()
        {
            var yearsRaw = await _db.Posts.Select(o => new { o.Date.Month, o.Date.Year })
                .GroupBy(o => o.Year).ToArrayAsync();
            var years = yearsRaw.Select(o => new YearViewModel(o.Key, 
                    o.GroupBy(m => m.Month).OrderBy(m => m.Key).Select(m => new MonthViewModel(_months[m.Key], m.Count())).ToArray()
                )).ToArray();

            var storiesRaw = await _db.Posts.Where(o => o.IsStory).Select(o => new { o.Title, o.Author, o.WordCount, o.Date, o.Key })
                .OrderByDescending(o => o.Date).ToArrayAsync();
            var stories = storiesRaw.Select(o => new Post
                {
                    Title = o.Title,
                    Author = o.Author,
                    WordCount = o.WordCount,
                    Date = o.Date,
                    Key = o.Key
                }).ToArray();

            return View(new ArchiveViewModel
            {
                Years = years,
                Stories = stories
            });
        }

        [HttpGet("m/{month}/{year}")]
        public async Task<IActionResult> Month(string month, int year)
        {
            if (month == null)
                return NotFound();
            var monthNum = Array.IndexOf(_months, month.ToLower());
            if (monthNum == -1)
                return NotFound();

            var posts = await _db.Posts.OrderBy(o => o.Date)
                .Where(o => o.Date.Year == year && o.Date.Month == monthNum)
                .Include(o => o.Author).Include(o => o.Comments)
                .ToArrayAsync();

            return View(nameof(Latest), new LatestViewModel(posts, month, year));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if(string.IsNullOrWhiteSpace(searchTerm))
                return View(new SearchViewModel());

            var results = await _db.Posts.Where(o => o.Title.Contains(searchTerm) || o.Content.Contains(searchTerm))
                .Include(o => o.Author)
                .OrderByDescending(o => o.Date)
                .Take(_maxSearchResults)
                .ToListAsync();
            results.ForEach(o => o.Content = TrimToSearchTerm(searchTerm, o.Content));

            return View(new SearchViewModel
            {
                SearchTerm = searchTerm, Results = results.ToArray()
            });
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

        [HttpGet("p/{key}")]
        public async Task<IActionResult> Single(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return NotFound();

            var post = await _db.Posts.Where(o => o.Key == key)
                .Include(o => o.Author)
                .Include(o => o.Comments).SingleAsync();

            return View(new SingleViewModel(post));
        }

	    [HttpGet("next/{currentKey}")]
	    public async Task<IActionResult> Next(string currentKey)
	    {
            if (string.IsNullOrWhiteSpace(currentKey))
                return NotFound();

	        var currentDate = await _db.Posts.Where(o => o.Key == currentKey)
                .Select(o => o.Date).SingleAsync();
            var nextKey = await _db.Posts.OrderBy(o => o.Date).Where(o => o.Date > currentDate)
                .Select(o => o.Key).FirstOrDefaultAsync();

            return RedirectToAction(nameof(Single), new { key = nextKey ?? currentKey });
	    }

	    [HttpGet("previous/{currentKey}")]
	    public async Task<IActionResult> Previous(string currentKey)
	    {
	        if (string.IsNullOrWhiteSpace(currentKey))
	            return NotFound();

	        var currentDate = await _db.Posts.Where(o => o.Key == currentKey)
	            .Select(o => o.Date).SingleAsync();
	        var previousKey = await _db.Posts.OrderByDescending(o => o.Date).Where(o => o.Date < currentDate)
	            .Select(o => o.Key).FirstOrDefaultAsync();

	        return RedirectToAction(nameof(Single), new {key = previousKey ?? currentKey});
	    }

        [HttpPost("p/{key}")]
        public async Task<IActionResult> PostComment(string key, SingleViewModel model)
        {
            if (string.IsNullOrWhiteSpace(key))
                return NotFound();

            var post = await _db.Posts.Where(o => o.Key == key)
                .Include(o => o.Comments).SingleAsync();

            if (string.IsNullOrWhiteSpace(model.CommentAuthor) || string.IsNullOrWhiteSpace(model.CommentContent))
            {
                ModelState.AddModelError(nameof(model.CommentContent), $"Sorry, both author and content must be specified.");
                return View(new SingleViewModel(post, model.CommentAuthor, model.CommentContent));
            }

            var invalidTokens = new[] { "http:", "https:", "www." };
            if (invalidTokens.Any(o => model.CommentContent.ToLower().Contains(o)))
            {
                ModelState.AddModelError(nameof(model.CommentContent), $"Sorry, posts may not contain urls or url-like content.");
                return View(new SingleViewModel(post, model.CommentAuthor));
            }

            if (post.Comments.Count >= _maxComments)
            {
                ModelState.AddModelError(nameof(model.CommentContent), $"Sorry, The max of {_maxComments} comments has been reached.");
                return View(new SingleViewModel(post));
            }

            var comment = new Comment 
            { 
                Post = post, 
                Date = DateTime.Now.ToUniversalTime(), 
                Author = model.CommentAuthor, 
                Content = model.CommentContent 
            };

            post.Comments.Add(comment);
            await _db.SaveChangesAsync();

            Program.AddEvent($"{model.CommentAuthor} posted a comment on '{post.Title}'");

            return RedirectToAction(nameof(Single), new {key});
        }

        [HttpGet("login")]
        public IActionResult Login() => View();

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var author = await _db.Authors.SingleOrDefaultAsync(o => o.Username == model.Username);
            if (author == null || Author.GenerateHash(model.Password, author.PasswordSalt) != author.PasswordHash)
            {
                ModelState.AddModelError(nameof(model.Username), "Username and Password not recognised");
                return View(model);
            }

            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, author.Username)
            }, "Cookie");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("Cookie", principal);

            var returnUrl = (string)Request.Query["ReturnUrl"] ?? "/";
            return Redirect(returnUrl);
        }

        [HttpGet("about")]
        public IActionResult About() => View();
    }
}