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
        public JsonResult GetTaskList(int? pageSize, int? pageNumber,string sortOrder,string sortName)
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
            Page<ImplementTask> pageTask = new CWTask().FindPageList(page,orderParam);
            var data = new {
                total=pageTask.TotalNumber,
                rows=pageTask.ItemLists
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
            ImplementTask task = new CWTask().Find(tsk=>tsk.ID==ID);
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
            foreach(int id in ids)
            {
               Response resp=cwtask.CompleteTask(id);
                if (resp.Code == 1)
                {
                    count++;
                }
            }
            return Content("操作成功,作用数量-"+count);
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
                Response resp= cwtask.ResetTask(id);
                if (resp.Code == 1)
                {
                    count++;
                }
            }
            return Content("操作成功,数量-"+count);
        }
    }
}