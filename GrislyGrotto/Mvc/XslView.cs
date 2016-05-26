using System.IO;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Xsl;

namespace GrislyGrotto.Mvc
{
    public class XslView: IView
    {
        private string xslPath;
        private XViewData ViewData;

        public XslView(XViewData viewData, string viewPath)
        {
            xslPath = viewPath;
            ViewData = viewData;
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var transformer = new XslCompiledTransform();
            transformer.Load(viewContext.HttpContext.Request.PhysicalApplicationPath + xslPath);
            transformer.Transform(ConvertToXmlDocument(ViewData), null, writer);
        }

        private XmlDocument ConvertToXmlDocument(XViewData viewData)
        {
            var document = new XmlDocument();
            document.LoadXml(viewData.Content.ToString());
            return document;
        }
    }
}
