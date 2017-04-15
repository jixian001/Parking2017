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

namespace Parking.Web.Controllers
{
    public class HomeController : Controller
    {       
        public HomeController()
        {

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

        public JsonResult GetDeviceList()
        {
            List<Device> devices = new CWDevice().FindList(smg=>true);
            if (devices == null)
            {
                devices = new List<Device>();
            }
            
            return Json(devices,JsonRequestBehavior.AllowGet);
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




        /*
         * signalr 推送调用
         *  Task.Factory.StartNew(()=> {
         *       var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
         *       hubs.Clients.All.getMessage(message);
         *   });
         */



    }
}