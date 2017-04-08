using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;

namespace Parking.Web.Areas.SystemManager.Controllers
{
    public class FaultSummaryController : Controller
    {
        // GET: SystemManager/FaultSummary
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetDeviceList()
        {
            List<Device> devlst = new CWDevice().FindList(dev => true);
            return Json(devlst,JsonRequestBehavior.AllowGet);
        }

    }
}