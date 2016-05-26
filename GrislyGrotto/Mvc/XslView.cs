using System;
using System.Web.Mvc;
using System.IO;
using System.Xml.Xsl;
using System.Xml;
using System.Web.Compilation;
using System.Xml.Linq;

namespace GrislyGrotto.Mvc
{
    public class XslView: IView
    {
        private string sXslPath;
        private XViewData xViewData;

        public XslView(XViewData viewData, string viewPath)
        {
            sXslPath = viewPath;
            xViewData = viewData;
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            XslCompiledTransform xslTransformer = new XslCompiledTransform();
            xslTransformer.Load(
                viewContext.HttpContext.Request.PhysicalApplicationPath + sXslPath);
            xslTransformer.Transform(ConvertToXmlDocument(xViewData), null, writer);
        }

        private XmlDocument ConvertToXmlDocument(XViewData xViewData)
        {
            XmlDocument xmlViewData = new XmlDocument();
            xmlViewData.LoadXml(xViewData.Content.ToString());
            return xmlViewData;
        }
    }
}
