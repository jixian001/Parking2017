using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Parking.Core;
using Parking.Data;
using Parking.Auxiliary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parking.Web.Areas.ExternalManager.Models;

namespace Parking.Web.Areas.ExternalManager.Controllers
{
    public class PLCController : Controller
    {
        private Log log = LogFactory.GetLogger("PLC");

        // GET: ExternalManager/PLC
        public async Task<JsonResult> FindQueueList(int warehouse)
        {
            var queueLst = await new CWTask().FindQueueLstAsync();
            return Json(queueLst, JsonRequestBehavior.AllowGet);
        }

        public async Task<ContentResult> TranferTempLocOccupy(int warehouse)
        {
            int nback= await new CWTask().DealTempLocOccupy(warehouse);
            return Content("success");
        }

        public async Task<JsonResult> FindDevice(int warehouse,int devicecode)
        {
            Device smg =await  new CWDevice().FindAsync(d=>d.Warehouse==warehouse&&d.DeviceCode==devicecode);
            return Json(smg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ContentResult> CreateDeviceTaskByQueue()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);

                JObject jo = (JObject)JsonConvert.DeserializeObject(req);

                string queueID = jo["queueID"].ToString();
                string wh = jo["warehouse"].ToString();
                string code = jo["devicecode"].ToString();

                int id = Convert.ToInt32(queueID);
                WorkTask queue = new CWTask().FindQueue(id);

                int warehouse = Convert.ToInt32(wh);
                int devicecode = Convert.ToInt32(code);

                Device smg = await new CWDevice().FindAsync(d => d.Warehouse == warehouse && d.DeviceCode == devicecode);
                Response resp = await new CWTask().CreateDeviceTaskByQueueAsync(queue, smg);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        public async Task<JsonResult> FindITask(int tid)
        {
            SubTask subtask = null;
            ImplementTask itsk =await new CWTask().FindAsync(tid);
            if (itsk != null)
            {
                subtask = new SubTask
                {
                    ID = itsk.ID,
                    Warehouse = itsk.Warehouse,
                    DeviceCode = itsk.DeviceCode,
                    Type = (int)itsk.Type,
                    Status = (int)itsk.Status,
                    SendStatusDetail = (int)itsk.SendStatusDetail,
                    SendDtime = itsk.SendDtime.ToString(),
                    CreateDate = itsk.CreateDate.ToString(),
                    HallCode = itsk.HallCode,
                    FromLctAddress = itsk.FromLctAddress,
                    ToLctAddress = itsk.ToLctAddress,
                    ICCardCode = itsk.ICCardCode,
                    Distance = itsk.Distance,
                    CarSize = itsk.CarSize,
                    CarWeight = itsk.CarWeight,
                    IsComplete = itsk.IsComplete,
                    LocSize = itsk.LocSize
                };
            }
            return Json(subtask,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ContentResult> DealTVUnloadTask()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);

                JObject jo = (JObject)JsonConvert.DeserializeObject(req);

                string queueID = jo["queueID"].ToString();
                string itaskID = jo["taskID"].ToString();

                int mtskID = Convert.ToInt32(queueID);
                int tid = Convert.ToInt32(itaskID);

                CWTask cwtask = new CWTask();
                WorkTask queue =await cwtask.FindQueueAsync(mtskID);
                ImplementTask task =await cwtask.FindITaskAsync(tid);
                cwtask.DealTVUnloadTask(task, queue);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        public async Task<JsonResult> FindLocation(string iccode)
        {
            Location loc =await new CWLocation().FindLocationAsync(l => l.ICCode == iccode);
            return Json(loc, JsonRequestBehavior.AllowGet);
        }

        public async Task<ContentResult> DeleteQueue(int queueID)
        {
            Response resp=await new CWTask().DeleteQueueAsync(queueID);
            return Content("success");
        }

        [HttpPost]
        public async Task<ContentResult> SendHallTelegramAndBuildTV()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);

                JObject jo = (JObject)JsonConvert.DeserializeObject(req);

                string queueID = jo["queueID"].ToString();
                string wh = jo["warehouse"].ToString();
                string locaddrs = jo["locaddrs"].ToString();
                string devicecode = jo["devicecode"].ToString();

                int qid = Convert.ToInt32(queueID);
                int warehouse = Convert.ToInt32(wh);
                int code = Convert.ToInt32(devicecode);

                WorkTask queue =await new CWTask().FindQueueAsync(qid);
                Device hall =await new CWDevice().FindAsync(d => d.Warehouse == warehouse && d.DeviceCode == code);
                Location loc =await new CWLocation().FindLocationAsync(l => l.Warehouse == warehouse && l.Address == locaddrs);
                
                new CWTask().SendHallTelegramAndBuildTV(queue, loc, hall);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        [HttpPost]
        public async Task<ContentResult> AheadTvTelegramAndBuildHall()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);

                JObject jo = (JObject)JsonConvert.DeserializeObject(req);

                string queueID = jo["queueID"].ToString();
                string wh = jo["warehouse"].ToString();
                string locaddrs = jo["locaddrs"].ToString();
                string devicecode = jo["devicecode"].ToString();

                int qid = Convert.ToInt32(queueID);
                int warehouse = Convert.ToInt32(wh);
                int code = Convert.ToInt32(devicecode);

                WorkTask queue =await new CWTask().FindQueueAsync(qid);
                Device hall =await new CWDevice().FindAsync(d => d.Warehouse == warehouse && d.DeviceCode == code);
                Location loc =await new CWLocation().FindLocationAsync(l => l.Warehouse == warehouse && l.Address == locaddrs);

                new CWTask().AheadTvTelegramAndBuildHall(queue, loc, hall);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        public async Task<ActionResult> ReleaseDeviceTaskIDButNoTask(int warehouse)
        {
            int nback=await new CWTask().ReleaseDeviceTaskIDButNoTaskAsync(warehouse);
            return Content("success");
        }

        [HttpPost]
        public async Task<JsonResult> FindITaskLst()
        {
            CWTask cwtask = new CWTask();  
                    
            List<ImplementTask> taskLst =await cwtask.FindITaskLstAsync();

            List<SubTask> subtaskLst = new List<SubTask>();
            foreach(ImplementTask itsk in taskLst)
            {
                SubTask sub = new SubTask {
                    ID = itsk.ID,
                    Warehouse = itsk.Warehouse,
                    DeviceCode = itsk.DeviceCode,
                    Type = (int)itsk.Type,
                    Status = (int)itsk.Status,
                    SendStatusDetail = (int)itsk.SendStatusDetail,
                    SendDtime = itsk.SendDtime.ToString(),
                    CreateDate = itsk.CreateDate.ToString(),
                    HallCode=itsk.HallCode,
                    FromLctAddress=itsk.FromLctAddress,
                    ToLctAddress=itsk.ToLctAddress,
                    ICCardCode=itsk.ICCardCode,
                    Distance=itsk.Distance,
                    CarSize=itsk.CarSize,
                    CarWeight=itsk.CarWeight,
                    IsComplete=itsk.IsComplete,
                    LocSize = itsk.LocSize
                };
                subtaskLst.Add(sub);
            }
           
            return Json(subtaskLst);
        }

        [HttpPost]
        public async Task<ContentResult> UpdateSendStatusDetail()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);

                JObject jo = (JObject)JsonConvert.DeserializeObject(req);

                string tskID = jo["itaskID"].ToString();
                string status = jo["status"].ToString();

                int itaskID = Convert.ToInt32(tskID);
                int sendstatus = Convert.ToInt32(status);

                CWTask cwtask = new CWTask();
                ImplementTask itask =await cwtask.FindITaskAsync(itaskID);
                Response resp = cwtask.UpdateSendStatusDetail(itask, (EnmTaskStatusDetail)sendstatus);
                if (resp.Code == 1)
                {
                    return Content("success");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("fail");
        }

        public async Task<ContentResult> UnpackUnloadOrder(int taskID)
        {
            try
            {
                CWTask cwtask = new CWTask();
                ImplementTask itask = await cwtask.FindITaskAsync(taskID);
                if (itask != null)
                {
                    int nback = await cwtask.UnpackUnloadOrderAsync(itask);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        [HttpPost]
        public async Task<ContentResult> DealICheckCar()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);

                JObject jo = (JObject)JsonConvert.DeserializeObject(req);

                string wh = jo["warehouse"].ToString();
                string hallID = jo["hallID"].ToString();
                string taskID = jo["taskID"].ToString();
                string distance = jo["Distance"].ToString();
                string checkcode = jo["CheckCode"].ToString();

                int warehouse = Convert.ToInt32(wh);
                int code = Convert.ToInt32(hallID);
                int tid = Convert.ToInt32(taskID);
                int Distance = Convert.ToInt32(distance);

                await new CWTaskTransfer(code, warehouse).DealICheckCarAsync(tid, Distance, checkcode, 0);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        [HttpPost]
        public async Task<ContentResult> UpdateTaskStatus()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);

                JObject jo = (JObject)JsonConvert.DeserializeObject(req);

                string tskID = jo["taskID"].ToString();
                string status = jo["taskstatus"].ToString();

                int itaskID = Convert.ToInt32(tskID);
                int taskstatus = Convert.ToInt32(status);

                CWTask cwtask = new CWTask();
                ImplementTask itask =await cwtask.FindITaskAsync(itaskID);
                Response resp = cwtask.DealUpdateTaskStatus(itask, (EnmTaskStatus)taskstatus);
                if (resp.Code == 1)
                {
                    return Content("success");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("fail");
        }

        [HttpPost]
        public async Task<ContentResult> DealCarLeave()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);

                JObject jo = (JObject)JsonConvert.DeserializeObject(req);

                string wh = jo["warehouse"].ToString();
                string nhallID = jo["hallID"].ToString();
                string tID = jo["taskID"].ToString();

                int warehouse = Convert.ToInt32(wh);
                int hallID = Convert.ToInt32(nhallID);
                int taskID = Convert.ToInt32(tID);
                await new CWTaskTransfer(hallID, warehouse).DealCarLeaveAsync(taskID);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        public async Task<ContentResult> ODealEVUp(int taskID)
        {
            try
            {
                CWTask cwtask = new CWTask();
                ImplementTask task =await cwtask.FindITaskAsync(taskID);
                if (task != null)
                {
                    cwtask.ODealEVUp(task);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        public async Task<ContentResult> DealCompleteTask(int taskID)
        {
            try
            {
                CWTask cwtask = new CWTask();
                ImplementTask task =await cwtask.FindITaskAsync(taskID);
                if (task != null)
                {
                    cwtask.DealCompleteTask(task);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        public async Task<ContentResult> DealLoadFinishing(int taskID,int distance)
        {
            try
            {
                CWTask cwtask = new CWTask();
                ImplementTask task =await cwtask.FindITaskAsync(taskID);
                if (task != null)
                {
                    cwtask.DealLoadFinishing(task, distance);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        public async Task<ContentResult> DealLoadFinished(int taskID)
        {
            try
            {
                CWTask cwtask = new CWTask();
                ImplementTask task =await cwtask.FindITaskAsync(taskID);
                if (task != null)
                {
                    await cwtask.DealLoadFinishedAsync(task);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }

            return Content("success");
        }

        public async Task<ContentResult> DealUnLoadFinishing(int taskID)
        {
            try
            {
                CWTask cwtask = new CWTask();
                ImplementTask task =await cwtask.FindITaskAsync(taskID);
                if (task != null)
                {
                    cwtask.DealUnLoadFinishing(task);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }

            return Content("success");
        }

        public async Task<ContentResult> DealCarEntrance(int warehouse, int hallID)
        {
            await new CWTaskTransfer(hallID, warehouse).DealCarEntranceAsync();
            return Content("success");
        }

        public async Task<JsonResult> FindDevicesList(int warehouse)
        {
            List<Device> DevsLst =await new CWDevice().FindListAsync(d=>d.Warehouse==warehouse);
            return Json(DevsLst, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> FindAlarmsList(int warehouse)
        {
            List<Alarm> alarmsLst =await new CWDevice().FindAlarmListAsync(d=>d.Warehouse==warehouse);
            return Json(alarmsLst, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ContentResult> UpdateDevice()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);

                Device smg = JsonConvert.DeserializeObject<Device>(req);

                CWDevice cwdevice = new CWDevice();
                Device current =await cwdevice.FindAsync(d => d.Warehouse == smg.Warehouse && d.DeviceCode == smg.DeviceCode);
                if (current != null)
                {
                    bool isReset = false;
                    if (smg.Mode != current.Mode &&
                        smg.Mode == EnmModel.Automatic &&
                        current.Type == EnmSMGType.Hall)
                    {
                        isReset = true;
                    }

                    current.IsAvailabe = smg.IsAvailabe;
                    current.IsAble = smg.IsAble;
                    current.RunStep = smg.RunStep;
                    current.InStep = smg.InStep;
                    current.OutStep = smg.OutStep;
                    current.Mode = smg.Mode;
                    current.Address = smg.Address;

                    cwdevice.Update(current);

                    if (isReset && current.TaskID != 0)
                    {
                       await new CWTask().ResetHallOnlyHasTaskAsync(current.Warehouse, current.DeviceCode);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }

            return Content("success");
        }

        [HttpPost]
        public async Task<ContentResult> UpdateAlarmsList()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.UTF8.GetString(bytes);

                List<Alarm> needUpdateLst = JsonConvert.DeserializeObject<List<Alarm>>(req);
                
                await new CWDevice().UpdateAlarmListAsync(needUpdateLst);               
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        /// <summary>
        /// 切换模式时，仅有车厅作业，则复位车厅作业
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ContentResult> ResetHallOnlyHasTask()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.Default.GetString(bytes);

                JObject jo = (JObject)JsonConvert.DeserializeObject(req);
                string wh = jo["warehouse"].ToString();
                string nhallID = jo["hallID"].ToString();

                int warehouse = Convert.ToInt32(wh);
                int hallID = Convert.ToInt32(nhallID);

                await new CWTask().ResetHallOnlyHasTaskAsync(warehouse, hallID);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        [HttpPost]
        public async Task<ContentResult> AddTelegramLog()
        {
            try
            {
                byte[] bytes = new byte[Request.InputStream.Length];
                Request.InputStream.Read(bytes, 0, bytes.Length);
                string req = System.Text.Encoding.UTF8.GetString(bytes);

                TelegramRecord rcd = JsonConvert.DeserializeObject<TelegramRecord>(req);

                await new CWTelegramLog().AddRecordAsync(rcd.Telegram, rcd.Type);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return Content("success");
        }

        public async Task<JsonResult> FindITaskBySmg(int warehouse, int devicecode)
        {
            List<ImplementTask> itaskLst =await new CWTask().FindITaskLstAsync(t => t.Warehouse == warehouse && t.DeviceCode == devicecode);
            SubTask subtask = null;
            
            ImplementTask itsk = null;
            if (itaskLst.Count > 1)
            {
                foreach (ImplementTask tsk in itaskLst)
                {
                    if (tsk.Status != EnmTaskStatus.WillWaitForUnload)
                    {
                        itsk = tsk;
                        break;
                    }
                }
            }
            else if(itaskLst.Count==1)
            {
                itsk = itaskLst[0];
            }
            if (itsk != null)
            {
                subtask = new SubTask
                {
                    ID = itsk.ID,
                    Warehouse = itsk.Warehouse,
                    DeviceCode = itsk.DeviceCode,
                    Type = (int)itsk.Type,
                    Status = (int)itsk.Status,
                    SendStatusDetail = (int)itsk.SendStatusDetail,
                    SendDtime = itsk.SendDtime.ToString(),
                    CreateDate = itsk.CreateDate.ToString(),
                    HallCode = itsk.HallCode,
                    FromLctAddress = itsk.FromLctAddress,
                    ToLctAddress = itsk.ToLctAddress,
                    ICCardCode = itsk.ICCardCode,
                    Distance = itsk.Distance,
                    CarSize = itsk.CarSize,
                    CarWeight = itsk.CarWeight,
                    IsComplete = itsk.IsComplete,
                    LocSize=itsk.LocSize
                };
            }
            return Json(subtask, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 处理车辆跑位信号
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="devicecode"></param>
        /// <returns></returns>
        public async Task<ContentResult> DealCarTraceOut(int warehouse, int devicecode)
        {
            await new CWTask().DealCarDriveOffTracingAsync(warehouse, devicecode);
            return Content("success");
        }

        public ActionResult AddSoundNotifi(int warehouse, int devicecode,string soundfile)
        {
            new CWTask().AddNofication(warehouse, devicecode, soundfile);
            return Content("success");
        }

        public async Task<ContentResult> CreateAvoidTaskByQueue(int queueID)
        {
            await new CWTask().CreateAvoidTaskByQueueAsync(queueID);

            return Content("success");
        }

        public async Task<JsonResult> DealAvoid(int queueID,int warehouse,int devicecode)
        {
            Response resp =await new CWTask().DealAvoidAsync(queueID, warehouse, devicecode);
            return Json(resp,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        /// <summary>
        /// 车厅装载时高度复检不通过
        /// </summary>
        /// <returns></returns>
        public async Task<ContentResult> ReCheckCarWithLoad()
        {
            byte[] bytes = new byte[Request.InputStream.Length];
            Request.InputStream.Read(bytes, 0, bytes.Length);
            string req = System.Text.Encoding.Default.GetString(bytes);

            JObject jo = (JObject)JsonConvert.DeserializeObject(req);
            string tid = jo["TaskID"].ToString();
            string checkcode = jo["CheckCode"].ToString();
            string distance = jo["Distance"].ToString();

            int taskid = Convert.ToInt32(tid);
            int dist = Convert.ToInt32(distance);

            await new CWTask().ReCheckCarWithLoadAsync(taskid, dist, checkcode);
            return Content("success");
        }

        [HttpPost]
        /// <summary>
        /// 车位卸载时,车位上有车，则重新分配车位
        /// </summary>
        public async Task<ContentResult> DealUnloadButHasCarBlock()
        {
            byte[] bytes = new byte[Request.InputStream.Length];
            Request.InputStream.Read(bytes, 0, bytes.Length);
            string req = System.Text.Encoding.Default.GetString(bytes);

            JObject jo = (JObject)JsonConvert.DeserializeObject(req);
            string tid = jo["TaskID"].ToString();

            int taskid = Convert.ToInt32(tid);

            await new CWTask().DealUnloadButHasCarBlock(taskid);
            return Content("success");
        }

    }
}