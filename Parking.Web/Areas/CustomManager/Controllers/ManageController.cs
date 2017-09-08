using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using System.Threading.Tasks;
using System.Net;
using Parking.Web.Areas.CustomManager.Models;

namespace Parking.Web.Areas.CustomManager.Controllers
{   
    public class ManageController : Controller
    {
        // GET: CustomManager/Manage/ICCard
        public ActionResult ICCard()
        {
            return View();
        }
        
        public ActionResult GetIccdIPAddress()
        {
            string ipaddress = XMLHelper.GetRootNodeValueByXpath("root", "ICCardIPAddress");
            if (string.IsNullOrEmpty(ipaddress))
            {
                return Content("没有配置相关的IP地址,请联系管理员");
            }
            return Content(ipaddress);
        }

        private static object objLocker = new object();
        /// <summary>
        /// 读卡
        /// </summary>
        /// <returns></returns>
        public ContentResult ReadCard()
        {
            Task.Factory.StartNew(() =>
            {
                Log log = LogFactory.GetLogger("ReadCard");
                try
                {
                    string msg = "null";
                    #region 网口的暂不用
                    //string ipaddrs = XMLHelper.GetRootNodeValueByXpath("root", "ICCardIPAddress");
                    //if (!string.IsNullOrEmpty(ipaddrs))
                    //{
                    //    ICCardReader reader = new ICCardReader(ipaddrs);
                    //    if (reader.Connect())
                    //    {
                    //        uint ICNum = 0;
                    //        int nback = reader.GetPhyscard(ref ICNum);
                    //        if (nback == 0)
                    //        {
                    //            msg = ICNum.ToString();
                    //            log.Info("ReadCard 时，物理卡号 - "+msg);
                    //        }
                    //        else
                    //        {
                    //            log.Info("ReadCard 时，找不到卡片!");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        log.Info("ReadCard 时，无法建立连接!");
                    //    }
                    //    reader.Disconnect();
                    //}
                    //else
                    //{
                    //    log.Info("ReadCard 时，IP地址为空!");
                    //}
                    #endregion
                    #region usb刷卡器读卡操作
                    CIcCardRWOne mcIccdObj = new CIcCardRWOne();
                    bool isConn = mcIccdObj.Connect();
                    if (isConn)
                    {
                        uint ICNum = 0;
                        int nback = mcIccdObj.GetPhyscard(ref ICNum);
                        if (nback == 0)
                        {
                            msg = ICNum.ToString();
                            log.Info("ReadCard 时，物理卡号 - " + msg);
                        }
                        else
                        {
                            log.Info("ReadCard 时，找不到卡片!");
                        }
                    }
                    else
                    {
                        log.Info("ReadCard 时，无法建立连接!");
                    }
                    mcIccdObj.Disconnect();
                    #endregion

                    SingleCallback.Instance().WatchICCard(msg);
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                }

            });
            return Content("读卡中...");            
        }

        public ActionResult GetReadCardWay()
        {
            string bgReadcard = XMLHelper.GetRootNodeValueByXpath("root", "BackGroundRdCd");
            return Content(bgReadcard);
        }

        /// <summary>
        /// 异步读卡
        /// </summary>
        /// <returns></returns>
        private Task<ICCard> ReadICCardAsync(string ipaddrs)
        {
            return Task<ICCard>.Factory.StartNew(()=> {
                ICCard iccd = new ICCard { PhysicCode = "0", UserCode = "" };
                #region                
                if (string.IsNullOrEmpty(ipaddrs))
                {
                    return iccd;
                }
                Log log = LogFactory.GetLogger("ReadICCardAsync");
                try
                {
                    IPAddress ip;
                    if (!IPAddress.TryParse(ipaddrs, out ip))
                    {
                        return iccd;
                    }
                    ICCardReader reader = new ICCardReader(ipaddrs);
                    bool nback = reader.Connect();
                    if (nback)
                    {
                        uint physic = 0;
                        int ret = reader.GetPhyscard(ref physic);
                        if (ret == 0)
                        {
                            string physc = physic.ToString();
                            iccd.PhysicCode = physc;
                            ICCard ccd = new CWICCard().Find(ic => ic.PhysicCode == physc);
                            if (ccd != null)
                            {
                                iccd.UserCode = ccd.UserCode;
                            }
                        }
                    }
                    reader.Disconnect();
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                }
                #endregion
                return iccd;
            });           
        }

        /// <summary>
        /// 制卡或修改卡号
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MakeCard()
        {
            string physc = Request.Form["physccode"];
            string code = Request.Form["iccode"];
            Response resp= new CWICCard().MakeICCard(physc, code);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult FindCard()
        {
            string iccode = Request.QueryString["iccode"];
            ICCard iccd = new CWICCard().Find(ic => ic.UserCode == iccode);
            if (iccd != null)
            {
                return Json(iccd,JsonRequestBehavior.AllowGet);
            }
            return Json(new ICCard(),JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 使用明进康刷卡器时，由物理卡号查询用户信息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="isPhysc"></param>
        /// <returns></returns>
        public ActionResult FindCardOfGUI(string iccode, bool isPhysc)
        {
            CWICCard cwiccd = new CWICCard();
            ICCard iccd=null;
            if (isPhysc)
            {
                iccd = cwiccd.Find(ic=>ic.PhysicCode==iccode);
            }
            else
            {
                iccd = cwiccd.Find(ic => ic.UserCode == iccode);
            }
            if (iccd == null)
            {
                iccd = new ICCard();
                if (isPhysc)
                {
                    //如果查找不到该卡号，则查询下存车指纹库内，是否有，有的话，其编号多少
                    SaveCertificate scert = new CWSaveProof().Find(s => s.Proof == iccode);
                    if (scert != null)
                    {
                        iccd.PhysicCode = iccode;
                        iccd.UserCode = scert.SNO.ToString();
                        iccd.Status = EnmICCardStatus.Init;
                        iccd.CreateDate = DateTime.Parse("2017-1-1");
                        iccd.LossDate = DateTime.Parse("2017-1-1");
                        iccd.LogoutDate = DateTime.Parse("2017-1-1");
                    }
                }
            }
            return Json(iccd,JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 操作卡类
        /// </summary>
        /// <param name="iccode"></param>
        /// <param name="type">1：挂失，2：取消挂失，3：注销</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult HandleCard(string iccode,int type)
        {
            CWICCard cwiccd = new CWICCard();     
            ICCard iccd = cwiccd.Find(ic => ic.UserCode == iccode);
            if (iccd == null)
            {
                var data = new Response
                {
                    Code = 0,
                    Message = "找不到该卡"
                };
                return Json(data);
            }
            switch (type)
            {
                case 1:
                    iccd.Status = EnmICCardStatus.Lost;
                    iccd.LossDate = DateTime.Now;
                    break;
                case 2:
                    iccd.Status = EnmICCardStatus.Normal;
                    iccd.LossDate = DateTime.Parse("2017-1-1");
                    break;
                case 3:
                    iccd.Status = EnmICCardStatus.Disposed;
                    iccd.LogoutDate = DateTime.Now;
                    break;
                default:
                    break;
            }
            Response resp = cwiccd.Update(iccd);

            return Json(resp);
        }

        /// <summary>
        /// 获取OCX远程访问时加的地址
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetOCXIPAddress()
        {
            string ipaddrs = "";
            string addrs = XMLHelper.GetRootNodeValueByXpath("root", "OCXIPAddrs");
            if (addrs != null)
            {
                ipaddrs = addrs;
            }
            return Content(ipaddrs);
        }

        public ActionResult GetClientCardIP()
        {
            string ipaddrs = "";
            string addrs = XMLHelper.GetRootNodeValueByXpath("root", "ICCardIPAddress");
            if (addrs != null)
            {
                ipaddrs = addrs;
            }
            return Content(ipaddrs);
        }

    }
}