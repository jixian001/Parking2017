using System.Web.Mvc;

namespace Parking.Web.Areas.ChargeManager
{
    public class ChargeManagerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ChargeManager";
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