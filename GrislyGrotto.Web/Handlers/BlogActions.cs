using System;
using System.Web;
using System.Web.Routing;
using GrislyGrotto.Web.Core;
using GrislyGrotto.Web.Data;

namespace GrislyGrotto.Web.Handlers
{
    public class BlogActions : IHttpHandler, IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            foreach (var key in requestContext.RouteData.Values.Keys)
                requestContext.HttpContext.Items.Add(key.ToLowerInvariant(), requestContext.RouteData.Values[key].ToString().ToLowerInvariant());

            return this;
        }

        public void ProcessRequest(HttpContext context)
        {
            var action = context.Items["action"].ToString();

            if (action.Equals("checklogin"))
                CheckLogin(context);
            else if (action.Equals("login"))
                Login(context);
            else if (action.Equals("logout"))
                Logout(context);
            else if (action.Equals("comment"))
                Comment(context);
            else if (action.Equals("post"))
                Post(context);

            if (!action.Equals("checklogin"))
                context.Response.Redirect(context.Request.UrlReferrer.AbsoluteUri);
        }

        private static void CheckLogin(HttpContext context)
        {
            var username = context.Request.Params["LoginName"];
            var password = context.Request.Params["LoginPassword"];
            context.Response.Write(
                (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) &&
                 UserServices.Validate(username, password) != null).ToString());
        }

        private static void Login(HttpContext context)
        {
            var username = context.Request.Params["LoginName"];
            var password = context.Request.Params["LoginPassword"];
            var user = UserServices.Validate(username, password);
            if(user == null) return;

            var isPermanant = context.Request.Params["RememberMe"] != null && context.Request.Params["RememberMe"].Equals("on");
                context.Response.Cookies.Add(new HttpCookie("grislygrotto.co.nz", user.FullName) 
                { Expires = isPermanant ? DateTime.MaxValue : DateTime.Now.AddMinutes(20) });
        }
        
        private static void Logout(HttpContext context)
        {
            if (context.Request.Cookies["grislygrotto.co.nz"] != null)
                context.Response.Cookies["grislygrotto.co.nz"].Expires = DateTime.Now.AddDays(-1);
        }

        private static void Comment(HttpContext context)
        {
            var postID = context.Request.Params["PostID"].As<int>();
            var author = context.Request.Params["Author"];
            var content = context.Request.Params["Content"];
            if (content.Contains("http") || content.Contains("<") || content.Contains(">"))
                return;
            if(postID != 0 && !string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(content))
                PostServices.AddComment(postID, new Comment { Author = author, Created = DateTime.Now, Content = content });
        }

        private static void Post(HttpContext context)
        {
            var postID = context.Request.Params["PostID"].As<int>();
            var author = context.Request.Params["LoggedUser"];
            var title = context.Request.Params["Title"];
            var content = context.Request.Params["Content"];

            if (string.IsNullOrEmpty(author) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
                return;

            if (postID != 0)
                PostServices.UpdatePost(postID, title, content);
            else
                postID = PostServices.AddPost(new Post
                {
                    User = UserServices.GetUserByFullName(author),
                    Title = title,
                    Content = content,
                    Created = DateTime.Now,
                    Status = PostStatus.Published
                }).ID;

            context.Response.Redirect("/Posts/" + postID, true);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}