using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GrislyGrotto.Mvc;

namespace GrislyGrotto
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Latest", "Blog/Latest/{Author}",
                new { controller = "Blog", action = "Latest", AuthorID = "" });
            routes.MapRoute("LatestAll", "Blog/Latest",
                new { controller = "Blog", action = "Latest", AuthorID = "" });
            routes.MapRoute("Specific", "Blog/Specific/{BlogID}",
                new { controller = "Blog", action = "Specific", BlogID = "" });
            routes.MapRoute("Editor", "Blog/Editor/{BlogID}",
                new { controller = "Blog", action = "Editor", BlogID = "" });
            routes.MapRoute("EditorNew", "Blog/Editor",
                new { controller = "Blog", action = "Editor", BlogID = "" });
            routes.MapRoute("EditBlog", "Blog/EditBlog/{BlogID}",
                new { controller = "Blog", action = "EditBlog", BlogID = "" });
            routes.MapRoute("CreateBlog", "Blog/CreateBlog",
                new { controller = "Blog", action = "CreateBlog" });
            routes.MapRoute("CreateComment", "Blog/CreateComment",
                new { controller = "Blog", action = "CreateComment" });
            routes.MapRoute("DeleteComment", "Blog/DeleteComment",
                new { controller = "Blog", action = "DeleteComment" });

            routes.MapRoute("All", "Blog/All",
                new { controller = "Blog", action = "All" });
            routes.MapRoute("AllUserImages", "Blog/AllUserImages",
                new { controller = "Blog", action = "AllUserImages" });
            routes.MapRoute("UploadImage", "Blog/UploadImage/{BlogID}",
                new { controller = "Blog", action = "UploadImage", BlogID = "" });

            routes.MapRoute("MonthWithAuthor", "Blog/{Year}/{Month}/{Author}",
                new { controller = "Blog", action = "Month", Year = 2006, Month = "June", AuthorID = "" });
            routes.MapRoute("Month", "Blog/{Year}/{Month}",
                new { controller = "Blog", action = "Month", Year = 2006, Month = "June", AuthorID = "" });

            //routes.MapRoute("LoadOldData", "Blog/LoadOldXml",
            //    new { controller = "Blog", action = "LoadOldXml" });

            routes.MapRoute("Feed", "Feed/{action}/{Author}",
                new { controller = "Feed", action = "Rss", Author = "" });

            routes.MapRoute("Default", "{controller}/{action}/{id}",
                new { controller = "Blog", action = "Latest", id = "" });  // Parameter defaults
        }

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new XslViewEngine());
        }
    }
}