using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Parking.Web
{
    public class HandlerErrorAttribute:HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);

            filterContext.HttpContext.Response.Redirect("/Error.html");
        }
    }
}