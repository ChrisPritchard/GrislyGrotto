using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrislyGrotto.Framework.Data;
using GrislyGrotto.Framework.Data.Primitives;

namespace GrislyGrotto.Framework.Handlers
{
    public class PostRangeHandler : IHandler
    {
        private readonly IPostData postData;
        private readonly IUserData userData;
        private readonly INavigationService navigationService;
        
        public PostRangeHandler(IPostData postData, IUserData userData, INavigationService navigationService)
        {
            this.postData = postData;
            this.userData = userData;
            this.navigationService = navigationService;
        }

        public static bool ShouldHandle(RequestData requestData)
        {
            return
                (requestData.Segments.Length > 0 && requestData.Segments[0].EqualsIgnoreCase("search"))
                || (requestData.Segments.Length == 2 && requestData.Segments[0].AsMonthNum() != -1 && requestData.Segments[1].IsInteger())
                || (requestData.Segments.Length == 3 && requestData.Segments[1].AsMonthNum() != -1 && requestData.Segments[2].IsInteger());
        }

        public IEnumerable<object> Get(RequestData requestData)
        {
            var user = userData.AllUsernames().Any(u => u.EqualsIgnoreCase(requestData.Segments[0]))
                           ? requestData.Segments[0]
                           : null;

            yield return postData.MonthPostCounts().OrderByDescending(mc => mc.Year).ThenByDescending(mc => mc.Month);
            yield return postData.PostsByStatus("Story", user).Select(p => new Story(p));

            if(requestData.Segments[0].EqualsIgnoreCase("search"))
            {
                if(requestData.Segments.Length != 2)
                    navigationService.RedirectToHome();
                var searchTerm = HttpUtility.UrlDecode(requestData.Segments[1]);
                yield return postData.SearchResults(searchTerm);
                yield return new KeyValuePair<string, string>("SearchTerm", searchTerm);
                yield break;
            }

            if (requestData.Segments.Length == 2)
                yield return postData.PostsForMonth(int.Parse(requestData.Segments[1]), requestData.Segments[0].AsMonthNum(), user).ToArray().AsXml();
            else
                yield return postData.PostsForMonth(int.Parse(requestData.Segments[2]), requestData.Segments[1].AsMonthNum(), user);
        }

        public IEnumerable<object> Post(RequestData requestData)
        {
            if (!requestData.FormCollection.ContainsKey("SearchTerm"))
                navigationService.RedirectToHome();
            
            yield return postData.MonthPostCounts().OrderByDescending(mc => mc.Year).ThenByDescending(mc => mc.Month);

            var searchTerm = HttpUtility.UrlDecode(requestData.FormCollection["SearchTerm"]);
            yield return postData.SearchResults(searchTerm);
            yield return new KeyValuePair<string, string>("SearchTerm", searchTerm);
        }
    }
}