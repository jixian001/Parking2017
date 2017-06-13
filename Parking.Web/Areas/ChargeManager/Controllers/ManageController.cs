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
            Log log = LogFactory.GetLogger("TempUserOutCar");
            #region
            try
            {
                string iccd = Request.Form["iccode"];
                string isplate = Request.Form["isplate"];
                string warehouse = Request.Form["wh"];
                string hallID = Request.Form["hallID"];
                string indate = Request.Form["indate"];
                string spantime = Request.Form["spantime"];
                string needfee = Request.Form["needfee"];
                string actualfee = Request.Form["actualfee"];
                string coinfee = Request.Form["coinfee"];

                CWLocation cwlctn = new CWLocation();
                Location loc = null;
                #region
                if (Convert.ToBoolean(isplate))
                {
                    //是车牌号
                    loc = cwlctn.FindLocation(l => l.PlateNum == iccd);
                }
                else
                {
                    ICCard icard = new CWICCard().Find(ic=>ic.UserCode==iccd);
                    if (icard == null)
                    {
                        resp.Message = "不是本系统用卡，ICCode - " + iccd;
                        return Json(resp);
                    }
                    if(icard.Status==EnmICCardStatus.Lost||
                        icard.Status == EnmICCardStatus.Disposed)
                    {
                        resp.Message = "卡已挂失或注销，ICCode - " + iccd;
                        return Json(resp);
                    }
                    //是卡号
                    loc = cwlctn.FindLocation(l => l.ICCode == iccd);
                }
                if (loc == null)
                {
                    resp.Message = "找不到取车位，proof - " + iccd;
                    return Json(resp);
                }
                #endregion
                int wh = Convert.ToInt16(warehouse);
                int hcode = Convert.ToInt32(hallID);
                Device hall = new CWDevice().Find(d => d.Warehouse == wh && d.DeviceCode == hcode);
                if (hall == null)
                {
                    resp.Message = "找不到出库车厅，wh - " + warehouse + " , code - " + hallID;
                    return Json(resp);
                }               
                resp = new CWTaskTransfer(hcode, wh).OCreateTempUserOfOutCar(loc);
                if (resp.Code == 1)
                {
                    //保存收费记录
                    TempUserChargeLog templog = new TempUserChargeLog
                    {
                        Proof = loc.ICCode,
                        Plate = loc.PlateNum,
                        Warehouse = wh,
                        Address = loc.Address,
                        InDate = loc.InDate.ToString(),
                        OutDate = DateTime.Now.ToString(),
                        SpanTime = spantime,
                        NeedFee = needfee,
                        ActualFee = actualfee,
                        CoinChange = coinfee,
                        OprtCode = "",
                        RecordDTime = DateTime.Now.ToString()
                    };
                    new CWTariff().AddTempLog(templog);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Message = "系统异常";
            }
            #endregion
            return Json(resp);
        }



    }
}