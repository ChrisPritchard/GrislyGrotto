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
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Latest", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Month",
                url: "Home/Month/{monthName}/{year}",
                defaults: new { controller = "Home", action = "Month" });
        }
    }
}