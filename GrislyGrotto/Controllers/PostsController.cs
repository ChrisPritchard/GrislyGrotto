using GrislyGrotto.Models;
using GrislyGrotto.ViewModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GrislyGrotto.Controllers
{
    public class PostsController : Controller
    {
        const int _maxComments = 20;

        private string SavedEditorContent
        {
            get { return Session["savedEditorContent"] as string; }
            set { Session["savedEditorContent"] = value; }
        }

        public new async Task<ActionResult> View(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return HttpNotFound();

			var post = await PostService.Current.Get(key);
            return View(new ViewViewModel { Post = post });
        }

        [HttpPost, ActionName("View")]
        public async Task<ActionResult> PostComment(string key, ViewViewModel model)
        {
            if (string.IsNullOrWhiteSpace(key))
                return HttpNotFound();
            if (!ModelState.IsValid)
                return Redirect("/p/" + key + "#commentsend");

            var invalidTokens = new [] { "http:", "https:", "www." };
            if (invalidTokens.Any(o => model.NewComment.Content.ToLower().Contains(o)))
                return HttpNotFound(); // fuck off spammers

            var post = await PostService.Current.Get(key);

            if(post.CommentCount >= _maxComments)
            {
                ModelState.AddModelError("Content", string.Format("Sorry, The max of {0} comments has been reached.", _maxComments));
                return Redirect("/p/" + key + "#commentsend");
            }

            var comments = CommentViewModel.GetCommentsExpanded(post);
            comments.Add(model.NewComment);
            post.Comments = JsonConvert.SerializeObject(comments.ToArray());
            post.CommentCount = comments.Count;

            await PostService.Current.CreateOrUpdate(post);
            PostService.Current.AddEvent("{0} posted a comment on '{1}'", model.NewComment.Author, post.Title);

            return Redirect("/p/" + key + "#commentsend");
        }

        [Authorize]
        public async Task<ActionResult> Edit(string key)
        {
            var model = new EditorViewModel();

            if(!string.IsNullOrWhiteSpace(key))
            {
                var post = await PostService.Current.Get(key);
                if (post.Author != Request.GetOwinContext().Authentication.User.Identity.Name)
                    return new HttpUnauthorizedResult();

                model.Title = post.Title;
                model.Content = post.Content;

                ViewBag.Editing = true;
            }

            if (!string.IsNullOrWhiteSpace(SavedEditorContent))
                model.Content = SavedEditorContent;

            return View(model);
        }

        [Authorize, ValidateInput(false)]
        public JsonResult SaveEditorContent(string content)
        {
            SavedEditorContent = content;
            return Json(true);
        }

        [HttpPost]
        public async Task<JsonResult> UploadImage()
        {
            if (Request.Files.Count == 0)
                return Json(false);

            var file = Request.Files[0];
            var fileName = Path.GetFileName(file.FileName);
            var imagePath = await AzureStorage.Upload(fileName, "usercontentpub", file.InputStream);

            return Json(imagePath);
        }

        [Authorize, HttpPost, ValidateInput(false)]
        public async Task<ActionResult> Edit(string key, EditorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!string.IsNullOrWhiteSpace(key))
                return await UpdatePost(key, model);
            else
                return await CreatePost(model);
        }

        private async Task<ActionResult> CreatePost(EditorViewModel model)
        {
            var existing = await PostService.Current.Exists(model.Key);
            if (existing)
            {
                ModelState.AddModelError("Title", "A post with title already exists");
                return View(model);
            }

            SavedEditorContent = null;

            var newPost = new Post
            {
                Title = model.Title,
                Key = model.Key,

                Author = Request.GetOwinContext().Authentication.User.Identity.Name,
                Date = DateTimeOffset.Now.LocalDateTime,
                Content = model.Content,
                WordCount = model.WordCount,
                IsStory = model.IsStory,
            };

            await PostService.Current.CreateOrUpdate(newPost);
            PostService.Current.AddEvent("{0} made a new post '{1}'", newPost.Author, newPost.Title);

            return RedirectToAction("View", new { model.Key });
        }

        private async Task<ActionResult> UpdatePost(string key, EditorViewModel model)
        {
            var post = await PostService.Current.Get(key);
            if (post.Author != Request.GetOwinContext().Authentication.User.Identity.Name)
                return new HttpUnauthorizedResult();

            SavedEditorContent = null;

            post.Title = model.Title;
            post.Content = model.Content;
            post.WordCount = model.WordCount;
            post.IsStory = model.IsStory;

            await PostService.Current.CreateOrUpdate(post);
            PostService.Current.AddEvent("{0} updated '{1}'", post.Author, post.Title);

            return RedirectToAction("View", new { key });
        }
    }
}