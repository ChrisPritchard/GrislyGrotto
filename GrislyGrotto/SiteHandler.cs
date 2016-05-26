using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.Xsl;
using GrislyGrotto.Framework;
using GrislyGrotto.Framework.Data;
using GrislyGrotto.Framework.Handlers;

namespace GrislyGrotto
{
    public class SiteHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var resourceService = Container.GetInstance<IResourceService>();
            if (Path.HasExtension(context.Request.Url.ToString()))
                resourceService.ReturnFile(context);

            var requestData = new RequestData(context.Request);
            var responseXml = new XElement("Response",
                new XAttribute("Url", context.Request.Url.AbsolutePath),
                resourceService.RandomQuote().AsXml());
            var authService = Container.GetInstance<IAuthenticationService>();
            if (authService.IsLoggedIn())
                responseXml.Add(new XElement("LoggedInUser", authService.LoggedInUser()));

            ProcessUsingAppropriateHandler(responseXml, requestData);

            var transformer = new XslCompiledTransform();
            var xslt = requestData.Segments.Length > 0 && requestData.Segments.Last().EqualsIgnoreCase("rss")
                           ? "/resources/rss.xslt"
                           : "/resources/site.xslt";
            transformer.Load(context.Server.MapPath(xslt));
            transformer.Transform(responseXml.CreateReader(), null, context.Response.OutputStream);
        }

        private static void ProcessUsingAppropriateHandler(XContainer responseXml, RequestData requestData)
        {
            if (SinglePostHandler.ShouldHandle(requestData))
                responseXml.Add(requestData.ProcessUsingHandler(Container.Create<SinglePostHandler>()));
            else if (EditorHandler.ShouldHandle(requestData))
                responseXml.Add(requestData.ProcessUsingHandler(Container.Create<EditorHandler>()));
            else if (PostRangeHandler.ShouldHandle(requestData))
                responseXml.Add(requestData.ProcessUsingHandler(Container.Create<PostRangeHandler>()));
            else
                responseXml.Add(requestData.ProcessUsingHandler(Container.Create<HomePageHandler>()));
        }
    }
}