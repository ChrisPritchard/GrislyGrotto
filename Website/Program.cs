using System.Xml;
using System.Xml.Xsl;
using System.Collections.Generic;
using System;
using System.Web;
using System.Web.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Web.SessionState;

/// <summary>
/// The core page controller for the web application. Entirely controls what is presented to the screen
/// </summary>
/// <remarks>Christopher Pritchard, 01/05/08</remarks>
public class Program : IHttpHandler, IRequiresSessionState
{
    /// <summary>
    /// In the beginrequest event the Xml is gathered and transformed via the Xslt onto the Response stream.
    /// The response stream is then ended.
    /// If the request is for a file with an extension, then the file in question is written to the response stream this method is exited
    /// </summary>
    /// <param name="context"></param>
    public void ProcessRequest(HttpContext context)
    {
        if (Path.HasExtension(context.Request.PhysicalPath) && File.Exists(context.Request.PhysicalPath)
            && (WebConfigurationManager.AppSettings["processFile"] == null || WebConfigurationManager.AppSettings["processFile"].ToLower() != Path.GetFileName(context.Request.PhysicalPath).ToLower()))
        {
            context.Response.WriteFile(context.Request.PhysicalPath);
            context.Response.End();
            return;
        }

        XslCompiledTransform transform = new XslCompiledTransform();
        transform.Load(context.Request.PhysicalApplicationPath + "Xslt.xslt");
        transform.Transform(GetResponseXmlReader(context), null, context.Response.Output);
        context.Response.End();
    }

    /// <summary>
    /// This method passes the request onto the site, takes the XDocument passed back and optionally
    /// saves it to the filesystem before passing it to be transformed.
    /// </summary>
    /// <param name="context"></param>
    /// <returns>A XmlReader from the generated Xml</returns>
    private XmlReader GetResponseXmlReader(HttpContext context)
    {
        XDocument document = new Site(context.Request, context.Session).GetXml();
        if (WebConfigurationManager.AppSettings["debugXmlPath"] != null)
            document.Save(context.Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["debugXmlPath"]);

        return document.CreateReader();
    }

    public bool IsReusable
    {
        get { return false; }
    }
}
