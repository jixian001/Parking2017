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

        /// <summary>
        /// 查找固定类
        /// </summary>       
        [HttpPost]
        public ActionResult FindFixRuleLst(int pageSize, int pageNumber)
        {
            Page<FixChargingRule> page = new CWTariff().FindPageFixRuleLst(pageSize, pageNumber);
            var data = new
            {
                total = page.TotalNumber,
                rows = page.ItemLists
            };
            return Json(data);

        }

        [HttpPost]
        public ActionResult AddFixRule()
        {
            Response resp = new Response();
            CWTariff cwtariff = new CWTariff();

            string cardtype = Request.Form["ccard"];
            if (string.IsNullOrEmpty(cardtype))
            {
                resp.Message = "传输错误，卡类型为空！";
                return Json(resp);
            }
            string cunit = Request.Form["cunit"];
            if (string.IsNullOrEmpty(cunit))
            {
                resp.Message = "传输错误，收费类型为空！";
                return Json(resp);
            }
            string fee = Request.Form["cfee"];

            int ctype = Convert.ToInt32(cardtype);
            int unit = Convert.ToInt32(cunit);

            FixChargingRule rule = cwtariff.FindFixCharge(f=>f.ICType==(EnmICCardType)ctype&&f.Unit==(EnmFeeUnit)unit);
            if (rule != null)
            {
                resp.Message = "已存在该记录，不允许重复添加！";
                return Json(resp);
            }
            rule = new FixChargingRule {
                ICType= (EnmICCardType)ctype,
                Unit= (EnmFeeUnit)unit,
                Fee=Convert.ToSingle(fee)
            };
            resp = cwtariff.AddFixRule(rule);

            return Json(resp);
        }

        [HttpPost]
        public ActionResult ModifyFixRule()
        {
            Response resp = new Response();
            CWTariff cwtariff = new CWTariff();

            string ID = Request.Form["cID"];
            if (string.IsNullOrEmpty(ID))
            {
                resp.Message = "传输错误，ID为空！";
                return Json(resp);
            }
            int mID = Convert.ToInt32(ID);
            FixChargingRule rule = cwtariff.FindFixCharge(mID);
            if (rule == null)
            {
                resp.Message = "找不到对应的记录，ID-"+ID;
                return Json(resp);
            }
            string cardtype = Request.Form["ccard"];
            if (string.IsNullOrEmpty(cardtype))
            {
                resp.Message = "传输错误，卡类型为空！";
                return Json(resp);
            }
            string cunit = Request.Form["cunit"];
            if (string.IsNullOrEmpty(cunit))
            {
                resp.Message = "传输错误，收费类型为空！";
                return Json(resp);
            }
            string fee = Request.Form["cfee"];

            int ctype = Convert.ToInt32(cardtype);
            int unit = Convert.ToInt32(cunit);

            FixChargingRule exstRule = cwtariff.FindFixCharge(f => f.ICType == (EnmICCardType)ctype && f.Unit == (EnmFeeUnit)unit);
            if (exstRule != null)
            {
                if (exstRule.ID != mID)
                {
                    resp.Message = "已存在该记录，不允许重复添加！";
                    return Json(resp);
                }
            }

            rule.ICType = (EnmICCardType)ctype;
            rule.Unit = (EnmFeeUnit)unit;
            rule.Fee = Convert.ToSingle(fee);
            resp = cwtariff.UpdateFixCharge(rule);
            return Json(resp);
        }

        /// <summary>
        /// 删除
        /// </summary>       
        public ActionResult DeleteFixRule(int ID)
        {
            Response resp = new CWTariff().DeleteFixRule(ID);

            return Json(resp, JsonRequestBehavior.AllowGet);
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
                foreach (OrderChargeDetail order in orderdetailLst)
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

                resp.Data = null;
                if (resp.Code == 1)
                {
                    var da = new
                    {
                        mainID = rule.ID,
                        orderID = odetail.ID
                    };
                    resp.Data = da;
                }
            }
            return Json(resp);
        }

        /// <summary>
        /// 新增临时类记录,按时
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddTempRuleByHour()
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
            TempChargingRule rule = new TempChargingRule()
            {
                ICType = EnmICCardType.Temp,
                TempChgType = (EnmTempChargeType)Convert.ToInt16(tType),
                PreChgID = Convert.ToInt32(preID)
            };
            resp = cwtrff.AddTempChgRule(rule);
            if (resp.Code == 1)
            {
                string strided = Request.Form["StrideDay"];
                string cyclet = Request.Form["CycleTime"];
                string topfee = Request.Form["StrideTopFee"];

                HourChargeDetail hour = new HourChargeDetail {
                    StrideDay = (EnmStrideDay)Convert.ToInt16(strided),
                    CycleTime = (EnmCycleTime)Convert.ToInt16(cyclet),
                    CycleTopFee = Convert.ToSingle(topfee),
                    TempChgID=rule.ID
                };
                resp = cwtrff.AddHourChgDetail(hour);

                resp.Data = null;
                if (resp.Code == 1)
                {
                    var da = new
                    {
                        mainID = rule.ID,
                        hourID = hour.ID
                    };
                    resp.Data = da;
                }
            }
            return Json(resp);
        }

        /// <summary>
        /// 修改按次临时收费规则
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModifyTempRuleOfOrder()
        {
            Response resp = new Response();
            #region
            CWTariff cwtarff = new CWTariff();
            int mainID = 0;
            string mID = Request.Form["MID"];
            if (!string.IsNullOrEmpty(mID))
            {
                TempChargingRule rule = cwtarff.FindTempChgRule(Convert.ToInt32(mID));
                if (rule != null)
                {
                    string preID = Request.Form["PreID"];
                    string tType = Request.Form["TType"];

                    rule.TempChgType = (EnmTempChargeType)Convert.ToInt16(tType);
                    rule.PreChgID = Convert.ToInt32(preID);

                    cwtarff.UpdateTempChgRule(rule);

                    mainID = rule.ID;
                }
            }

            string orderID = Request.Form["OrderID"];
            if (!string.IsNullOrEmpty(orderID))
            {
                string freetime = Request.Form["FreeTime"];
                string fee = Request.Form["OrderFee"];

                if (orderID == "0")
                {
                    //添加新的
                    if (mainID != 0)
                    {
                        OrderChargeDetail odetail = new OrderChargeDetail()
                        {
                            TempChgID = mainID,
                            OrderFreeTime = freetime,
                            Fee = Convert.ToSingle(fee)
                        };
                        resp = cwtarff.AddOrderDetail(odetail);
                    }
                }
                else
                {
                    OrderChargeDetail order = cwtarff.FindOrderDetail(Convert.ToInt32(orderID));
                    if (order != null)
                    {                       
                        order.OrderFreeTime = freetime;
                        order.Fee = Convert.ToSingle(fee);
                        cwtarff.UpdateOrderDetail(order);
                    }
                }              
            }
            resp.Message = "修改数据成功";
            #endregion
            return Json(resp);
        }

        /// <summary>
        /// 修改 按时 周期性策略
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModifyTempRuleOfHour()
        {
            Response resp = new Response();
            CWTariff cwtarff = new CWTariff();
            int mainID = 0;
            string mID = Request.Form["MID"];
            if (!string.IsNullOrEmpty(mID))
            {
                TempChargingRule rule = cwtarff.FindTempChgRule(Convert.ToInt32(mID));
                if (rule != null)
                {
                    string preID = Request.Form["PreID"];
                    string tType = Request.Form["TType"];

                    rule.TempChgType = (EnmTempChargeType)Convert.ToInt16(tType);
                    rule.PreChgID = Convert.ToInt32(preID);

                    cwtarff.UpdateTempChgRule(rule);

                    mainID = rule.ID;
                }
            }
            string hourID = Request.Form["hourID"];
            if (!string.IsNullOrEmpty(hourID))
            {
                string strided = Request.Form["StrideDay"];
                string cyclet = Request.Form["CycleTime"];
                string topfee= Request.Form["StrideTopFee"];
                if (hourID == "0")
                {
                    //新增
                    HourChargeDetail hour = new HourChargeDetail
                    {
                        StrideDay = (EnmStrideDay)Convert.ToInt16(strided),
                        CycleTime = (EnmCycleTime)Convert.ToInt16(cyclet),
                        CycleTopFee = Convert.ToSingle(topfee),
                        TempChgID = mainID
                    };
                    resp = cwtarff.AddHourChgDetail(hour);
                }
                else
                {
                    //修改
                    HourChargeDetail detail = cwtarff.FindHourChgDetail(Convert.ToInt32(hourID));
                    if (detail != null)
                    {
                        detail.StrideDay = (EnmStrideDay)Convert.ToInt16(strided);
                        detail.CycleTime = (EnmCycleTime)Convert.ToInt16(cyclet);
                        detail.CycleTopFee = Convert.ToSingle(topfee);

                        resp = cwtarff.UpdateHourChgDetail(detail);
                    }
                }

            }
            resp.Message = "修改数据成功";
            return Json(resp);
        }

        /// <summary>
        /// 新增时间段
        /// </summary>
        [HttpPost]
        public ActionResult AddHourSectionRule()
        {
            Response resp = new Response();
            CWTariff cwtarff = new CWTariff();

            string hourchgID = Request.Form["HourChgID"];
            if (string.IsNullOrEmpty(hourchgID))
            {
                resp.Message = "周期性计费策略ID为空，传输错误!";
                return Json(resp);
            }
            int hourID = Convert.ToInt32(hourchgID);
            HourChargeDetail temprule = cwtarff.FindHourChgDetail(hourID);
            if (temprule == null)
            {
                resp.Message = "传输错误,找不到相关的周期性计费，ID-" + hourchgID;
                return Json(resp);
            }

            //重点是时间段的判断
            string start = Request.Form["StartTime"];
            string end = Request.Form["EndTime"];

            DateTime st_dtime = DateTime.Parse("2017-1-1 " + start + ":00");
            DateTime end_dtime = DateTime.Parse("2017-1-1 " + end + ":00").AddSeconds(-1);
            if (DateTime.Compare(st_dtime, end_dtime) > 0)
            {
                end_dtime = end_dtime.AddDays(1);
            }

            List<HourSectionInfo> timeSlotLst = cwtarff.FindHourSectionList(hr => true);
            foreach(HourSectionInfo section in timeSlotLst)
            {
                DateTime sttime = section.StartTime;
                DateTime endtime = section.EndTime.AddSeconds(-1);
                #region
                if (DateTime.Compare(sttime, st_dtime) < 0 && DateTime.Compare(endtime, st_dtime) > 0)
                {
                    resp.Message = "当前时段设置错误，原来-" + sttime.ToString() + "，现-" + st_dtime.ToString();
                    return Json(resp);
                }

                if (DateTime.Compare(sttime, end_dtime) < 0&& DateTime.Compare(endtime, end_dtime) >0)
                {
                    resp.Message = "当前时段设置错误，原来-" + sttime.ToString() + "，现-" + end_dtime.ToString();
                    return Json(resp);
                }
                if(DateTime.Compare(st_dtime, sttime) < 0 && DateTime.Compare(end_dtime, sttime) > 0)
                {
                    resp.Message = "当前时段设置错误，现-" + st_dtime.ToString()+ ",原来 - " + sttime.ToString();
                    return Json(resp);
                }
                if (DateTime.Compare(st_dtime, endtime) < 0 && DateTime.Compare(end_dtime, endtime) > 0)
                {
                    resp.Message = "当前时段设置错误，现-" + st_dtime.ToString() + ",原来 - " + endtime.ToString();
                    return Json(resp);
                }
                #endregion

                if(DateTime.Compare(endtime,DateTime.Parse("2017-1-1 23:59:59")) > 0)
                {
                    DateTime newstart = DateTime.Parse("2017-1-1");
                    DateTime newend = endtime.AddDays(-1);

                    if (DateTime.Compare(newstart, st_dtime) < 0 && DateTime.Compare(newend, st_dtime) > 0)
                    {
                        resp.Message = "当前时段设置错误，现-" + st_dtime.ToString() + ",原来 New- " + newstart.ToString();
                        return Json(resp);
                    }

                    if (DateTime.Compare(newstart, end_dtime) < 0 && DateTime.Compare(newend, end_dtime) > 0)
                    {
                        resp.Message = "当前时段设置错误，现-" + end_dtime.ToString() + ",原来 New- " + newstart.ToString();
                        return Json(resp);
                    }                   
                }
            }

            if (DateTime.Compare(end_dtime, DateTime.Parse("2017-1-1 23:59:59")) > 0)
            {
                DateTime newend= DateTime.Parse("2017-1-1 " + end + ":00").AddSeconds(-1);
                foreach (HourSectionInfo section in timeSlotLst)
                {
                    DateTime sttime = section.StartTime;
                    DateTime endtime = section.EndTime.AddSeconds(-1);

                    if (DateTime.Compare(sttime, newend) < 0 && DateTime.Compare(endtime, newend) > 0)
                    {
                        resp.Message = "当前时段设置错误，原来-" + sttime.ToString() + "，现-" + newend.ToString();
                        return Json(resp);
                    }
                }
            }

            string topfee = Request.Form["SectionTopFee"];
            string freetime = Request.Form["SectionFreeTime"];
            string firstvoid = Request.Form["FirstVoidTime"];
            string firstfee = Request.Form["FirstVoidFee"];
            string intervalvoid = Request.Form["IntervalVoidTime"];
            string intervalfee = Request.Form["IntervalVoidFee"];

            HourSectionInfo hoursection = new HourSectionInfo() {
                HourChgID=hourID,
                StartTime=st_dtime,
                EndTime=end_dtime.AddSeconds(1),
                SectionTopFee=Convert.ToSingle(topfee),
                SectionFreeTime=freetime,
                FirstVoidTime=firstvoid,
                FirstVoidFee=Convert.ToSingle(firstfee),
                IntervalVoidTime=intervalvoid,
                IntervalVoidFee=Convert.ToSingle(intervalfee)
            };
            resp = cwtarff.AddHourSection(hoursection);

            return Json(resp);
        }

        /// <summary>
        /// 修改时间段
        /// </summary>
        [HttpPost]
        public ActionResult ModifyHourSectionRule()
        {
            Response resp = new Response();
            CWTariff cwtarff = new CWTariff();
            string hID = Request.Form["HourID"];
            if (string.IsNullOrEmpty(hID))
            {
                resp.Message = "传输故障，ID为空";
                return Json(resp);
            }
            int hourID = Convert.ToInt32(hID);
            HourSectionInfo hoursection = cwtarff.FindHourSection(hourID);
            if (hoursection == null)
            {
                resp.Message = "传输故障，找不到对应时间段，ID-"+hID;
                return Json(resp);
            }
            //如果修改时间区间，则要判断
            string start = Request.Form["StartTime"];
            string end = Request.Form["EndTime"];

            DateTime st_dtime = DateTime.Parse("2017-1-1 " + start + ":00");
            DateTime end_dtime = DateTime.Parse("2017-1-1 " + end + ":00").AddSeconds(-1);
            if (DateTime.Compare(st_dtime, end_dtime) > 0)
            {
                end_dtime = end_dtime.AddDays(1);
            }

            List<HourSectionInfo> timeSlotLst = cwtarff.FindHourSectionList(hr => true);
            foreach (HourSectionInfo section in timeSlotLst)
            {
                if (section.ID == hourID)
                {
                    continue;
                }
                DateTime sttime = section.StartTime;
                DateTime endtime = section.EndTime.AddSeconds(-1);
                #region
                if (DateTime.Compare(sttime, st_dtime) < 0 && DateTime.Compare(endtime, st_dtime) > 0)
                {
                    resp.Message = "当前时段设置错误，原来-" + sttime.ToString() + "，现-" + st_dtime.ToString();
                    return Json(resp);
                }

                if (DateTime.Compare(sttime, end_dtime) < 0 && DateTime.Compare(endtime, end_dtime) > 0)
                {
                    resp.Message = "当前时段设置错误，原来-" + sttime.ToString() + "，现-" + end_dtime.ToString();
                    return Json(resp);
                }
                if (DateTime.Compare(st_dtime, sttime) < 0 && DateTime.Compare(end_dtime, sttime) > 0)
                {
                    resp.Message = "当前时段设置错误，现-" + st_dtime.ToString() + ",原来 - " + sttime.ToString();
                    return Json(resp);
                }
                if (DateTime.Compare(st_dtime, endtime) < 0 && DateTime.Compare(end_dtime, endtime) > 0)
                {
                    resp.Message = "当前时段设置错误，现-" + st_dtime.ToString() + ",原来 - " + endtime.ToString();
                    return Json(resp);
                }
                #endregion

                if (DateTime.Compare(endtime, DateTime.Parse("2017-1-1 23:59:59")) > 0)
                {
                    DateTime newstart = DateTime.Parse("2017-1-1");
                    DateTime newend = endtime.AddDays(-1);

                    if (DateTime.Compare(newstart, st_dtime) < 0 && DateTime.Compare(newend, st_dtime) > 0)
                    {
                        resp.Message = "当前时段设置错误，现-" + st_dtime.ToString() + ",原来 New- " + newstart.ToString();
                        return Json(resp);
                    }

                    if (DateTime.Compare(newstart, end_dtime) < 0 && DateTime.Compare(newend, end_dtime) > 0)
                    {
                        resp.Message = "当前时段设置错误，现-" + end_dtime.ToString() + ",原来 New- " + newstart.ToString();
                        return Json(resp);
                    }                   
                }
            }

            if (DateTime.Compare(end_dtime, DateTime.Parse("2017-1-1 23:59:59")) > 0)
            {
                DateTime newend = DateTime.Parse("2017-1-1 " + end + ":00").AddSeconds(-1);
                foreach (HourSectionInfo section in timeSlotLst)
                {
                    if (section.ID == hourID)
                    {
                        continue;
                    }
                    DateTime sttime = section.StartTime;
                    DateTime endtime = section.EndTime.AddSeconds(-1);

                    if (DateTime.Compare(sttime, newend) < 0 && DateTime.Compare(endtime, newend) > 0)
                    {
                        resp.Message = "当前时段设置错误，原来-" + sttime.ToString() + "，现-" + newend.ToString();
                        return Json(resp);
                    }
                }
            }

            string topfee = Request.Form["SectionTopFee"];
            string freetime = Request.Form["SectionFreeTime"];
            string firstvoid = Request.Form["FirstVoidTime"];
            string firstfee = Request.Form["FirstVoidFee"];
            string intervalvoid = Request.Form["IntervalVoidTime"];
            string intervalfee = Request.Form["IntervalVoidFee"];

            hoursection.StartTime = st_dtime;
            hoursection.EndTime = end_dtime.AddSeconds(1);
            hoursection.SectionTopFee = Convert.ToSingle(topfee);
            hoursection.SectionFreeTime = freetime;
            hoursection.FirstVoidTime = firstvoid;
            hoursection.FirstVoidFee = Convert.ToSingle(firstfee);
            hoursection.IntervalVoidTime = intervalvoid;
            hoursection.IntervalVoidFee = Convert.ToSingle(intervalfee);
            resp = cwtarff.UpdateHourSection(hoursection);

            return Json(resp);
        }

        /// <summary>
        /// 分页查找
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult FindHourSectionList(int pageSize, int pageNumber)
        {
            Page<HourSectionInfo> page = new CWTariff().FindPageHourRuleList(pageSize, pageNumber);
            var data = new {
                total = page.TotalNumber,
                rows = page.ItemLists
            };
            return Json(data);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="hourID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteHourSection(int hourID)
        {
            Response resp = new CWTariff().DeleteHourSection(hourID);
            return Json(resp);
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
                List<HourChargeDetail> hourList = cwtariff.FindHourChgDetailList();
                if (hourList.Count > 0)
                {
                    policy = hourList[0];
                    if (policy != null)
                    {
                        policy.TempChgID = tempID;
                        cwtariff.UpdateHourChgDetail(policy);
                    }
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
                cwtariff.AddOrderDetail(orderDetail);
            }
            return Json(orderDetail, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}