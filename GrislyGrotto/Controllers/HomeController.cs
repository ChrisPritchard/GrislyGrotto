using GrislyGrotto.Models;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GrislyGrotto.Controllers
{
	public class HomeController : Controller
	{
        const int _latestCount = 5;
        const int _searchResultsPerPage = 10;

		public async Task<ActionResult> Latest(int? pageNum)
		{
			var page = pageNum.HasValue ? pageNum.Value : 1;
			ViewBag.Page = page;

            var model = await PostService.Current.Latest(_latestCount, page);
			return View(model);
		}

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Search()
        {
            return View();
        }

        public async Task<JsonResult> RunSearch(string freeText, bool storiesOnly, string author, string orderBy, int page)
        {
            var filter = new List<string>();
            if (storiesOnly)
                filter.Add("isStory");
            if (!string.IsNullOrWhiteSpace(author))
                filter.Add("author eq '" + author + "'");

            var results = await SearchClient.Current.Search(orderBy: orderBy, filter: string.Join(" and ", filter),
                count: _searchResultsPerPage, skip: (page - 1) * _searchResultsPerPage, 
                searchText: freeText,
                returnFields: new[] { "title", "key", "author", "isStory", "date" },
                searchFields: new[] { "title", "content" });

            var model = results.Select(o => new
            {
                key = o.Document.Key,
                title = o.Document.Title,
                date = o.Document.Date.Add(PostService.NzTimeZone).ToString("dddd, dd/MM/yyyy"),
                author = o.Document.Author,
                isStory = o.Document.IsStory,
                snippet = GetHighlight(o)
            });

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private string GetHighlight(SearchResult<Post> post)
        {
            if (post.Highlights == null || !post.Highlights.ContainsKey("content"))
                return null;

            var source = "..." + string.Join("...", post.Highlights["content"]) + "...";

            source = source.Replace("<em>", "EMPHASIS").Replace("</em>", "EMPHASISEND");

            var firstEnd = source.IndexOf('>');
            if (firstEnd != -1 && firstEnd < source.IndexOf('<'))
                source = source.Substring(firstEnd + 1);

            var lastEnd = source.LastIndexOf('>');
            var lastStart = source.LastIndexOf('<');
            if (lastStart > lastEnd)
                source = source.Substring(0, lastStart);

            var fragments = source.Split('<');
            var full = fragments[0];
            for (var i = 1; i < fragments.Length; i++)
            {
                full += fragments[i].Substring(fragments[i].IndexOf('>') + 1);
            }

            return full.Replace("EMPHASISEND", "</em>").Replace("EMPHASIS", "<em>");
        }
	}
}