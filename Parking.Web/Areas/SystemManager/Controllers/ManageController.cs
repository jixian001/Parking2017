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
        /// 故障处理
        /// </summary>
        /// <returns></returns>
        public ActionResult TaskManager()
        {
            return View();
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
        /// 队列处理
        /// </summary>
        /// <returns></returns>
        public ActionResult QueueManager()
        {
            return View();
        }

        public ActionResult CarpotManager()
        {
            return View();
        }

    }
}