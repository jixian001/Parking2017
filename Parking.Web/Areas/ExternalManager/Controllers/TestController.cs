using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Core;

namespace Parking.Web.Areas.ExternalManager.Controllers
{
    public class TestController : Controller
    {
        // GET: ExternalManager/Test
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AllocateTest(int warehouse,int hallCol,string checkcode)
        {
           
            return Content("test");
        }

    }
}