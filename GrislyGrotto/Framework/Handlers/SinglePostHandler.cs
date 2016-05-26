using System;
using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.Framework.Data;
using GrislyGrotto.Framework.Data.Primitives;

namespace GrislyGrotto.Framework.Handlers
{
    public class SinglePostHandler : IHandler
    {
        private readonly IPostData postData;

        public SinglePostHandler(IPostData postData)
        {
            this.postData = postData;
        }

        public static bool ShouldHandle(RequestData requestData)
        {
            return
                requestData.Segments.Length == 2 && requestData.Segments[0].EqualsIgnoreCase("posts") && requestData.Segments[1].IsInteger();
        }

        public IEnumerable<object> Get(RequestData requestData)
        {
            var post = postData.SinglePost(int.Parse(requestData.Segments[1]));
            yield return post;

            yield return postData.MonthPostCounts(post.Username).OrderByDescending(mc => mc.Year).ThenByDescending(mc => mc.Month);
            yield return postData.PostsByStatus("Story", post.Username).Select(p => new Story(p));
        }

        public IEnumerable<object> Post(RequestData requestData)
        {
            Func<string, bool> isClean =
                content => !content.Contains("http") && !content.Contains("<") && !content.Contains(">");

            if (requestData.FormCollection.ContainsKey("Author") && requestData.FormCollection.ContainsKey("Content") && isClean(requestData.FormCollection["Content"]))
            postData.AddComment(
                new Comment(requestData.FormCollection["Author"], requestData.FormCollection["Content"]),
                int.Parse(requestData.Segments[1]));

            yield return postData.SinglePost(int.Parse(requestData.Segments[1]));
        }
    }
}