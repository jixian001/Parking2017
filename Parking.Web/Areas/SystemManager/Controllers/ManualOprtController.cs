using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Web.Areas.SystemManager.Models;
using Parking.Auxiliary;
using Parking.Data;
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

        public JsonResult GetOutHallName()
        {
            List<SelectItem> itemsLst = new List<SelectItem>();
            #region
            List<Device> hallsLst = new CWDevice().FindList(dv => dv.Type == EnmSMGType.Hall && 
                                                           (dv.HallType == EnmHallType.EnterOrExit || dv.HallType == EnmHallType.Exit));
            foreach(Device dev in hallsLst)
            {
                SelectItem item = new SelectItem
                {
                    OptionValue=dev.DeviceCode.ToString(),
                    OptionText=(dev.DeviceCode-10).ToString()+" #车厅"
                };
                itemsLst.Add(item);
            }
            #endregion
            return Json(itemsLst, JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// 获取移动设备清单
        /// </summary>
        /// <returns></returns>
        public JsonResult GetEtvsName()
        {
            List<SelectItem> itemsLst = new List<SelectItem>();
            #region
            List<Device> hallsLst = new CWDevice().FindList(dv => dv.Type == EnmSMGType.ETV);
            foreach (Device dev in hallsLst)
            {
                SelectItem item = new SelectItem
                {
                    OptionValue = dev.DeviceCode.ToString(),
                    OptionText =  dev.DeviceCode.ToString() + " #ETV"
                };
                itemsLst.Add(item);
            }
            #endregion
            return Json(itemsLst, JsonRequestBehavior.AllowGet);
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
        public ActionResult TempGet(string iccode,int hallID, bool isplate)
        {
           
            Response resp = new Response();
            //先查找车位，后获取库区号          
            Location lct = null;
            if (isplate)
            {
                lct = new CWLocation().FindLocation(l => l.PlateNum == iccode);
            }
            else
            {
                lct = new CWLocation().FindLocation(l => l.ICCode == iccode);
            }
            if (lct != null)
            {
                int warehouse = lct.Warehouse;
                resp = new CWTaskTransfer(hallID, warehouse).TempGetCar(lct.ICCode);
            }
            else
            {
                resp.Code = 0;
                resp.Message = "找不到取车车位";
            }
            return Json(new ReturnModel() { code = resp.Code, message = resp.Message });
        }
        /// <summary>
        /// 临时取物，查询
        /// </summary>
        /// <param name="iccode"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TempFind(string iccode,bool isplate)
        {
            int warehouse = 0;
            int hallID = 0;
            string address = "";
            Response resp = new Response();            
            resp = new CWTask().TempFindInfo(isplate, iccode, out warehouse, out hallID, out address);
            return Json(new ReturnModel() { code = resp.Code, message = resp.Message, warehouse = warehouse, hallID =hallID, locaddrs = address, iccode = iccode });
        }
    }
}