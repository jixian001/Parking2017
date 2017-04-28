using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
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
                DateTime.TryParse(startdtime, out start);                
            }
            if (!string.IsNullOrEmpty(enddtime))
            {
                DateTime.TryParse(enddtime, out end);
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
                DateTime.TryParse(startdtime, out start);
            }
            if (!string.IsNullOrEmpty(enddtime))
            {
                DateTime.TryParse(enddtime, out end);
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
                DateTime.TryParse(startdtime, out start);
            }
            if (!string.IsNullOrEmpty(enddtime))
            {
                DateTime.TryParse(enddtime, out end);
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

        /// <summary>
        /// 后期改为异步的ACTION
        /// </summary>       
        [HttpPost]
        public JsonResult RenderToExcel(int code,DateTime startdt,DateTime enddt,string queryname,string content,string filename)
        {
            Response resp = new Response();
            #region
            int totalnum;
            string basepath= XMLHelper.GetRootNodeValueByXpath("root", "FilePath");
            string fname = basepath + filename;
            if (code == 1) //是报文记录
            {
                List<TelegramLog> lst = new CWTelegramLog().FindPageList(0, 0, startdt, enddt, queryname, content,out totalnum);
                if (lst.Count > 0)
                {
                    DataTable dt = ConvertToDataTable.ToDataTable<TelegramLog>(lst);
                    ExcelUtility.Instance.RenderDataTableToExcel(dt, "", fname, 11);
                    resp.Code = 1;
                    resp.Message = "数据导出成功，文件路径-"+fname;                    
                }
            }
            else if (code == 2) //是操作记录
            {
                List<OperateLog> lst = new CWOperateRecordLog().FindPageList(0, 0, startdt, enddt, queryname, content, out totalnum);
                if (lst.Count > 0)
                {
                    DataTable dt = ConvertToDataTable.ToDataTable<OperateLog>(lst);
                    ExcelUtility.Instance.RenderDataTableToExcel(dt, "", fname, 4);
                    resp.Code = 1;
                    resp.Message = "数据导出成功，文件路径-" + fname;
                }
            }
            else if (code == 3) //是故障记录
            {
                List<FaultLog> lst=new CWFaultLog().FindPageList(0, 0, startdt, enddt, queryname, content, out totalnum);
                if (lst.Count > 0)
                {
                    DataTable dt = ConvertToDataTable.ToDataTable<FaultLog>(lst);
                    ExcelUtility.Instance.RenderDataTableToExcel(dt, "", fname, 8);
                    resp.Code = 1;
                    resp.Message = "数据导出成功，文件路径-" + fname;
                }
            }
            #endregion
            return Json(resp, JsonRequestBehavior.AllowGet);
        }



    }
}