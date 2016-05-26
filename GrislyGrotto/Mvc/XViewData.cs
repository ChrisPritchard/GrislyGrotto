using System.Xml.Linq;

namespace GrislyGrotto.Mvc
{
    public class XViewData
    {
        public XDocument Content { get; private set; }
        private XElement rootNode;

        public XViewData()
        {
            rootNode = new XElement("ViewData");
            Content = new XDocument(rootNode);
        }

        public void Add(params object[] content)
        {
            rootNode.Add(content);
        }
    }
}
