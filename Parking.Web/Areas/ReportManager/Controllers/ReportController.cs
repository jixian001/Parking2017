using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using Parking.Web.Areas.ReportManager.Models;

namespace Parking.Web.Areas.ReportManager.Controllers
{
    [HandleError]
    public class ReportController : Controller
    {
        private Log log;

        public ReportController()
        {
            log = LogFactory.GetLogger("ReportController");
        }

        // GET: ReportManager/Report
        public ActionResult TelegramReport()
        {
            return View();
        }

        public JsonResult GetQueryNameForTelegram()
        {
            List<SelectItem> items = new List<SelectItem>();
            #region
            items.Add(new SelectItem { OptionValue = "Warehouse", OptionText = "库区" });
            items.Add(new SelectItem { OptionValue = "DeviceCode", OptionText = "设备号" });
            items.Add(new SelectItem { OptionValue = "ICCode", OptionText = "用户卡号" });
            items.Add(new SelectItem { OptionValue = "CarInfo", OptionText = "车辆信息" });
            items.Add(new SelectItem { OptionValue = "Telegram", OptionText = "报文信息" });
            items.Add(new SelectItem { OptionValue = "Address", OptionText = "车位地址" });
            items.Add(new SelectItem { OptionValue = "SaveNum", OptionText = "存车次数" });
            items.Add(new SelectItem { OptionValue = "GetNum", OptionText = "取车次数" });
            #endregion
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 报文查询
        /// </summary>
        /// <returns></returns>
        public ActionResult FindTelegramList()
        {
            #region
            string pageSize = Request.QueryString["pageSize"];
            string pageIndex = Request.QueryString["pageIndex"];
            string startdtime = Request.QueryString["stdtime"];
            string enddtime = Request.QueryString["enddtime"];
            string queryName = Request.QueryString["queryName"];
            string queryValue = Request.QueryString["queryValue"];

            int psize = Convert.ToInt32(pageSize);
            int pIndex = Convert.ToInt32(pageIndex);
            DateTime start = DateTime.Parse("2017-1-1");
            DateTime end = DateTime.Parse("2017-1-1");
            if (!string.IsNullOrEmpty(startdtime))
            {
                start = DateTime.Parse(startdtime);
            }
            if (!string.IsNullOrEmpty(enddtime))
            {
                end = DateTime.Parse(enddtime);
            }
            #endregion
            int totalNum = 0;
            List<TelegramLog> telegramLst = new CWTelegramLog().FindPageList(psize, pIndex, start, end, queryName, queryValue, out totalNum);
            var value = new
            {
                total = totalNum,
                rows = telegramLst
            };
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetQueryNameForOprt()
        {
            List<SelectItem> items = new List<SelectItem>();
            #region
            items.Add(new SelectItem { OptionValue = "Description", OptionText = "描述" });
            items.Add(new SelectItem { OptionValue = "OptName", OptionText = "操作者" });           
            #endregion
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OprtReport()
        {
            return View();
        }

        public ActionResult FindOprtRecordList()
        {
            #region
            string pageSize = Request.QueryString["pageSize"];
            string pageIndex = Request.QueryString["pageIndex"];
            string startdtime = Request.QueryString["stdtime"];
            string enddtime = Request.QueryString["enddtime"];
            string queryName = Request.QueryString["queryName"];
            string queryValue = Request.QueryString["queryValue"];

            int psize = Convert.ToInt32(pageSize);
            int pIndex = Convert.ToInt32(pageIndex);
            DateTime start = DateTime.Parse("2017-1-1");
            DateTime end = DateTime.Parse("2017-1-1");
            if (!string.IsNullOrEmpty(startdtime))
            {
                start = DateTime.Parse(startdtime);
            }
            if (!string.IsNullOrEmpty(enddtime))
            {
                end = DateTime.Parse(enddtime);
            }
            #endregion
            int totalNum = 0;
            List<OperateLog> oprtLst=new CWOperateRecordLog().FindPageList(psize, pIndex, start, end, queryName, queryValue, out totalNum);

            var value = new
            {
                total = totalNum,
                rows = oprtLst
            };
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetQueryNameForFault()
        {
            List<SelectItem> items = new List<SelectItem>();
            #region
            items.Add(new SelectItem { OptionValue = "Description", OptionText = "描述" });
            items.Add(new SelectItem { OptionValue = "Warehouse", OptionText = "库区" });
            items.Add(new SelectItem { OptionValue = "DeviceCode", OptionText = "设备" });
            #endregion
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FaultReport()
        {
            return View();
        }

        public ActionResult FindFaultList()
        {
            #region
            string pageSize = Request.QueryString["pageSize"];
            string pageIndex = Request.QueryString["pageIndex"];
            string startdtime = Request.QueryString["stdtime"];
            string enddtime = Request.QueryString["enddtime"];
            string queryName = Request.QueryString["queryName"];
            string queryValue = Request.QueryString["queryValue"];

            int psize = Convert.ToInt32(pageSize);
            int pIndex = Convert.ToInt32(pageIndex);
            DateTime start = DateTime.Parse("2017-1-1");
            DateTime end = DateTime.Parse("2017-1-1");
            if (!string.IsNullOrEmpty(startdtime))
            {
                start = DateTime.Parse(startdtime);
            }
            if (!string.IsNullOrEmpty(enddtime))
            {
                end = DateTime.Parse(enddtime);
            }
            #endregion
            int totalNum = 0;
            List<FaultLog> oprtLst = new CWFaultLog().FindPageList(psize, pIndex, start, end, queryName, queryValue, out totalNum);

            var value = new
            {
                total = totalNum,
                rows = oprtLst
            };
            return Json(value, JsonRequestBehavior.AllowGet);
        }

    }
}