using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using Parking.Web.Areas.ChargeManager.Models;

namespace Parking.Web.Areas.ChargeManager.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {

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
        public JsonResult TempUserFeeInfo(string iccode, bool isPlate)
        {
            Response resp = new CWTariff().GetTempUserInfo(iccode, isPlate);
            return Json(resp, JsonRequestBehavior.AllowGet);
        }
       
        /// <summary>
        /// 临时用户缴费出车
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> TempUserOutCar()
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
                    loc =await cwlctn.FindLocationAsync(l => l.PlateNum == iccd);
                }
                else
                {
                    #region
                    //ICCard icard = new CWICCard().Find(ic=>ic.UserCode==iccd);
                    //if (icard == null)
                    //{
                    //    resp.Message = "不是本系统用卡，ICCode - " + iccd;
                    //    return Json(resp);
                    //}
                    //if(icard.Status==EnmICCardStatus.Lost||
                    //    icard.Status == EnmICCardStatus.Disposed)
                    //{
                    //    resp.Message = "卡已挂失或注销，ICCode - " + iccd;
                    //    return Json(resp);
                    //}
                    #endregion
                    //是卡号
                    loc =await cwlctn.FindLocationAsync(l => l.ICCode == iccd);
                }
                if (loc == null)
                {
                    resp.Message = "找不到取车位，proof - " + iccd;
                    return Json(resp);
                }
                #endregion
                int wh = Convert.ToInt16(warehouse);
                int hcode = Convert.ToInt32(hallID);
                Device hall =await new CWDevice().FindAsync(d => d.Warehouse == wh && d.DeviceCode == hcode);
                if (hall == null)
                {
                    resp.Message = "找不到出库车厅，wh - " + warehouse + " , code - " + hallID;
                    return Json(resp);
                }
                resp = new CWTaskTransfer(hcode, wh).OCreateTempUserOfOutCar(loc);
                if (resp.Code == 1)
                {
                    string oprt = User.Identity.Name;
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
                        OprtCode = oprt,
                        RecordDTime = DateTime.Now
                    };
                    await new CWTariffLog().AddTempLogAsync(templog);
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

        public ActionResult QueryFixTariffFee(int utype, int feeunit)
        {
            Response resp = new Response();
            if (utype > 3 || feeunit > 3)
            {
                resp.Message = "系统异常, Unit- " + feeunit + " ,ictype- " + utype;
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            FixChargingRule rule = new CWTariff().FindFixCharge(fix => fix.Unit == (EnmFeeUnit)feeunit && fix.ICType == (EnmICCardType)utype);
            if (rule == null)
            {
                resp.Message = "找不到对应的收费规则记录, Unit- " + feeunit;
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            resp.Code = 1;
            resp.Message = "查询成功";
            resp.Data = rule.Fee;
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="uiccd"></param>
        /// <returns></returns>
        public async Task<JsonResult> QueryCustInfo(int type, string uiccd)
        {
            Response resp = new Response();
            #region
            CWICCard cwiccd = new CWICCard();
            Customer cust = null;
            if (type == 1)
            {
                //是卡号
                ICCard iccd =await cwiccd.FindAsync(ic => ic.UserCode == uiccd);
                if (iccd == null)
                {
                    resp.Message = "不是本系统用卡，iccode - " + uiccd;
                    return Json(resp, JsonRequestBehavior.AllowGet);
                }
                if (iccd.CustID == 0)
                {
                    resp.Message = "当前用卡为临时用卡，无法完成操作！ ICCode - " + uiccd;
                    return Json(resp, JsonRequestBehavior.AllowGet);
                }
                cust = cwiccd.FindCust(iccd.CustID);
            }
            else if (type == 2)
            {
                //是车牌
                cust =await cwiccd.FindCustAsync(cc => cc.PlateNum == uiccd);
            }
            else if (type == 3)
            {
                //是车主姓名
                cust = await cwiccd.FindCustAsync(cc => cc.UserName == uiccd);
            }
            if (cust == null)
            {
                resp.Message = "当前用户不存在，无法进行操作！iccode - " + uiccd;
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            if (cust.Type == EnmICCardType.Temp)
            {
                resp.Message = "临时用户，不在此界面缴费！ iccode - " + uiccd;
                return Json(resp, JsonRequestBehavior.AllowGet);
            }

            FixChargingRule rule = new CWTariff().FindFixCharge(fix => fix.Unit == EnmFeeUnit.Month && fix.ICType == cust.Type);
            if (rule == null)
            {
                resp.Message = "找不到(月份)收费规则，ICType - " + cust.Type.ToString();
                return Json(resp, JsonRequestBehavior.AllowGet);
            }

            FixCustInfo info = new FixCustInfo
            {
                CustID = cust.ID,
                Proof = uiccd,
                ICType = (int)cust.Type,
                CurrDeadline = cust.Deadline.ToString(),
                MonthFee = rule.Fee
            };
            resp.Code = 1;
            resp.Message = "查询成功";
            resp.Data = info;

            TempData["CustInfo"] = info;
            #endregion
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 固定用户缴费
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SetFixUserFee()
        {
            Log log = LogFactory.GetLogger("SetFixUserFee");
            Response resp = new Response();
            #region
            CWICCard cwiccd = new CWICCard();
            try
            {
                string proof = Request.Form["uiccd"];
                //计费类型
                string feetype = Request.Form["feetype"];
                //费用标准
                string feeStd = Request.Form["feeunit"];
                //实收费用
                string actualfee = Request.Form["actualfee"];

                if (TempData["CustInfo"] == null)
                {
                    resp.Message = "请先点击< 查询信息 >，再 确认缴费 ！";
                    return Json(resp);
                }

                FixCustInfo oldInfo = (FixCustInfo)TempData["CustInfo"];
                if (proof.Trim() != oldInfo.Proof.Trim())
                {
                    resp.Message = "查询条件已改变，请先点击< 查询信息 >，再 确认缴费 ！";
                    return Json(resp);
                }
                Customer cust =await cwiccd.FindCustAsync(oldInfo.CustID);

                float standard = Convert.ToSingle(feeStd);
                float actual = Convert.ToSingle(actualfee);
                int rnd = (int)(actual / standard);

                EnmFeeUnit unit = (EnmFeeUnit)Convert.ToInt16(feetype);
                int months = 0;
                switch (unit)
                {
                    case EnmFeeUnit.Month:
                        months = 1 * rnd;
                        break;
                    case EnmFeeUnit.Season:
                        months = 3 * rnd;
                        break;
                    case EnmFeeUnit.Year:
                        months = 12 * rnd;
                        break;
                    default:
                        break;
                }
                if (months == 0)
                {
                    resp.Message = "系统异常，rnd- " + rnd + " , Unit- " + unit.ToString();
                    return Json(resp);
                }
                DateTime current = cust.Deadline;
                if (current == DateTime.Parse("2017-1-1"))
                {
                    current = DateTime.Now;
                }
                cust.StartDTime = DateTime.Now;
                cust.Deadline = current.AddMonths(months);
                //更新期限
                resp = cwiccd.UpdateCust(cust);

                oldInfo.LastDeadline = oldInfo.CurrDeadline;
                oldInfo.CurrDeadline = cust.Deadline.ToString();

                if (resp.Code == 1)
                {
                    #region 记录日志
                    string uty = "";
                    switch (cust.Type)
                    {
                        case EnmICCardType.Periodical:
                            uty = "定期用户";
                            break;
                        case EnmICCardType.FixedLocation:
                            uty = "固定用户";
                            break;
                        default:
                            uty = cust.Type.ToString();
                            break;
                    }
                    string umsg = "";
                    switch (unit)
                    {
                        case EnmFeeUnit.Month:
                            umsg = "月";
                            break;
                        case EnmFeeUnit.Season:
                            umsg = "季";
                            break;
                        case EnmFeeUnit.Year:
                            umsg = "年";
                            break;
                        default:
                            break;
                    }
                    string oprt = User.Identity.Name;
                    FixUserChargeLog fixlog = new FixUserChargeLog
                    {
                        UserName = cust.UserName,
                        PlateNum = cust.PlateNum,
                        UserType = uty,
                        Proof = oldInfo.Proof,
                        LastDeadline = oldInfo.LastDeadline,
                        CurrDeadline = oldInfo.CurrDeadline,
                        FeeType = umsg,
                        FeeUnit = standard,
                        CurrFee = actual,
                        OprtCode=oprt,
                        RecordDTime=DateTime.Now
                    };

                    await new CWTariffLog().AddFixLogAsync(fixlog);

                    #endregion

                    resp.Message = "缴费成功！";
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            #endregion
            return Json(resp);
        }

        /// <summary>
        /// 固定用户收费界面出车
        /// </summary>
        /// <returns></returns>        
        public ActionResult FixGUIOutCar(int type, string uiccd, int warehouse, int hallID)
        {
            Response resp = new Response();
            #region
            Log log = LogFactory.GetLogger("FixGUIOutCar");
            try
            {
                CWICCard cwiccd = new CWICCard();
                CWLocation cwlctn = new CWLocation();
                Customer cust = null;
                #region
                Location loc = null;
                if (type == 1)
                {
                    #region
                    ////是卡号
                    //ICCard iccd = cwiccd.Find(ic => ic.UserCode == uiccd);
                    //if (iccd == null)
                    //{
                    //    resp.Message = "不是本系统用卡，iccode - " + uiccd;
                    //    return Json(resp, JsonRequestBehavior.AllowGet);
                    //}
                    //if (iccd.CustID == 0)
                    //{
                    //    resp.Message = "当前用卡为临时用卡，无法完成操作！ ICCode - " + uiccd;
                    //    return Json(resp, JsonRequestBehavior.AllowGet);
                    //}
                    //cust = cwiccd.FindCust(iccd.CustID);
                    #endregion
                    loc = cwlctn.FindLocation(l => l.ICCode == uiccd);
                }
                else if (type == 2)
                {                    
                    loc = cwlctn.FindLocation(l => l.PlateNum == uiccd);
                }
                if (loc == null)
                {
                    resp.Message = "当前用户没有存车！ Proof - " + uiccd;
                    return Json(resp, JsonRequestBehavior.AllowGet);
                }
                int sno = Convert.ToInt32(loc.ICCode);
                SaveCertificate scert = new CWSaveProof().Find(s => s.SNO == sno);
                if (scert != null)
                {
                    cust = new CWICCard().FindCust(scert.CustID);                   
                }
                if (type == 3)
                {
                    //是车主姓名
                    cust = cwiccd.FindCust(cc => cc.UserName == uiccd);
                }
                if (cust == null)
                {
                    resp.Message = "不是注册用户，无法进行操作！iccode - " + uiccd;
                    return Json(resp, JsonRequestBehavior.AllowGet);
                }
                if (cust.Type == EnmICCardType.Temp)
                {
                    resp.Message = "临时用户，不在此界面缴费！ iccode - " + uiccd;
                    return Json(resp, JsonRequestBehavior.AllowGet);
                }
                if (DateTime.Compare(DateTime.Now, cust.Deadline) > 0)
                {
                    resp.Message = "当前用户已欠费，请缴费后出车！ iccode - " + uiccd + " ,Deadline- " + cust.Deadline.ToString();
                    return Json(resp, JsonRequestBehavior.AllowGet);
                }
                ////如果是以车牌或用户名取车，
                //if (type > 1)
                //{
                //    loc = cwlctn.FindLocation(lc => lc.PlateNum == cust.PlateNum);
                //    //以车牌找不到存车车辆，则以卡号进行查询
                //    if (loc == null)
                //    {
                //        //以绑定的卡号查询
                //        ICCard iccd = cwiccd.Find(ic => ic.CustID == cust.ID);
                //        if (iccd != null)
                //        {
                //            loc = cwlctn.FindLocation(l => l.ICCode == iccd.UserCode);
                //        }
                //    }
                //}               
                #endregion
                resp = new CWTaskTransfer(hallID, warehouse).FixGUIGetCar(loc);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            #endregion
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

    }
}