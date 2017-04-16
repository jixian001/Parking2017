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
        /// <summary>
        /// 故障处理
        /// </summary>
        /// <returns></returns>
        public ActionResult TaskManager()
        {
            return View();
        }
        /// <summary>
        /// 故障处理
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortOrder"></param>
        /// <param name="sortName"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetTaskList(int? pageSize, int? pageIndex, string sortOrder, string sortName)
        {
            Page<ImplementTask> page = new Page<ImplementTask>();
            if (pageSize != null)
            {
                page.PageSize = (int)pageSize;
            }
            if (pageIndex != null)
            {
                page.PageIndex = (int)pageIndex;
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

        /// <summary>
        /// 手动完成（多选）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 手动复位（多选）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 查询存车车位
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 数据挪移
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 车位禁用
        /// </summary>
        /// <returns></returns>
        public ActionResult DisableLocation(string txtDisWh, string txtDisLoc, bool isDis)
        {
            if (string.IsNullOrEmpty(txtDisWh))
            {
                return Content("库区为空，传输数据丢失！");
            }
            int wh = Convert.ToInt32(txtDisWh);           
            Response resp = new CWLocation().DisableLocation(wh,txtDisLoc, isDis);
            return Content(resp.Message);
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
            string plate = Request.Form["txtInPlate"];

            loc.Status = EnmLocationStatus.Occupy;
            loc.ICCode = iccd;
            loc.WheelBase = Convert.ToInt32(distance);
            loc.CarSize = carsize;
            loc.PlateNum = plate;
            loc.InDate = dt;           
            Response resp = new CWLocation().UpdateLocation(loc);
            return Content(resp.Message);
        }
        /// <summary>
        /// 数据出库
        /// </summary>
        /// <returns></returns>
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
            loc.Status = EnmLocationStatus.Space;
            loc.ICCode = "";
            loc.WheelBase = 0;
            loc.CarSize = "";
            loc.InDate = DateTime.Parse("2017-1-1");
            Response resp = new CWLocation().UpdateLocation(loc);
            return Content(resp.Message);
        }

        /// <summary>
        /// 获取队列信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult FindQueueList(int? pageSize, int? pageIndex, string warehouse,string code)
        {
            CWTask cwtask = new CWTask();
            Page<WorkTask> page = new Page<WorkTask>();
            if (pageSize != null)
            {
                page.PageSize = (int)pageSize;
            }
            if (pageIndex != null)
            {
                page.PageIndex = (int)pageIndex;
            }
            if (!string.IsNullOrEmpty(warehouse) && !string.IsNullOrEmpty(code))
            {
                int wh = Convert.ToInt32(warehouse);
                int smg = Convert.ToInt32(code);
                page = cwtask.FindPagelist(page, (wtsk => wtsk.Warehouse == wh && wtsk.DeviceCode == smg), null);
                var data = new
                {
                    total = page.TotalNumber,
                    rows = page.ItemLists
                };
                return Json(data);
            }
            page = cwtask.FindPageList(page, null);
            var value = new
            {
                total = page.TotalNumber,
                rows = page.ItemLists
            };           
            return Json(value);
        }
        /// <summary>
        /// 点击详情，查看信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult QueueDetail(int ID)
        {
            WorkTask queue = new CWTask().FindQueue(mtsk => mtsk.ID == ID);
            return View(queue);
        }

        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteQueue(int ID)
        {
            Response rsp = new CWTask().DeleteQueue(ID);
            return Content(rsp.Message);
        }

        /// <summary>
        /// 删除队列清单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult DeleteQueueList(List<int> ids)
        {
            if (ids == null)
            {
                return Content("ids为空，操作失败！");
            }
            CWTask cwtask = new CWTask();
            int count = 0;
            foreach(int id in ids)
            {
                Response rsp = cwtask.DeleteQueue(id);
                if (rsp.Code == 1)
                {
                    count++;
                }
            }

            return Content("删除队列成功，数量-" + count);
        }

    }
}