using System;
using System.Web.Mvc;
using System.Xml.Linq;
using GrislyGrotto.Models;
using GrislyGrotto.Models.Components;
using GrislyGrotto.Mvc;

namespace GrislyGrotto.Controllers
{
    public class FeedController : XController
    {
        IBlogRepository blogRepository;
        ICommentRepository commentRepository;

        DtoToXElementMapper mapper;

        public FeedController(IBlogRepository blogRepository, ICommentRepository commentRepository)
        {
            this.blogRepository = blogRepository;
            this.commentRepository = commentRepository;

            mapper = new DtoToXElementMapper();
        }

        /// <summary>
        /// Returns latest posts with a date timestamp suitable for atom
        /// </summary>
        public ActionResult Atom(string userFullname)
        {
            ViewData.Add(new XElement("Date", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssZ")));

            var latestPosts = mapper.Posts(blogRepository.GetLatestPosts(userFullname, 5, commentRepository));
            ViewData.Add(latestPosts);

            return View();
        }

        /// <summary>
        /// Returns latest posts suitable for rss
        /// </summary>
        public ActionResult Rss(string userFullname)
        {
            var latestPosts = mapper.Posts(blogRepository.GetLatestPosts(userFullname, 5, commentRepository));
            ViewData.Add(latestPosts);

            return View();
        }
    }
}
