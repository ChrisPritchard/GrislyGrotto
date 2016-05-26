using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using GrislyGrotto.Data;
using GrislyGrotto.Data.Primitives;

namespace GrislyGrotto
{
    public class PostsServiceHandler : IHttpHandler
    {
        private readonly IPosts posts;

        public PostsServiceHandler()
        {
            posts = PostsFactory.GetInstance();
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            var segments = context.Request.Url.AbsolutePath
                .ToLower().Replace("posts", string.Empty).Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0 && context.Request.HttpMethod.Equals("GET"))
                context.Response.WriteJson(posts.LatestPosts());
            else if (segments.Length == 0)
                AddPost(context);
            else if (segments[0].Equals("quote"))
                ReturnQuote(context);
            else if (segments[0].Equals("rss"))
                ReturnRss(context);
            else if (segments[0].Equals("archives"))
                context.Response.WriteJson(posts.MonthPostCounts().OrderByDescending(m => m));
            else if (segments[0].Equals("stories"))
                context.Response.WriteJson(posts.PostsThatAreStories().OrderByDescending(p => p.TimePosted));
            else if (segments[0].Equals("search"))
                ReturnSearchResults(context);
            else if (segments[0].Equals("authenticate"))
                CheckAuthentication(context, segments);
            else if (segments.Length == 2 && segments[1].Equals("addcomment"))
                AddComment(context, segments);
            else if (segments.Length == 2)
                ReturnMonth(context, segments);
            else
                HandleSinglePost(context, segments);
        }

        private static void ReturnQuote(HttpContext context)
        {
            context.Response.WriteJson(Quotes.RandomQuote());
        }

        private void ReturnRss(HttpContext context)
        {
            context.Response.ContentType = "application/rss+xml";

            var serializer = new XmlSerializer(typeof (Post[]));
            var xml = new StringBuilder();
            var textWriter = new StringWriter(xml);
            serializer.Serialize(textWriter, posts.LatestPosts().ToArray());

            var transform = new XslCompiledTransform();
            transform.Load(context.Server.MapPath("/resources/rss.xslt"));
            transform.Transform(new XmlDocument { InnerXml = xml.ToString() }, null, context.Response.OutputStream);
        }

        private void ReturnMonth(HttpContext context, IList<string> segments)
        {
            var month = segments[1].AsMonthNum();
            int year;
            if (int.TryParse(segments[0], out year))
                context.Response.WriteJson(posts.PostsForMonth(year, month));
        }

        private void ReturnSearchResults(HttpContext context)
        {
            var searchTerm = context.Request.Params["searchTerm"];
            context.Response.WriteJson(posts.SearchResults(searchTerm));
        }

        private void CheckAuthentication(HttpContext context, IList<string> segments)
        {
            int id;
            if (segments.Count == 2 && int.TryParse(segments[1], out id))
                context.Response.WriteJson(GetValidUser(context, posts.SinglePost(id).Author));
            else
                context.Response.WriteJson(GetValidUser(context));
        }

        private void AddPost(HttpContext context)
        {
            string author;
            if ((author = GetValidUser(context)) == null)
                return;
            var post = new Post
            {
                Author = author,
                Title = context.Request.Params["title"],
                Content = context.Request.Params["content"],
                IsStory = bool.Parse(context.Request.Params["isstory"])
            };
            posts.AddOrEditPost(post);
        }

        private void AddComment(HttpContext context, IList<string> segments)
        {
            int id;
            if (!int.TryParse(segments[0], out id))
                return;

            var comment = new Comment(context.Request.Params["author"], context.Request.Params["content"]);
            posts.AddComment(comment, id);
        }

        private void HandleSinglePost(HttpContext context, IList<string> segments)
        {
            int id;
            if (!int.TryParse(segments[0], out id))
                return;
            var post = posts.SinglePost(id);

            if (context.Request.HttpMethod.Equals("GET"))
                context.Response.WriteJson(post);
            else
            {
                if (GetValidUser(context, post.Author) == null)
                    return;
                post.Title = context.Request.Params["title"];
                post.Content = context.Request.Params["content"];
                post.IsStory = bool.Parse(context.Request.Params["isstory"]);
                posts.AddOrEditPost(post);
            }
        }

        private static string GetValidUser(HttpContext context, string postAuthor = null)
        {
            var username = context.Request.Params["username"];
            var password = context.Request.Params["password"];

            if (username == null || password == null)
                return null;

            if (password.Equals("test") && (username.Equals("daedalus") || username.Equals("icarus") || username.Equals("helios")))
                return username[0].ToString().ToUpper() + username.Substring(1);

            if (username.Equals("pdc") && password.Equals("***REMOVED***") && (postAuthor == null || postAuthor.Equals("Peter")))
                return "Peter";

            if (username.Equals("aquinas") && password.Equals("***REMOVED***") && (postAuthor == null || postAuthor.Equals("Christopher")))
                return "Christopher";

            return null;
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}