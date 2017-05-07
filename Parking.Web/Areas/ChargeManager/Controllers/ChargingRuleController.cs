using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;

namespace Parking.Web.Areas.ChargeManager.Controllers
{
    public class ChargingRuleController : Controller
    {

        // GET: ChargeManager/ChargingRule
        public ActionResult Index()
        {
            return View();
        }

        #region 预付类
        /// <summary>
        /// 预付类规则
        /// </summary>
        /// <returns></returns>
        public ActionResult PreRule()
        {
            return View();
        }

        [HttpPost]
        public JsonResult FindPreRuleList(int pageSize,int pageIndex, string sortOrder, string sortName)
        {
            Page<PreCharging> page = new CWTariff().FindPreRulePageList(pageSize, pageIndex, sortOrder, sortName);
            var data = new {
                total = page.TotalNumber,
                rows = page.ItemLists
            };
            return Json(data);
        }

        public JsonResult AddPre()
        {
            string cunit = Request.QueryString["cunit"];
            string cnum = Request.QueryString["cnum"];
            string cfee = Request.QueryString["cfee"];
           
            PreCharging prechg = new PreCharging {
                CycleUnit = (EnmCycleUnit)Convert.ToInt16(cunit),
                CycleNum = Convert.ToInt32(cnum),
                Fee=Convert.ToSingle(cfee)
            };
            Response resp = new CWTariff().AddPreCharge(prechg);
            return Json(resp,JsonRequestBehavior.AllowGet);
        }

        public JsonResult ModifyPre()
        {
            string ID = Request.QueryString["cID"];
            string cunit = Request.QueryString["cunit"];
            string cnum = Request.QueryString["cnum"];
            string cfee = Request.QueryString["cfee"];
            Response resp = new Response();
            if (!string.IsNullOrEmpty(ID))
            {
                PreCharging prechg = new CWTariff().FindPreCharge(int.Parse(ID));
                if (prechg != null)
                {
                    prechg.CycleUnit = (EnmCycleUnit)Convert.ToInt16(cunit);
                    prechg.CycleNum = Convert.ToInt32(cnum);
                    prechg.Fee = Convert.ToSingle(cfee);
                    resp = new CWTariff().UpdatePreCharge(prechg);
                }
            }        
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletePre(int ID)
        {
            CWTariff cwtariff = new CWTariff();
            //同时删除计费绑定的
            TempChargingRule temp = cwtariff.FindTempChgRule(tp => tp.PreChgID == ID);
            if (temp != null)
            {
                temp.PreChgID = 0;
                cwtariff.UpdateTempChgRule(temp);
            }
            FixChargingRule fix = cwtariff.FindFixCharge(fx => fx.PreChgID == ID);
            if (fix != null)
            {
                fix.PreChgID = 0;
                cwtariff.UpdateFixCharge(fix);
            }
            Response resp = cwtariff.DeletePreCharge(ID);

            return Json(resp, JsonRequestBehavior.AllowGet);

        }
        #endregion
        /// <summary>
        /// 固定类规则
        /// </summary>
        /// <returns></returns>
        public ActionResult FixRule()
        {
            return View();
        }
        /// <summary>
        /// 临时类规则
        /// </summary>
        /// <returns></returns>
        public ActionResult TempRule()
        {
            return View();
        }

    }
}