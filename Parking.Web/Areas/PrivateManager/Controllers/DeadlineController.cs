using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Parking.Web.Areas.PrivateManager.Controllers
{
    /// <summary>
    /// 控制停库操作
    /// </summary>
    public class DeadlineController : Controller
    {
        // GET: PrivateManager/Deadline
        /// <summary>
        /// 修改停库时间
        /// </summary>
        /// <returns></returns>       
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 修改记录的当前时间
        /// </summary>
        /// <returns></returns>
        public ActionResult Modify()
        {
            return View();
        }
    }
}