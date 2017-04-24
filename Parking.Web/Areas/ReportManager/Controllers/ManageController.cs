using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Parking.Web.Areas.ReportManager.Controllers
{
    public class ManageController : Controller
    {
        // GET: ReportManager/Manage
        public ActionResult Index()
        {
            return View();
        }


    }
}