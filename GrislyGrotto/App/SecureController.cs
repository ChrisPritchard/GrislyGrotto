using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GrislyGrotto.App.Shared;
using GrislyGrotto.App.Data;
using System.Data.Entity;

namespace GrislyGrotto.App
{
    [Authorize]
    public class SecureController : Controller
    {
        private readonly GrottoContext _db = new GrottoContext();
        private const string _expiredMessage = "Your session has expired. You may want to save your post content (to notepad or similar), return to the homepage and login again.";

        private string SavedEditorContent
        {
            get { return Session["savedEditorContent"] as string; }
            set { Session["savedEditorContent"] = value; }
        }

        [Route("new")]
        public ActionResult New()
        {
            var model = new Post();

            if (!string.IsNullOrWhiteSpace(SavedEditorContent))
                model.Content = SavedEditorContent;

            return View("Editor", model);
        }
        
        [HttpPost, ValidateInput(false)]
        [Route("new")]
        public async Task<ActionResult> New(Post model)
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

            var currentUsername = Request.GetOwinContext().Authentication?.User?.Identity?.Name;
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
            Events.Add($"{newPost.Author.DisplayName} made a new post '{newPost.Title}'");

            return RedirectToAction("Single", "Open", new { newPost.Key });
        }

        [Route("edit/{key}")]
        public async Task<ActionResult> Edit(string key)
        {
            var model = await _db.Posts.Where(o => o.Key == key).Include(o => o.Author).SingleOrDefaultAsync();
            if (model == null)
                return new HttpNotFoundResult();

            var currentUsername = Request.GetOwinContext().Authentication?.User?.Identity?.Name;
            if (currentUsername == null)
            {
                ModelState.AddModelError("Title", _expiredMessage);
                return View("Editor", model);
            }

            if (model.Author.Username != currentUsername)
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrWhiteSpace(SavedEditorContent))
                model.Content = SavedEditorContent;
            
            return View("Editor", model);
        }

        [HttpPost, ValidateInput(false)]
        [Route("edit/{key}")]
        public async Task<ActionResult> Edit(string key, Post model)
        {
            if (!ModelState.IsValid)
                return View("Editor", model);

            

            var post = await _db.Posts.Where(o => o.Key == key).Include(o => o.Author).SingleAsync();
            if (post.Author.Username != Request.GetOwinContext().Authentication.User.Identity.Name)
                return new HttpUnauthorizedResult();

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
            Events.Add($"{post.Author.DisplayName} updated '{post.Title}'");

            return RedirectToAction("Single", "Open", new { post.Key });
        }

        [ValidateInput(false)]
        [Route("api/saveeditorcontent")]
        public JsonResult SaveEditorContent(string content)
        {
            SavedEditorContent = content;
            return Json(true);
        }

        [HttpPost]
        [Route("api/uploadimage")]
        public async Task<JsonResult> UploadImage()
        {
            if (Request.Files.Count == 0 || Request.Files[0] == null)
                return Json(false);

            var file = Request.Files[0];
            var fileName = Path.GetFileName(file.FileName);

            var result = await AzureStorage.Upload(fileName, file.InputStream);

            if (result.Item1)
                return Json(result.Item2); // success - this is the filename
            else
                return Json("file already exists");
        }
    }
}