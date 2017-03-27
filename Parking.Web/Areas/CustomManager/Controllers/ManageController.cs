using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Parking.Web.Areas.CustomManager.Controllers
{
    public class ManageController : Controller
    {
        // GET: CustomManager/Manage
        public ActionResult Index()
        {
            return View();
        }
    }
}