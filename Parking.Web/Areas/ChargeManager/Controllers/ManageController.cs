using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Parking.Web.Areas.ChargeManager.Controllers
{
    public class ManageController : Controller
    {
        // GET: ChargeManager/
        /// <summary>
        /// 临时卡缴费
        /// </summary>
        /// <returns></returns>
        public ActionResult TempCardCharge()
        {
            return View();
        }

        /// <summary>
        /// 定期卡、固定卡缴费
        /// </summary>
        /// <returns></returns>
        public ActionResult FixCardCharge()
        {
            return View();
        }

    }
}