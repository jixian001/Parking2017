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

        //Hall1的信息
        public ActionResult Index()
        {
            return View();
        }

        //Hall2的信息
        public ActionResult Hall2()
        {
            return View();
        }

        //TV1的信息
        public ActionResult TV1()
        {
            return View();
        }

        //TV2的信息
        public ActionResult TV2()
        {
            return View();
        }              


        public JsonResult FindAlarmsLst(int warehouse,int devicecode)
        {
            List<Alarm> stateList = new CWDevice().FindAlarmList(d => d.Warehouse == warehouse && d.DeviceCode == devicecode);
            List<BackAlarm> dispLst = new List<BackAlarm>();
            for(int i = 0; i < stateList.Count; i++)
            {
                Alarm alarm = stateList[i];
                if (alarm.Value == 1)
                {
                    if (!alarm.Description.Contains("备用"))
                    {
                        if (alarm.Color == EnmAlarmColor.Green)
                        {
                            BackAlarm back = new BackAlarm {
                                Type=1,
                                Description=alarm.Description
                            };
                            dispLst.Add(back);
                        }
                        else if (alarm.Color == EnmAlarmColor.Red)
                        {
                            BackAlarm back = new BackAlarm
                            {
                                Type = 2,
                                Description = alarm.Description
                            };
                            dispLst.Add(back);
                        }
                    }
                }
            }

            return Json(dispLst,JsonRequestBehavior.AllowGet);
        }
        

        /// <summary>
        /// 故障管理，显示故障信息，修改故障信息
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageFaultInfo()
        {
            return View();
        }

    }
}