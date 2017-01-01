using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GrislyGrotto
{
    [Authorize]
    public class SecureController : Controller
    {
        private string SavedEditorContent
        {
            get { return HttpContext.Session.GetString("savedEditorContent"); }
            set 
            { 
                if(value != null)
                    HttpContext.Session.SetString("savedEditorContent", value);
                else
                    HttpContext.Session.Remove("savedEditorContent");
            }
        }

        private const string _expiredMessage = "Your session has expired. You may want to save your post content (to notepad or similar), return to the homepage and login again.";

        private readonly GrislyGrottoDbContext _db;

        public SecureController(GrislyGrottoDbContext dbContext)
        {
            _db = dbContext;
        }

        [HttpGet("new")]
        public IActionResult New()
        {
            var model = new Post();

            if (!string.IsNullOrWhiteSpace(SavedEditorContent))
                 model.Content = SavedEditorContent;

            return View("Editor", model);
        }
        
        [HttpPost("new")]
        public async Task<IActionResult> New(Post model)
        {
            if (!ModelState.IsValid)
                return View("Editor", model);

            var newKey = model.TitleAsKey();
            var existing = await _db.Posts.Where(o => o.Key == newKey).SingleOrDefaultAsync();
            if (existing != null)
            {
                ModelState.AddModelError("Title", "A post with a similar title already exists");
                return View("Editor", model);
            }

            SavedEditorContent = null;

            var currentUsername = HttpContext.User?.Identity?.Name;
            if (currentUsername == null)
            {
                ModelState.AddModelError("Title", _expiredMessage);
                return View("Editor", model);
            }

            var author = await _db.Authors.SingleAsync(o => o.Username == currentUsername);
            var newPost = new Post
            {
                Key = model.TitleAsKey(),
                Title = model.Title,

                Author = author,
                Date = DateTime.Now.ToUniversalTime(),
                Content = model.Content,
                IsStory = model.IsStory,
            };
            newPost.UpdateWordCount();
            _db.Posts.Add(newPost);

            await _db.SaveChangesAsync();
            Program.AddEvent($"{newPost.Author.DisplayName} made a new post '{newPost.Title}'");

            return RedirectToAction("Single", "Open", new { newPost.Key });
        }

        [HttpGet("edit/{key}")]
        public async Task<IActionResult> Edit(string key)
        {
            var model = await _db.Posts.Where(o => o.Key == key).Include(o => o.Author).SingleOrDefaultAsync();
            if (model == null)
                return NotFound();

            var currentUsername = HttpContext.User.Identity?.Name;
            if (currentUsername == null)
            {
                ModelState.AddModelError("Title", _expiredMessage);
                return View("Editor", model);
            }

            if (model.Author.Username != currentUsername)
                return Unauthorized();

            if (!string.IsNullOrWhiteSpace(SavedEditorContent))
                model.Content = SavedEditorContent;
            
            return View("Editor", model);
        }

        [HttpPost("edit/{key}")]
        public async Task<IActionResult> Edit(string key, Post model)
        {
            if (!ModelState.IsValid)
                return View("Editor", model);

            var post = await _db.Posts.Where(o => o.Key == key).Include(o => o.Author).SingleAsync();
            if (post.Author.Username != HttpContext.User.Identity.Name)
                return Unauthorized();

            if(post.Title != model.Title)
            {
                var newKey = model.TitleAsKey();
                var existing = await _db.Posts.Where(o => o.Key == newKey).SingleOrDefaultAsync();
                if (existing != null)
                {
                    ModelState.AddModelError("Title", "A post with a similar title already exists");
                    return View("Editor", model);
                }

                post.Key = newKey;
            }

            SavedEditorContent = null;

            post.Title = model.Title;
            post.Content = model.Content;
            post.WordCount = model.WordCount;
            post.IsStory = model.IsStory;

            await _db.SaveChangesAsync();
            Program.AddEvent($"{post.Author.DisplayName} updated '{post.Title}'");

            return RedirectToAction("Single", "Open", new { post.Key });
        }

        [HttpPost("api/saveeditorcontent")]
        public IActionResult SaveEditorContent([FromBody]string content)
        {
            SavedEditorContent = content;
            return Json(true);
        }

        [HttpPost("api/deletecomment/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _db.Comments.SingleOrDefaultAsync(o => o.Id == id);
            if(comment == null)
                return NotFound();

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();

            var currentUser = User.Identity.Name;
            var author = await _db.Authors.SingleAsync(o => o.Username == currentUser);
            Program.AddEvent($"{author.DisplayName} deleted a comment by author '{comment.Author}'");

            return Ok();
        }
    }
}