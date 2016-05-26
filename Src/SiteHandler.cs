using System.Collections.Generic;
using System;
using System.Web;
using System.Web.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Web.SessionState;

namespace GrislyGrotto
{
	/// <summary>
	/// The core page controller for the web application. Entirely controls what is presented to the screen
	/// </summary>
	/// <remarks>Christopher Pritchard, 01/05/08</remarks>
    public class SiteHandler : IHttpHandler, IRequiresSessionState
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
                && ("default.aspx" != Path.GetFileName(context.Request.PhysicalPath).ToLower()))
            {
                context.Response.WriteFile(context.Request.PhysicalPath);
                context.Response.End();
                return;
            }

            //determine which handler should process the request
	        switch (context.Request.Url.Segments[0].ToLower())
	        {
	            default:
					new Blog(context.Request, context.Session).TransformOntoStream(context.Response.Output);
	                break;
                //case "feeds":
                //    new Feeds(context.Request, context.Session).TransformOntoStream(context.Response.Output);
                //    break;
	        }

	        context.Response.End();
	    }

	    public bool IsReusable
	    {
	        get { return false; }
	    }
	}
}