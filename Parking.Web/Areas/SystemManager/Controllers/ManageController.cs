using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using Parking.Web.Areas.SystemManager.Models;
using Parking.Web.Models;

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

            List<DisplayITask> dispTaskLst = new List<DisplayITask>();
           
            foreach(ImplementTask itask in pageTask.ItemLists)
            {
                DisplayITask dtask = new DisplayITask {
                    ID = itask.ID,
                    Warehouse = itask.Warehouse,
                    DeviceCode = itask.DeviceCode,
                    Type = PlusCvt.ConvertTaskType(itask.Type),
                    Status = PlusCvt.ConvertTaskStatus(itask.Status, itask.SendStatusDetail),
                    SendStatusDetail = PlusCvt.ConvertSendStateDetail(itask.SendStatusDetail),
                    CreateDate=itask.CreateDate.ToString(),
                    SendDtime=itask.SendDtime.ToString(),
                    HallCode=itask.HallCode,
                    FromLctAddress=itask.FromLctAddress,
                    ToLctAddress=itask.ToLctAddress,
                    ICCardCode=itask.ICCardCode,
                    Distance=itask.Distance,
                    CarSize=itask.CarSize,
                    CarWeight=itask.CarWeight
                };
                dispTaskLst.Add(dtask);
            }
            int rcdNum = pageTask.TotalNumber;
            var data = new
            {
                total = pageTask.TotalNumber,
                rows = dispTaskLst
            };
            return Json(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult TaskDetail(int ID)
        {
            ViewBag.ID = ID;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tID"></param>
        /// <returns></returns>
        public ActionResult GetTaskDetailByID(int ID)
        {
            ImplementTask itask = new CWTask().Find(tsk => tsk.ID == ID);
            DisplayITask dtask = new DisplayITask();
            if (itask != null)
            {
                dtask = new DisplayITask
                {
                    ID = itask.ID,
                    Warehouse = itask.Warehouse,
                    DeviceCode = itask.DeviceCode,
                    Type = PlusCvt.ConvertTaskType(itask.Type),
                    Status = PlusCvt.ConvertTaskStatus(itask.Status, itask.SendStatusDetail),
                    SendStatusDetail = PlusCvt.ConvertSendStateDetail(itask.SendStatusDetail),
                    CreateDate = itask.CreateDate.ToString(),
                    SendDtime = itask.SendDtime.ToString(),
                    HallCode = itask.HallCode,
                    FromLctAddress = itask.FromLctAddress,
                    ToLctAddress = itask.ToLctAddress,
                    ICCardCode = itask.ICCardCode,
                    Distance = itask.Distance,
                    CarSize = itask.CarSize,
                    CarWeight = itask.CarWeight
                };
            }
            return Json(dtask,JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 队列处理
        /// </summary>
        /// <returns></returns>
        public ActionResult QueueManager()
        {
            return View();
        }

        public ActionResult GetSelectItemName(int warehouse)
        {
            List<SelectItem> items = new List<SelectItem>();
            //先以一个库算，后面做联动查询的
            List<Device> devLst = new CWDevice().FindList(d => d.Warehouse == warehouse);
            foreach (Device dev in devLst)
            {
                if (dev.DeviceCode > 10)
                {
                    items.Add(new SelectItem { OptionValue = dev.DeviceCode.ToString(), OptionText = "车厅 " + (dev.DeviceCode - 10).ToString() });
                }
                else
                {
                    items.Add(new SelectItem { OptionValue = dev.DeviceCode.ToString(), OptionText = "TV" + dev.DeviceCode.ToString() });
                }
            }
            return Json(items, JsonRequestBehavior.AllowGet);
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
            Response res = new CWTask().ManualCompleteTask(tid);
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
                Response resp = cwtask.ManualCompleteTask(id);
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
            Response res = new CWTask().ManualResetTask(tid);
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
                Response resp = cwtask.ManualResetTask(id);
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
            Response resp = new Response();
            string iccd = Request.QueryString["txtIccd"];
            string isPlate = Request.QueryString["isplate"];
            bool frplate = false;
            if (!string.IsNullOrEmpty(isPlate))
            {
                frplate = Convert.ToBoolean(isPlate);
            }
            if (!frplate)
            {
                Location loc = new CWLocation().FindLocation(lc => lc.ICCode == iccd);
                if (loc != null && loc.Status != EnmLocationStatus.Space)
                {
                    resp.Code = 1;
                    resp.Data = loc;
                }
            }
            else
            {
                //依车牌号查询存车位
                Location loc = new CWLocation().FindLocation(lc=>lc.PlateNum==iccd);
                if (loc != null && loc.Status != EnmLocationStatus.Space)
                {
                    resp.Code = 1;
                    resp.Data = loc;
                }
            }
            return Json(resp, JsonRequestBehavior.AllowGet);
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
            if ((!string.IsNullOrEmpty(warehouse) && warehouse != "0") &&
                (!string.IsNullOrEmpty(code) && code != "0"))
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

        [HttpGet]
        public ActionResult GetOCXIPAddress()
        {
            string ipaddrs = "";
            string addrs = XMLHelper.GetRootNodeValueByXpath("root", "OCXIPAddrs");
            if (addrs != null)
            {
                ipaddrs = addrs;
            }
            return Content(ipaddrs);
        }

        [HttpPost]
        public ActionResult SubmitFPrintFeacture(string strTZ)
        {
            Response resp = new CWFingerPrint().FindCustByFPrintFeacture(strTZ);
            return Json(resp);
        }

    }
}