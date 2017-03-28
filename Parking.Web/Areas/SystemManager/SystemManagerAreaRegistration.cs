using System.Web.Mvc;

namespace Parking.Web.Areas.SystemManager
{
    public class SystemManagerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SystemManager";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                this.AreaName+"_default",
                this.AreaName+"/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new string[] { "Parking.Web.Areas."+this.AreaName+".Controllers" }
            );
        }
    }
}