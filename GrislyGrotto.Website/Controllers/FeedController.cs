using System;
using System.Web.Mvc;
using System.Xml.Linq;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Website.Models;
using Mvc.XslViewEngine;

namespace GrislyGrotto.Website.Controllers
{
    public class FeedController : XController
    {
        IPostRepository postRepository;
        ICommentRepository commentRepository;

        XElementMapper mapper;
        BlogServices services;

        public FeedController(IPostRepository postRepository, ICommentRepository commentRepository)
        {
            this.postRepository = postRepository;
            this.commentRepository = commentRepository;

            mapper = new XElementMapper();
            services = new BlogServices(postRepository, commentRepository);
        }

        /// <summary>
        /// Returns latest posts with a date timestamp suitable for atom
        /// </summary>
        public ActionResult Atom(string userFullname)
        {
            ViewData.Add(new XElement("Date", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssZ")));

            var posts = postRepository.GetLatestPosts(userFullname, 5);
            ViewData.Add(mapper.Posts(services.PostsWithCommentCounts(posts)));

            return View();
        }

        /// <summary>
        /// Returns latest posts suitable for rss
        /// </summary>
        public ActionResult Rss(string userFullname)
        {
            var posts = postRepository.GetLatestPosts(userFullname, 5);
            ViewData.Add(mapper.Posts(services.PostsWithCommentCounts(posts)));

            return View();
        }
    }
}
