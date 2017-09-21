using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using Parking.Web.Areas.ReportManager.Models;

namespace Parking.Web.Areas.ReportManager.Controllers
{
    public class DeviceInfoController : Controller
    {      
        public ActionResult FaultReport()
        {
            return View();
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

        public ActionResult StatusBitReport()
        {
            return View();
        }

        /// <summary>
        /// 查找状态位信息
        /// </summary>
        /// <returns></returns>
        public ActionResult FindStatusList()
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
            List<StatusInfoLog> oprtLst = new CWStatusLog().FindPageList(psize, pIndex, start, end, queryName, queryValue, out totalNum);
            var value = new
            {
                total = totalNum,
                rows = oprtLst
            };
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeviceInfoReport()
        {
            return View();
        }

        public JsonResult GetQueryNameForDevice()
        {
            List<SelectItem> items = new List<SelectItem>();
            #region           
            items.Add(new SelectItem { OptionValue = "Warehouse", OptionText = "库区" });
            items.Add(new SelectItem { OptionValue = "DeviceCode", OptionText = "设备" });
            #endregion
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FindDeviceInfoLst()
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
            List<DeviceInfoLog> oprtLst = new CWDeviceStatusLog().FindPageList(psize, pIndex, start, end, queryName, queryValue, out totalNum);
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
                if (code == 1) //是故障记录
                {
                    List<FaultLog> lst = new CWFaultLog().FindPageList(0, 0, startdt, enddt, queryname, content, out totalnum);
                    if (lst.Count > 0)
                    {
                        List<FaultLog> filterLst = new List<FaultLog>();
                        if (totalnum > 1000)
                        {
                            filterLst = lst.Skip(0).Take(1000).ToList();
                        }
                        else
                        {
                            filterLst.AddRange(lst);
                        }
                        DataTable dt = ConvertToDataTable.ToDataTable<FaultLog>(filterLst);
                        string path = ExcelUtility.Instance.RenderDataTableToExcel(dt, "故障日志报表", filename, 8);
                        resp.Code = 1;
                        resp.Message = "数据导出成功，记录数 - " + filterLst.Count + " ,文件路径 - " + path;
                    }
                    else
                    {
                        resp.Message = "没有记录要导出.";
                    }
                }
                else if (code == 2) //是状态位记录
                {
                    List<StatusInfoLog> lst = new CWStatusLog().FindPageList(0, 0, startdt, enddt, queryname, content, out totalnum);
                    if (lst.Count > 0)
                    {
                        List<StatusInfoLog> filterLst = new List<StatusInfoLog>();
                        if (totalnum > 1000)
                        {
                            filterLst = lst.Skip(0).Take(1000).ToList();
                        }
                        else
                        {
                            filterLst.AddRange(lst);
                        }
                        DataTable dt = ConvertToDataTable.ToDataTable<StatusInfoLog>(filterLst);
                        string path = ExcelUtility.Instance.RenderDataTableToExcel(dt, "状态信息日志报表", filename, 5);
                        resp.Code = 1;
                        resp.Message = "数据导出成功，记录数 - " + filterLst.Count + " ,文件路径 - " + path;
                    }
                    else
                    {
                        resp.Message = "没有记录要导出.";
                    }
                }
                else if (code == 3) //是状态位记录
                {
                    List<DeviceInfoLog> lst = new CWDeviceStatusLog().FindPageList(0, 0, startdt, enddt, queryname, content, out totalnum);
                    if (lst.Count > 0)
                    {
                        List<DeviceInfoLog> filterLst = new List<DeviceInfoLog>();
                        if (totalnum > 1000)
                        {
                            filterLst = lst.Skip(0).Take(1000).ToList();
                        }
                        else
                        {
                            filterLst.AddRange(lst);
                        }
                        DataTable dt = ConvertToDataTable.ToDataTable<DeviceInfoLog>(filterLst);
                        string path = ExcelUtility.Instance.RenderDataTableToExcel(dt, "设备状态日志报表", filename, 12);
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