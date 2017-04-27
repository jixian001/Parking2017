using System.Web;
using System.Web.Optimization;

namespace Parking.Web
{
    public class BundleConfig
    {
        // 有关绑定的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));
           
            // 使用要用于开发和学习的 Modernizr 的开发版本。然后，当你做好
            // 生产准备时，请使用 http://modernizr.com 上的生成工具来仅选择所需的测试。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                     "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/defsignalr").Include(
                     "~/Scripts/signalr.js"));

            bundles.Add(new ScriptBundle("~/bundles/plugin").Include(
                     "~/Scripts/bootstrap-dialog.js",
                     "~/Scripts/bootstrap-datetimepicker.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrapplugin").Include(
               "~/Content/bootstrap-dialog.css",
               "~/Content/bootstrap-datetimepicker.css"));

            bundles.Add(new StyleBundle("~/Content/bootstraptable").Include(
                "~/Content/bootstrap-table.css"));

            bundles.Add(new ScriptBundle("~/bundles/bootstraptable").Include(
                "~/Scripts/bootstrap-table.js",
                "~/Scripts/bootstrap-table-zh-CN.js"));

            bundles.Add(new StyleBundle("~/Content/defform").Include(
                "~/Content/defform.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrapselect").Include(
                "~/Content/bootstrap-select/css/bootstrap-select.css"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrapselect").Include(
                "~/Content/bootstrap-select/js/bootstrap-select.js",
                "~/Content/bootstrap-select/js/defaults-zh_CN.js"));

            bundles.Add(new StyleBundle("~/Content/loccharts").Include(
                "~/Content/jquery-seat-charts/jquery.seat-charts.css",
                "~/Content/jquery-seat-charts/Style.css"));

            bundles.Add(new ScriptBundle("~/bundles/loccharts").Include(
                "~/Content/jquery-seat-charts/jquery.seat-charts.js"));

            bundles.Add(new StyleBundle("~/Content/common").Include(
                "~/Content/common.css"));

            bundles.Add(new StyleBundle("~/Content/devform").Include(
               "~/Content/devform.css"));

            bundles.Add(new StyleBundle("~/Content/rocket").Include(
                "~/Content/rocket-to-top/css/rockettop.css"));

            bundles.Add(new ScriptBundle("~/bundles/rocket").Include(
               "~/Content/rocket-to-top/js/rockettop.js"));
        }
    }
}
