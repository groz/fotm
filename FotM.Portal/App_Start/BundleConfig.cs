using System.Web;
using System.Web.Optimization;

namespace FotM.Portal
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/my").Include("~/Scripts/myUtils.js"));

#if DEBUG
            bundles.Add(new ScriptBundle("~/bundles/activeView").Include(
                "~/Scripts/knockout-3.1.0.debug.js",
                "~/Scripts/jquery.signalR-2.0.2.js"));
#else
            bundles.Add(new ScriptBundle("~/bundles/activeView").Include(
                "~/Scripts/knockout-3.1.0.js",
                "~/Scripts/jquery.signalR-2.0.2.min.js"));
#endif

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
