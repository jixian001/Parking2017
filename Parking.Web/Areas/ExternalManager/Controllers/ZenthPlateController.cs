using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parking.Data;
using Parking.Auxiliary;
using Parking.Core;
using Parking.Web.Areas.ExternalManager.Models;

namespace Parking.Web.Areas.ExternalManager.Controllers
{
    public class ZenthPlateController : Controller
    {
        private static string basePath = null;
        private static Dictionary<string,int> dicHallsInfo = new Dictionary<string,int>();

        // GET: ExternalManager/ZenthPlate
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> PlateResult()
        {
            Log log = LogFactory.GetLogger("PlateResult");
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.UTF8.GetString(bytes);
                log.Warn("有车牌识别结果上传...");
                try
                {
                    ZenthPlateResult zenthresult = JsonConvert.DeserializeObject<ZenthPlateResult>(req);

                    ZPlateResult result = zenthresult.AlarmInfoPlate.result.PlateResult;
                    if (result != null)
                    {
                        string ipaddrs = zenthresult.AlarmInfoPlate.ipaddr;
                        string plate = result.license;
                        int triggerType = result.triggerType;

                        log.Warn("车厅IP - " + ipaddrs + " ,车牌识别结果 - " + plate+" ,触发类型 - "+result.triggerType);

                        string headpath = null;
                        #region
                        try
                        {
                            string picBase64Str = result.imageFile;
                            byte[] picBuffer = Convert.FromBase64String(picBase64Str);
                            MemoryStream ms = new MemoryStream(picBuffer);
                            Image img = Image.FromStream(ms);

                            log.Warn("解析图片byte长度 - " + picBuffer.Length + " ,读到的beforeBase64imageFileLen - "+result.beforeBase64imageFileLen + " ,读到的imageFileLen - " + result.imageFileLen);

                            if (string.IsNullOrEmpty(basePath))
                            {
                                basePath = XMLHelper.GetRootNodeValueByXpath("root", "PlatePicPath");
                                //log.Debug("车牌图片路径 - " + basePath);
                            }
                            if (string.IsNullOrEmpty(basePath))
                            {
                                log.Error("车牌图片保存路径为空");
                            }
                            string imgBasePath = basePath + DateTime.Now.Year + "-" + DateTime.Now.Month + "-"+DateTime.Now.Day + "\\";
                            if (!Directory.Exists(imgBasePath))
                            {
                                Directory.CreateDirectory(imgBasePath);
                            }
                            string ti = DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') +
                                  DateTime.Now.Second.ToString().PadLeft(2, '0');
                            string sTime = ti + "_" + plate;
                            headpath = imgBasePath + sTime + ".jpg";
                            img.Save(headpath);
                            log.Warn("图片保存成功， Path - " + headpath);
                        }
                        catch (Exception e1)
                        {
                            log.Error("图片保存异常 - " + e1.ToString());
                        }
                        #endregion

                        if (dicHallsInfo.Count != 2)
                        {
                            dicHallsInfo.Clear();
                        }
                        #region
                        string h1ipadrs = XMLHelper.GetRootNodeValueByXpath("root", "H1PlateIPAddrss");
                        if (string.IsNullOrEmpty(h1ipadrs))
                        {
                            log.Error("读取不到H1PlateIPAddrss节点值");
                        }
                        else
                        {
                            if (!dicHallsInfo.ContainsKey(h1ipadrs))
                            {
                                dicHallsInfo.Add(h1ipadrs, 11);
                            }
                        }
                        string h2ipadrs = XMLHelper.GetRootNodeValueByXpath("root", "H2PlateIPAddrss");
                        if (string.IsNullOrEmpty(h2ipadrs))
                        {
                            log.Error("读取不到H2PlateIPAddrss节点值");
                        }
                        else
                        {
                            if (!dicHallsInfo.ContainsKey(h2ipadrs))
                            {
                                dicHallsInfo.Add(h2ipadrs, 12);
                            }
                        }
                        #endregion

                        int warehouse = 1;
                        if (dicHallsInfo.ContainsKey(ipaddrs))
                        {
                            int hallID = dicHallsInfo[ipaddrs];
                            await new CWPlate().AddPlateAsync(warehouse, hallID, plate,headpath,triggerType);
                        }
                        else
                        {
                            log.Error("数据字典中找不到对应的IP地址 - "+ipaddrs);
                        }
                    }
                }
                catch (Exception ex)
                {                   
                    log.Error("解析数据异常 - " + ex.ToString());
                }
                             
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }

            CResponse_AlarmInfoPlate resp = new CResponse_AlarmInfoPlate();
            #region 打包响应数据
            ZTriggerImage trImage = new ZTriggerImage
            {
                port = 80,
                snapImageRelativeUrl = "",
                snapImageAbsolutelyUrl = ""
            };

            List<PortData> dataLst = new List<PortData>();
            dataLst.Add(new PortData
            {
                serialChannel = 0,
                data = "",
                dataLen = 0
            });
            dataLst.Add(new PortData
            {
                serialChannel = 1,
                data = "",
                dataLen = 0
            });

            resp.info = "NO";
            resp.channelNum = 0;
            resp.manualTigger = "NO";
            resp.TriggleImage = trImage;
            resp.serialData = dataLst;

            #endregion
            var obj = new
            {
                Response_AlarmInfoPlate = resp
            };
            return Json(obj);
        }

    }
}