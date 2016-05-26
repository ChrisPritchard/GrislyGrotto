using System;
using System.Web.Mvc;

namespace GrislyGrotto.Mvc
{
    public class XslViewEngine : VirtualPathProviderViewEngine
    {
        public XslViewEngine()
        {
            this.ViewLocationFormats = new[] 
            { 
                "/Views/{1}/{0}.xslt" 
            };
            PartialViewLocationFormats = ViewLocationFormats;
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return CreateView(controllerContext, partialPath, null);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            if (!(controllerContext.Controller.ViewData.Model is XViewData))
                throw new ArgumentException("the view data object should be of type XViewData");

            return new XslView(controllerContext.Controller.ViewData.Model as XViewData, viewPath);
        }
    }
}
