using System;
using GrislyGrotto.Mvc;
using System.Web.Mvc;
using GrislyGrotto.Models;
using System.Xml.Linq;

namespace GrislyGrotto.Controllers
{
    public class FeedController : XController
    {
        GrislyGrottoDBDataContext db;

        public FeedController()
        {
            db = new GrislyGrottoDBDataContext();
        }

        public ActionResult Atom(string Author)
        {
            ViewData.Add(new XElement("Date", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssZ")));
            ViewData.Add(Util.LatestBlogs(Author, db));

            return View();
        }

        public ActionResult Rss(string Author)
        {
            ViewData.Add(Util.LatestBlogs(Author, db));

            return View();
        }
    }
}
