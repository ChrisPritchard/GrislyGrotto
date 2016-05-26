using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.Framework.Data;
using GrislyGrotto.Framework.Data.Primitives;

namespace GrislyGrotto.Framework.Handlers
{
    public class HomePageHandler : IHandler
    {
        private readonly IPostData postData;
        private readonly IUserData userData;
        private readonly INavigationService navigationService;
        private readonly IAuthenticationService authenticationService;

        public HomePageHandler(IPostData postData, IUserData userData, INavigationService navigationService, IAuthenticationService authenticationService)
        {
            this.postData = postData;
            this.userData = userData;
            this.navigationService = navigationService;
            this.authenticationService = authenticationService;
        }

        public IEnumerable<object> Get(RequestData requestData)
        {
            if(requestData.Segments.Length > 0 && requestData.Segments[0].EqualsIgnoreCase("logout"))
            {
                authenticationService.Logout();
                navigationService.RedirectToHome();
                yield break;
            }

            var user = (requestData.Segments.Length > 0 && userData.AllUsernames().Any(u => u.EqualsIgnoreCase(requestData.Segments[0])))
                           ? requestData.Segments[0]
                           : null;

            yield return postData.LatestPosts(Constants.LatestPostsCount, user);

            yield return postData.LatestPosts(Constants.LatestPostsCount, "Christopher").Select(p => new RecentPost(p));
            yield return postData.LatestPosts(Constants.LatestPostsCount, "Peter").Select(p => new RecentPost(p));

            yield return postData.MonthPostCounts(user).OrderByDescending(mc => mc.Year).ThenByDescending(mc => mc.Month);
            yield return postData.PostsByStatus("Story", user).Select(p => new Story(p));

            if (requestData.Segments.Length > 0 && requestData.Segments[0].EqualsIgnoreCase("LoginFailed"))
                yield return "LoginFailed";
        }

        public IEnumerable<object> Post(RequestData requestData)
        {
            if ((!requestData.FormCollection.ContainsKey("Username") || !requestData.FormCollection.ContainsKey("Password")) && authenticationService.IsLoggedIn())
                authenticationService.Logout();
            else
            {
                if (userData.ValidateCredentials(requestData.FormCollection["Username"], requestData.FormCollection["Password"]))
                    authenticationService.Login(
                        userData.FullNameOf(requestData.FormCollection["Username"]), 
                        requestData.FormCollection.Value("RememberMe").Equals("on"));
                else
                    navigationService.RedirectToHome(true);
            }

            navigationService.RedirectToHome();
            yield break;
        }
    }
}