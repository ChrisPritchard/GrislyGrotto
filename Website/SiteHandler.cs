using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using GrislyGrotto.Core;
using GrislyGrotto.Core.Entities;

namespace GrislyGrotto.Website
{
    public class SiteHandler : IHttpHandler, IRequiresSessionState
    {
        private readonly IUserService userService;
        private readonly IPostService postService;

        public SiteHandler()
        {
            userService = Global.Container.Resolve<IUserService>();
            postService = Global.Container.Resolve<IPostService>();
        }

        public void ProcessRequest(HttpContext context)
        {
            if(context.Request.Url.AbsolutePath.EndsWith("saveDraft.ashx"))
            {
                Thread.Sleep(1000);
                var post = TryEditorPost(context, PostStatus.Draft);
                if(post != null)
                    context.Response.Write(post.ID);
                context.Response.End();
                return;
            }
            
            var transform = new XslCompiledTransform();
            transform.Load(GetXslt(context));
            transform.Transform(GetXml(context), null, context.Response.Output);
            context.Response.End();
        }

        private static XmlReader GetXslt(HttpContext context)
        {
            var xsltName = context.Request.Url.AbsolutePath.EndsWith("Rss.aspx") ? "Rss.xslt" : "Site.xslt";
            return XmlReader.Create(new StringReader(File.ReadAllText(context.Server.MapPath("/Resources/" + xsltName))));
        }

        private XmlDocument GetXml(HttpContext context)
        {
            var xml = new XElement("Page");

            AddQuote(context, xml);

            if (TryEditorPost(context, PostStatus.Published) != null)
                context.Response.Redirect("/");

            if(ValidDeletePost(context))
                context.Response.Redirect("/");

            if (ValidAddComment(context))
                context.Response.Redirect(string.Format("?Post={0}", context.Request.Params["Post"]));
            
            if(ValidLoginStatusChange(context, xml))
                context.Response.Redirect("/");

            if(!string.IsNullOrEmpty(context.Session["LoggedUser"] as string))
                AddUserInfo(context, xml);

            var user = (User)null;
            if (!string.IsNullOrEmpty(context.Request.Params["ShowUser"]))
                user = userService.GetUserByFullName(context.Request.Params["ShowUser"]);

            AddPostHistory(user, xml);

            if (ValidEditorRequest(context, xml))
                return new XmlDocument { InnerXml = xml.ToString() };

            if (ValidPostRequest(context, xml))
                return new XmlDocument { InnerXml = xml.ToString() };

            if (ValidSearchRequest(user, context, xml))
                return new XmlDocument { InnerXml = xml.ToString() };

            if(ValidMonthRequest(user, context, xml))
                return new XmlDocument { InnerXml = xml.ToString() };

            var latestPosts = postService.GetLatest(user, DateTime.Now, 5, PostStatus.Published);
            foreach (var post in latestPosts)
                xml.Add(post.AsXElement());
            return new XmlDocument { InnerXml = xml.ToString() };
        }

        private static void AddQuote(HttpContext context, XContainer xml)
        {
            var quoteXml = (XDocument)context.Application["Quotes"];
            if (quoteXml == null)
            {
                quoteXml = XDocument.Load(context.Server.MapPath("/Resources/Quotes.xml"));
                context.Application["Quotes"] = quoteXml;
            }

            if (quoteXml.Root == null) return;

            var allQuotes = quoteXml.Root.Elements("Quote");
            xml.Add(allQuotes.ElementAt(new Random().Next(0, allQuotes.Count())));
        }

        private bool ValidDeletePost(HttpContext context)
        {
            if (string.IsNullOrEmpty(context.Request.Params["DeletePost"]))
                return false;

            int postID;
            if (!Int32.TryParse(context.Request.Params["DeletePost"], out postID))
                return false;

            var post = postService.GetSpecific(postID);
            if (post == null)
                return false;

            postService.DeletePost(post.ID);
            return true;
        }

        private void AddUserInfo(HttpContext context, XContainer xml)
        {
            var userFullName = context.Session["LoggedUser"].ToString();
            xml.Add(new XAttribute("LoggedUser", userFullName));

            var drafts = new XElement("Drafts");
            foreach (var post in postService.GetLatest(userService.GetUserByFullName(userFullName), DateTime.Now, null, PostStatus.Draft))
                drafts.Add(post.AsRecentTitleXElement());
            xml.Add(drafts);
        }

        private bool ValidLoginStatusChange(HttpContext context, XContainer xml)
        {
            if(!string.IsNullOrEmpty(context.Request.Params["Action"])
                && context.Request.Params["Action"].Equals("Logout"))
            {
                context.Session["LoggedUser"] = null;
                return true;
            }

            if (string.IsNullOrEmpty(context.Request.Params["LoginName"])
                || string.IsNullOrEmpty(context.Request.Params["LoginPassword"]))
                return false;

            var user = userService.Validate(context.Request.Params["LoginName"], context.Request.Params["LoginPassword"]);
            if(user == null)
            {
                xml.Add(new XAttribute("LastLoginNameTry", context.Request.Params["LoginName"]));
                return false;
            }

            context.Session["LoggedUser"] = user.FullName;
            return true;
        }

        private Post TryEditorPost(HttpContext context, PostStatus postStatusToUpdateWith)
        {
            if ((string.IsNullOrEmpty(context.Request.Params["Title"]) && postStatusToUpdateWith == PostStatus.Published)
                || string.IsNullOrEmpty(context.Request.Params["Content"]))
                return null;

            if (string.IsNullOrEmpty(context.Request.Params["Post"]))
            {
                return postService.AddPost(new Post
                    {
                        Title = context.Request.Params["Title"],
                        Content = context.Request.Params["Content"], 
                        Created = DateTime.Now,
                        Status = postStatusToUpdateWith,
                        User = userService.GetUserByFullName(context.Request.Params["LoggedUser"])
                    });
            }

            int postID;
            if (!Int32.TryParse(context.Request.Params["Post"], out postID))
                return null;

            var post = postService.GetSpecific(postID);
            if (post == null)
                return null;

            if (!context.Session["LoggedUser"].ToString().Equals(post.User.FullName))
                return null;

            var updatedPost = new Post
            {
                ID = post.ID,
                Title = context.Request.Params["Title"],
                Content = context.Request.Params["Content"],
                Created = post.Status == PostStatus.Draft ? DateTime.Now : post.Created,
                Status = postStatusToUpdateWith,
                User = post.User
            };
            postService.UpdatePost(updatedPost);
            return updatedPost;
        }

        private bool ValidAddComment(HttpContext context)
        {
            if (string.IsNullOrEmpty(context.Request.Params["Author"])
                || string.IsNullOrEmpty(context.Request.Params["Content"])
                || string.IsNullOrEmpty(context.Request.Params["Post"]))
                return false;

            int postID;
            if (!Int32.TryParse(context.Request.Params["Post"], out postID))
                return false;

            var post = postService.GetSpecific(postID);
            if (post == null)
                return false;

            var content = context.Request.Params["Content"];
            if (content.Contains("http") || content.Contains("<") || content.Contains(">"))
                return false;

            postService.AddComment(post, new Comment
            {
                Author = context.Request.Params["Author"],
                Content = content,
                Created = DateTime.Now,
            });
            return true;
        }

        private void AddPostHistory(User user, XContainer xml)
        {
            var recentPosts = new XElement("RecentPosts");
            foreach (var post in postService.GetLatest(user, DateTime.Now, 10, PostStatus.Published))
                recentPosts.Add(post.AsRecentTitleXElement());
            xml.Add(recentPosts);

            var postHistory = new XElement("PostHistory");
            foreach (var monthPostCount in postService.GetMonthPostCounts(user))
                postHistory.Add(monthPostCount.AsXElement());
            xml.Add(postHistory);
        }

        private bool ValidEditorRequest(HttpContext context, XContainer xml)
        {
            if (string.IsNullOrEmpty(context.Request.Params["Page"]) || !context.Request.Params["Page"].Equals("Editor"))
                return false;
            
            if (string.IsNullOrEmpty(context.Request.Params["Post"]))
            {
                xml.Add(new XAttribute("Editor", "new"));
                return true;
            }
                
            int postID;
            if (!Int32.TryParse(context.Request.Params["Post"], out postID))
                return false;
                    
            var post = postService.GetSpecific(postID);
            if (post == null)
                return false;

            xml.Add(new XAttribute("Editor", "existing"), post.AsXElement());
                return true;
        }

        private bool ValidPostRequest(HttpContext context, XContainer xml)
        {
            if (string.IsNullOrEmpty(context.Request.Params["Post"]))
                return false;

            int postID;
            if (!Int32.TryParse(context.Request.Params["Post"], out postID))
                return false;
            
            var post = postService.GetSpecific(postID);
            if (post == null)
                return false;

            xml.Add(post.AsXElement());
            if(post.Comments != null)
                foreach (var comment in post.Comments)
                    xml.Add(comment.AsXElement());

            return true;
        }

        private bool ValidSearchRequest(User user, HttpContext context, XContainer xml)
        {
            if (string.IsNullOrEmpty(context.Request.Params["SearchTerm"]))
                return false;
            xml.Add(new XAttribute("SearchTerm", context.Request.Params["SearchTerm"]));

            var postResults = postService.Search(user, context.Request.Params["SearchTerm"], PostStatus.Published);
            foreach (var post in postResults)
                xml.Add(post.AsXElement());
            return true;
        }

        private bool ValidMonthRequest(User user, HttpContext context, XContainer xml)
        {
            if (string.IsNullOrEmpty(context.Request.Params["Year"]) || string.IsNullOrEmpty(context.Request.Params["Month"]))
                return false;
            
            int year;
            if (!Int32.TryParse(context.Request.Params["Year"], out year) || !CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Contains(context.Request.Params["Month"]))
                return false;
                
            var monthPosts = postService.GetFromMonth(user, year, context.Request.Params["Month"], PostStatus.Published);
            foreach (var post in monthPosts)
                xml.Add(post.AsXElement());
            return true;
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}