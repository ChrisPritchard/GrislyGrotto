using System;
using System.Web.Mvc;
using System.Web.Routing;

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

        public new ViewResult View(string sViewName)
        {
            return base.View(sViewName, ViewData);
        }
    }
}
