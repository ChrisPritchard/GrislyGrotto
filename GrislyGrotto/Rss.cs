using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace GrislyGrotto
{
    public class Rss : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/rss+xml";

            var serializer = new XmlSerializer(typeof(Post[]));
            var xml = new StringBuilder();
            var textWriter = new StringWriter(xml);

            var data = new GrislyGrottoEntities();
            serializer.Serialize(textWriter, data.Posts.OrderByDescending(p => p.Created).Take(5).ToArray());

            var transform = new XslCompiledTransform();
            transform.Load(context.Server.MapPath("/resources/rss.xslt"));
            transform.Transform(new XmlDocument { InnerXml = xml.ToString() }, null, context.Response.OutputStream);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}