using System;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.Xsl;
using GrislyGrotto.Web.Core;
using GrislyGrotto.Web.Data;
using System.Web.Routing;

namespace GrislyGrotto.Web.Handlers
{
    public class BlogContent : IHttpHandler, IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            foreach (var key in requestContext.RouteData.Values.Keys)
                requestContext.HttpContext.Items.Add(key.ToLowerInvariant(), requestContext.RouteData.Values[key].ToString().ToLowerInvariant());

            return this;
        }

        public void ProcessRequest(HttpContext context)
        {
            var filterUser = context.Items.Contains("author")
                ? UserServices.GetUserByFullName(context.Items["author"].ToString()) : null;

            var pageXml = new XElement("Page",
                GetQuote(context),
                new XElement("RecentPosts", PostServices.GetLatest(10, PostStatus.Published, filterUser)
                    .Select(post => post.AsRecentTitleXElement())),
                new XElement("PostHistory", PostServices.GetMonthPostCounts(filterUser)
                    .Select(month => month.AsXElement())));

            var userIsLoggedIn = context.Request.Cookies["grislygrotto.co.nz"] != null;
            if (userIsLoggedIn)
                pageXml.Add(new XAttribute("LoggedUser", context.Request.Cookies["grislygrotto.co.nz"].Value));

            if (userIsLoggedIn && context.Items.Contains("editor"))
                pageXml.Add(context.Items["editor"].Equals("new") 
                    ? new XElement("Editor") : new XElement("Editor", PostServices.GetSpecific(context.Items["editor"].As<int>()).AsXElement(false)));
            else if(context.Items.Contains("id"))
                pageXml.Add(PostServices.GetSpecific(context.Items["id"].As<int>())
                    .AsXElement(true));
            else if(context.Items.Contains("month"))
                pageXml.Add(PostServices.GetFromMonth(context.Items["year"].As<int>(), context.Items["month"].ToString(), filterUser)
                    .Select(post => post.AsXElement()));
            else if(context.Items.Contains("searchterm"))
                pageXml.Add(PostServices.Search(context.Items["searchterm"].ToString(), 30, filterUser)
                    .Select(post => post.AsXElement()));
            else
                pageXml.Add(PostServices.GetLatest(5, PostStatus.Published, filterUser)
                    .Select(post => post.AsXElement()));

            var transformer = new XslCompiledTransform();
            transformer.Load(context.Server.MapPath("/Resources/"
                + (context.Items.Contains("format") ? context.Items["format"] + ".xslt" : "Site.xslt")));
            transformer.Transform(pageXml.CreateReader(), null, context.Response.Output);
            context.Response.End();
        }

        private static XElement GetQuote(HttpContext context)
        {
            var quoteXml = context.Application["QuoteXml"] != null 
                ? XElement.Parse(context.Application["QuoteXml"].ToString()) 
                : XElement.Load(context.Server.MapPath("/Resources/Quotes.xml"));
            if (context.Application["QuoteXml"] == null) context.Application["QuoteXml"] = quoteXml.ToString();
            return quoteXml.Descendants("Quote").ElementAt(new Random().Next(0, quoteXml.Descendants("Quote").Count()));
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}