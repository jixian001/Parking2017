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

            List<Device> hasTask =await new CWDevice().FindListAsync(dv => dv.TaskID != 0);
            CWTask cwtask = new CWTask();
            foreach (Device dev in hasTask)
            {
                ImplementTask itask =await cwtask.FindAsync(dev.TaskID);
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
        public async Task<JsonResult> ManualDisLocation(string info,bool isdis)
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
            List<Location> locList = await new CWLocation().FindLocationListAsync();
            if (locList.Count > 0)
            {
                var nback = new
                {
                    code = 1,
                    data = locList
                };
                return Json(nback, JsonRequestBehavior.AllowGet);
            }
            else
            {

                var bback = new
                {
                    code = 0,
                    data = ""
                };
                return Json(bback, JsonRequestBehavior.AllowGet);
            }
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
        public async Task<JsonResult> ZhiWen()
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
                    resp =await new CWTaskTransfer(hall,wh).DealFingerPrintMessageAsync(psTZ);
                    if (resp.Code == 1)
                    {
                        ZhiWenResult result = resp.Data as ZhiWenResult;
                        isGetCar = result.IsTakeCar;
                        plateNum = result.PlateNum;
                    }
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
        public async Task<JsonResult> IcCard()
        {
            int isGetCar = 0;
            string plateNum = "";
            string sound = "";

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
                resp =await new CWTaskTransfer(hall,wh).DealFingerICCardMessageAsync(ccode);
                if (resp.Code == 1)
                {
                    ZhiWenResult result = resp.Data as ZhiWenResult;
                    isGetCar = result.IsTakeCar;
                    plateNum = result.PlateNum;
                    sound = result.Sound;
                }
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

        #region 测试用
        public ActionResult AnalogFPrintSubmit()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> TestSubmitFPrint(int wh,int hall,string FPrint)
        {
            Response resp = new Response();
            byte[] psTZ = FPrintBase64.Base64FingerDataToHex(FPrint.Trim());
            resp =await new CWTaskTransfer(hall, wh).DealFingerPrintMessageAsync(psTZ);
            return Json(resp);
        }


        [HttpPost]
        public async Task<JsonResult> TestSubmitICCard(int wh,int hall,string physcode)
        {
            Response resp = await new CWTaskTransfer(hall, wh).DealFingerICCardMessageAsync(physcode);
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

        #endregion
    }
}