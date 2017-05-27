using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Data;
using Parking.Core;
using Parking.Auxiliary;

namespace Parking.Web.Areas.ExternalManager.Controllers
{
    public class ExternalController : Controller
    {
        // GET: ExternalManager/External      
        public ActionResult GetCurrentSound(int warehouse, int devicecode)
        {
            string sound = new CWTask().GetNotification(warehouse, devicecode);
            return Content(sound);
        }

        /// <summary>
        /// 接收指纹一体机上传上来的指纹信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SubmitFingerPrint()
        {
            string warehouse = Request.Form["warehouse"];
            string hallID = Request.Form["hallID"];
            string fingerPrint = Request.Form["fingerInfo"];


            var data = new
            {
                Status = "",
                Message = "success"
            };
            return Json(data);
        }

        /// <summary>
        /// 接收指纹一体机上传上来的刷卡信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SubmitCardsInfo()
        {
            string warehouse = Request.Form["warehouse"];
            string hallID = Request.Form["hallID"];
            string ccode = Request.Form["physcode"];



            var data = new
            {
                Status = "",
                Message = "success"
            };
            return Json(data);
        }

    }
}