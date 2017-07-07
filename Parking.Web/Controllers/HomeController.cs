using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using System.Threading.Tasks;
using Parking.Web.Models;
using System.Web.Script.Serialization;
using System.Text;

namespace Parking.Web.Controllers
{   
    public class HomeController : Controller
    {
       
        public HomeController()
        { 
        }

        #region 不用的
        /*
        * signalr 推送调用
        *  Task.Factory.StartNew(()=> {
        *       var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
        *       hubs.Clients.All.getMessage(message);
        *   });
        *   
        *   [HttpPost]
        *    public ActionResult Send(string cid, string msg)
        *    {
        *        var context = GlobalHost.ConnectionManager.GetHubContext<CommHub>();
        *        if (cid == "*") //對所有Client傳送
        *            //注意: Clients是dynamic，故ShowMessage等方法名稱只要跟Client可以對應即可
        *            context.Clients.ShowMessage(msg);
        *        else 
        *            //利用Clients[connection_id]指定特定的Client, 呼叫其ShowMessage()
        *            context.Clients[cid].ShowMessage(msg);
        *        return Content("OK");
        *    }
        *   
        */

        /// <summary>
        /// 推送车位信息
        /// </summary>
        /// <param name="loc"></param>
        //private void FileWatch_LctnWatchEvent(Location loca)
        //{
        //    #region
        //    int total = 0;
        //    int occupy = 0;
        //    int space = 0;
        //    int fix = 0;
        //    int bspace = 0;
        //    int sspace = 0;
        //    List<Location> locLst = new CWLocation().FindLocationList(lc => lc.Type != EnmLocationType.Invalid && lc.Type != EnmLocationType.Hall);
        //    total = locLst.Count;
        //    CWICCard cwiccd = new CWICCard();
        //    foreach (Location loc in locLst)
        //    {
        //        #region
        //        if (loc.Type == EnmLocationType.Normal)
        //        {
        //            if (cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null)
        //            {
        //                if (loc.Type == EnmLocationType.Normal)
        //                {
        //                    if (loc.Status == EnmLocationStatus.Space)
        //                    {
        //                        space++;
        //                        if (loc.LocSize.Length == 3)
        //                        {
        //                            string last = loc.LocSize.Substring(2);
        //                            if (last == "1")
        //                            {
        //                                sspace++;
        //                            }
        //                            else if (last == "2")
        //                            {
        //                                bspace++;
        //                            }
        //                        }
        //                    }
        //                    else if (loc.Status == EnmLocationStatus.Occupy)
        //                    {
        //                        occupy++;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                fix++;
        //            }
        //        }
        //        #endregion
        //    }
        //    StatisInfo info = new StatisInfo
        //    {
        //        Total = total,
        //        Occupy = occupy,
        //        Space = space,
        //        SmallSpace = sspace,
        //        BigSpace = bspace,
        //        FixLoc = fix
        //    };
        //    #endregion

        //    Task.Factory.StartNew(()=> {
        //        var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
        //        //推送车位信息变化
        //        hubs.Clients.All.feedbackLocInfo(loca);
        //        //推送统计信息
        //        hubs.Clients.All.feedbackStatistInfo(info);
        //    });
        //}

        /// <summary>
        /// 推送设备信息
        /// </summary>
        /// <param name="entity"></param>
        //private void FileWatch_DeviceWatchEvent(Device smg)
        //{
        //    if (log != null)
        //    {
        //        log.Debug("  warehouse- " + smg.Warehouse + " ,devicecode-" + smg.DeviceCode);
        //    }
        //    Task.Factory.StartNew(() =>
        //    {
        //        var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();
        //        hubs.Clients.All.feedbackDevice(smg);
        //    });                      
        //}

        /// <summary>
        /// 推送执行作业信息
        /// </summary>
        /// <param name="itask"></param>
        //private void FileWatch_IMPTaskWatchEvent(ImplementTask itask)
        //{
        //    Task.Factory.StartNew(() => {
        //        var hubs = GlobalHost.ConnectionManager.GetHubContext<ParkingHub>();

        //        string desp = itask.Warehouse.ToString() + itask.DeviceCode.ToString();
        //        string type = PlusCvt.ConvertTaskType(itask.Type);
        //        string status = PlusCvt.ConvertTaskStatus(itask.Status, itask.SendStatusDetail);
        //        DeviceTaskDetail detail = new DeviceTaskDetail
        //        {
        //            DevDescp = desp,
        //            TaskType = type,
        //            Status = status,
        //            Proof = itask.ICCardCode
        //        };

        //        hubs.Clients.All.feedbackImpTask(detail);
        //    });
        //}

        #endregion

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
           
            return Json(devices, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取有作业的设备信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDeviceTaskLst()
        {
            List<DeviceTaskDetail> detailLst = new List<DeviceTaskDetail>();
            try
            {
                List<Device> hasTask = new CWDevice().FindList(dv => dv.TaskID != 0);
                CWTask cwtask = new CWTask();
                foreach (Device dev in hasTask)
                {
                    ImplementTask itask = cwtask.Find(dev.TaskID);
                    if (itask != null)
                    {
                        string desp = dev.Warehouse.ToString() + dev.DeviceCode.ToString();
                        string type = PlusCvt.ConvertTaskType(itask.Type);
                        string status = PlusCvt.ConvertTaskStatus(itask.Status, itask.SendStatusDetail);
                        DeviceTaskDetail detail = new DeviceTaskDetail
                        {
                            DevDescp = desp,
                            TaskType = type,
                            Status = status,
                            Proof = itask.ICCardCode
                        };
                        detailLst.Add(detail);
                    }
                }

            }
            catch (Exception ex)
            {
                Log log = LogFactory.GetLogger("GetDeviceTaskLst");
                log.Error(ex.ToString());
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
            Location lctn = null;
            try
            {
                int wh = Convert.ToInt32(info[0]);
                string address = info[1] + info[2].PadLeft(2, '0') + info[3].PadLeft(2, '0');
                lctn = new CWLocation().FindLocation(lc => lc.Address == address && lc.Warehouse == wh);
            }
            catch (Exception ex)
            {
                Log log = LogFactory.GetLogger("GetLocation");
                log.Error(ex.ToString());
            }
            if (lctn == null)
            {
                var nback = new
                {
                    code = 0,
                    data = "找不到车位，" + locinfo
                };
                return Json(nback, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var ret = new
                {
                    code = 1,
                    data = lctn
                };
                return Json(ret, JsonRequestBehavior.AllowGet);
            }            
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
            List<Location> locList = new CWLocation().FindLocList();
            if (locList == null || 
                locList.Count == 0)
            {
                var resp = new
                {
                    code = 0,
                    data = ""
                };
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            else
            {
                TempData["locations"] = locList;
                var nback = new
                {
                    code = 1,
                    data = locList
                };
                return Json(nback, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// 查询车位统计信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLocStatInfo()
        {
            StatisInfo info = new StatisInfo();
            if (TempData["locations"] != null)
            {
                #region
                int total = 0;
                int occupy = 0;
                int space = 0;
                int fix = 0;
                int bspace = 0;
                int sspace = 0;
                List<Location> locLst = ((List<Location>)TempData["locations"]).Where(lc => lc.Type != EnmLocationType.Invalid && lc.Type != EnmLocationType.Hall).ToList();
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
                info.Total = total;
                info.Occupy = occupy;
                info.Space = space;
                info.SmallSpace = sspace;
                info.BigSpace = bspace;              
                #endregion
            }
            return Json(info,JsonRequestBehavior.AllowGet);
        }

        public ActionResult Error()
        {
            return View("Error");
        }

        /// <summary>
        /// 接收指纹一体机上传上来的指纹信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ZhiWen()
        {
            int isGetCar = 0;
            string plateNum = "";
            Response resp = new Response();
           
            Log log = LogFactory.GetLogger("ZhiWen");
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);
                log.Debug("有指纹信息上传！");

                JavaScriptSerializer js = new JavaScriptSerializer();
                AIOFingerPrint fpring = js.Deserialize<AIOFingerPrint>(req);
                          
                string warehouse = "1";
                string hallID = fpring.equipmentID;
                string fingerPrint = fpring.zhiWenInfo;
                
                int wh = 1;
                if (!string.IsNullOrEmpty(warehouse))
                {
                    wh = Convert.ToInt32(warehouse);
                }
                int hall = 0;
                if (!string.IsNullOrEmpty(hallID))
                {
                    hall = Convert.ToInt32(hallID);
                }
                log.Debug("指纹信息中，warehouse - " + warehouse + " ,hallID - " + hall);
                if (hall < 10)
                {
                    resp.Message = "车厅号不正确，hallID- " + hallID;
                    return Json(resp);
                }

                string[] arrayFinger = fingerPrint.Trim().Split(' ');                
                byte[] psTZ = new byte[arrayFinger.Length];
                for (int i = 0; i < arrayFinger.Length; i++)
                {
                    psTZ[i] = Convert.ToByte(arrayFinger[i].Trim(),16);
                }

                log.Debug("接收到的指纹数量- "+psTZ.Length);
                if (psTZ.Length > 380)
                {
                    resp = new CWTaskTransfer(hall,wh).DealFingerPrintMessage(psTZ,out isGetCar,out plateNum);
                }
                else
                {
                    resp.Message = "上传的指纹特性数量不正确，Length- "+psTZ.Length;
                }

                log.Debug(resp.Message);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            var json = new
            {
                status = resp.Code,
                msg = resp.Message,
                isTakeCar = isGetCar,
                carBrand = plateNum,
                counter = 0,
                sound = ""
            };
            return Json(json);
        }

        /// <summary>
        /// 接收指纹一体机上传上来的刷卡信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IcCard()
        {
            int isGetCar = 0;
            string plateNum = "";

            Response resp = new Response();
            Log log = LogFactory.GetLogger("IcCard");
            try
            {               
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes,0,bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);
                log.Debug("有卡号信息上传，解析流得到字符串 - " + req);
               
                JavaScriptSerializer js = new JavaScriptSerializer();
                AIOICCard iccard = js.Deserialize<AIOICCard>(req);

                string warehouse = "1";
                string hallID = iccard.equipmentID;
                string ccode = iccard.cardInfo;
                
                int wh = 1;
                if (!string.IsNullOrEmpty(warehouse))
                {
                    wh = Convert.ToInt32(warehouse);
                }
                int hall = 0;
                if (!string.IsNullOrEmpty(hallID))
                {
                    hall = Convert.ToInt32(hallID);
                }
                if (hall < 10)
                {
                    resp.Message = "车厅号不正确，hallID- " + hallID;
                    return Json(resp);
                }
                log.Debug("一体机刷卡信息中，warehouse - " + warehouse + " ,hallID - " + hall);
                resp = new CWTaskTransfer(hall,wh).DealFingerICCardMessage(ccode,out isGetCar,out plateNum);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            var json = new
            {
                status = resp.Code,
                msg = resp.Message,
                isTakeCar = isGetCar,
                carBrand = plateNum,
                counter = 0,
                sound = ""
            };
            return Json(json);
        }

        #region 测试用
        public ActionResult AnalogFPrintSubmit()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TestSubmitFPrint(int wh,int hall,string FPrint)
        {
            Response resp = new Response();
            byte[] psTZ = FPrintBase64.Base64FingerDataToHex(FPrint.Trim());

            int isGetCar = 0;
            string plate = "";
            resp = new CWTaskTransfer(hall, wh).DealFingerPrintMessage(psTZ,out isGetCar,out plate);
            return Json(resp);
        }


        [HttpPost]
        public ActionResult TestSubmitICCard(int wh,int hall,string physcode)
        {
            Response resp = new Response();

            int isGetCar = 0;
            string plate = "";
            resp = new CWTaskTransfer(hall, wh).DealFingerICCardMessage(physcode,out isGetCar,out plate);
            return Json(resp);
        }

        #endregion
    }
}