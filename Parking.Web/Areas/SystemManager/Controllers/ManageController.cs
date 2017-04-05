using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;

namespace Parking.Web.Areas.SystemManager.Controllers
{
    public class ManageController : Controller
    {
        // GET: SystemManager/Manage/Index
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 故障处理,暂跟INDEX关系，后面再做细划
        /// </summary>
        /// <returns></returns>
        public ActionResult TaskManager()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult GetTaskList(int? pageSize, int? pageNumber, string sortOrder, string sortName)
        {
            Page<ImplementTask> page = new Page<ImplementTask>();
            if (pageSize != null)
            {
                page.PageSize = (int)pageSize;
            }
            if (pageNumber != null)
            {
                page.PageIndex = (int)pageNumber;
            }
            OrderParam orderParam = null;
            if (!string.IsNullOrEmpty(sortName))
            {
                orderParam = new OrderParam();
                orderParam.PropertyName = sortName;
                if (!string.IsNullOrEmpty(sortOrder))
                {
                    orderParam.Method = sortOrder.ToLower() == "asc" ? OrderMethod.Asc : OrderMethod.Desc;
                }
                else
                {
                    orderParam.Method = OrderMethod.Asc;
                }
            }
            Page<ImplementTask> pageTask = new CWTask().FindPageList(page, orderParam);
            var data = new
            {
                total = pageTask.TotalNumber,
                rows = pageTask.ItemLists
            };
            return Json(data);
        }
        /// <summary>
        /// 点击详情，查看信息
        /// </summary>
        /// <param name="tID"></param>
        /// <returns></returns>
        public ActionResult TaskDetail(int ID)
        {
            ImplementTask task = new CWTask().Find(tsk => tsk.ID == ID);
            return View(task);
        }

        /// <summary>
        /// 队列处理
        /// </summary>
        /// <returns></returns>
        public ActionResult QueueManager()
        {
            return View();
        }

        /// <summary>
        /// 车位维护
        /// </summary>
        /// <returns></returns>
        public ActionResult CarpotManager()
        {
            return View();
        }

        /// <summary>
        /// 手动完成
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CompleteTask(int tid)
        {
            Response res = new CWTask().CompleteTask(tid);
            return Content(res.Message);
        }

        public ActionResult CompleteTask(List<int> ids)
        {
            if (ids == null)
            {
                return Content("Fail");
            }
            CWTask cwtask = new CWTask();
            int count = 0;
            foreach (int id in ids)
            {
                Response resp = cwtask.CompleteTask(id);
                if (resp.Code == 1)
                {
                    count++;
                }
            }
            return Content("操作成功,作用数量-" + count);
        }

        /// <summary>
        /// 手动复位
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ResetTask(int tid)
        {
            Response res = new CWTask().ResetTask(tid);
            return Content(res.Message);
        }

        public ActionResult ResetTask(List<int> ids)
        {
            if (ids == null)
            {
                return Content("Fail");
            }
            CWTask cwtask = new CWTask();
            int count = 0;
            foreach (int id in ids)
            {
                Response resp = cwtask.ResetTask(id);
                if (resp.Code == 1)
                {
                    count++;
                }
            }
            return Content("操作成功,数量-" + count);
        }

        public ActionResult FindLocByICCard()
        {
            string iccd = Request.QueryString["txtIccd"];
            Location loc = new CWLocation().FindLocation(lc => lc.ICCode == iccd);
            if (loc != null)
            {
                var data = new
                {
                    Warehouse = loc.Warehouse,
                    LocAddress = loc.Address
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Warehouse = "", LocAddress = "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TransferLoc()
        {
            string wh = Request.Form["txtTransWh"];
            if (string.IsNullOrEmpty(wh))
            {
                return Content("库区为空，传输数据丢失！");
            }
            int warehouse = Convert.ToInt32(wh);
            string fromAddrs = Request.Form["txtFrom"];
            string toAddrs = Request.Form["txtTo"];
            CWLocation cwlctn = new CWLocation();
            Location fromLoc = cwlctn.FindLocation(lc => lc.Warehouse == warehouse && lc.Address == fromAddrs);
            if (fromLoc == null)
            {
                return Content("找不到源车位-" + fromAddrs);
            }
            Location toLoc = cwlctn.FindLocation(lc => lc.Warehouse == warehouse && lc.Address == toAddrs);
            if (toLoc == null)
            {
                return Content("找不到目的车位-" + toLoc);
            }
            int ret = new CWLocation().TransportLoc(fromLoc, toLoc);
            if (ret == 1)
            {
                return Content("操作成功！");
            }
            else
            {
                return Content("操作引发异常！");
            }
        }

        public ActionResult DisableLocation(string txtDisWh, string txtDisLoc, bool isDis)
        {
            if (string.IsNullOrEmpty(txtDisWh))
            {
                return Content("库区为空，传输数据丢失！");
            }
            int wh = Convert.ToInt32(txtDisWh);
            Location loc = new CWLocation().FindLocation(lc => lc.Warehouse == wh && lc.Address == txtDisLoc);
            if (loc == null)
            {
                return Content("找不到车位-" + txtDisLoc);
            }
            if (loc.Type == EnmLocationType.Invalid ||
                loc.Type == EnmLocationType.Hall ||
                loc.Type == EnmLocationType.ETV)
            {
                return Content("当前车位-" + txtDisLoc + "无效，不允许操作！");
            }
            int nback = new CWLocation().DisableLocation(loc, isDis);
            if (nback == 1)
            {
                return Content("操作成功！");
            }
            else
            {
                return Content("操作引发异常！");
            }
        }

        /// <summary>
        /// 车位数据入库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LocationIn()
        {
            string wh = Request.Form["txtInWh"];
            if (string.IsNullOrEmpty(wh))
            {
                return Content("库区为空，传输数据丢失！");
            }
            int warehouse = Convert.ToInt32(wh);
            string Addrs = Request.Form["txtInLoc"];
            if (string.IsNullOrEmpty(Addrs))
            {
                return Content("入库车位为空，传输数据丢失！");
            }
            Location loc = new CWLocation().FindLocation(lc => lc.Warehouse == warehouse && lc.Address == Addrs);
            if (loc == null)
            {
                return Content("找不到车位-" + Addrs);
            }
            string indate = Request.Form["txtInDtime"];
            if (string.IsNullOrEmpty(indate))
            {
                return Content("入库时间为空，操作失败！");
            }
            DateTime dt = DateTime.Parse(indate);
            string iccd = Request.Form["txtInIccd"];
            if (string.IsNullOrEmpty(iccd))
            {
                return Content("入库卡号为空，操作失败！");
            }
            string distance = Request.Form["txtInDist"];
            if (string.IsNullOrEmpty(distance))
            {
                return Content("入库轴距为空，操作失败！");
            }
            string carsize = Request.Form["txtInSize"];
            loc.ICCode = iccd;
            loc.WheelBase = Convert.ToInt32(distance);
            loc.CarSize = carsize;
            loc.InDate = dt;
            Response resp = new CWLocation().UpdateLocation(loc);
            return Content(resp.Message);
        }

        [HttpPost]
        public ActionResult LocationOut()
        {
            string wh = Request.Form["txtOutWh"];
            if (string.IsNullOrEmpty(wh))
            {
                return Content("库区为空，传输数据丢失！");
            }
            int warehouse = Convert.ToInt32(wh);
            string Addrs = Request.Form["txtOutLoc"];
            if (string.IsNullOrEmpty(Addrs))
            {
                return Content("车位为空，传输数据丢失！");
            }
            Location loc = new CWLocation().FindLocation(lc => lc.Warehouse == warehouse && lc.Address == Addrs);
            if (loc == null)
            {
                return Content("找不到车位-" + Addrs);
            }
            loc.ICCode = "";
            loc.WheelBase = 0;
            loc.CarSize = "";
            loc.InDate = DateTime.Parse("2017-1-1");
            Response resp = new CWLocation().UpdateLocation(loc);
            return Content(resp.Message);
        }
    }
}