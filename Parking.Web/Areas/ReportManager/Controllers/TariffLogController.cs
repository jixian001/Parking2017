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
    public class TariffLogController : Controller
    {
        #region 固定用户收费记录
        /// <summary>
        /// 固定用户收费记录
        /// </summary>
        /// <returns></returns>
        public ActionResult FixChargeRecord()
        {
            return View();
        }

        /// <summary>
        /// 固定用户缴费记录查询条件
        /// </summary>
        /// <returns></returns>
        public JsonResult GetQueryNameForFixChgRcd()
        {
            List<SelectItem> items = new List<SelectItem>();
            #region
            items.Add(new SelectItem { OptionValue = "UserName", OptionText = "用户名" });
            items.Add(new SelectItem { OptionValue = "PlateNum", OptionText = "车牌号" });
            items.Add(new SelectItem { OptionValue = "Proof", OptionText = "缴费凭证" });
            items.Add(new SelectItem { OptionValue = "CurrDeadline", OptionText = "小于期限" });
            items.Add(new SelectItem { OptionValue = "OprtCode", OptionText = "操作员" });
            #endregion
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 报文查询
        /// </summary>
        /// <returns></returns>
        public ActionResult FindFixChgRecordList()
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
            List<FixUserChargeLog> fixchgLst = new CWTariffLog().FindPageListForFixChgLog(psize, pIndex, start, end, queryName, queryValue, out totalNum);
            var value = new
            {
                total = totalNum,
                rows = fixchgLst
            };
            return Json(value, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region 临时用户收费记录
        public ActionResult TempChargeRecord()
        {
            return View();
        }
        /// <summary>
        /// 临时用户缴费记录查询条件
        /// </summary>
        /// <returns></returns>
        public JsonResult GetQueryNameForTempChgRcd()
        {
            List<SelectItem> items = new List<SelectItem>();
            #region
            items.Add(new SelectItem { OptionValue = "Proof", OptionText = "缴费凭证" });
            items.Add(new SelectItem { OptionValue = "Plate", OptionText = "车牌号" });
            items.Add(new SelectItem { OptionValue = "Address", OptionText = "存车位" });
            items.Add(new SelectItem { OptionValue = "OprtCode", OptionText = "操作员" });
            #endregion
            return Json(items, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 报文查询
        /// </summary>
        /// <returns></returns>
        public ActionResult FindTempChgRecordList()
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
            List<TempUserChargeLog> tempchgLst = new CWTariffLog().FindPageListForTempChgLog(psize, pIndex, start, end, queryName, queryValue, out totalNum);
            var value = new
            {
                total = totalNum,
                rows = tempchgLst
            };
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        #endregion

        /// <summary>
        /// 
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
                if (code == 1) //是临时用户
                {
                    List<TempUserChargeLog> lst = new CWTariffLog().FindPageListForTempChgLog(0, 0, startdt, enddt, queryname, content, out totalnum);
                    if (lst.Count > 0)
                    {
                        List<TempUserChargeLog> filterLst = new List<TempUserChargeLog>();
                        if (totalnum > 1000)
                        {
                            filterLst = lst.Skip(0).Take(1000).ToList();
                        }
                        else
                        {
                            filterLst.AddRange(lst);
                        }
                        DataTable dt = ConvertToDataTable.ToDataTable<TempUserChargeLog>(filterLst);
                        string path = ExcelUtility.Instance.RenderDataTableToExcel(dt, "临时用户缴费日志报表", filename, 13);
                        resp.Code = 1;
                        resp.Message = "数据导出成功，记录数 - " + filterLst.Count + " ,文件路径 - " + path;
                    }
                    else
                    {
                        resp.Message = "没有记录要导出.";
                    }
                }
                else if (code == 2) //是固定用户
                {
                    List<FixUserChargeLog> lst = new CWTariffLog().FindPageListForFixChgLog(0, 0, startdt, enddt, queryname, content, out totalnum);
                    if (lst.Count > 0)
                    {
                        List<FixUserChargeLog> filterLst = new List<FixUserChargeLog>();
                        if (totalnum > 1000)
                        {
                            filterLst = lst.Skip(0).Take(1000).ToList();
                        }
                        else
                        {
                            filterLst.AddRange(lst);
                        }
                        DataTable dt = ConvertToDataTable.ToDataTable<FixUserChargeLog>(filterLst);
                        string path = ExcelUtility.Instance.RenderDataTableToExcel(dt, "固定用户缴费报表", filename, 12);
                        resp.Code = 1;
                        resp.Message = "数据导出成功，记录数 - " + filterLst.Count + " ,文件路径 - " + path;
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