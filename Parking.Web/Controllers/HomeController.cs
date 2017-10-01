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
using Newtonsoft.Json;

namespace Parking.Web.Controllers
{
    public class HomeController : Controller
    {
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

        public async Task<JsonResult> GetDeviceList()
        {
            List<Device> devices = await new CWDevice().FindListAsync();
            return Json(devices, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取有作业的设备信息
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetDeviceTaskLst()
        {
            List<DeviceTaskDetail> detailLst = new List<DeviceTaskDetail>();

            List<Device> hasTask = await new CWDevice().FindListAsync(dv => dv.TaskID != 0);
            CWTask cwtask = new CWTask();
            foreach (Device dev in hasTask)
            {
                ImplementTask itask = await cwtask.FindAsync(dev.TaskID);
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

            return Json(detailLst, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询车位
        /// </summary>
        /// <param name="locinfo">库区_边_列_层</param>
        /// <returns></returns>
        public async Task<JsonResult> GetLocation(string locinfo)
        {
            string[] info = locinfo.Split('_');
            if (info.Length < 4)
            {
                var nback = new
                {
                    code = 0,
                    data = "数据长度不正确," + locinfo
                };
                return Json(nback, JsonRequestBehavior.AllowGet);
            }
            Location lctn = null;

            int wh = Convert.ToInt32(info[0]);
            string address = info[1] + info[2].PadLeft(2, '0') + info[3].PadLeft(2, '0');
            lctn = await new CWLocation().FindLocationAsync(lc => lc.Address == address && lc.Warehouse == wh);
            if (lctn == null)
            {
                var nback = new
                {
                    code = 0,
                    data = "找不到车位，" + locinfo
                };
                return Json(nback, JsonRequestBehavior.AllowGet);
            }

            Customer cust = new CWICCard().FindFixLocationByAddress(lctn.Warehouse, lctn.Address);
            int isfix = 0;
            string custname = "";
            string deadline = "";
            string rcdplate = "";
            if (cust != null)
            {
                isfix = 1;
                custname = cust.UserName;
                deadline = cust.Deadline.ToString();
                rcdplate = cust.PlateNum;
            }
            LocsMapping map = new LocsMapping
            {
                Warehouse = lctn.Warehouse,
                Address = lctn.Address,
                LocSide = lctn.LocSide,
                LocColumn = lctn.LocColumn,
                LocLayer = lctn.LocLayer,
                Type = lctn.Type,
                Status = lctn.Status,
                LocSize = lctn.LocSize,
                ICCode = lctn.ICCode,
                WheelBase = lctn.WheelBase,
                CarWeight = lctn.CarWeight,
                CarSize = lctn.CarSize,
                InDate = lctn.InDate.ToString(),
                PlateNum = lctn.PlateNum,
                IsFixLoc = isfix,
                CustName = custname,
                Deadline = deadline,
                RcdPlate = rcdplate
            };
            var ret = new
            {
                code = 1,
                data = map
            };
            return Json(ret, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<JsonResult> ManualDisLocation(string info, bool isdis)
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
            _resp = await new CWLocation().DisableLocationAsync(wh, address, isdis);
            return Json(_resp);
        }

        /// <summary>
        /// 初始化界面用
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetLocationList()
        {
            Log log = LogFactory.GetLogger("GetLocationList");
            try
            {
                CWICCard cwiccd = new CWICCard();

                List<Location> locList = await new CWLocation().FindLocationListAsync();
                List<Customer> bindCustsLst = await cwiccd.FindCustListAsync(cc => cc.Type == EnmICCardType.FixedLocation || cc.Type == EnmICCardType.VIP);

                List<LocsMapping> mappingsLst = new List<LocsMapping>();
                foreach (Location loc in locList)
                {
                    Customer cust = bindCustsLst.Find(cc => cc.LocAddress == loc.Address && cc.Warehouse == loc.Warehouse);
                    int isfix = 0;
                    string custname = "";
                    string deadline = "";
                    string rcdplate = "";

                    if (cust != null)
                    {
                        isfix = 1;
                        custname = cust.UserName;
                        deadline = cust.Deadline.ToString();
                        rcdplate = cust.PlateNum;
                    }
                    LocsMapping map = new LocsMapping
                    {
                        Warehouse = loc.Warehouse,
                        Address = loc.Address,
                        LocSide = loc.LocSide,
                        LocColumn = loc.LocColumn,
                        LocLayer = loc.LocLayer,
                        Type = loc.Type,
                        Status = loc.Status,
                        LocSize = loc.LocSize,
                        ICCode = loc.ICCode,
                        WheelBase = loc.WheelBase,
                        CarWeight = loc.CarWeight,
                        CarSize = loc.CarSize,
                        InDate = loc.InDate.ToString(),
                        PlateNum = loc.PlateNum,
                        IsFixLoc = isfix,
                        CustName = custname,
                        Deadline = deadline,
                        RcdPlate = rcdplate
                    };
                    mappingsLst.Add(map);
                }
                var nback = new
                {
                    code = 1,
                    data = mappingsLst
                };
                return Json(nback, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            var bback = new
            {
                code = 0,
                data = "系统异常"
            };
            return Json(bback, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询车位统计信息
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetLocStatInfo()
        {
            var info = await new CWLocation().GetLocStatisInfoAsync();
            return Json(info, JsonRequestBehavior.AllowGet);
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
                    psTZ[i] = Convert.ToByte(arrayFinger[i].Trim(), 16);
                }

                log.Debug("接收到的指纹数量- " + psTZ.Length);
                if (psTZ.Length > 380)
                {
                    resp =new CWTaskTransfer(hall, wh).DealFingerPrintMessage(psTZ);
                    if (resp.Code == 1)
                    {
                        ZhiWenResult result = resp.Data as ZhiWenResult;
                        isGetCar = result.IsTakeCar;
                        plateNum = result.PlateNum;
                    }
                }
                else
                {
                    resp.Message = "上传的指纹特性数量不正确，Length- " + psTZ.Length;
                }
                log.Debug("指纹上传返回值 - " + resp.Message);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Message = "系统异常";
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
            string sound = "";

            Response resp = new Response();
            Log log = LogFactory.GetLogger("IcCard");
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
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
                resp = new CWTaskTransfer(hall, wh).DealFingerICCardMessage(ccode);
                if (resp.Code == 1)
                {
                    ZhiWenResult result = resp.Data as ZhiWenResult;
                    isGetCar = result.IsTakeCar;
                    plateNum = result.PlateNum;
                    sound = result.Sound;
                }
                log.Debug("一体机刷卡,返回 - " + resp.Message);
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
                sound = sound
            };
            return Json(json);
        }

        /// <summary>
        /// 指纹机获取当前车厅的取车队列和正在作业信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> QueryCarQueue()
        {
            Log log = LogFactory.GetLogger("QueryCarQueue");
            try
            {               
                #region
                CWTask cwtask = new CWTask();
                List<WorkTask> mtaskLst = await cwtask.FindQueueListAsync(m => m.IsMaster == 2);
                //如果是连取时，ETV先执行了，队列消失了，但车厅的队列要显示出来
                List<WorkTask> mtaskhashallLst = await cwtask.FindQueueListAsync(m => m.IsMaster == 1 && (m.MasterType == EnmTaskType.GetCar || m.MasterType == EnmTaskType.TempGet));
                //获取车厅正在执行的作业
                List<ImplementTask> hallTaskLst = await cwtask.FindITaskLstAsync();
                //获取车厅设备
                List<Device> hallsLst = await new CWDevice().FindListAsync(d => d.Type == EnmSMGType.Hall);

                List<FeedbackFingerData> fdataLst = new List<FeedbackFingerData>();
                foreach (Device hall in hallsLst)
                {
                    List<FeedbackQueue> queueLst = new List<FeedbackQueue>();
                    #region
                    int i = 1;
                    WorkTask nextworktask = mtaskhashallLst.Find(mt => mt.DeviceCode == hall.DeviceCode && mt.Warehouse == hall.Warehouse);
                    if (nextworktask != null)
                    {
                        string plate = "";
                        if (string.IsNullOrEmpty(nextworktask.PlateNum))
                        {
                            plate = nextworktask.ICCardCode;
                        }
                        else
                        {
                            plate = nextworktask.PlateNum;
                        }

                        FeedbackQueue queue = new FeedbackQueue
                        {
                            queueNum = i++,
                            queueCarbrand =plate,
                            queueIcCode = nextworktask.ICCardCode
                        };

                        queueLst.Add(queue);
                    }

                    List<WorkTask> worktaskLst = mtaskLst.FindAll(mt => mt.DeviceCode == hall.DeviceCode && mt.Warehouse == hall.Warehouse);
                    foreach (WorkTask wtask in worktaskLst)
                    {
                        string plate = "";
                        if (string.IsNullOrEmpty(wtask.PlateNum))
                        {
                            plate = wtask.ICCardCode;
                        }
                        else
                        {
                            plate = wtask.PlateNum;
                        }

                        FeedbackQueue queue = new FeedbackQueue
                        {
                            queueNum = i++,
                            queueCarbrand = plate,
                            queueIcCode = wtask.ICCardCode
                        };

                        queueLst.Add(queue);
                    }
                    #endregion
                    string platenum = "";
                    string tasktype = "";
                    #region
                    ImplementTask itask = hallTaskLst.Find(ht => ht.DeviceCode == hall.DeviceCode && ht.Warehouse == hall.Warehouse);
                    if (itask != null)
                    {
                        if (string.IsNullOrEmpty(itask.PlateNum))
                        {
                            platenum = itask.ICCardCode;
                        }
                        else
                        {
                            platenum = itask.PlateNum;
                        }
                        if (itask.Type == EnmTaskType.SaveCar)
                        {
                            tasktype = "正在存车";
                        }
                        else
                        {
                            tasktype = "正在取车";
                        }
                    }
                    #endregion
                    FeedbackFingerData single = new FeedbackFingerData
                    {
                        wareHouseName = (hall.DeviceCode - 10).ToString(),
                        carBarnd = platenum,
                        taskType = tasktype,
                        queueList = queueLst
                    };
                    fdataLst.Add(single);
                }

                var iRet = new
                {
                    status = 1,
                    msg = "查询成功",
                    data =fdataLst
                };

                return Json(iRet);

                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            var nback = new
            {
                status = 0,
                msg = "查询异常"
            };
            return Json(nback);
        }

        #region 测试用
        public ActionResult AnalogFPrintSubmit()
        {
            return View();
        }

        [HttpPost]
        public JsonResult TestSubmitFPrint(int wh, int hall, string FPrint)
        {
            Response resp = new Response();
            byte[] psTZ = FPrintBase64.Base64FingerDataToHex(FPrint.Trim());
            resp = new CWTaskTransfer(hall, wh).DealFingerPrintMessage(psTZ);
            return Json(resp);
        }


        [HttpPost]
        public JsonResult TestSubmitICCard(int wh, int hall, string physcode)
        {
            Response resp = new CWTaskTransfer(hall, wh).DealFingerICCardMessage(physcode);
            return Json(resp);
        }

        /// <summary>
        /// 主页面加载时，更新下时间，
        /// 同步系统时间，同时判断下是否设置了合同期限
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateLocalTime()
        {
            Response resp = new Response();
            //resp = new ContractLimit().UpdateLocalTime();
            return Json(resp);
        }

        [HttpGet]
        public ContentResult BookingLocByCheckCode(string checkcode)
        {
            Location loc = new AllocateLocByBook().AllocateLoc(checkcode);

            return Content(loc.Address);
        }

        #endregion
    }
}