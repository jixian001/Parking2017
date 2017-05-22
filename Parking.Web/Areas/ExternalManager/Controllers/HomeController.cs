using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Data;
using Parking.Core;
using Parking.Auxiliary;

namespace Parking.Web.Areas.ExternalManager.Controllers
{
    public class HomeController : Controller
    {
        // GET: ExternalManager/Home/GetCurrentSound
        public ActionResult GetCurrentSound(int warehouse,int devicecode)
        {
            string sound = new CWTask().GetNotification(warehouse,devicecode);           
            return Content(sound);
        }
    }
}