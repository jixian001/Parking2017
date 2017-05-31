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
        
        /// <summary>
        /// 读卡,异步方式
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ReadCard()
        {
            //多线程下HttpContext.Current.Server为空
            string ipaddrs = XMLHelper.GetRootNodeValueByXpath("root", "ICCardIPAddress"); 
            var iccd = await ReadICCardAsync(ipaddrs);
            var data = new {
                code = 1,
                physccode = iccd.PhysicCode,
                iccode=iccd.UserCode
            };
            return Json(data, JsonRequestBehavior.AllowGet);
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
                IPAddress ip;
                if(!IPAddress.TryParse(ipaddrs,out ip))
                {
                    return iccd;
                }
                ICCardReader reader = new ICCardReader(ipaddrs);
                bool nback = reader.Connect();
                if (nback)
                {
                    uint physic = 0;
                    int ret= reader.GetPhyscard(ref physic);
                    if (ret == 0)
                    {
                        iccd.PhysicCode = physic.ToString();
                        ICCard ccd = new CWICCard().Find(ic=>ic.PhysicCode==physic.ToString());
                        if (ccd != null)
                        {
                            iccd = ccd;
                        }
                    }
                }
                reader.Disconnect();
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
    }
}