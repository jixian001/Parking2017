using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using System.Threading.Tasks;

namespace Parking.Web.Controllers
{   
    public class HomeController : Controller
    {
        private Log log;
          
        public HomeController()
        {
            //订阅事件
            MainCallback<Location>.Instance().WatchEvent+= FileWatch_LctnWatchEvent;

            MainCallback<Device>.Instance().WatchEvent += FileWatch_DeviceWatchEvent;

            log = LogFactory.GetLogger("HomeController");
        }

        /*
        * signalr 推送调用
        *  Task.Factory.StartNew(()=> {
        *       var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
        *       hubs.Clients.All.getMessage(message);
        *   });
        */

        /// <summary>
        /// 推送车位信息
        /// </summary>
        /// <param name="loc"></param>
        private void FileWatch_LctnWatchEvent(Location loc)
        {
            Task.Factory.StartNew(()=> {
                var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
                hubs.Clients.All.feedbackLocInfo(loc);
            });
        }

        /// <summary>
        /// 推送设备信息
        /// </summary>
        /// <param name="entity"></param>
        private void FileWatch_DeviceWatchEvent(Device entity)
        {
            Task.Factory.StartNew(() => {
                var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
                hubs.Clients.All.feedbackDevice(entity);
            });
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {            
            return View();
        }

        public ActionResult WeChat()
        {            
            return View();
        }

        public ActionResult GetDeviceList()
        {
            List<Device> devices = new CWDevice().FindList(smg => true);
            if (devices == null)
            {
                devices = new List<Device>();
            }
            return Json(devices, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 查询车位
        /// </summary>
        /// <param name="locinfo">库区_边_列_层</param>
        /// <returns></returns>
        public JsonResult GetLocation(string locinfo)
        {
            string[] info = locinfo.Split('_');
            if (info.Length < 4)
            {
                var nback = new {
                    code = 0,
                    data = "数据长度不正确," + locinfo
                };
                return Json(nback,JsonRequestBehavior.AllowGet);
            }
            int wh = Convert.ToInt32(info[0]);
            string address = info[1] + info[2].PadLeft(2, '0') + info[3].PadLeft(2,'0');
            Location lctn = new CWLocation().FindLocation(lc=>lc.Address==address&&lc.Warehouse==wh);
            if (lctn == null)
            {
                var nback = new
                {
                    code = 0,
                    data = "找不到车位，"+locinfo
                };
                return Json(nback, JsonRequestBehavior.AllowGet);
            }
            var ret = new {
                code=1,
                data=lctn
            };
            return Json(ret, JsonRequestBehavior.AllowGet);
        }        
        
        [HttpPost]
        public JsonResult ManualDisLocation(string info,bool isdis)
        {
            Response _resp = new Response();
            string[] msg = info.Split('_');
            if (msg.Length < 4)
            {
                _resp.Code = 0;
                _resp.Message = "数据长度不正确," + info;
                return Json(_resp);
            }
            int wh = Convert.ToInt16(msg[0]);
            string address = msg[1] + msg[2].PadLeft(2, '0') + msg[3].PadLeft(2, '0');
            _resp = new CWLocation().DisableLocation(wh, address, isdis);
            return Json(_resp);
        }

        /// <summary>
        /// 初始化界面用
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLocationList()
        {
            List<Location> locList = new CWLocation().FindLocationList(lc => true);
            if (locList == null || locList.Count == 0)
            {
                var resp = new
                {
                    code = 0,
                    data = ""
                };
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            var nback = new
            {
                code = 1,
                data = locList
            };
            return Json(nback, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Error()
        {
            return View("Error");
        }

    }
}