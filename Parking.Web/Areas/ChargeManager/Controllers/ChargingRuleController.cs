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
    public class ChargingRuleController : Controller
    {

        // GET: ChargeManager/ChargingRule
        public ActionResult Index()
        {
            return View();
        }

        #region 预付类
        /// <summary>
        /// 预付类规则
        /// </summary>
        /// <returns></returns>
        public ActionResult PreRule()
        {
            return View();
        }

        [HttpPost]
        public JsonResult FindPreRuleList(int pageSize,int pageIndex, string sortOrder, string sortName)
        {
            Page<PreCharging> page = new CWTariff().FindPreRulePageList(pageSize, pageIndex, sortOrder, sortName);
            var data = new {
                total = page.TotalNumber,
                rows = page.ItemLists
            };
            return Json(data);
        }

        public JsonResult AddPre()
        {
            string cunit = Request.QueryString["cunit"];
            string cnum = Request.QueryString["cnum"];
            string cfee = Request.QueryString["cfee"];
           
            PreCharging prechg = new PreCharging {
                CycleUnit = (EnmCycleUnit)Convert.ToInt16(cunit),
                CycleNum = Convert.ToInt32(cnum),
                Fee=Convert.ToSingle(cfee)
            };
            Response resp = new CWTariff().AddPreCharge(prechg);
            return Json(resp,JsonRequestBehavior.AllowGet);
        }

        public JsonResult ModifyPre()
        {
            string ID = Request.QueryString["cID"];
            string cunit = Request.QueryString["cunit"];
            string cnum = Request.QueryString["cnum"];
            string cfee = Request.QueryString["cfee"];
            Response resp = new Response();
            if (!string.IsNullOrEmpty(ID))
            {
                PreCharging prechg = new CWTariff().FindPreCharge(int.Parse(ID));
                if (prechg != null)
                {
                    prechg.CycleUnit = (EnmCycleUnit)Convert.ToInt16(cunit);
                    prechg.CycleNum = Convert.ToInt32(cnum);
                    prechg.Fee = Convert.ToSingle(cfee);
                    resp = new CWTariff().UpdatePreCharge(prechg);
                }
            }        
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletePre(int ID)
        {
            CWTariff cwtariff = new CWTariff();
            //同时删除计费绑定的
            TempChargingRule temp = cwtariff.FindTempChgRule(tp => tp.PreChgID == ID);
            if (temp != null)
            {
                temp.PreChgID = 0;
                cwtariff.UpdateTempChgRule(temp);
            }
            FixChargingRule fix = cwtariff.FindFixCharge(fx => fx.PreChgID == ID);
            if (fix != null)
            {
                fix.PreChgID = 0;
                cwtariff.UpdateFixCharge(fix);
            }
            Response resp = cwtariff.DeletePreCharge(ID);

            return Json(resp, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 固定类
        /// <summary>
        /// 固定类规则
        /// </summary>
        /// <returns></returns>
        public ActionResult FixRule()
        {
            return View();
        }
        #endregion

        #region 临时类
        /// <summary>
        /// 临时类规则
        /// </summary>
        /// <returns></returns>
        public ActionResult TempRule()
        {
            return View();
        }

        /// <summary>
        /// 查找临时卡记录
        /// </summary>
        /// <returns></returns>
        public JsonResult GetTempRule()
        {
            Response resp = new Response();
            resp.Code = 0;
            TempChargingRule tempRule = new CWTariff().GetTempChgRuleList().FirstOrDefault();
            if (tempRule != null)
            {
                resp.Code = 1;
                resp.Message = "查询成功";
                resp.Data = tempRule;
            }
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增临时类记录,按次
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddTempRuleByOrder()
        {
            Response resp = new Response();
            CWTariff cwtrff = new CWTariff();
            List<TempChargingRule> ruleList = cwtrff.GetTempChgRuleList();
            if (ruleList.Count > 0)
            {
                resp.Code = 0;
                resp.Message = "系统故障，存在临时类记录，无法完成新增工作！";
                return Json(resp);
            }
            string preID = Request.Form["PreID"];
            string tType = Request.Form["TType"]; //计费类型
            string freetime = Request.Form["FreeTime"];
            string fee = Request.Form["OrderFee"];

            TempChargingRule rule = new TempChargingRule() {
                ICType = EnmICCardType.Temp,
                TempChgType = (EnmTempChargeType)Convert.ToInt16(tType),
                PreChgID=Convert.ToInt32(preID)
            };
            resp = cwtrff.AddTempChgRule(rule);
            if (resp.Code == 1)
            {
                //先删除原来的记录
                List<OrderChargeDetail> orderdetailLst = cwtrff.GetOrderDetailList();
                foreach(OrderChargeDetail order in orderdetailLst)
                {
                    cwtrff.DeleteOrderDetail(order.ID);
                }
                //添加新的
                OrderChargeDetail odetail = new OrderChargeDetail()
                {
                    TempChgID = rule.ID,
                    OrderFreeTime = freetime,
                    Fee = Convert.ToSingle(fee)
                };
                resp = cwtrff.AddOrderDetail(odetail);
            }
            return Json(resp);
        }

        /// <summary>
        /// 新增时间段
        /// </summary>
        [HttpPost]
        public ActionResult AddHourTempRule()
        {
            Response resp = new Response();




            return Json(resp);
        }

        [HttpPost]
        public JsonResult FindTempHourRuleList(int pageSize, int pageIndex)
        {
            Page<TempChargingRule> page = new CWTariff().FindPageTempRuleList(pageSize, pageIndex);
            var data = new {
                total = page.TotalNumber,
                rows = page.ItemLists
            };
            return Json(data);
        }

        /// <summary>
        /// 获取预付类清单
        /// </summary>
        /// <returns></returns>
        public JsonResult PreSelectNameList()
        {
            List<SelectItem> items = new List<SelectItem>();
            List<PreCharging> preLst = new CWTariff().FindPreChargeList(pr => true);
            foreach (PreCharging pre in preLst)
            {
                SelectItem it = new SelectItem();
                it.OptionValue = pre.ID.ToString();
                string cycle = "";
                switch (pre.CycleUnit)
                {
                    case EnmCycleUnit.Day:
                        cycle = "天";
                        break;
                    case EnmCycleUnit.Hour:
                        cycle = "小时";
                        break;
                    case EnmCycleUnit.Month:
                        cycle = "月";
                        break;
                    case EnmCycleUnit.Season:
                        cycle = "季度";
                        break;
                }
                it.OptionText =pre.CycleNum + " " + cycle + " " + pre.Fee + "元";

                items.Add(it);
            }
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取按时计费策略表
        /// </summary>
        /// <param name="tempID"></param>
        /// <returns></returns>
        public JsonResult FindHourDetail(int tempID)
        {
            CWTariff cwtariff = new CWTariff();
            HourChargeDetail policy = cwtariff.FindHourChgDetail(hr => hr.TempChgID == tempID);
            if (policy == null)
            {
                policy = cwtariff.FindHourChgDetailList().First();
                if (policy != null)
                {
                    policy.TempChgID = tempID;
                    cwtariff.UpdateHourChgDetail(policy);
                }
                else
                {
                    policy = new HourChargeDetail();
                    policy.TempChgID = tempID;
                    policy.StrideDay = EnmStrideDay.Continue;
                    policy.CycleTime = EnmCycleTime.Hour_24;
                    policy.CycleTopFee = 0;
                    cwtariff.AddHourChgDetail(policy);
                }
            }
            return Json(policy,JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取按次计费策略表
        /// </summary>
        public JsonResult FindOrderDetail(int tempID)
        {
            CWTariff cwtariff = new CWTariff();
            OrderChargeDetail orderDetail = cwtariff.FindOrderDetail(od => od.TempChgID == tempID);
            if (orderDetail == null)
            {
                orderDetail = new OrderChargeDetail
                {
                    TempChgID = tempID,
                    OrderFreeTime = "00:00",
                    Fee = 0
                };
            }
            return Json(orderDetail, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}