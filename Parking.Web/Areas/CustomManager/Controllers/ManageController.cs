using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
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
        
        public ActionResult ReadCard()
        {
            var data = new {
                code = 1,
                physccode = "0",
                iccode="0"
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult MakeCard()
        {
            string physc = Request.Form["physccode"];
            string code = Request.Form["iccode"];
            Response resp = new Response() { Code=1,Message="Success"};
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

        public ActionResult ChangeDeadline(string iccode)
        {
            CWICCard cwiccd = new CWICCard();
            ChangeDeadlineModel model = new ChangeDeadlineModel();
            ICCard iccd = cwiccd.Find(ic => ic.UserCode == iccode);            
            if (iccd != null)
            {
                model.ID = iccd.ID;
                model.ICCode = iccd.UserCode;
                model.OldDeadline = iccd.Deadline;
                model.Type = (int)iccd.Type;
                if (model.Type < 2)
                {
                    ModelState.AddModelError("", "临时卡，无法设置使用期限");
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("ICCard");
            }           
        }

        [HttpPost]
        public ActionResult ChangeDeadline(ChangeDeadlineModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "参数设置不正确");
                return View(model);
            }
            if (model.Type < 2)
            {
                ModelState.AddModelError("", "临时卡，无法设置使用期限");
                return View(model);
            }
            CWICCard cwiccd = new CWICCard();
            ICCard iccd = cwiccd.Find(ic => ic.UserCode == model.ICCode);
            if (iccd != null)
            {
                iccd.Deadline = model.NewDeadline;
                Response resp = cwiccd.Update(iccd);
                if (resp.Code == 1)
                {
                    return RedirectToAction("ICCard");
                }
                ModelState.AddModelError("", resp.Message);
            }
            return View(model);
        }

    }
}