using AutoMapper;
using GrislyGrotto.Data;
using GrislyGrotto.ViewModels.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Mvc;
using GrislyGrotto.ViewModels.Shared;
using Authorize = System.Web.Mvc.AuthorizeAttribute;
using HttpPost = System.Web.Mvc.HttpPostAttribute;
using System.Web;

namespace GrislyGrotto.Controllers
{
    public class PostsController : Controller
    {
        readonly GrislyGrottoContext database;

        public PostsController()
        {
            database = new GrislyGrottoContext();
        }

        public ActionResult Specific(int id)
        {
            var data = database.Posts
                .SingleOrDefault(p => p.ID == id);

            if (data == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
                
            var mapped = Mapper.Map<PostViewModel>(data);
            mapped.Comments = data.Comments.Select(Mapper.Map<CommentViewModel>).ToList();
            return View(mapped);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            var existingPost = database.Posts
                .SingleOrDefault(p => p.ID == id);

            if (existingPost == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if(existingPost.Author.Username != Utility.LoggedUser().Username)
                throw new HttpResponseException(HttpStatusCode.Unauthorized);

            var viewModel = EditorViewModel.FromPost(existingPost);
            var saved = SavedState(id);
            if (saved != null)
                viewModel.UpdateWith(saved);
            viewModel.ExistingTags = database.Tags.Select(t => t.Text).ToList();
            viewModel.Comments = existingPost.Comments.Select(Mapper.Map<CommentViewModel>).ToList();

            return View(viewModel);
        }

        [HttpPost, Authorize, ValidateInput(false)]
        public ActionResult Edit(int id, EditorViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                ModelState.AddModelError("Title", "Title cannot be blank");
            if (string.IsNullOrWhiteSpace(model.Content))
                ModelState.AddModelError("Content", "Content cannot be blank");

            if (!ModelState.IsValid)
            {
                model.Comments = database.Comments
                    .Where(c => c.Post.ID == model.ID)
                    .ToList()
                    .Select(Mapper.Map<CommentViewModel>).ToList();
                return View(model);
            }

            var existingPost = database.Posts
                .SingleOrDefault(p => p.ID == id);

            if (existingPost == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (existingPost.Author.Username != Utility.LoggedUser().Username)
                throw new HttpResponseException(HttpStatusCode.Unauthorized);

            existingPost.Title = model.Title;
            existingPost.Content = model.Content;
            existingPost.Type = model.IsStory ? PostType.Story : PostType.Normal;
            SetWordCount(existingPost);
            UpdateTags(existingPost, model.SelectedTags);

            database.SaveChanges();

            ClearSavedState(id);
            return RedirectToAction("Specific", new { id });
        }

        [HttpPost, Authorize]
        public ActionResult Delete(int id)
        {
            var existingPost = database.Posts
                .SingleOrDefault(p => p.ID == id);

            if (existingPost == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (existingPost.Author.Username != Utility.LoggedUser().Username)
                throw new HttpResponseException(HttpStatusCode.Unauthorized);

            database.Posts.Remove(existingPost);
            database.SaveChanges();

            return RedirectToAction("Latest", "Home");
        }

        [Authorize]
        public ActionResult New()
        {
            var viewModel = new EditorViewModel();
            var saved = SavedState(null);
            if (saved != null)
                viewModel.UpdateWith(saved);
            viewModel.ExistingTags = database.Tags.Select(t => t.Text).ToList();

            return View(viewModel);
        }

        [HttpPost, Authorize, ValidateInput(false)]
        public ActionResult New(EditorViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                ModelState.AddModelError("Title", "Title cannot be blank");
            if (string.IsNullOrWhiteSpace(model.Content))
                ModelState.AddModelError("Content", "Content cannot be blank");

            if (!ModelState.IsValid)
                return View(model);

            var loggedUser = Utility.LoggedUser().Username;
            var newPost = new Post
            {
                Author = database.Users.Single(u => u.Username.Equals(loggedUser)),
                Created = Utility.CurrentNzTime(),
                Title = model.Title,
                Content = model.Content,
                Type = model.IsStory ? PostType.Story : PostType.Normal,
            };
            SetWordCount(newPost);
            UpdateTags(newPost, model.SelectedTags);

            database.Posts.Add(newPost);
            database.SaveChanges();

            ClearSavedState(null);
            return RedirectToAction("Specific", new { id = newPost.ID });
        }

        private void SetWordCount(Post post)
        {
            var stripped = Regex.Replace(post.Content, @"<[^>]*>", string.Empty);
            post.WordCount = stripped.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Length;
        }

        private void UpdateTags(Post post, IEnumerable<string> selectedTags)
        {
            if (post.Tags != null)
                post.Tags.Clear();
            else 
                post.Tags = new List<Tag>();

            var allTags = database.Tags.ToArray();
            foreach (var selectedTag in selectedTags.Distinct())
            {
                var existingTag = allTags.SingleOrDefault(t => t.Text.Equals(selectedTag));
                if (existingTag != null)
                    post.Tags.Add(existingTag);
                else
                {
                    var newTag = new Tag { Text = selectedTag };
                    post.Tags.Add(newTag);
                    database.Tags.Add(newTag);
                }
            }
        }

        [HttpPost, Authorize, ValidateInput(false)]
        public JsonResult SaveState(int? id, string title, string content, bool isStory)
        {
            var post = new PostViewModel
            {
                Title = title,
                Content = content,
                Type = isStory ? PostTypeViewModel.Story : PostTypeViewModel.Normal
            };
            Session["savedstate_" + (id.HasValue ? id.Value.ToString() : "new")] = post;

            return Json(true);
        }

        private void ClearSavedState(int? id)
        {
            Session.Remove("savedstate_" + (id.HasValue ? id.Value.ToString() : "new"));
        }

        private PostViewModel SavedState(int? id)
        {
            return (PostViewModel)Session["savedstate_" + (id.HasValue ? id.Value.ToString() : "new")];
        }

        public JsonResult GetEncodedImageHtml(HttpPostedFileBase image)
        {
            const string imageTag = "<img src='data:image/png;base64,{0}' />";

            var data = new byte[image.ContentLength];
            image.InputStream.Read(data, 0, data.Length);

            var base64 = Convert.ToBase64String(data);
            return Json(string.Format(imageTag, base64), JsonRequestBehavior.AllowGet);
        }
    }
}