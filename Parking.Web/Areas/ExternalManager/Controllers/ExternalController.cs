using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Data;
using Parking.Core;
using Parking.Auxiliary;
using Newtonsoft.Json;

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

        /// <summary>
        /// APP,扫码或其它存车,
        /// </summary>
        /// <returns></returns>
        public Response SaveCarInterface()
        {
            Response resp = new Response();
            #region
            Log log = LogFactory.GetLogger("SaveCarInterface");
            try
            {
                string warehouse = Request.QueryString["warehouse"];
                string hallID = Request.QueryString["hallID"];
                string iccode = Request.QueryString["iccode"];
                string plate = Request.QueryString["platenum"];
                if (string.IsNullOrEmpty(warehouse) || string.IsNullOrEmpty(hallID))
                {                    
                    log.Error("APP存车，hallID为空或warehouse为空！");
                    resp.Message = "参数错误";
                    return resp;
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
                    return resp;
                }
                if (moHall.Mode != EnmModel.Automatic)
                {
                    log.Error("APP存车时， 车厅不是全自动. warehouse - " + warehouse + " ,hallID - " + hallID);
                    resp.Message = "车厅不是全自动";
                    return resp;
                }
                if (moHall.HallType == EnmHallType.Entrance ||
                    moHall.HallType == EnmHallType.EnterOrExit)
                {
                    if (moHall.TaskID == 0)
                    {
                        //车厅无车，不能存车
                        log.Error("APP存车时， 车厅无车，不能存车. ");
                        resp.Message = "车厅无车，不能存车";
                        return resp;
                    }
                    ImplementTask tsk = motsk.Find(moHall.TaskID);
                    if (tsk == null)
                    {
                        log.Error("APP存车时， 依车厅TaskID找不到作业信息，TaskID-" + moHall.TaskID + "  hallCode-" + moHall.DeviceCode);
                        //系统故障
                        resp.Message = "系统异常，找不到作业";
                        return resp;
                    }
                    if (tsk.Status != EnmTaskStatus.ICarInWaitFirstSwipeCard)
                    {
                        log.Error("APP存车时，不是处于刷卡阶段");
                        resp.Message = "不是处于刷卡阶段";
                        return resp;
                    }
                    if (string.IsNullOrEmpty(iccode))
                    {
                        iccode = "444";
                    }
                    //更新作业状态为第二次刷卡，启动流程
                    motsk.DealISwipedSecondCard(tsk, iccode);
                    resp.Code = 1;
                    resp.Message = "流程进行中";
                    return resp;
                }
                else
                {
                    log.Error("APP存车时，不是进（进出）车厅");
                    resp.Message = "不是进（进出）车厅";
                    return resp;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                resp.Message = "系统异常";
            }
            #endregion
            return resp;
        }
        
        /// <summary>
        /// APP,远程取车
        /// </summary>
        /// <returns></returns>
        public Response RemoteGetCarInterface()
        {
            Response resp = new Response();
            #region


            #endregion
            return resp;
        }

        /// <summary>
        /// APP,预定、取消预定车位
        /// </summary>
        /// <returns></returns>
        public Response RemoteBookLoc()
        {
            Response resp = new Response();
            #region


            #endregion
            return resp;
        }

    }
}