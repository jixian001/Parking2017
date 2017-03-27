using System.Web.Mvc;

namespace Parking.Web.Areas.CustomManager
{
    public class CustomManagerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Custom";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Custom_default",
                "Custom/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new string[] { "Parking.Web.Areas.CustomManager.Controllers" }
            );
        }
    }
}