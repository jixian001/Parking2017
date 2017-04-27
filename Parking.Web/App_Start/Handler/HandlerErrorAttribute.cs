using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;

namespace Parking.Web
{
    public class HandlerErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);

            Log log = LogFactory.GetLogger(filterContext.Exception.Source);
            log.Error(filterContext.Exception.Message);

            //if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            //{
            //    return;
            //}
            filterContext.HttpContext.Response.Redirect("Error");

            //if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            //{
            //    return;
            //}

            //if (!ExceptionType.IsInstanceOfType(filterContext.Exception))
            //{
            //    return;
            //}
            //if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            //{
            //    filterContext.Result = new JsonResult
            //    {
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            //        Data = new
            //        {
            //            error = true,
            //            message = filterContext.Exception.Message
            //        }
            //    };
            //}
            //else
            //{
            //    var controllerName = (string)filterContext.RouteData.Values["controller"];
            //    var actionName = (string)filterContext.RouteData.Values["action"];
            //    var model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
            //    filterContext.Result = new ViewResult
            //    {
            //        ViewName = View,
            //        MasterName = Master,
            //        ViewData = new ViewDataDictionary(model),
            //        TempData = filterContext.Controller.TempData
            //    };
            //}
                     
            //filterContext.ExceptionHandled = true;
            //filterContext.HttpContext.Response.Clear();
            //filterContext.HttpContext.Response.StatusCode = 500;
            //filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;   
                    
        }
    }
}