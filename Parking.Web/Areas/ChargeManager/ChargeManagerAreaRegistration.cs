using System.Web.Mvc;

namespace Parking.Web.Areas.ChargeManager
{
    public class ChargeManagerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Charge";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Charge_default",
                "Charge/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new string[] { "Parking.Web.Areas.ChargeManager.Controllers" }
            );
        }
    }
}