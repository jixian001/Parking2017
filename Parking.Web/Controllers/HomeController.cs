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
using Parking.Web.Models;

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

            MainCallback<ImplementTask>.Instance().WatchEvent+= FileWatch_IMPTaskWatchEvent;
            
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
        private void FileWatch_LctnWatchEvent(Location loca)
        {
            #region
            int total = 0;
            int occupy = 0;
            int space = 0;
            int fix = 0;
            int bspace = 0;
            int sspace = 0;
            List<Location> locLst = new CWLocation().FindLocationList(lc => lc.Type != EnmLocationType.Invalid && lc.Type != EnmLocationType.Hall);
            total = locLst.Count;
            CWICCard cwiccd = new CWICCard();
            foreach (Location loc in locLst)
            {
                #region
                if (loc.Type == EnmLocationType.Normal)
                {
                    if (cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null)
                    {
                        if (loc.Type == EnmLocationType.Normal)
                        {
                            if (loc.Status == EnmLocationStatus.Space)
                            {
                                space++;
                                if (loc.LocSize.Length == 3)
                                {
                                    string last = loc.LocSize.Substring(2);
                                    if (last == "1")
                                    {
                                        sspace++;
                                    }
                                    else if (last == "2")
                                    {
                                        bspace++;
                                    }
                                }
                            }
                            else if (loc.Status == EnmLocationStatus.Occupy)
                            {
                                occupy++;
                            }
                        }
                    }
                    else
                    {
                        fix++;
                    }
                }
                #endregion
            }
            StatisInfo info = new StatisInfo
            {
                Total = total,
                Occupy = occupy,
                Space = space,
                SmallSpace = sspace,
                BigSpace = bspace,
                FixLoc = fix
            };
            #endregion

            Task.Factory.StartNew(()=> {
                var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
                //推送车位信息变化
                hubs.Clients.All.feedbackLocInfo(loca);
                //推送统计信息
                hubs.Clients.All.feedbackStatistInfo(info);
            });
        }

        /// <summary>
        /// 推送设备信息
        /// </summary>
        /// <param name="entity"></param>
        private void FileWatch_DeviceWatchEvent(Device smg)
        {
            Task.Factory.StartNew(() => {
                var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
                hubs.Clients.All.feedbackDevice(smg);
            });
        }

        /// <summary>
        /// 推送执行作业信息
        /// </summary>
        /// <param name="itask"></param>
        private void FileWatch_IMPTaskWatchEvent(ImplementTask itask)
        {
            Task.Factory.StartNew(() => {
                var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();

                string desp = itask.Warehouse.ToString() + itask.DeviceCode.ToString();
                string type = PlusCvt.ConvertTaskType(itask.Type);
                string status = PlusCvt.ConvertTaskStatus(itask.Status, itask.SendStatusDetail);
                DeviceTaskDetail detail = new DeviceTaskDetail
                {
                    DevDescp = desp,
                    TaskType = type,
                    Status = status,
                    Proof = itask.ICCardCode
                };

                hubs.Clients.All.feedbackImpTask(detail);
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
        /// 获取有作业的设备信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDeviceTaskLst()
        {
            CWTask cwtask = new CWTask();
            List<Device> hasTask = new CWDevice().FindList(smg => smg.TaskID != 0);
            List<DeviceTaskDetail> detailLst = new List<DeviceTaskDetail>();
            foreach(Device dev in hasTask)
            {
                ImplementTask itask = cwtask.Find(dev.TaskID);
                if (itask != null)
                {
                    string desp = dev.Warehouse.ToString() + dev.DeviceCode.ToString();
                    string type = PlusCvt.ConvertTaskType(itask.Type);
                    string status = PlusCvt.ConvertTaskStatus(itask.Status,itask.SendStatusDetail);
                    DeviceTaskDetail detail = new DeviceTaskDetail {
                        DevDescp=desp,
                        TaskType=type,
                        Status=status,
                        Proof=itask.ICCardCode
                    };
                    detailLst.Add(detail);
                }
            }
            return Json(detailLst,JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// 查询车位统计信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLocStatInfo()
        {
            #region
            int total = 0;
            int occupy = 0;
            int space = 0;
            int fix = 0;
            int bspace = 0;
            int sspace = 0;
            List<Location> locLst = new CWLocation().FindLocationList(lc=>lc.Type!=EnmLocationType.Invalid&&lc.Type!=EnmLocationType.Hall);
            total = locLst.Count;
            CWICCard cwiccd = new CWICCard();
            foreach(Location loc in locLst)
            {
                #region
                if (loc.Type == EnmLocationType.Normal)
                {
                    if (cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null)
                    {
                        if (loc.Type == EnmLocationType.Normal)
                        {
                            if (loc.Status == EnmLocationStatus.Space)
                            {
                                space++;
                                if (loc.LocSize.Length == 3)
                                {
                                    string last = loc.LocSize.Substring(2);
                                    if (last == "1")
                                    {
                                        sspace++;
                                    }
                                    else if (last == "2")
                                    {
                                        bspace++;
                                    }
                                }
                            }
                            else if (loc.Status == EnmLocationStatus.Occupy)
                            {
                                occupy++;
                            }
                        }
                    }
                    else
                    {
                        fix++;
                    }
                }
                #endregion
            }
            StatisInfo info = new StatisInfo {
                Total=total,
                Occupy=occupy,
                Space=space,
                SmallSpace=sspace,
                BigSpace=bspace,
                FixLoc=fix
            };
            #endregion
            return Json(info,JsonRequestBehavior.AllowGet);
        }

        public ActionResult Error()
        {
            return View("Error");
        }

    }
}