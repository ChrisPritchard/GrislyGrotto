using System.Web.Mvc;

namespace GrislyGrotto.Mvc
{
    public class XController : Controller
    {
        protected new XViewData ViewData;

        public XController()
        {
            ViewData = new XViewData();
        }

        public new ViewResult View()
        {
            return base.View(ViewData);
        }

        public new ViewResult View(string viewName)
        {
            return base.View(viewName, ViewData);
        }
    }
}
