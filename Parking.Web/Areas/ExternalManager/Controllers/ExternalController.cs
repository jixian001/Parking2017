using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Data;
using Parking.Core;
using Parking.Auxiliary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Parking.Web.Areas.ExternalManager.Controllers
{
    public class ExternalController : Controller
    {
        // GET: ExternalManager/External      
        public ActionResult GetCurrentSound(int warehouse, int devicecode)
        {
            Log log = LogFactory.GetLogger("GetCurrentSound");
            try
            {
                string sound = new CWTask().GetNotification(warehouse, devicecode);
                return Content(sound);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return Content("null");
        }

        [HttpPost]
        /// <summary>
        /// APP,扫码或其它存车,
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveCarInterface()
        {
            Response resp = new Response();
            #region
            Log log = LogFactory.GetLogger("SaveCarInterface");
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);
                //显示，记录
                log.Info(req);
                JObject jo = (JObject)JsonConvert.DeserializeObject(req);

                string warehouse = jo["warehouse"].ToString();
                string hallID = jo["hallID"].ToString();
                string iccode = jo["iccode"].ToString();
                string plate = jo["platenum"].ToString();

                if (string.IsNullOrEmpty(warehouse) || string.IsNullOrEmpty(hallID))
                {                    
                    log.Error("APP存车，hallID为空或warehouse为空！");
                    resp.Message = "参数错误";
                    return Json(resp);
                }
                int wh = Convert.ToInt32(warehouse);
                int code = Convert.ToInt32(hallID);

                CWDevice cwdevice = new CWDevice();
                CWTask motsk = new CWTask();

                Device moHall = cwdevice.Find(dev=>dev.Warehouse==wh&&dev.DeviceCode==code);
                if (moHall == null)
                {
                    log.Error("APP存车时， 找不到车厅设备. warehouse - "+warehouse+" ,hallID - "+hallID);
                    resp.Message = "找不到车厅";
                    return Json(resp);
                }
                if (moHall.Mode != EnmModel.Automatic)
                {
                    log.Error("APP存车时， 车厅不是全自动. warehouse - " + warehouse + " ,hallID - " + hallID);
                    resp.Message = "车厅不是全自动";
                    return Json(resp);
                }
                if (moHall.HallType == EnmHallType.Entrance ||
                    moHall.HallType == EnmHallType.EnterOrExit)
                {
                    if (moHall.TaskID == 0)
                    {
                        //车厅无车，不能存车
                        log.Error("APP存车时， 车厅无车，不能存车. ");
                        resp.Message = "车厅无车，不能存车";
                        return Json(resp);
                    }
                    ImplementTask tsk = motsk.Find(moHall.TaskID);
                    if (tsk == null)
                    {
                        log.Error("APP存车时， 依车厅TaskID找不到作业信息，TaskID-" + moHall.TaskID + "  hallCode-" + moHall.DeviceCode);
                        //系统故障
                        resp.Message = "系统异常，找不到作业";
                        return Json(resp);
                    }
                    if (tsk.Status != EnmTaskStatus.ICarInWaitFirstSwipeCard)
                    {
                        log.Error("APP存车时，不是处于刷卡阶段");
                        resp.Message = "不是处于刷卡阶段";
                        return Json(resp);
                    }
                    if (string.IsNullOrEmpty(iccode))
                    {
                        iccode = "444";
                    }
                    //更新作业状态为第二次刷卡，启动流程
                    motsk.DealISwipedSecondCard(tsk, iccode);
                    resp.Code = 1;
                    resp.Message = "流程进行中";
                    return Json(resp);
                }
                else
                {
                    log.Error("APP存车时，不是进（进出）车厅");
                    resp.Message = "不是进（进出）车厅";
                    return Json(resp);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                resp.Message = "系统异常";
            }
            #endregion
            return Json(resp);
        }

        [HttpPost]
        /// <summary>
        /// APP,远程取车
        /// </summary>
        /// <returns></returns>
        public ActionResult RemoteGetCarInterface()
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("RemoteGetCarInterface");
            #region
            try
            {
                CWDevice cwdevice = new CWDevice();

                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);
                //显示，记录
                log.Info(req);
                JObject jo = (JObject)JsonConvert.DeserializeObject(req);
                string warehouse = jo["warehouse"].ToString();
                string hallID = jo["hallID"].ToString();
                string plate = jo["plateNum"].ToString();
                string iccode = jo["iccode"].ToString();

                if (string.IsNullOrEmpty(warehouse) ||
                   string.IsNullOrEmpty(hallID) ||
                   string.IsNullOrEmpty(plate))
                {
                    log.Error("参数错误,库区、车厅号或车牌号有为空的！");
                    resp.Message = "参数错误";
                    return Json(resp);
                }
                int wh = Convert.ToInt32(warehouse);
                int hcode = Convert.ToInt32(hallID);

                Device moHall = cwdevice.Find(d => d.Warehouse == wh && d.DeviceCode == hcode);
                if (moHall == null)
                {
                    log.Error("APP取车时，查找不到车厅，warehouse- " + warehouse + " ,hallID- " + hallID);
                    resp.Message = "找不到车厅";
                    return Json(resp);
                }
                if (moHall.Mode != EnmModel.Automatic)
                {
                    log.Error("APP取车时，车厅模式不是全自动 , Mode - " + moHall.Mode.ToString());
                    resp.Message = "车厅不是全自动";
                    return Json(resp);
                }
                if (moHall.HallType == EnmHallType.Entrance)
                {
                    log.Error("APP取车时，车厅不是出车厅,halltype - " + moHall.HallType.ToString());
                    resp.Message = "车厅不是全自动";
                    return Json(resp);
                }
                Location loc = new CWLocation().FindLocation(lc => lc.PlateNum == plate);
                if (loc == null)
                {
                    log.Error("APP取车时，找不到取车位,plate - " + plate);
                    resp.Message = "没有存车";
                    return Json(resp);
                }
                CWTask motsk = new CWTask();
                ImplementTask task = motsk.Find(tk => tk.ICCardCode == loc.ICCode && tk.IsComplete == 0);
                if (task != null)
                {
                    log.Error("APP取车时，车位正在作业，iccode - " + loc.ICCode + " ,plate - " + plate);
                    resp.Message = "正在作业";
                    return Json(resp);
                }
                WorkTask queue = motsk.FindQueue(qu => qu.ICCardCode == loc.ICCode);
                if (queue != null)
                {
                    log.Error("APP取车时，已经加入队列，iccode - " + loc.ICCode + " ,plate - " + plate);
                    resp.Message = "已经加入队列";
                    return Json(resp);
                }
                if (loc.Type != EnmLocationType.Normal)
                {
                    log.Error("APP取车时，车位已被禁用，address - " + loc.Address);
                    resp.Message = "车位已被禁用";
                    return Json(resp);
                }
                resp = motsk.DealOSwipedCard(moHall, loc);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Message = "系统异常";
            }
            #endregion
            return Json(resp);
        }

        [HttpPost]
        /// <summary>
        /// APP,预定、取消预定车位
        /// </summary>
        /// <returns></returns>
        public ActionResult RemoteBookLoc()
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("RemoteBookLoc");
            #region
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);
                //显示，记录
                log.Info(req);
                JObject jo = (JObject)JsonConvert.DeserializeObject(req);
                string type = jo["ptype"].ToString();
                string wh = jo["warehouse"].ToString();
                string proof = jo["proof"].ToString();
                string plate = jo["platenum"].ToString();
                if (string.IsNullOrEmpty(proof) ||                   
                    string.IsNullOrEmpty(type))
                {
                    log.Error("参数错误,车位或类型 有为空的！");
                    resp.Message = "参数错误";
                    return Json(resp);
                }
                int deftype = Convert.ToInt32(type);
                //车位预定
                if (deftype == 3)
                {
                    if (string.IsNullOrEmpty(plate))
                    {
                        log.Error("参数错误：车位预定时，车牌为空的！");
                        resp.Message = "参数错误";
                        return Json(resp);
                    }
                }

                int warehouse = 1;
                if (!string.IsNullOrEmpty(wh))
                {
                    warehouse = Convert.ToInt32(warehouse);
                }
                string[] arr = proof.Split('(');
                if (arr.Length < 2)
                {
                    log.Error("车位参数错误,proof- " + proof);
                    resp.Message = "车位参数错误";
                    return Json(resp);
                }
                string address = arr[0];
                string size = arr[1].Substring(0, 3);

                CWLocation cwlctn = new CWLocation();
                Location loc = cwlctn.FindLocation(lc=>lc.Warehouse==warehouse&&lc.Address==address);
                if (loc == null)
                {
                    log.Error("APP预定时，找不到取车位,plate - " + plate);
                    resp.Message = "没有存车";
                    return Json(resp);
                }
                if (loc.Type != EnmLocationType.Normal)
                {
                    log.Error("APP预定时，不是正常车位，address- " + address);
                    resp.Message = "当前车位不允许操作";
                    return Json(resp);
                }

                //车位预定
                if (deftype == 3)
                {
                    if (loc.Status != EnmLocationStatus.Space)
                    {
                        log.Error("APP预定时，不是空闲车位，address- " + address);
                        resp.Message = "当前车位不允许操作";
                        return Json(resp);

                    }
                    loc.Status = EnmLocationStatus.Book;
                    loc.PlateNum = plate;
                    loc.InDate = DateTime.Now;
                    cwlctn.UpdateLocation(loc);

                    resp.Code = 1;
                    resp.Message = "预定成功";                   
                }
                else if (deftype == 4)
                {
                    //车位取消
                    if (loc.Status != EnmLocationStatus.Book)
                    {
                        log.Error("APP取消预定时，该车位没有被预定，address- " + address + " ,status - " + loc.Status.ToString());
                        resp.Message = "当前车位操作失败";
                        return Json(resp);

                    }
                    loc.Status = EnmLocationStatus.Space;
                    loc.PlateNum = "";
                    loc.InDate = DateTime.Parse("2017-7-4");
                    cwlctn.UpdateLocation(loc);

                    resp.Code = 1;
                    resp.Message = "取消预定成功";                   
                }
                else
                {
                    resp.Message = "接口类型不正确，type- " + type;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Message = "系统异常";
            }
            #endregion
            return Json(resp);
        }

    }
}