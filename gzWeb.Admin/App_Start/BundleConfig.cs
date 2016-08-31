using System.Web;
using System.Web.Optimization;

namespace gzWeb.Admin
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/css/bootstrap")
                                .Include("~/Content/Styles/bootstrap/bootstrap.css", new CssRewriteUrlTransform())
                                .Include("~/Content/Styles/bootstrap/bootstrap-theme.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/css/fa")
                .Include("~/Content/Styles/font-awesome/font-awesome.min.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/css/app")
                                .Include("~/Content/Styles/_app/basic.css",
                                         "~/Content/Styles/_app/header.css",
                                         "~/Content/Styles/_app/admin.css"));

            bundles.Add(new ScriptBundle("~/js/jquery").Include(
                "~/Scripts/jquery/jquery-{version}.js"
            ));

            bundles.Add(new ScriptBundle("~/js/jqueryval").Include(
                "~/Scripts/jquery.validate/jquery.validate*"
            ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/js/modernizr").Include(
                "~/Scripts/modernizr-*"
            ));

            bundles.Add(new ScriptBundle("~/js/bootstrap").Include(
                "~/Scripts/bootstrap/bootstrap.js"
                , "~/Scripts/respond/respond.js"
            ));

        }
    }
}
