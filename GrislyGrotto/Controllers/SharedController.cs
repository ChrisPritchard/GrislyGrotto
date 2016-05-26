using System;
using AutoMapper;
using GrislyGrotto.Data;
using GrislyGrotto.ViewModels.Shared;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace GrislyGrotto.Controllers
{
    public class SharedController : Controller
    {
        readonly GrislyGrottoContext database;

        public SharedController()
        {
            database = new GrislyGrottoContext();
        }

        public JsonResult Quote()
        {
            if (Session["quote"] != null)
                return Json(Session["quote"], JsonRequestBehavior.AllowGet);

            var quoteCount = database.Quotes.Count();
            if (quoteCount == 0)
                return Json((Session["quote"] = new QuoteViewModel { Author = "Admin", Text = "No quotes in database" }));

            var randomIndex = new Random().Next(0, quoteCount - 1);

            var quote = Mapper.Map<QuoteViewModel>(database.Quotes.OrderBy(q => q.ID).Skip(randomIndex).Take(1).Single());
            return Json((Session["quote"] = quote), JsonRequestBehavior.AllowGet); 
        }

        public JsonResult Stories()
        {
            var allStories = database.Posts.Where(p => p.Type == PostType.Story)
                .Select(p => new { p.ID, p.Title, p.Author.DisplayName, p.Created, p.WordCount })
                .OrderByDescending(p => p.Created)
                .ToList()
                .Select(p => new StoryViewModel { ID = p.ID, Author = p.DisplayName, Created = p.Created, Title = p.Title, WordCount = p.WordCount })
                .ToArray();
            return Json(allStories, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TagCounts()
        {
            var allTags = database.Tags
                .Select(t => new { t.Text, Count = t.Posts.Count() })
                .Where(t => t.Count > 0)
                .OrderByDescending(t => t.Count)
                .ToList()
                .Select(t => new TagCountViewModel { TagName = t.Text, Count = t.Count })
                .ToArray();
            return Json(allTags, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MonthCounts()
        {
            var monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames;
            var months = database.Posts.Select(p => new { p.Created.Month, p.Created.Year })
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .ToArray()
                .GroupBy(p => new { p.Month, p.Year })
                .Select(p => new MonthCountViewModel { Month = monthNames[p.Key.Month - 1], Year = p.Key.Year, Count = p.Count() })
                .ToArray();
            return Json(months, JsonRequestBehavior.AllowGet);
        }
    }
}
