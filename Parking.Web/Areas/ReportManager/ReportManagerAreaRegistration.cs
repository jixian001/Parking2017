using System.Web.Mvc;

namespace Parking.Web.Areas.ReportManager
{
    public class ReportManagerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Report";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Report_default",
                "Report/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 new string[] { "Parking.Web.Areas.ReportManager.Controllers" }
            );
        }
    }
}