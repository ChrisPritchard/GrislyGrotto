using System.Xml.Linq;

namespace GrislyGrotto.Mvc
{
    public class XViewData
    {
        public XDocument Content { get; private set; }
        XElement xRootNode;

        public XViewData()
        {
            xRootNode = new XElement("ViewData");
            Content = new XDocument(xRootNode);
        }

        public void Add(params object[] content)
        {
            xRootNode.Add(content);
        }
    }
}
