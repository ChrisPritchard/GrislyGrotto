using System.Web.Mvc;
using System.Web.Routing;

namespace GrislyGrotto
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Posts",
				url: "p/{key}",
				defaults: new { controller = "Posts", action = "View" }
			);

            routes.MapRoute(
                name: "Login",
                url: "Login",
                defaults: new { controller = "Account", action = "Login" }
            );

            routes.MapRoute(
                name: "EditorCreate",
                url: "new",
                defaults: new { controller = "Posts", action = "Edit" }
            );

            routes.MapRoute(
                name: "EditorEdit",
                url: "edit/{key}",
                defaults: new { controller = "Posts", action = "Edit" }
            );

			routes.MapRoute(
				name: "Home",
				url: "{action}",
				defaults: new { controller = "Home", action = "Latest" }
			);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "Latest" }
            );
		}
	}
}
