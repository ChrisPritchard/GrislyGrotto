using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GrislyGrotto.Models;
using GrislyGrotto.Models.Components;
using GrislyGrotto.Models.Defaults;
using GrislyGrotto.Models.LinqToSql;
using GrislyGrotto.Mvc;
using Microsoft.Practices.Unity;
using MvcContrib.Unity;

namespace GrislyGrotto
{
    public class MvcApplication : HttpApplication, IUnityContainerAccessor
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Latest", "Blog/Latest/{userFullname}",
                new { controller = "Blog", action = "Latest", userFullname = "" });
            routes.MapRoute("LatestAll", "Blog/Latest",
                new { controller = "Blog", action = "Latest", userFullname = "" });
            routes.MapRoute("Specific", "Blog/Specific/{PostID}",
                new { controller = "Blog", action = "Specific", postID = "" });
            routes.MapRoute("Editor", "Blog/Editor/{PostID}",
                new { controller = "Blog", action = "Editor", postID = "" });
            routes.MapRoute("EditorNew", "Blog/Editor",
                new { controller = "Blog", action = "Editor", postID = "" });
            routes.MapRoute("EditPost", "Blog/EditPost/{PostID}",
                new { controller = "Blog", action = "EditPost", postID = "" });
            routes.MapRoute("CreatePost", "Blog/CreatePost",
                new { controller = "Blog", action = "CreatePost" });
            routes.MapRoute("CreateComment", "Blog/CreateComment",
                new { controller = "Blog", action = "CreateComment" });
            routes.MapRoute("DeleteComment", "Blog/DeleteComment",
                new { controller = "Blog", action = "DeleteComment" });

            routes.MapRoute("All", "Blog/All",
                new { controller = "Blog", action = "All" });
            routes.MapRoute("AllUserImages", "Blog/AllUserImages",
                new { controller = "Blog", action = "AllUserImages" });
            routes.MapRoute("UploadImage", "Blog/UploadImage/{PostID}",
                new { controller = "Blog", action = "UploadImage", postID = "" });

            routes.MapRoute("MonthWithAuthor", "Blog/{Year}/{Month}/{userFullname}",
                new { controller = "Blog", action = "Month", year = 2006, month = "June", userFullname = "" });
            routes.MapRoute("Month", "Blog/{Year}/{Month}",
                new { controller = "Blog", action = "Month", year = 2006, month = "June", userFullname = "" });

            routes.MapRoute("Feed", "Feed/{action}/{userFullname}",
                new { controller = "Feed", action = "Rss", userFullname = "" });

            routes.MapRoute("Default", "{controller}/{action}/{id}",
                new { controller = "Blog", action = "Latest", id = "" });  // Parameter defaults
        }

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new XslViewEngine());

            ControllerBuilder.Current.SetControllerFactory(new UnityControllerFactory());

            InitialiseContainer();
        }

        private void InitialiseContainer()
        {
            if (container.IsNull())
            {
                container = new UnityContainer();

                container.RegisterType(typeof(IAuthentication), typeof(SessionAuthentication));
                container.RegisterInstance(typeof(GrislyGrottoDBDataContext), new GrislyGrottoDBDataContext());

                container.RegisterInstance(typeof(IUserRepository), new DefaultUserRepository());
                string quoteFilePath = Path.Combine(Server.MapPath("/"), "Content/Quotes.xml");
                container.RegisterInstance(typeof(IQuoteRepository), new XmlQuotesRepository(quoteFilePath));

                container.RegisterType(typeof(IBlogRepository), typeof(LinqBlogRepository));
                container.RegisterType(typeof(ICommentRepository), typeof(LinqCommentRepository));
            }
        }

        private static UnityContainer container;
        public IUnityContainer Container
        {
            get { return container; }
        }
    }
}