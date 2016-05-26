using System.Web.Optimization;

namespace GrislyGrotto
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/scripts/vendor").Include(
                "~/scripts/jquery-{version}.js", 
                "~/scripts/knockout-{version}.js"));

            bundles.Add(new ScriptBundle("~/app/editor").Include(
                "~/app/editor.js",
                "~/app/editorTags.js",
                "~/scripts/jquery.form.js"));

            bundles.Add(new StyleBundle("~/content/styles").Include("~/content/site.css"));
        }
    }
}