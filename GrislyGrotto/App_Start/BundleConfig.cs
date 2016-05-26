using System.Web.Optimization;

namespace GrislyGrotto
{
	public class BundleConfig
	{
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/grislygrotto").Include(
						"~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/knockout-{version}.js",
                        "~/Scripts/grislygrotto/background-animation.js",
                        "~/Scripts/grislygrotto/search.js",
                        "~/Scripts/grislygrotto/editor.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
					  "~/Content/normalize.css",
					  "~/Content/skeleton.css",
					  "~/Content/grislygrotto.css"));
		}
	}
}
