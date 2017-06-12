using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using Parking.Web.Areas.ChargeManager.Models;

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

        /// <summary>
        /// 填充出车厅
        /// </summary>
        public JsonResult GetOutHallName()
        {
            List<SelectItem> itemsLst = new List<SelectItem>();
            #region
            List<Device> hallsLst = new CWDevice().FindList(dv => dv.Type == EnmSMGType.Hall &&
                                                           (dv.HallType == EnmHallType.EnterOrExit || dv.HallType == EnmHallType.Exit));
            foreach (Device dev in hallsLst)
            {
                SelectItem item = new SelectItem
                {
                    OptionValue = dev.DeviceCode.ToString(),
                    OptionText = (dev.DeviceCode - 10).ToString() + " #车厅"
                };
                itemsLst.Add(item);
            }
            #endregion
            return Json(itemsLst, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 临时用户查询停车费用
        /// </summary>
        /// <param name="iccode">卡号或车牌号</param>
        /// <param name="isPlate"></param>
        /// <returns></returns>
        public JsonResult TempUserFeeInfo(string iccode,bool isPlate)
        {           
            Response resp = new CWTariff().GetTempUserInfo(iccode, isPlate);            
            return Json(resp,JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 临时用户缴费出车
        /// </summary>
        [HttpPost]        
        public JsonResult TempUserOutCar()
        {
            Response resp = new Response();
            #region


            #endregion
            return Json(resp);
        }



    }
}