using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Web.Areas.SystemManager.Models;
using Parking.Auxiliary;
using Parking.Core;

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
            Response resp = new CWTaskTransfer(hallID, warehouse).ManualGetCar(warehouse, address);

            ReturnModel ret = new ReturnModel
            {
                code=resp.Code,
                message=resp.Message
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
            Response resp = new CWTask().TransportLocation(warehouse, fromaddress, toaddress);
            return Json(new ReturnModel() {code=resp.Code, message=resp.Message});
        }

        public ActionResult Move()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Move(int warehouse,int code,string address)
        {
            Response resp = new CWTask().ManualMove(warehouse, code, address);

            return Json(new ReturnModel() { code = resp.Code, message = resp.Message });
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
            Response resp = new CWTaskTransfer(hallID, warehouse).TempGetCar(iccode);
            return Json(new ReturnModel() { code=resp.Code,message=resp.Message});
        }
        /// <summary>
        /// 临时取物，查询
        /// </summary>
        /// <param name="iccode"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TempFind(string iccode)
        {
            int warehouse = 0;
            int hallID = 0;
            string address = "";
            Response resp = new CWTask().TempFindInfo(iccode, out warehouse, out hallID, out address);
            return Json(new ReturnModel() { code = resp.Code, message = resp.Message, warehouse = warehouse, hallID =hallID, locaddrs = address, iccode = iccode });
        }
    }
}