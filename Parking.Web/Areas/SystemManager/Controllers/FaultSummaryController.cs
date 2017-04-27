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
    public class FaultSummaryController : Controller
    {
        // GET: SystemManager/FaultSummary
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetDeviceList()
        {
            List<Device> devlst = new CWDevice().FindList(dev => true);
            return Json(devlst,JsonRequestBehavior.AllowGet);
        }

        public JsonResult FindStateBitList(int wh,int smg)
        {
            List<Alarm> stateList = new CWDevice().FindAlarmList(state => state.Warehouse == wh &&
                                                                          state.DeviceCode == smg &&
                                                                          state.IsBackup == 0 &&
                                                                          state.Value == 1 &&
                                                                          state.Color == EnmAlarmColor.Green);
            List<string> descList = new List<string>();
            if (stateList != null && stateList.Count > 0)
            {
                foreach(Alarm alarm in stateList)
                {
                    descList.Add(alarm.Description);
                }
            }
            return Json(descList,JsonRequestBehavior.AllowGet);
        }

        public JsonResult FindFaultBitList(int wh,int smg)
        {
            List<Alarm> alarmList = new CWDevice().FindAlarmList(state => state.Warehouse == wh &&
                                                                         state.DeviceCode == smg &&
                                                                         state.IsBackup == 0 &&
                                                                         state.Value == 1 &&
                                                                         state.Color == EnmAlarmColor.Red);
            List<string> descList = new List<string>();
            if (alarmList != null && alarmList.Count > 0)
            {
                foreach (Alarm alarm in alarmList)
                {
                    descList.Add(alarm.Description);
                }
            }
            return Json(descList, JsonRequestBehavior.AllowGet);
        }


    }
}