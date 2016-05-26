using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Infrastructure.Domain;
using GrislyGrotto.Website.Models;
using Mvc.XslViewEngine;

namespace GrislyGrotto.Website.Controllers
{
    public class BlogController : XController
    {
        IAuthentication authentication;

        IUserRepository userRepository;
        IQuoteRepository quoteRepository;
        IPostRepository postRepository;
        ICommentRepository commentRepository;

        XElementMapper mapper;
        PredicateValidator validator;
        BlogServices services;

        public BlogController(
            IAuthentication authentication,
            IUserRepository userRepository,
            IQuoteRepository quoteRepository,
            IPostRepository blogRepository,
            ICommentRepository commentRepository)
        {
            this.authentication = authentication;

            this.userRepository = userRepository;
            this.quoteRepository = quoteRepository;
            this.postRepository = blogRepository;
            this.commentRepository = commentRepository;

            mapper = new XElementMapper();
            validator = new PredicateValidator();
            services = new BlogServices(postRepository, commentRepository);
        }

        /// <summary>
        /// Add the common elements to the view data (quote, author info and history)
        /// </summary>
        private void AddCommonElements(string authorFullname)
        {
            ViewData.Add(mapper.QuoteAsXElement(quoteRepository.GetRandomQuote()));

            string loggedUsername = authentication.UserIsLoggedIn() ? authentication.GetLoggedInUser().Username : null;
            ViewData.Add(mapper.Authors(userRepository.AllUsers(), loggedUsername, authorFullname));

            ViewData.Add(mapper.RecentEntries(services.RecentEntries(authorFullname)));
            ViewData.Add(mapper.MonthPostCounts(services.MonthPostCounts(authorFullname)));
        }

        /// <summary>
        /// Shows top X posts by Y author
        /// </summary>
        public ActionResult Latest(string authorFullname)
        {
            AddCommonElements(authorFullname);

            var posts = postRepository.GetLatestPosts(authorFullname, 5);
            ViewData.Add(mapper.Posts(services.PostsWithCommentCounts(posts)));

            return View();
        }

        /// <summary>
        /// Shows X posts of Y month by Z Author
        /// </summary>
        public ActionResult Month(int year, string month, string authorFullname)
        {
            validator.Validate(2005 < year && year <= DateTime.Now.Year);
            validator.Validate(month != null && services.GetMonthIndex(month) != -1);
            if (!validator.Valid)
                return RedirectToAction("/");

            AddCommonElements(authorFullname);

            var posts = postRepository.GetPostsByMonth(authorFullname, year, services.GetMonthIndex(month));
            ViewData.Add(mapper.Posts(services.PostsWithCommentCounts(posts)));

            return View("Latest");
        }

        /// <summary>
        /// Shows Y blog, by name or by id
        /// Also shows nested comments for blog with comment editor
        /// </summary>
        public ActionResult Specific(int postID)
        {
            validator.Validate(postID > 0);
            if (!validator.Valid)
                return RedirectToAction("/");

            var post = postRepository.GetPostByID(postID);
            AddCommonElements(post.Author);

            ViewData.Add(mapper.PostAsXElement(post));

            var comments = commentRepository.GetCommentsOfPost(post.PostID);
            ViewData.Add(mapper.Comments(comments));

            return View();
        }

        /// <summary>
        /// Presents the Editor
        /// </summary>
        public ActionResult Editor(int? postID)
        {
            validator.Validate(authentication.UserIsLoggedIn());
            if (!validator.Valid)
                return RedirectToAction("/");

            var user = authentication.GetLoggedInUser();

            if (postID.HasValue)
            {
                Post post = postRepository.GetPostByID(postID.Value);
                if (!post.Author.Equals(user.Fullname))
                    return RedirectToAction("Latest");

                ViewData.Add(mapper.PostAsXElement(post));
            }

            AddCommonElements(user.Fullname);

            return View();
        }

        /// <summary>
        /// Creates a nested comment for a blog, redirects to ShowSpecific with Y blog
        /// </summary>
        public ActionResult CreateComment(int postID, string author, string text)
        {
            validator.Validate(postID > 0);
            validator.Validate(!string.IsNullOrEmpty(author));
            validator.Validate(!string.IsNullOrEmpty(text));
            if (!validator.Valid)
                return RedirectToAction("/");

            commentRepository.AddComment(postID, author, text);

            return RedirectToAction("Specific", new RouteValueDictionary { { "PostID", postID } });
        }

        /// <summary>
        /// Updates an existing blog, redirects to ShowSpecific with Y blog
        /// </summary>
        [ValidateInput(false)]
        public ActionResult EditPost(string authorFullname, int postID, string title, string content)
        {
            validator.Validate(postID > 0);
            validator.Validate(!string.IsNullOrEmpty(title));
            validator.Validate(!string.IsNullOrEmpty(content));
            if (!validator.Valid)
                return RedirectToAction("/");

            postRepository.UpdatePost(postID, title, content);

            return RedirectToAction("Specific", new RouteValueDictionary { { "PostID", postID } });
        }

        /// <summary>
        /// Creates a new post, redirects to ShowLatest with Y author
        /// </summary>
        [ValidateInput(false)]
        public ActionResult CreatePost(string authorFullname, string title, string content)
        {
            validator.Validate(!string.IsNullOrEmpty(authorFullname));
            validator.Validate(!string.IsNullOrEmpty(title));
            validator.Validate(!string.IsNullOrEmpty(content));
            if (!validator.Valid)
                return RedirectToAction("/");

            postRepository.AddPost(authorFullname, title, content);

            return RedirectToAction("Latest");
        }

        /// <summary>
        /// Returns a list of all user content
        /// </summary>
        public JsonResult AllUserImages()
        {
            var physicalFiles = Directory.GetFiles(Path.Combine(Request.PhysicalApplicationPath, "UserContent"));
            var validFormats = new string[]
            {
                ".bmp", ".gif", ".jpg", ".jpeg", ".png"
            };

            return Json(physicalFiles.Where(s => validFormats.Contains(s.ToLower().Substring(s.Length - 4))).Select(s => "/UserContent/" + Path.GetFileName(s)).ToList());
        }

        /// <summary>
        /// Saves a posted file to the usercontent folder, if the file is a valid content type
        /// </summary>
        public void UploadImage()
        {
            if (Request.Files.Count > 0)
            {
                var validFormats = new string[]
                {
                    "image/bmp", "image/gif", "image/jpg", "image/jpeg", "image/png", "image/pjpeg"
                };

                if (validFormats.Contains(Request.Files[0].ContentType))
                    Request.Files[0].SaveAs(Path.Combine(Request.PhysicalApplicationPath, "UserContent\\" + Path.GetFileName(Request.Files[0].FileName)));
            }
        }
    }
}
