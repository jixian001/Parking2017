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
    /// <summary>
    /// 报文日志、操作日志
    /// </summary>
    public class TelegramController : Controller
    {
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
            List<OperateLog> oprtLst = new CWOperateRecordLog().FindPageList(psize, pIndex, start, end, queryName, queryValue, out totalNum);

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
        public JsonResult RenderToExcel(int code, DateTime startdt, DateTime enddt, string queryname, string content, string filename)
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("RenderToExcel");
            #region
            try
            {
                int totalnum;
                if (code == 1) //是报文记录
                {
                    List<TelegramLog> lst = new CWTelegramLog().FindPageList(0, 0, startdt, enddt, queryname, content, out totalnum);
                    if (lst.Count > 0)
                    {
                        List<TelegramLog> filterLst = new List<TelegramLog>();
                        if (totalnum > 1000)
                        {
                            filterLst = lst.Skip(0).Take(1000).ToList();
                        }
                        else
                        {
                            filterLst.AddRange(lst);
                        }
                        DataTable dt = ConvertToDataTable.ToDataTable<TelegramLog>(filterLst);
                        string path = ExcelUtility.Instance.RenderDataTableToExcel(dt, "报文日志报表", filename, 11);
                        resp.Code = 1;
                        resp.Message = "数据导出成功，记录数- " + filterLst.Count + " ,文件路径- " + path;
                    }
                    else
                    {
                        resp.Message = "没有记录要导出.";
                    }
                }
                else if (code == 2) //是操作记录
                {
                    List<OperateLog> lst = new CWOperateRecordLog().FindPageList(0, 0, startdt, enddt, queryname, content, out totalnum);
                    if (lst.Count > 0)
                    {
                        List<OperateLog> filterLst = new List<OperateLog>();
                        if (totalnum > 1000)
                        {
                            filterLst = lst.Skip(0).Take(1000).ToList();
                        }
                        else
                        {
                            filterLst.AddRange(lst);
                        }

                        DataTable dt = ConvertToDataTable.ToDataTable<OperateLog>(filterLst);
                        string path = ExcelUtility.Instance.RenderDataTableToExcel(dt, "操作日志报表", filename, 4);
                        resp.Code = 1;
                        resp.Message = "数据导出成功，记录数- " + filterLst.Count + " ,文件路径- " + path;
                    }
                    else
                    {
                        resp.Message = "没有记录要导出.";
                    }
                }               
            }
            catch (Exception ex)
            {
                log.Error("报表，导出到EXCEL异常：" + ex.ToString());
                resp.Message = "数据导出失败，请联供应商！";
            }
            #endregion
            return Json(resp, JsonRequestBehavior.AllowGet);
        }



    }
}