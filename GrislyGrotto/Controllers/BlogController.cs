using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using GrislyGrotto.Models;
using GrislyGrotto.Models.Components;
using GrislyGrotto.Models.DTO;
using GrislyGrotto.Mvc;

namespace GrislyGrotto.Controllers
{
    public class BlogController : XController
    {
        IAuthentication authentication;

        IUserRepository userRepository;
        IQuoteRepository quoteRepository;
        IBlogRepository blogRepository;
        ICommentRepository commentRepository;        

        DtoToXElementMapper mapper;
        PredicateValidator validator;

        public BlogController(
            IAuthentication authentication, 
            IUserRepository userRepository, 
            IQuoteRepository quoteRepository, 
            IBlogRepository blogRepository, 
            ICommentRepository commentRepository)
        {
            this.authentication = authentication;

            this.userRepository = userRepository;
            this.quoteRepository = quoteRepository;
            this.blogRepository = blogRepository;
            this.commentRepository = commentRepository;

            mapper = new DtoToXElementMapper();
            validator = new PredicateValidator();
        }

        /// <summary>
        /// Add the common elements to the view data (quote, author info and history)
        /// </summary>
        private void AddCommonElements(string userFullname)
        {
            ViewData.Add(mapper.QuoteAsXElement(quoteRepository.GetRandomQuote()));

            string loggedUsername = authentication.UserIsLoggedIn() ? authentication.GetLoggedInUser().Username : null;
            ViewData.Add(mapper.Authors(userRepository.AllUsers(), loggedUsername, userFullname));

            ViewData.Add(mapper.Entries(blogRepository.GetRecentPostEntries(userFullname, 10)));
            ViewData.Add(mapper.Months(blogRepository.GetMonthPostCounts(userFullname)));
        }

        /// <summary>
        /// Shows top X posts by Y author
        /// </summary>
        public ActionResult Latest(string userFullname)
        {
            AddCommonElements(userFullname);

            var posts = blogRepository.GetLatestPosts(userFullname, 5, commentRepository);
            ViewData.Add(mapper.Posts(posts));

            return View();
        }

        /// <summary>
        /// Shows X posts of Y month by Z Author
        /// </summary>
        public ActionResult Month(int year, string month, string userFullname)
        {
            validator.Validate(2005 < year && year <= DateTime.Now.Year);
            validator.Validate(month != null && Enum.IsDefined(typeof(Month), month));
            if (!validator.Valid)
                return RedirectToAction("/");

            AddCommonElements(userFullname);

            var posts = blogRepository.GetPostsByMonth(userFullname, new MonthInfo(year, month), commentRepository);
            ViewData.Add(mapper.Posts(posts));
          
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

            var post = blogRepository.GetPostByID(postID, commentRepository);
            AddCommonElements(post.Author.Fullname);

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
                PostInfo post = blogRepository.GetPostByID(postID.Value, commentRepository);
                if (!post.Author.Username.Equals(user.Username))
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

            commentRepository.AddComment(new CommentInfo(postID, DateTime.Now, author, text));

            return RedirectToAction("Specific", new RouteValueDictionary { { "PostID", postID } });
        }

        /// <summary>
        /// Updates an existing blog, redirects to ShowSpecific with Y blog
        /// </summary>
        [ValidateInput(false)]
        public ActionResult EditPost(string authorUsername, int postID, string title, string content)
        {
            validator.Validate(postID > 0);
            validator.Validate(!string.IsNullOrEmpty(title));
            validator.Validate(!string.IsNullOrEmpty(content));
            if (!validator.Valid)
                return RedirectToAction("/");

            blogRepository.UpdatePost(new PostInfo(postID, title, content));

            return RedirectToAction("Specific", new RouteValueDictionary { { "PostID", postID } });
        }

        /// <summary>
        /// Creates a new post, redirects to ShowLatest with Y author
        /// </summary>
        [ValidateInput(false)]
        public ActionResult CreatePost(string authorUsername, string title, string content)
        {
            validator.Validate(!string.IsNullOrEmpty(authorUsername));
            validator.Validate(!string.IsNullOrEmpty(title));
            validator.Validate(!string.IsNullOrEmpty(content));
            if (!validator.Valid)
                return RedirectToAction("/");

            blogRepository.AddPost(new PostInfo(DateTime.Now, userRepository.GetUserByUsername(authorUsername), title, content));

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
