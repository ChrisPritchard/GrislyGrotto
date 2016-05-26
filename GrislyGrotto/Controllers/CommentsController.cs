using AutoMapper;
using GrislyGrotto.Data;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using GrislyGrotto.ViewModels.Shared;
using Authorize = System.Web.Mvc.AuthorizeAttribute;
using HttpPost = System.Web.Mvc.HttpPostAttribute;

namespace GrislyGrotto.Controllers
{
    public class CommentsController : Controller
    {
        readonly GrislyGrottoContext database;

        public CommentsController()
        {
            database = new GrislyGrottoContext();
        }

        [HttpPost]
        public JsonResult NewComment(int postID, string author, string content)
        {
            if (string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(content))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var post = database.Posts.SingleOrDefault(p => p.ID == postID);
            if (post == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var comment = new Comment
            {
                Created = Utility.CurrentNzTime(),
                Author = author,
                Content = content
            };
            post.Comments.Add(comment);
            database.SaveChanges();

            return Json(Mapper.Map<CommentViewModel>(comment), JsonRequestBehavior.DenyGet);
        }

        [HttpPost, Authorize]
        public JsonResult DeleteComment(int id)
        {
            var comment = database.Comments.SingleOrDefault(c => c.ID == id);
            if (comment == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            database.Comments.Remove(comment);
            database.SaveChanges();

            return Json(true);
        }
    }
}
