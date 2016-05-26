using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GrislyGrotto.Models;
using System.Xml.Linq;
using System.Web.Routing;
using GrislyGrotto.Mvc;
using System.IO;

namespace GrislyGrotto.Controllers
{
    public class BlogController : XController
    {
        GrislyGrottoDBDataContext db;
        PredicateValidator InputContract;

        public BlogController()
        {
            db = new GrislyGrottoDBDataContext();
            InputContract = new PredicateValidator();
        }

        /// <summary>
        /// Shows top X blogs by Y author
        /// </summary>
        public ActionResult Latest(string Author)
        {
            //add common elements
            ViewData.Add(Util.RandomQuote(db));
            ViewData.Add(Util.AuthorDetails(Author, db));
            ViewData.Add(Util.BlogHistory(Author, db));

            ViewData.Add(Util.LatestBlogs(Author, db));

            return View();
        }

        /// <summary>
        /// Shows X blogs of Y month by Z Author
        /// </summary>
        public ActionResult Month(int Year, string Month, string Author)
        {
            InputContract.Validate(2005 < Year && Year <= DateTime.Now.Year);
            InputContract.Validate(Month != null && Enum.IsDefined(typeof(Months), Month));
            if (!InputContract.Valid)
                return RedirectToAction("/");

            //add common elements
            ViewData.Add(Util.RandomQuote(db));
            ViewData.Add(Util.AuthorDetails(Author, db));
            ViewData.Add(Util.BlogHistory(Author, db));

            List<Blog> blogsList;
            blogsList = db.Blogs.Where(b => string.IsNullOrEmpty(Author) || b.User.Fullname == Author).
                Where(b => b.EntryDate.Year == Year && b.EntryDate.Month == (int)(Enum.Parse(typeof(Months), Month))).
                OrderByDescending(b => b.EntryDate).ToList();

            XElement xBlogsElement = new XElement("Blogs");
            foreach (Blog blog in blogsList)
            {
                XElement xBlog = blog.AsXElement();
                xBlog.Add(new XAttribute("Comments", db.Comments.Where(c => c.Blog == blog).Count()));
                xBlogsElement.Add(xBlog);
            }

            ViewData.Add(xBlogsElement);
          
            return View("Latest");
        }

        /// <summary>
        /// Shows Y blog, by name or by id
        /// Also shows nested comments for blog with comment editor
        /// </summary>
        public ActionResult Specific(int BlogID)
        {
            InputContract.Validate(BlogID > 0);
            if (!InputContract.Valid)
                return RedirectToAction("/");

            Blog blog = db.Blogs.Where(b => b.BlogID == BlogID).Single();
            ViewData.Add(blog.AsXElement());

            //add common elements
            ViewData.Add(Util.RandomQuote(db));
            ViewData.Add(Util.AuthorDetails(blog.User.Fullname, db));
            ViewData.Add(Util.BlogHistory(blog.User.Fullname, db));

            List<Comment> commentsList = db.Comments.Where(comment => comment.BlogID == blog.BlogID).ToList();

            XElement xCommentsElement = new XElement("Comments");
            foreach (Comment comment in commentsList)
                xCommentsElement.Add(comment.AsXElement());

            ViewData.Add(xCommentsElement);

            return View();
        }

        /// <summary>
        /// Presents the Editor
        /// </summary>
        public ActionResult Editor(int? BlogID)
        {
            InputContract.Validate(!Util.LoggedInAuthor().IsNull());
            if (!InputContract.Valid)
                return RedirectToAction("/");

            if (BlogID.HasValue)
            {
                Blog blog = db.Blogs.Where(b => b.BlogID == BlogID).Single();
                if(blog.AuthorID != Util.LoggedInAuthor().UserID)
                    return RedirectToAction("Latest");
                ViewData.Add(blog.AsXElement());
            }

            //add common elements
            ViewData.Add(Util.RandomQuote(db));
            string sAuthor = Util.LoggedInAuthor().Fullname;
            ViewData.Add(Util.AuthorDetails(sAuthor, db));
            ViewData.Add(Util.BlogHistory(sAuthor, db));

            return View();
        }

        #region Post Actions CreateComment, DeleteComment, CreateBlog and EditBlog

        /// <summary>
        /// Creates a nested comment for a blog, redirects to ShowSpecific with Y blog
        /// </summary>
        public ActionResult CreateComment(int BlogID, int? ParentCommentID, string Author, string Content)
        {
            InputContract.Validate(BlogID > 0);
            InputContract.Validate(!string.IsNullOrEmpty(Author));
            InputContract.Validate(!string.IsNullOrEmpty(Content));
            if (!InputContract.Valid)
                return RedirectToAction("/");

            Comment newComment = new Comment();

            newComment.BlogID = BlogID;
            newComment.ParentCommentID = ParentCommentID;
            newComment.Author = Author;
            newComment.Content = Content;
            newComment.EntryDate = DateTime.Now;

            db.Comments.InsertOnSubmit(newComment);
            db.SubmitChanges();

            return RedirectToAction("Specific", new RouteValueDictionary { { "BlogID", BlogID } });
        }

        public ActionResult DeleteComment(int BlogID, int CommentID)
        {
            InputContract.Validate(BlogID > 0);
            InputContract.Validate(CommentID > 0);
            if (!InputContract.Valid)
                return RedirectToAction("/");

            Comment comment = db.Comments.Where(c => c.CommentID == CommentID).Single();
            db.Comments.DeleteOnSubmit(comment);
            db.SubmitChanges();

            return RedirectToAction("Specific", new RouteValueDictionary { { "BlogID", BlogID } });
        }

        /// <summary>
        /// Updates an existing blog, redirects to ShowSpecific with Y blog
        /// </summary>
        [ValidateInput(false)]
        public ActionResult EditBlog(int BlogID, string Title, string Tags, string Content)
        {
            InputContract.Validate(BlogID > 0);
            InputContract.Validate(!string.IsNullOrEmpty(Title));
            InputContract.Validate(!string.IsNullOrEmpty(Content));
            if (!InputContract.Valid)
                return RedirectToAction("/");

            Blog blog = db.Blogs.Where(b => b.BlogID == BlogID).Single();

            blog.Title = Title;
            blog.Tags = Tags;
            blog.Content = Content;

            db.SubmitChanges();

            return RedirectToAction("Specific", new RouteValueDictionary { { "BlogID", BlogID } });
        }

        /// <summary>
        /// Creates a new blog, redirects to ShowLatest with Y author
        /// </summary>
        [ValidateInput(false)]
        public ActionResult CreateBlog(int AuthorID, string Title, string Tags, string Content)
        {
            InputContract.Validate(AuthorID > 0);
            InputContract.Validate(!string.IsNullOrEmpty(Title));
            InputContract.Validate(!string.IsNullOrEmpty(Content));
            if (!InputContract.Valid)
                return RedirectToAction("/");

            Blog newBlog = new Blog();

            newBlog.AuthorID = AuthorID;
            newBlog.Title = Title;
            newBlog.Tags = Tags;
            newBlog.Content = Content;
            newBlog.EntryDate = DateTime.Now;

            db.Blogs.InsertOnSubmit(newBlog);
            db.SubmitChanges();

            return RedirectToAction("Latest");
        }

        #endregion

        /// <summary>
        /// Gets all database data as xml
        /// </summary>
        public ActionResult All()
        {
            XElement blogs = new XElement("blogs");
            foreach (Blog blog in db.Blogs)
                blogs.Add(blog.AsXElement(false));
            ViewData.Add(blogs);

            XElement comments = new XElement("comments");
            foreach (Comment comment in db.Comments)
                comments.Add(comment.AsXElement(false));
            ViewData.Add(comments);

            return View();
        }

        /// <summary>
        /// Returns a list of all user content
        /// </summary>
        /// <returns></returns>
        public JsonResult AllUserImages()
        {
            string[] sPhysicalFiles = Directory.GetFiles(Path.Combine(Request.PhysicalApplicationPath, "UserContent"));
            string[] sValidFormats = new string[]{
                ".bmp", ".gif", ".jpg", ".jpeg", ".png"
            };

            return Json(sPhysicalFiles.Where(s => sValidFormats.Contains(s.ToLower().Substring(s.Length - 4))).Select(s => "/UserContent/" + Path.GetFileName(s)).ToList());
        }

        /// <summary>
        /// Saves a posted file to the usercontent folder, if the file is a valid content type
        /// </summary>
        public void UploadImage()
        {
            if (Request.Files.Count > 0)
            {
                string[] sValidFormats = new string[]{
                "image/bmp", "image/gif", "image/jpg", "image/jpeg", "image/png", "image/pjpeg"
                };

                if (sValidFormats.Contains(Request.Files[0].ContentType))
                    Request.Files[0].SaveAs(Path.Combine(Request.PhysicalApplicationPath, "UserContent\\" + Path.GetFileName(Request.Files[0].FileName)));
            }
        }

        //public ActionResult LoadOldXml()
        //{
        //    #region Load Old Blogs From XML
        //    //XDocument xBlogs = XDocument.Load(this.ControllerContext.HttpContext.Request.PhysicalApplicationPath + "/Content/Blogs.xml");

        //    //foreach (XElement xBlog in xBlogs.Descendants("blog"))
        //    //{
        //    //    Blog newBlog = new Blog();

        //    //    newBlog.AuthorID = db.Users.Where(user => user.Fullname == xBlog.Attribute("author").Value).Single().UserID;
        //    //    newBlog.Title = xBlog.Attribute("title").Value;
        //    //    newBlog.Tags = string.Empty;
        //    //    newBlog.Content = xBlog.Value;
        //    //    newBlog.EntryDate = Util.To<DateTime>(xBlog.Attribute("date").Value);

        //    //    db.Blogs.InsertOnSubmit(newBlog);
        //    //    db.SubmitChanges();
        //    //}
        //    #endregion

        //    #region Load Old Comments From XML
        //    //XDocument xComments = XDocument.Load(this.ControllerContext.HttpContext.Request.PhysicalApplicationPath + "/Content/Comments.xml");

        //    //foreach (XElement xComment in xComments.Descendants("comment"))
        //    //{
        //    //    Comment newComment = new Comment();

        //    //    newComment.BlogID = db.Blogs.Where(blog => blog.Title == xComment.Attribute("blog").Value).Single().BlogID;
        //    //    newComment.ParentCommentID = null;
        //    //    newComment.Author = xComment.Attribute("author").Value;
        //    //    newComment.Content = xComment.Value;
        //    //    newComment.EntryDate = Util.To<DateTime>(xComment.Attribute("date").Value);

        //    //    db.Comments.InsertOnSubmit(newComment);
        //    //    db.SubmitChanges();
        //    //}
        //    #endregion

        //    #region Load Old Quotes From XML
        //    //XDocument xQuotes = XDocument.Load(this.ControllerContext.HttpContext.Request.PhysicalApplicationPath + "/Content/Quotes.xml");

        //    //foreach (XElement xQuote in xQuotes.Descendants("quote"))
        //    //{
        //    //    Quote newQuote = new Quote();

        //    //    newQuote.Author = xQuote.Attribute("author").Value;
        //    //    newQuote.Content = xQuote.Value;

        //    //    db.Quotes.InsertOnSubmit(newQuote);
        //    //    db.SubmitChanges();
        //    //}
        //    #endregion

        //    return RedirectToAction("Latest");
        //}
    }
}
