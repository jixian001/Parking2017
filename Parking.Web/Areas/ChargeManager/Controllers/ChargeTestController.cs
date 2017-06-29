using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Core;

namespace Parking.Web.Areas.ChargeManager.Controllers
{
    public class ChargeTestController : Controller
    {
        // GET: ChargeManager/ChargeTest
        public ActionResult Index()
        {
            return View();
        }
        
        public JsonResult CalculateFee()
        {
            Log log = LogFactory.GetLogger("CalculateFee");
            Response resp = new Response();
            try
            {
                var start = Request.QueryString["indtime"].ToString();
                var end = Request.QueryString["outdtime"].ToString();
                DateTime indate = DateTime.Parse(start);
                DateTime outdate = DateTime.Parse(end);
                float cfee = 0;
                resp= new CWTariff().CalculateTempFee(indate, outdate, out cfee);
                if (resp.Code == 1)
                {
                    TimeSpan ts = outdate - indate;
                    string msg = ts.Days + " 天 " + ts.Hours + " 小时 " + ts.Minutes + " 分 " + ts.Seconds + " 秒";
                    var nback = new {
                        fee = cfee,
                        msg = msg
                    };
                    return Json(nback, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Message = "系统异常";
            }
            var data = new {
                fee="0",
                msg=resp.Message
            };
            return Json(data,JsonRequestBehavior.AllowGet);
        }


    }
}