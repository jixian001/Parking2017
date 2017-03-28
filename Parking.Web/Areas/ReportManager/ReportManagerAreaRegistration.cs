using System.Web.Mvc;

namespace Parking.Web.Areas.ReportManager
{
    public class ReportManagerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ReportManager";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {           
            context.MapRoute(
                this.AreaName + "_default",
                this.AreaName + "/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new string[] { "Parking.Web.Areas." + this.AreaName + ".Controllers" }
            );
        }
    }
}