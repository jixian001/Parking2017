using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Web.Areas.SystemManager.Models;

namespace Parking.Web.Areas.SystemManager.Controllers
{
    /// <summary>
    /// 手动指令
    /// </summary>
    public class ManualOprtController : Controller
    {
        // GET: SystemManager/ManualOprt
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetCar(int warehouse,string address,int hallID)
        {
            ReturnModel ret = new ReturnModel {
                code=1,
                message="已经将你加入取车队列，请稍后！"
            };
            return Json(ret);
        }

        public ActionResult Transport()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Transport(int warehouse,string fromaddress,string toaddress)
        {
            return Json(new ReturnModel() { message="挪移成功"});
        }

        public ActionResult Move()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Move(int warehouse,int code,string address)
        {
            return Json(new ReturnModel() { message = "操作成功" });
        }

        public ActionResult TempGet()
        {
            return View();
        }

        /// <summary>
        /// 确认取物
        /// </summary>
        /// <param name="iccode"></param>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TempGet(string iccode,int warehouse,int hallID)
        {
            return Json(new ReturnModel() { code=1,message="操作成功"});
        }
        /// <summary>
        /// 临时取物，查询
        /// </summary>
        /// <param name="iccode"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TempFind(string iccode)
        {
            return Json(new ReturnModel() { code = 1, message = "操作成功", warehouse = 1, hallID = 11, locaddrs = "10104", iccode = iccode });
        }
    }
}