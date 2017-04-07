using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

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

        /*
         * signalr 推送调用
         *  Task.Factory.StartNew(()=> {
         *       var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
         *       hubs.Clients.All.getMessage(message);
         *   });
         */
    }
}