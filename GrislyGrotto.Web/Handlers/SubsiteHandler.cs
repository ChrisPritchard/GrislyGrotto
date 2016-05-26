using System.Web;
using System.Web.Routing;
using System.Web.SessionState;

namespace GrislyGrotto.Web.Handlers
{
    public class SubsiteHandler : IRouteHandler, IHttpHandler, IRequiresSessionState
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write("<h1>Test</h1>");
        }
    }
}