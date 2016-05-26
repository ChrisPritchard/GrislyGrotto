using System;
using System.Web;
using System.Web.Routing;
using GrislyGrotto.Web.Handlers;

namespace GrislyGrotto.Web
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.Add(new Route("Format/{Format}", new BlogContent()));

            RouteTable.Routes.Add(new Route(string.Empty, new BlogContent()));
            RouteTable.Routes.Add(new Route("Posts/{ID}", new BlogContent()));
            RouteTable.Routes.Add(new Route("Posts/{Month}/{Year}", new BlogContent()));
            RouteTable.Routes.Add(new Route("Search/{SearchTerm}", new BlogContent()));

            RouteTable.Routes.Add(new Route("{Author}", new BlogContent()));
            RouteTable.Routes.Add(new Route("{Author}/Posts/{ID}", new BlogContent()));
            RouteTable.Routes.Add(new Route("{Author}/Posts/{Month}/{Year}", new BlogContent()));
            RouteTable.Routes.Add(new Route("{Author}/Search/{SearchTerm}", new BlogContent()));

            RouteTable.Routes.Add(new Route("Editor/{Editor}", new BlogContent()));
            RouteTable.Routes.Add(new Route("Action/{Action}", new BlogActions()));

            RouteTable.Routes.Add(new Route("Sites/{HandlerPath}", new SubsiteHandler()));
        }
    }
}