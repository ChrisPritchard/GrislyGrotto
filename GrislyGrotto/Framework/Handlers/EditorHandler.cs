using System;
using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.Framework.Data;
using GrislyGrotto.Framework.Data.Primitives;

namespace GrislyGrotto.Framework.Handlers
{
    public class EditorHandler : IHandler
    {
        private readonly IPostData postData;
        private readonly INavigationService navigationService;
        private readonly IAuthenticationService authenticationService;

        public EditorHandler(IPostData postData, INavigationService navigationService, IAuthenticationService authenticationService)
        {
            this.postData = postData;
            this.navigationService = navigationService;
            this.authenticationService = authenticationService;
        }

        public static bool ShouldHandle(RequestData requestData)
        {
            return requestData.Segments.Length > 0 && requestData.Segments[0].EqualsIgnoreCase("editor");
        }

        public IEnumerable<object> Get(RequestData requestData)
        {
            if(!authenticationService.IsLoggedIn())
                navigationService.RedirectToHome();

            var user = authenticationService.LoggedInUser();
            yield return postData.MonthPostCounts(user).OrderByDescending(mc => mc.Year).ThenByDescending(mc => mc.Month);
            yield return postData.PostsByStatus("Story", user).Select(p => new Story(p));

            if (requestData.Segments.Length == 1 || !requestData.Segments[1].IsInteger())
                yield break;

            var existingPost = postData.SinglePost(int.Parse(requestData.Segments[1]));
            if (existingPost.Username.Equals(authenticationService.LoggedInUser()))
                yield return existingPost;
        }

        public IEnumerable<object> Post(RequestData requestData)
        {
            if (!requestData.FormCollection.ContainsKey("Title")
                || !requestData.FormCollection.ContainsKey("Content"))
            {
                navigationService.RedirectToHome();
                yield break;
            }

            var editorUser = requestData.FormCollection["EditorUser"];

            var newPost = requestData.Segments.Length > 1 && requestData.Segments[1].IsInteger()
                ? postData.SinglePost(int.Parse(requestData.Segments[1]))
                : new Post { Username = editorUser, TimePosted = DateTime.Now };

            if (!newPost.Username.Equals(editorUser))
                navigationService.RedirectToHome();
            else
            {
                newPost.Title = requestData.FormCollection["Title"];
                newPost.Content = requestData.FormCollection["Content"];
                newPost.IsStory = requestData.FormCollection.Value("IsStory").Equals("on");

                navigationService.RedirectToPost(postData.AddOrEditPost(newPost));
            }

            yield break;
        }
    }
}