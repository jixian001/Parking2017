using System.Web.Mvc;

namespace Parking.Web.Areas.CustomManager
{
    public class CustomManagerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "CustomManager";
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