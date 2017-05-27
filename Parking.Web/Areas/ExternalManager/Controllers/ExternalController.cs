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

            Log log = LogFactory.GetLogger("FingerPrint");
            log.Info("Warehouse-" + warehouse + " ,Hall-" + hallID + " , FingerPrint Info- " + fingerPrint);

            Response resp = new Response();
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
                resp.Message = "车厅号不正确，hallID- "+hallID;
                return Json(resp);
            }

            string[] arrayFinger = fingerPrint.Trim().Split(' ');
            byte[] psTZ = new byte[arrayFinger.Length];
            for (int i = 0; i < arrayFinger.Length; i++)
            {
                psTZ[i] = Convert.ToByte(arrayFinger[i]);
            }

            resp = new CWTaskTransfer(wh, hall).DealFingerPrintMessage(psTZ);           
            return Json(resp);
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

            Response resp = new Response();
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
            resp = new CWTaskTransfer(wh, hall).DealFingerICCardMessage(ccode);           
            return Json(resp);
        }

    }
}