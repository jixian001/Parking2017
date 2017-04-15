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

        




        /*
         * signalr 推送调用
         *  Task.Factory.StartNew(()=> {
         *       var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
         *       hubs.Clients.All.getMessage(message);
         *   });
         */



    }
}