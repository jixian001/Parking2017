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
using Parking.Web.Areas.ExternalManager.Models;

namespace Parking.Web.Areas.ExternalManager.Controllers
{
    public class ExternalController : Controller
    {
        // GET: ExternalManager/External      
        public ActionResult GetCurrentSound()
        {
            Log log = LogFactory.GetLogger("GetCurrentSound");
            try
            {
                string wh = Request.QueryString["warehouse"];
                string code = Request.QueryString["devicecode"];

                if (string.IsNullOrEmpty(wh) ||
                    string.IsNullOrEmpty(code))
                {
                    log.Error("传输出现错误，参数为空！");
                    return Content("null");
                }
                int warehouse = Convert.ToInt32(wh);
                int devicecode = Convert.ToInt32(code);

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
                string req = System.Text.Encoding.UTF8.GetString(bytes);
                //显示，记录
                log.Info(req);
                JObject jo = (JObject)JsonConvert.DeserializeObject(req);

                string warehouse = jo["warehouse"].ToString();
                string hallID = jo["hallID"].ToString();
                string iccode = jo["iccode"].ToString();
                string plate = jo["plateNum"].ToString();

                if (string.IsNullOrEmpty(hallID))
                {
                    log.Error("APP存车，hallID为空！");
                    resp.Message = "参数错误，hallID为空！";
                    return Json(resp);
                }
                if (string.IsNullOrEmpty(plate))
                {
                    log.Error("APP存车，车牌号为空！");
                    resp.Message = "参数错误，车牌号为空！";
                    return Json(resp);
                }

                int wh = 1;
                int code = Convert.ToInt32(hallID);

                CWDevice cwdevice = new CWDevice();
                CWTask motsk = new CWTask();

                Device moHall = cwdevice.Find(dev => dev.Warehouse == wh && dev.DeviceCode == code);
                if (moHall == null)
                {
                    log.Error("APP存车时， 找不到车厅设备. warehouse - " + warehouse + " ,hallID - " + hallID);
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
                    if (tsk.Status != EnmTaskStatus.ICarInWaitFirstSwipeCard &&
                        tsk.Status != EnmTaskStatus.TempOCarOutWaitforDrive)
                    {
                        log.Error("APP存车时，不是处于刷卡阶段");
                        resp.Message = "不是处于刷卡阶段";
                        return Json(resp);
                    }
                    if (tsk.PlateNum != plate)
                    {
                        log.Error("APP存车时，入库识别车牌与给定车牌不一致");
                        resp.Message = "APP绑定车牌与车辆车牌不一致";
                        return Json(resp);
                    }

                    CWICCard cwiccd = new CWICCard();

                    if (tsk.Type == EnmTaskType.SaveCar)
                    {
                        string physiccode = "1234567890";
                        Customer cust = cwiccd.FindCust(cc => cc.PlateNum == plate);
                        if (cust != null)
                        {
                            ICCard iccd = cwiccd.Find(ic => ic.CustID == cust.ID);
                            if (iccd != null)
                            {
                                iccode = iccd.UserCode;
                                physiccode = iccd.PhysicCode;
                            }
                        }
                        CWSaveProof cwsaveproof = new CWSaveProof();
                        if (string.IsNullOrEmpty(iccode))
                        {
                            iccode = cwsaveproof.GetMaxSNO().ToString();
                        }

                        SaveCertificate scert = new SaveCertificate();
                        scert.IsFingerPrint = 2;
                        scert.Proof = physiccode;
                        scert.SNO = Convert.ToInt32(iccode);
                        //添加凭证到存车库中
                        Response respe = cwsaveproof.Add(scert);

                        tsk.PlateNum = plate;
                        //更新作业状态为第二次刷卡，启动流程
                        motsk.DealISwipedSecondCard(tsk, iccode);

                        resp.Code = 1;
                        resp.Message = "流程进行中";
                        return Json(resp);
                    }
                    else if (tsk.Type == EnmTaskType.TempGet)
                    {
                        motsk.DealAPPSwipeThreeCard(tsk);
                    }
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
        /// APP,远程取车,只允许使用车牌取车
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
                string req = System.Text.Encoding.UTF8.GetString(bytes);
                //显示，记录
                log.Info(req);
                JObject jo = (JObject)JsonConvert.DeserializeObject(req);
                string warehouse = jo["warehouse"].ToString();
                string hallID = jo["hallID"].ToString();
                string plate = jo["plateNum"].ToString();

                if (string.IsNullOrEmpty(plate))
                {
                    log.Error("车牌号为空的！");
                    resp.Message = "参数错误";
                    return Json(resp);
                }
                #region 暂不用
                //int wh = Convert.ToInt32(warehouse);
                //int hcode = Convert.ToInt32(hallID);

                //Device moHall = cwdevice.Find(d => d.Warehouse == wh && d.DeviceCode == hcode);
                //if (moHall == null)
                //{
                //    log.Error("APP取车时，查找不到车厅，warehouse- " + warehouse + " ,hallID- " + hallID);
                //    resp.Message = "找不到车厅";
                //    return Json(resp);
                //}
                //if (moHall.Mode != EnmModel.Automatic)
                //{
                //    log.Error("APP取车时，车厅模式不是全自动 , Mode - " + moHall.Mode.ToString());
                //    resp.Message = "车厅不是全自动";
                //    return Json(resp);
                //}
                //if (moHall.HallType == EnmHallType.Entrance)
                //{
                //    log.Error("APP取车时，车厅不是出车厅,halltype - " + moHall.HallType.ToString());
                //    resp.Message = "车厅不是全自动";
                //    return Json(resp);
                //}
                #endregion
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
                if (loc.Status != EnmLocationStatus.Occupy)
                {
                    log.Error("APP取车时，当前车位正在任务业，address - " + loc.Address+" ,status - "+loc.Status.ToString());
                    resp.Message = "车位已被禁用";
                    return Json(resp);
                }
                int mcount = new CWTask().FindQueueList(q => q.IsMaster == 2).Count();
                if (mcount > 6)
                {
                    log.Error("APP取车时，取车队列已满");
                    resp.Message = "取车队列已满";
                    return Json(resp);
                }

                //分配车厅
                int hallcode = new CWDevice().AllocateHall(loc, false);
                Device moHall = new CWDevice().Find(d => d.DeviceCode == hallcode);

                if (moHall != null)
                {
                    resp = motsk.DealOSwipedCard(moHall, loc);

                    log.Info("APP取车，取车车位 - "+loc.Address+" ,出车车厅 - "+moHall.DeviceCode);
                }
                else
                {
                    resp.Message = "找不到合适车厅";
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

        /// <summary>
        /// APP，取消取车,暂没有
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RemoteCancelGetCarIFace()
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("RemoteCancelGetCarIFace");
            #region 取消取车
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.UTF8.GetString(bytes);
                //显示，记录
                log.Info(req);
                JObject jo = (JObject)JsonConvert.DeserializeObject(req);
                string iccode = jo["iccode"].ToString();
                string plate = jo["platenum"].ToString();

                CWTask motask = new CWTask();
                CWLocation cwlctn = new CWLocation();
                #region 不用
                //if (!string.IsNullOrEmpty(iccode))
                //{
                //    WorkTask mtsk = motask.FindQueue(mt => mt.ICCardCode == iccode);
                //    if (mtsk != null)
                //    {
                //        Location loc = cwlctn.FindLocation(lc => lc.ICCode == iccode);
                //        loc.Status = EnmLocationStatus.Occupy;
                //        cwlctn.UpdateLocation(loc);

                //        resp = motask.DeleteQueue(mtsk.ID);
                //        return Json(resp);
                //    }
                //}
                #endregion
                if (!string.IsNullOrEmpty(plate))
                {
                    Location loc = cwlctn.FindLocation(lc => lc.PlateNum == plate);
                    if (loc != null)
                    {
                        WorkTask mtsk = motask.FindQueue(mt => mt.ICCardCode == loc.ICCode);
                        if (mtsk != null)
                        {
                            loc.Status = EnmLocationStatus.Occupy;
                            cwlctn.UpdateLocation(loc);

                            motask.DeleteQueue(mtsk.ID);
                            resp.Code = 1;
                            resp.Message = "取消成功";
                            return Json(resp);
                        }
                        else
                        {
                            resp.Code = 1;
                            resp.Message = "当前车辆还没有取车";
                        }
                    }
                    else
                    {
                        resp.Message = "车辆不在库里";
                    }
                }
                else
                {
                    resp.Message = "车牌不允许为空";
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
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
                string req = System.Text.Encoding.UTF8.GetString(bytes);
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
                    log.Error("参数错误,车位尺寸或接口类型 有为空的！");
                    resp.Message = "参数错误";
                    return Json(resp);
                }
                if (string.IsNullOrEmpty(plate))
                {
                    log.Error("参数错误：车位预定时，车牌为空的！");
                    resp.Message = "参数错误";
                    return Json(resp);
                }
                CWLocation cwlctn = new CWLocation();
               
                int deftype = Convert.ToInt32(type);
                //车位预定
                if (deftype == 3)
                {
                    Location lctn = cwlctn.FindLocation(l => l.PlateNum == plate);
                    if (lctn != null)
                    {
                        if (lctn.Status == EnmLocationStatus.Book)
                        {
                            resp.Message = "当前车辆已预约";
                        }
                        else
                        {
                            resp.Message = "当前车辆已存车";
                        }
                        log.Error("当前车辆已存在，不允许预约！");
                        return Json(resp);
                    }

                    string checkcode = "122";
                    if (proof == "111")
                    {
                        checkcode = "121";
                    }
                    Location loc = new AllocateLocByBook().AllocateLoc(checkcode);
                    if (loc != null)
                    {
                        loc.Status = EnmLocationStatus.Book;
                        loc.PlateNum = plate;
                        loc.InDate = DateTime.Now;
                        cwlctn.UpdateLocation(loc);

                        resp.Code = 1;
                        resp.Message = "预定成功";

                        log.Info("预定成功, checkcode - " + proof + " ,address - " + loc.Address + " ,locsize - " + loc.LocSize);
                    }
                    else
                    {
                        resp.Message = "找不到合适车位";
                    }
                }
                else if (deftype == 4)
                {
                    Location loc = cwlctn.FindLocation(lc => lc.PlateNum == plate && lc.Status == EnmLocationStatus.Book);
                    if (loc != null)
                    {
                        loc.Status = EnmLocationStatus.Space;
                        loc.PlateNum = "";
                        loc.InDate = DateTime.Parse("2017-1-1");
                        cwlctn.UpdateLocation(loc);

                        resp.Code = 1;
                        resp.Message = "取消预定成功";
                    }
                    else
                    {
                        resp.Message = "车辆没有预定车位";
                    }
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

        /// <summary>
        /// 云服务请求临时取物
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RemoteTempGetInterface()
        {
            Response resp = new Response();
            #region
            Log log = LogFactory.GetLogger("RemoteTempGetInterface");
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.UTF8.GetString(bytes);
                //显示，记录
                log.Info(req);
                JObject jo = (JObject)JsonConvert.DeserializeObject(req);
               
                string plate = jo["platenum"].ToString();
                if (string.IsNullOrEmpty(plate))
                {
                    log.Info("参数错误,车牌为空的！");
                    resp.Message = "参数错误";
                    return Json(resp);
                }
                Location loc = new CWLocation().FindLocation(lc => lc.PlateNum == plate);
                if (loc == null)
                {
                    log.Error("APP取物时，找不到取车位，plate - " + plate);
                    resp.Message = "没有存车";
                    return Json(resp);
                }
                CWTask motsk = new CWTask();
                ImplementTask task = motsk.Find(tk => tk.ICCardCode == loc.ICCode);
                if (task != null)
                {
                    log.Error("APP取物时，车位正在作业，iccode - " + loc.ICCode + " ,plate - " + plate);
                    resp.Message = "正在作业";
                    return Json(resp);
                }
                WorkTask queue = motsk.FindQueue(qu => qu.ICCardCode == loc.ICCode);
                if (queue != null)
                {
                    log.Error("APP取物时，已经加入队列，iccode - " + loc.ICCode + " ,plate - " + plate);
                    resp.Message = "已经加入队列";
                    return Json(resp);
                }
                if (loc.Type != EnmLocationType.Normal)
                {
                    log.Error("APP取物时，车位已被禁用，address - " + loc.Address);
                    resp.Message = "车位已被禁用";
                    return Json(resp);
                }
                //分配车厅
                int hallcode = new CWDevice().AllocateHall(loc, false);

                Device moHall = new CWDevice().Find(d => d.DeviceCode == hallcode);
                if (moHall != null)
                {
                    resp = motsk.TempGetCar(moHall, loc); 
                }
                else
                {
                    resp.Message = "找不到合适车厅";
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

        /// <summary>
        /// 查询停车费用
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult QueryParkingFee()
        {
            ParkingFeeInfo resp = new ParkingFeeInfo();
            #region
            Log log = LogFactory.GetLogger("QueryParkingFee");
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.UTF8.GetString(bytes);
                //显示，记录
                log.Info(req);
                JObject jo = (JObject)JsonConvert.DeserializeObject(req);
                string iccode = jo["iccode"].ToString();
                string plate = jo["plateNum"].ToString();

                CWLocation cwlctn = new CWLocation();
                Location loc = null;
                if (!string.IsNullOrEmpty(plate))
                {
                    loc = cwlctn.FindLocation(lc => lc.PlateNum == plate);
                }
                if (loc == null)
                {
                    if (!string.IsNullOrEmpty(iccode))
                    {
                        loc = cwlctn.FindLocation(lc => lc.ICCode == iccode);
                    }
                }
                if (loc == null)
                {
                    log.Error("APP查询费用时, 找不到取车位, plate - " + plate + " ,iccode-" + iccode);
                    resp.Message = "没有存车";
                    return Json(resp);
                }
                float fee = 0;
                Response res = new CWTariff().CalculateTempFee(loc.InDate, DateTime.Now, out fee);
                if (res.Code == 1)
                {
                    resp.Code = 1;
                    resp.Message = "查询费用成功";
                    resp.Fee = fee;
                    resp.InDtime = loc.InDate.ToString();
                    resp.OutDtime = DateTime.Now.ToString();
                }
                else
                {
                    log.Error("APP查询费用,系统异常- " + resp.Message);
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

        /// <summary>
        /// 厅外刷卡上报
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ReadICCard()
        {
            Log log = LogFactory.GetLogger("External.ReadICCard");
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);
                //显示，记录
                log.Info(req);
                JObject jo = (JObject)JsonConvert.DeserializeObject(req);
                string wh = jo["warehouse"].ToString();
                string code = jo["hallID"].ToString();
                string physcode = jo["physcode"].ToString();

                if (string.IsNullOrEmpty(wh) ||
                   string.IsNullOrEmpty(code))
                {
                    log.Error("传输出现错误，设备参数为空！");
                    return Content("fail");
                }
                if (string.IsNullOrEmpty(physcode))
                {
                    log.Error("传输出现错误，物理卡号为空！");
                    return Content("fail");
                }
                int warehouse = Convert.ToInt32(wh);
                int hallID = Convert.ToInt32(code);

                new CWTaskTransfer(hallID, warehouse).DealICCardMessage(physcode);

                return Content("success");
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("fail");
        }
    }
}