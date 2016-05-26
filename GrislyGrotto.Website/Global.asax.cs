using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using GrislyGrotto.DAL.SQLServer;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Website.Models.Defaults;
using Microsoft.Practices.Unity;
using Mvc.XslViewEngine;
using MvcContrib.Unity;

namespace GrislyGrotto.Website
{
    public class MvcApplication : HttpApplication, IUnityContainerAccessor
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Latest", "Blog/Latest/{authorFullname}",
                new { controller = "Blog", action = "Latest", authorFullname = "" });
            routes.MapRoute("LatestAll", "Blog/Latest",
                new { controller = "Blog", action = "Latest", authorFullname = "" });
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

            routes.MapRoute("MonthWithAuthor", "Blog/{Year}/{Month}/{authorFullname}",
                new { controller = "Blog", action = "Month", year = 2006, month = "June", authorFullname = "" });
            routes.MapRoute("Month", "Blog/{Year}/{Month}",
                new { controller = "Blog", action = "Month", year = 2006, month = "June", authorFullname = "" });

            routes.MapRoute("Feed", "Feed/{action}/{authorFullname}",
                new { controller = "Feed", action = "Rss", authorFullname = "" });

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
            if (container == null)
            {
                container = new UnityContainer();

                container.RegisterInstance(typeof(ConnectionStringSettingsCollection), WebConfigurationManager.ConnectionStrings);

                container.RegisterType(typeof(IUserRepository), typeof(SQLServerUserRepository));
                container.RegisterType(typeof(IAuthentication), typeof(SessionAuthentication));

                string quoteFilePath = Path.Combine(Server.MapPath("/"), "Content/Quotes.xml");
                container.RegisterInstance(typeof(IQuoteRepository), new XmlQuotesRepository(quoteFilePath));

                container.RegisterType(typeof(IPostRepository), typeof(SQLServerPostRepository));
                container.RegisterType(typeof(ICommentRepository), typeof(SQLServerCommentRepository));
            }
        }

        private static UnityContainer container;
        public IUnityContainer Container
        {
            get { return container; }
        }
    }
}