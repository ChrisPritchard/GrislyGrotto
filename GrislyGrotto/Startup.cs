using System.Linq;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace GrislyGrotto
{
    /// <summary>
    /// OWIN Startup class. Called by Host.SystemWeb
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class Startup
    {
        // ReSharper disable once UnusedMember.Global
        public void Configuration(IAppBuilder app)
        {
            RouteTable.Routes.MapMvcAttributeRoutes();
            RegisterBundles(BundleTable.Bundles);

            ViewEngines.Engines.OfType<RazorViewEngine>().First()
                .ViewLocationFormats = new[] { "~/Views/{0}.cshtml" }; // all views are in the root of views

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookie",
                LoginPath = new PathString("/Account/Login")
            });
        }

        private static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/grislygrotto").Include(
                        "~/content/jquery-{version}.js",
                        "~/content/background-animation.js",
                        "~/content/editor.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/normalize.css",
                      "~/Content/skeleton.css",
                      "~/Content/grislygrotto.css"));
        }
    }
}