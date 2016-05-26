using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;
using AutoMapper;
using GrislyGrotto.Data;
using GrislyGrotto.ViewModels.Home;
using GrislyGrotto.ViewModels.Shared;
using System.Linq;
using System.Web.Mvc;

namespace GrislyGrotto.Controllers
{
    public class HomeController : Controller
    {
        readonly GrislyGrottoContext database;

        const int latestCount = 5;
        const int searchCount = 10;
        const int tagCount = 10;

        public HomeController()
        {
            database = new GrislyGrottoContext();
        }

        public ActionResult Latest()
        {
            var initialPosts = LatestFromDatabase(0, latestCount);
            var viewModel = new LatestViewModel(initialPosts);
            return View(viewModel);
        }

        public JsonResult LatestInRange(int start, int count)
        {
            return Json(LatestFromDatabase(start, count), JsonRequestBehavior.AllowGet);
        }

        private PostViewModel[] LatestFromDatabase(int start, int count)
        {
            return database.Posts
                .OrderByDescending(p => p.Created)
                .Skip(start).Take(count)
                .ToArray()
                .Select(Mapper.Map<PostViewModel>).ToArray();
        }

        public ActionResult Search(string searchTerm)
        {
            if(string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction("Latest");

            var initialPosts = SearchFromDatabase(searchTerm, 0, searchCount);
            var viewModel = new SearchViewModel(initialPosts, searchTerm);
            return View(viewModel);
        }

        public JsonResult SearchInRange(string searchTerm, int start, int count)
        {
            return Json(SearchFromDatabase(searchTerm, start, count), JsonRequestBehavior.AllowGet);
        }

        private PostViewModel[] SearchFromDatabase(string searchTerm, int start, int count)
        {
            return database.Posts
                .Where(p => p.Title.Contains(searchTerm) || p.Content.Contains(searchTerm)
                    || p.Tags.Any(t => t.Text.Replace("_", "").Contains(searchTerm)))
                .OrderByDescending(p => p.Created)
                .Skip(start).Take(count)
                .ToArray()
                .Select(Mapper.Map<PostViewModel>).ToArray();
        }

        public ActionResult Tag(string id)
        {
            var tagName = id;
            if (string.IsNullOrWhiteSpace(tagName))
                return RedirectToAction("Latest");

            var initialPosts = ForTagFromDatabase(tagName, 0, tagCount);
            var viewModel = new TagViewModel(initialPosts, tagName);
            return View(viewModel);
        }

        public JsonResult ForTagInRange(string tagName, int start, int count)
        {
            return Json(ForTagFromDatabase(tagName, start, count), JsonRequestBehavior.AllowGet);
        }

        private PostViewModel[] ForTagFromDatabase(string tagName, int start, int count)
        {
            return database.Posts
                .Where(p => p.Tags.Any(t => t.Text.Equals(tagName)))
                .OrderByDescending(p => p.Created)
                .Skip(start).Take(count)
                .ToArray()
                .Select(Mapper.Map<PostViewModel>).ToArray();
        }

        public ActionResult Month(string monthName, int year)
        {
            var monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames;
            var month = Array.IndexOf(monthNames, monthName) + 1;
            if (month == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var posts = database.Posts.Where(p => p.Created.Month == month && p.Created.Year == year)
                .OrderByDescending(p => p.Created).ToArray()
                .Select(Mapper.Map<PostViewModel>).ToArray();

            return View(new MonthViewModel(monthName, year, posts));
        }

        public FileResult Rss()
        {
            var posts = database.Posts.OrderByDescending(p => p.Created).Take(latestCount).ToArray();
            var host = Request.Url.GetLeftPart(UriPartial.Authority);

            var items = new List<SyndicationItem>();
            foreach (var post in posts)
            {
                var newItem = new SyndicationItem(post.Title, post.Content,
                    new Uri(host + "/Posts/Specific/" + post.ID, UriKind.Absolute)) {PublishDate = post.Created};
                newItem.Authors.Add(new SyndicationPerson{ Name = post.Author.DisplayName});
                items.Add(newItem);
            }

            var feed = new SyndicationFeed("The Grisly Grotto", "Deviant Minds Think Alike",
                new Uri(host, UriKind.Absolute), null, posts.First().Created, items)
            {
                ImageUrl = new Uri(host + "/content/favicon.png", UriKind.Absolute)
            };

            var builder = new StringBuilder();
            var writer = XmlWriter.Create(builder);
            var formatter = new Rss20FeedFormatter(feed);
            formatter.WriteTo(writer);
            writer.Flush();

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "application/rss+xml");
        }
    }
}
