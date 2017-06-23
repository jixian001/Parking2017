using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;
using System.Linq.Expressions;

namespace Parking.Core
{
    /// <summary>
    /// 处理处于执行的作业
    /// </summary>
    public class CWTask
    {
        private static CurrentTaskManager manager = new CurrentTaskManager();

        private static List<string> soundsList = new List<string>();

        private Log clog = null;

        public CWTask()
        {
            clog = LogFactory.GetLogger("CWTask");
        }

        /// <summary>
        /// 添加声音文件
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        /// <param name="soundFile"></param>
        public void AddNofication(int warehouse,int hallID,string soundFile)
        {
            string sfile = warehouse.ToString() + ";" + hallID.ToString() + ";" + soundFile;
            if (!soundsList.Contains(sfile))
            {
                soundsList.Add(sfile);
            }           
            clog.Info(DateTime.Now.ToString() + "  warehouse-" + warehouse + "   hallID-" + hallID + " make sound, file name-" + soundFile);
        }

        /// <summary>
        /// 移除声音
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        public void ClearNotification(int warehouse,int hallID)
        {
            for (int i = 0; i < soundsList.Count; i++)
            {
                string[] infos = soundsList[i].Split(';');
                if (infos.Length > 1)
                {
                    int wh = Convert.ToInt32(infos[0]);
                    int hall = Convert.ToInt32(infos[1]);
                    if (wh == warehouse && hall == hallID)
                    {
                        soundsList.RemoveAt(i);
                    }
                }
            }
        }
        /// <summary>
        /// 获取声音
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        /// <returns></returns>
        public string GetNotification(int warehouse, int hallID)
        {
            try
            {
                for (int i = 0; i < soundsList.Count; i++)
                {
                    string[] infos = soundsList[i].Split(';');
                    if (infos.Length > 2)
                    {
                        int wh = Convert.ToInt32(infos[0]);
                        int hall = Convert.ToInt32(infos[1]);
                        if (wh == warehouse && hall == hallID)
                        {
                            string sound = infos[2];
                            soundsList.RemoveAt(i);
                            return sound;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clog.Error(ex.ToString());
            }
            return null;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public List<ImplementTask> GetExecuteTasks(int warehouse)
        {
            return manager.FindList(tsk => tsk.Warehouse == warehouse && tsk.IsComplete == 0);
        }       

        /// <summary>
        /// 更新作业报文的发送状态
        /// </summary>
        /// <param name="task"></param>
        /// <param name="detail"></param>
        public void UpdateSendStatusDetail(ImplementTask task,EnmTaskStatusDetail detail)
        {
            task.SendStatusDetail = detail;
            task.SendDtime = DateTime.Now;
            manager.Update(task);
        }

        /// <summary>
        /// 更新作业状态
        /// </summary>
        /// <param name="task"></param>
        /// <param name="status"></param>
        public void DealUpdateTaskStatus(ImplementTask task, EnmTaskStatus status)
        {
            task.Status = status;
            task.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            task.SendDtime = DateTime.Now;
            manager.Update(task);
        }

        /// <summary>
        /// 依ID获取任务
        /// </summary>
        public ImplementTask Find(int ID)
        {
            return manager.Find(ID);
        }

        /// <summary>
        /// 查询任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ImplementTask Find(Expression<Func<ImplementTask,bool>> where)
        {
            return manager.Find(where);
        }

        /// <summary>
        /// 获取正在执行的作业
        /// </summary>
        /// <param name="smg"></param>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public ImplementTask GetImplementTaskBySmgID(int smg,int warehouse)
        {
            return manager.Find(tsk => tsk.DeviceCode == smg && tsk.Warehouse == warehouse && tsk.IsComplete == 0);
        }

        /// <summary>
        /// 有车入库
        /// </summary>
        /// <param name="hall"></param>
        public void DealFirstCarEntrance(Device hall)
        {
            try
            {
                ImplementTask task = new ImplementTask();
                task.Warehouse = hall.Warehouse;
                task.DeviceCode = hall.DeviceCode;
                task.Type = EnmTaskType.SaveCar;
                task.Status = EnmTaskStatus.ICarInWaitFirstSwipeCard;
                task.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                task.CreateDate = DateTime.Now;              
                task.SendDtime = DateTime.Now.AddMinutes(-1);               
                task.HallCode = hall.DeviceCode;
                task.FromLctAddress = hall.Address;
                task.ToLctAddress = "";
                task.ICCardCode = "";
                task.Distance = 0;
                task.CarSize = "";
                task.IsComplete = 0;
                Response _resp = manager.Add(task);
                if (_resp.Code == 1)
                {
                    //这里是否可以获取到ID？或者再查询一次
                    hall.TaskID = task.ID;
                    new CWDevice().Update(hall);
                }               
            }
            catch (Exception ex)
            {
                clog.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 外形检测上报处理
        /// </summary>
        /// <param name="hallID"></param>
        /// <param name="distance"></param>
        /// <param name="carSize"></param>
        public void IDealCheckedCar(ImplementTask htsk, int hallID,int distance,string checkCode,int weight)
        {
            Log log = LogFactory.GetLogger("CWTask IDealCheckedCar");
            try
            {
                #region 上报外形数据不正确
                htsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                htsk.SendDtime = DateTime.Now.AddMinutes(-1);
                htsk.Distance = distance;
                htsk.CarSize = checkCode;
                htsk.CarWeight = weight;
                if (checkCode.Length != 3)
                {
                    htsk.Status = EnmTaskStatus.ISecondSwipedWaitforCarLeave;
                    //更新任务信息
                    manager.Update(htsk);
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "60.wav");
                    return;
                }
                #endregion
                Customer cust = null;
                #region
                if (Convert.ToInt32(htsk.ICCardCode) >= 10000) //是指纹激活的
                {
                    int prf = Convert.ToInt32(htsk.ICCardCode);
                    FingerPrint print = new CWFingerPrint().Find(p => p.SN_Number == prf);
                    if (print == null)
                    {
                        //上位控制系统故障
                        this.AddNofication(htsk.Warehouse, hallID, "20.wav");
                        return;
                    }
                    cust = new CWICCard().FindCust(print.CustID);
                    if (cust == null)
                    {
                        //上位控制系统故障
                        this.AddNofication(htsk.Warehouse, hallID, "20.wav");
                        return;
                    }
                }
                else
                {
                    ICCard iccd = new CWICCard().Find(ic => ic.UserCode == htsk.ICCardCode);
                    if (iccd == null)
                    {
                        //上位控制系统故障
                        this.AddNofication(htsk.Warehouse, hallID, "20.wav");
                        return;
                    }
                    if (iccd.CustID != 0)
                    {
                        cust = new CWICCard().FindCust(iccd.CustID);
                    }
                }
                #endregion

                Device hall = new CWDevice().SelectSMG(hallID, htsk.Warehouse);
                int tvID = 0;
                Location lct = new AllocateLocation().IAllocateLocation(checkCode, cust, hall, out tvID);
                if (lct == null)
                {
                    htsk.Status = EnmTaskStatus.ISecondSwipedWaitforCarLeave;
                    //更新任务信息
                    manager.Update(htsk);
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "62.wav");
                    return;
                }
                if (tvID == 0)
                {
                    htsk.Status = EnmTaskStatus.ISecondSwipedWaitforCarLeave;
                    manager.Update(htsk);
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "42.wav");
                    return;
                }
                //再判断下车位尺寸
                if (string.Compare(lct.LocSize, checkCode) < 0)
                {
                    htsk.Status = EnmTaskStatus.ISecondSwipedWaitforCarLeave;
                    manager.Update(htsk);
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "63.wav");
                    return;
                }
                #region 获取车牌识别车牌信息
                PlateMappingDev device_plate = new CWDevice().FindPlateInfo(pt => pt.Warehouse == hall.Warehouse && pt.DeviceCode == hall.DeviceCode);
                if (device_plate != null)
                {
                    if (!string.IsNullOrEmpty(device_plate.PlateNum) &&
                        DateTime.Compare(DateTime.Now, device_plate.InDate.AddMinutes(3)) < 0)
                    {
                        lct.PlateNum = device_plate.PlateNum;
                        lct.ImagePath = device_plate.HeadImagePath;
                    }
                }
                #endregion
                #region 如果没有车牌识别，则从顾客登记的信息中给车牌号，以作为后续界面取车用
                if (string.IsNullOrEmpty(lct.PlateNum))
                {
                    if (cust != null)
                    {
                        if (!string.IsNullOrEmpty(cust.PlateNum))
                        {
                            lct.PlateNum = cust.PlateNum;
                        }
                    }
                }
                #endregion
                //补充车位信息
                lct.WheelBase = distance;
                lct.CarSize = checkCode;
                lct.InDate = DateTime.Now;
                lct.Status = EnmLocationStatus.Entering;
                lct.ICCode = htsk.ICCardCode;
                lct.CarWeight = weight;
                Response resp = new CWLocation().UpdateLocation(lct);

                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + " 更新车位-" + lct.Address + "数据，iccode-" + lct.ICCode + " status-" + lct.Status.ToString());
                    #region 清空车牌存储区
                    if (device_plate != null)
                    {
                        device_plate.HeadImagePath = "";
                        device_plate.PlateImagePath = "";
                        device_plate.PlateNum = "";
                        new CWDevice().UpdatePlateInfo(device_plate);
                    }
                    #endregion
                }

                htsk.ToLctAddress = lct.Address;
                htsk.Status = EnmTaskStatus.ISecondSwipedWaitforEVDown;
                resp = manager.Update(htsk);
                //添加TV的存车装载，将其加入队列中
                WorkTask queue = new WorkTask()
                {
                    IsMaster = 1,
                    Warehouse = lct.Warehouse,
                    DeviceCode = tvID,
                    MasterType = EnmTaskType.SaveCar,
                    TelegramType = 13,
                    SubTelegramType = 1,
                    HallCode=hallID,
                    FromLctAddress = hall.Address,
                    ToLctAddress = lct.Address,
                    ICCardCode = htsk.ICCardCode,
                    Distance = distance,
                    CarSize = checkCode,
                    CarWeight = weight
                };
                resp = manager_queue.Add(queue);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + " 队列中添加TV装载作业，存车位-" + lct.Address + "，iccode-" + lct.ICCode);
                }
               
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 临时取物刷卡转存时，处理外形检测上报
        /// </summary>
        public void ITempDealCheckCar(ImplementTask htsk,Location lct, int distance,string carsize, int weight)
        {
            Log log = LogFactory.GetLogger("CWTask ITempDealCheckCar");
            try
            {
                Device hall = new CWDevice().SelectSMG(htsk.HallCode, htsk.Warehouse);
                if (hall == null)
                {
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "20.wav");
                    log.Error("系统故障，找不到对应的车厅，HallCode-" + htsk.HallCode);
                    return;
                }
                Device smg = new AllocateLocation().PXDAllocateEtvOfFixLoc(hall, lct);
                if (smg == null)
                {
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "42.wav");
                    log.Error("系统故障，找不到TV");
                    return;
                }

                //补充车位信息
                lct.WheelBase = distance;
                lct.CarSize = carsize;
                lct.InDate = DateTime.Now;
                lct.ICCode = htsk.ICCardCode;
                lct.Status = EnmLocationStatus.Entering;
                Response resp = new CWLocation().UpdateLocation(lct);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + " 转存更新车位-" + lct.Address + " 数据，iccode-" + lct.ICCode + " status-" + lct.Status.ToString());
                }

                htsk.CarSize = carsize;
                htsk.Distance = distance;
                htsk.SendDtime = DateTime.Now;
                htsk.ToLctAddress = lct.Address;
                htsk.Status = EnmTaskStatus.ISecondSwipedWaitforEVDown;
                htsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                resp = manager.Update(htsk);

                //添加TV的存车装载，将其加入队列中
                WorkTask queue = new WorkTask()
                {
                    IsMaster = 1,
                    Warehouse = lct.Warehouse,
                    DeviceCode = smg.DeviceCode,
                    MasterType = EnmTaskType.SaveCar,
                    TelegramType = 13,
                    SubTelegramType = 1,
                    HallCode=hall.DeviceCode,
                    FromLctAddress = htsk.FromLctAddress,
                    ToLctAddress = lct.Address,
                    ICCardCode = htsk.ICCardCode,
                    Distance = distance,
                    CarSize = carsize,
                    CarWeight = weight
                };
                resp = manager_queue.Add(queue);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + " 队列中添加TV装载作业，转存，车位-" + lct.Address + "，iccode-" + lct.ICCode);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        public Page<ImplementTask> FindPageList(Page<ImplementTask> pageTask,OrderParam param)
        {
            if (param == null)
            {
                param = new OrderParam()
                {
                    PropertyName = "ID",
                    Method = OrderMethod.Asc
                };
            }
            Page<ImplementTask> page = manager.FindPageList(pageTask,param);
            return page;
        }

        /// <summary>
        /// 手动完成作业,只完成所选的作业，
        /// 如果该作业生成相应的挪移作业，则这个操作不影响其操作
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public Response ManualCompleteTask(int tid)
        {
            Response resp = new Response();
            CWLocation cwlctn = new CWLocation();
            CWDevice cwdevice = new CWDevice();
            ImplementTask etvtask = null;
            try
            {
                ImplementTask itask = manager.Find(tid);
                if (itask == null)
                {
                    resp.Code = 0;
                    resp.Message = "找不到对应的任务,ID-" + tid;
                    return resp;
                }

                Device dev = cwdevice.Find(d => d.Warehouse == itask.Warehouse && d.DeviceCode == itask.DeviceCode);
                if (dev != null)
                {
                    #region 释放设备
                    if (dev.Type == EnmSMGType.ETV)
                    {
                        etvtask = itask;
                    }
                    dev.TaskID = 0;
                    if (dev.SoonTaskID != 0)
                    {
                        dev.TaskID = dev.SoonTaskID;
                        dev.SoonTaskID = 0;
                    }
                    cwdevice.Update(dev);
                    #endregion
                }

                //获取相关联的作业
                string iccode = itask.ICCardCode;
                ImplementTask relatetask = manager.Find(tsk => tsk.ICCardCode == iccode && tsk.ID != tid && tsk.Type != EnmTaskType.Avoid&&tsk.IsComplete==0);
                if (relatetask != null)
                {
                    #region 释放关联的车厅或TV设备
                    dev = cwdevice.Find(d => d.Warehouse == relatetask.Warehouse && d.DeviceCode == relatetask.DeviceCode);
                    if (dev != null)
                    {
                        if (dev.Type == EnmSMGType.ETV)
                        {
                            etvtask = relatetask;
                        }
                        dev.TaskID = 0;
                        if (dev.SoonTaskID != 0)
                        {
                            dev.TaskID = dev.SoonTaskID;
                            dev.SoonTaskID = 0;
                        }
                        cwdevice.Update(dev);
                    }
                    #endregion
                }

                #region 只有涉及ETV作业，才会涉及到车位
                if (etvtask != null)
                {
                    if (etvtask.Type == EnmTaskType.SaveCar)
                    {
                        Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.ToLctAddress);
                        if (toLct != null)
                        {
                            toLct.Status = EnmLocationStatus.Occupy;
                            toLct.InDate = DateTime.Now;
                            toLct.ICCode = etvtask.ICCardCode;
                            toLct.WheelBase = etvtask.Distance;
                            toLct.CarSize = etvtask.CarSize;

                            cwlctn.UpdateLocation(toLct);
                        }
                    }
                    else if (etvtask.Type == EnmTaskType.GetCar ||
                             etvtask.Type == EnmTaskType.TempGet)
                    {
                        Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.FromLctAddress);
                        if (toLct != null)
                        {
                            toLct.Status = EnmLocationStatus.Space;
                            toLct.InDate = DateTime.Parse("2017-1-1");
                            toLct.ICCode = "";
                            toLct.WheelBase = 0;
                            toLct.CarSize = "";
                            toLct.CarWeight = 0;

                            cwlctn.UpdateLocation(toLct);
                        }
                    }
                    else if (etvtask.Type == EnmTaskType.Transpose)
                    {
                        Location frLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.FromLctAddress);
                        Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.ToLctAddress);
                        if (frLct != null && toLct != null)
                        {
                            toLct.Status = EnmLocationStatus.Occupy;
                            toLct.ICCode = frLct.ICCode;
                            toLct.InDate = frLct.InDate;
                            toLct.WheelBase = frLct.WheelBase;
                            toLct.CarSize = frLct.CarSize;
                            toLct.CarWeight = frLct.CarWeight;

                            cwlctn.UpdateLocation(toLct);

                            frLct.Status = EnmLocationStatus.Space;
                            frLct.InDate = DateTime.Parse("2017-1-1");
                            frLct.ICCode = "";
                            frLct.WheelBase = 0;
                            frLct.CarSize = "";
                            frLct.CarWeight = 0;

                            cwlctn.UpdateLocation(frLct);
                        }
                    }
                }
                #endregion

                #region 删除相关队列
                List<WorkTask> queueLst = manager_queue.FindList(wk => wk.ICCardCode == iccode);
                foreach (WorkTask wtsk in queueLst)
                {
                    manager_queue.Delete(wtsk.ID);
                }
                #endregion

                //删除作业
                if (relatetask != null)
                {
                    manager.Delete(relatetask.ID);
                }
                resp = manager.Delete(tid);
                if (resp.Code == 1)
                {
                    resp.Message = "手动完成作业成功,ID-" + tid;
                }
              
            }
            catch (Exception ex)
            {
                clog.Error(ex.ToString());
            }
            return resp;
        }

        /// <summary>
        /// 手动复位作业
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public Response ManualResetTask(int tid)
        {
            Response resp = new Response();
            CWLocation cwlctn = new CWLocation();
            CWDevice cwdevice = new CWDevice();
            try
            {
                ImplementTask etvtask = null;
                ImplementTask itask = manager.Find(tid);
                if (itask == null)
                {
                    resp.Code = 0;
                    resp.Message = "找不到对应的任务,ID-" + tid;
                    return resp;
                }

                Device dev = cwdevice.Find(d => d.Warehouse == itask.Warehouse && d.DeviceCode == itask.DeviceCode);
                if (dev != null)
                {
                    #region 释放设备
                    if (dev.Type == EnmSMGType.ETV)
                    {
                        etvtask = itask;
                    }
                    dev.TaskID = 0;
                    if (dev.SoonTaskID != 0)
                    {
                        dev.TaskID = dev.SoonTaskID;
                        dev.SoonTaskID = 0;
                    }
                    cwdevice.Update(dev);
                    #endregion
                }

                //获取相关联的作业
                string iccode = itask.ICCardCode;
                ImplementTask relatetask = manager.Find(tsk => tsk.ICCardCode == iccode && tsk.ID != tid && tsk.Type != EnmTaskType.Avoid);
                if (relatetask != null)
                {
                    #region 释放关联的车厅或TV设备
                    dev = cwdevice.Find(d => d.Warehouse == relatetask.Warehouse && d.DeviceCode == relatetask.DeviceCode);
                    if (dev != null)
                    {
                        if (dev.Type == EnmSMGType.ETV)
                        {
                            etvtask = relatetask;
                        }
                        dev.TaskID = 0;
                        if (dev.SoonTaskID != 0)
                        {
                            dev.TaskID = dev.SoonTaskID;
                            dev.SoonTaskID = 0;
                        }
                        cwdevice.Update(dev);
                    }
                    #endregion
                }

                #region 只有涉及ETV作业，才会涉及到车位
                if (etvtask != null)
                {
                    if (etvtask.Type == EnmTaskType.SaveCar)
                    {
                        Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.ToLctAddress);
                        if (toLct != null)
                        {
                            toLct.Status = EnmLocationStatus.Space;
                            toLct.InDate = DateTime.Parse("2017-1-1");
                            toLct.ICCode = "";
                            toLct.WheelBase = 0;
                            toLct.CarSize = "";
                            toLct.CarWeight = 0;
                            cwlctn.UpdateLocation(toLct);
                        }
                    }
                    else if (etvtask.Type == EnmTaskType.GetCar ||
                             etvtask.Type == EnmTaskType.TempGet)
                    {
                        Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.FromLctAddress);
                        if (toLct != null)
                        {
                            toLct.Status = EnmLocationStatus.Occupy;
                            toLct.InDate = DateTime.Now;
                            toLct.ICCode = etvtask.ICCardCode;
                            toLct.WheelBase = etvtask.Distance;
                            toLct.CarSize = etvtask.CarSize;
                            cwlctn.UpdateLocation(toLct);
                        }
                    }
                    else if (etvtask.Type == EnmTaskType.Transpose)
                    {
                        Location frLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.FromLctAddress);
                        Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.ToLctAddress);
                        if (frLct != null && toLct != null)
                        {
                            frLct.Status = EnmLocationStatus.Occupy;
                            frLct.InDate = DateTime.Now;
                            frLct.ICCode = etvtask.ICCardCode;
                            frLct.WheelBase = etvtask.Distance;
                            frLct.CarSize = etvtask.CarSize;
                            cwlctn.UpdateLocation(frLct);

                            toLct.Status = EnmLocationStatus.Space;
                            toLct.InDate = DateTime.Parse("2017-1-1");
                            toLct.ICCode = "";
                            toLct.WheelBase = 0;
                            toLct.CarSize = "";
                            toLct.CarWeight = 0;
                            cwlctn.UpdateLocation(toLct);
                        }
                    }
                }
                #endregion

                //删除作业
                if (relatetask != null)
                {
                    manager.Delete(relatetask.ID);
                }
                resp = manager.Delete(tid);
                if (resp.Code == 1)
                {
                    resp.Message = "手动复位作业成功,ID-" + tid;
                }
                
            }
            catch (Exception ex)
            {
                clog.Error(ex.ToString());
            }        
            return resp;
        }

        /// <summary>
        /// 中途退出，任务完成
        /// </summary>
        /// <param name="task"></param>
        public void ICancelInAndDeleteTask(ImplementTask task)
        {
            CWDevice cwdevice = new CWDevice();
            try
            {
                Device hall = cwdevice.Find(dev => dev.Warehouse == task.Warehouse && dev.DeviceCode == task.DeviceCode);
                if (hall != null)
                {
                    hall.TaskID = 0;
                    new CWDevice().Update(hall);
                }
                task.Status = EnmTaskStatus.Finished;
                task.IsComplete = 1;
                //manager.Update(task,false);
                manager.Delete(task.ID);
            }
            catch (Exception ex)
            {
                clog.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 处理取车、取物车辆离开
        /// </summary>
        /// <param name="task"></param>
        public void ODealCarDriveaway(ImplementTask task)
        {
            try
            {
                if (task.Type == EnmTaskType.TempGet)
                {
                    //释放车位
                    Location lct = new CWLocation().FindLocation(l => l.Warehouse == task.Warehouse && l.Address == task.FromLctAddress);
                    if (lct != null)
                    {
                        if (lct.Status == EnmLocationStatus.Entering ||
                            lct.Status == EnmLocationStatus.Outing)
                        {
                        }
                        else
                        {
                            lct.Status = EnmLocationStatus.Space;
                            new CWLocation().UpdateLocation(lct);
                        }
                    }
                }
                CWDevice cwdevice = new CWDevice();
                Device hall = cwdevice.Find(dev => dev.Warehouse == task.Warehouse && dev.DeviceCode == task.DeviceCode);
                if (hall != null)
                {
                    hall.TaskID = 0;
                    new CWDevice().Update(hall);
                }
                task.Status = EnmTaskStatus.Finished;
                task.IsComplete = 1;
                //manager.Update(task,false);
                manager.Delete(task.ID);
               
            }
            catch (Exception ex)
            {
                clog.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 处理装载完成（1013，1）
        /// </summary>
        /// <param name="etsk"></param>
        /// <param name="distance"></param>
        public void DealLoadFinishing(ImplementTask etsk,int distance)
        {
            Log log = LogFactory.GetLogger("CWTask.DealLoadFinishing");
            try
            {
                CWLocation cwlocation = new CWLocation();
                CWDevice cwdevice = new CWDevice();

                Location frLct = null;
                Location toLct = null;
                #region 存车
                if (etsk.Type == EnmTaskType.SaveCar)
                {
                    #region 将车厅作业完成
                    Device hall = cwdevice.Find(cd => cd.Warehouse == etsk.Warehouse && cd.DeviceCode == etsk.HallCode);
                    if (hall != null)
                    {
                        ImplementTask halltask = Find(tt => tt.Warehouse == hall.Warehouse && tt.DeviceCode == hall.DeviceCode);
                        if (halltask != null)
                        {
                            halltask.Status = EnmTaskStatus.Finished;
                            halltask.IsComplete = 1;
                            //manager.Update(halltask);
                            manager.Delete(halltask.ID);
                        }
                        hall.TaskID = 0;
                        hall.SoonTaskID = 0;
                        cwdevice.Update(hall);
                    }
                    else
                    {
                        log.Error("存车装载完成，要复位车厅设备时，车厅设备为NULL");
                    }
                    #endregion
                    #region 更新下车位信息
                    toLct = cwlocation.FindLocation(l => l.Address == etsk.ToLctAddress && l.Warehouse == etsk.Warehouse);
                    if (toLct != null)
                    {
                        toLct.WheelBase = distance;
                        toLct.Status = EnmLocationStatus.Entering;
                        cwlocation.UpdateLocation(toLct);
                    }
                    else
                    {
                        log.Error("存车装载完成，要更新存车位信息，但车位为NULL，address-" + etsk.ToLctAddress);
                    }
                    #endregion
                }
                #endregion
                #region 取车 取物
                else if (etsk.Type == EnmTaskType.GetCar ||
                         etsk.Type == EnmTaskType.TempGet)
                {
                    #region 更新下车位信息
                    frLct = cwlocation.FindLocation(l => l.Address == etsk.FromLctAddress && l.Warehouse == etsk.Warehouse);
                    if (frLct != null)
                    {
                        frLct.WheelBase = distance;
                        frLct.Status = EnmLocationStatus.Outing;
                        cwlocation.UpdateLocation(frLct);
                    }
                    else
                    {
                        log.Error("存车装载完成，要更新存车位信息，但车位为NULL，address-" + etsk.ToLctAddress);
                    }
                    #endregion
                }
                #endregion
                #region 挪移
                else if (etsk.Type == EnmTaskType.Transpose)
                {
                    int warehouse = etsk.Warehouse;
                    string fradrs = etsk.FromLctAddress;
                    string toadrs = etsk.ToLctAddress;
                    frLct = cwlocation.FindLocation(lt => lt.Warehouse == warehouse && lt.Address == fradrs);
                    toLct = cwlocation.FindLocation(lt => lt.Warehouse == warehouse && lt.Address == toadrs);
                    if (frLct == null || toLct == null)
                    {
                        log.Error("挪移装载完成，源车位或目的车位为空，from address-" + etsk.FromLctAddress + "  to address-" + etsk.ToLctAddress);
                    }
                    else
                    {
                        toLct.Status = EnmLocationStatus.Entering;
                        toLct.WheelBase = frLct.WheelBase;
                        toLct.CarSize = frLct.CarSize;
                        toLct.PlateNum = frLct.PlateNum;
                        toLct.InDate = frLct.InDate;
                        toLct.ICCode = frLct.ICCode;

                        frLct.Status = EnmLocationStatus.Outing;
                        frLct.WheelBase = 0;
                        frLct.CarSize = "";
                        frLct.PlateNum = "";
                        frLct.InDate = DateTime.Parse("2017-1-1");

                        cwlocation.UpdateLocation(toLct);
                        cwlocation.UpdateLocation(frLct);
                    }
                }
                #endregion

                etsk.Status = EnmTaskStatus.LoadFinishing;
                etsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                etsk.SendDtime = DateTime.Now;
                etsk.Distance = distance;
                manager.Update(etsk);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 收到（13，51，9999）
        /// 将存车装载完成，生成卸载指令，加入队列中
        /// </summary>
        /// <param name="tsk"></param>
        public void DealLoadFinished(ImplementTask tsk)
        {
            Log log = LogFactory.GetLogger("CWTask.DealLoadFinished");
            try
            {
                //将当前装载作业置完成，
                tsk.Status = EnmTaskStatus.WillWaitForUnload;
                tsk.SendStatusDetail = EnmTaskStatusDetail.Asked;
                tsk.SendDtime = DateTime.Now;
                tsk.IsComplete = 0;
                manager.Update(tsk);
                //生成卸载指令，加入队列
                WorkTask queue = new WorkTask()
                {
                    IsMaster = 1,
                    Warehouse = tsk.Warehouse,
                    DeviceCode = tsk.DeviceCode,
                    MasterType = tsk.Type,
                    TelegramType = 14,
                    SubTelegramType = 1,
                    HallCode = tsk.HallCode,
                    FromLctAddress = tsk.FromLctAddress,
                    ToLctAddress = tsk.ToLctAddress,
                    ICCardCode = tsk.ICCardCode,
                    Distance = tsk.Distance,
                    CarSize = tsk.CarSize,
                    CarWeight = tsk.CarWeight
                };
                manager_queue.Add(queue);
            }
            catch(Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 处理卸载完成（1014，1）
        /// </summary>
        /// <param name="etsk"></param>
        public void DealUnLoadFinishing(ImplementTask etsk)
        {
            Log log = LogFactory.GetLogger("CWTask.DealUnLoadFinishing");
            CWLocation cwlocation = new CWLocation();
            CWDevice cwdevice = new CWDevice();
            try
            {
                int warehouse = etsk.Warehouse;
                string fraddrs = etsk.FromLctAddress;
                string toaddrs = etsk.ToLctAddress;

                Location frLct = null;
                Location toLct = null;

                if (etsk.Type == EnmTaskType.SaveCar)
                {
                    toLct = cwlocation.FindLocation(l => l.Warehouse == warehouse && l.Address == toaddrs);
                    if (toLct != null)
                    {
                        toLct.Status = EnmLocationStatus.Occupy;
                        toLct.ICCode = etsk.ICCardCode;
                        toLct.WheelBase = etsk.Distance;
                        toLct.CarSize = etsk.CarSize;

                        cwlocation.UpdateLocation(toLct);
                    }
                }
                else if (etsk.Type == EnmTaskType.GetCar ||
                         etsk.Type == EnmTaskType.TempGet)
                {
                    #region 释放车位
                    frLct = cwlocation.FindLocation(l => l.Warehouse == warehouse && l.Address == fraddrs);
                    if (frLct != null)
                    {
                        frLct.WheelBase = 0;
                        frLct.CarSize = "";
                        frLct.ICCode = "";
                        if (etsk.Type == EnmTaskType.TempGet)
                        {
                            frLct.Status = EnmLocationStatus.TempGet;
                        }
                        else
                        {
                            frLct.Status = EnmLocationStatus.Space;
                            frLct.InDate = DateTime.Parse("2017-1-1");
                        }
                        cwlocation.UpdateLocation(frLct);
                    }
                    #endregion
                    #region 修改车厅作业状态
                    Device hall = cwdevice.Find(cd => cd.Warehouse == etsk.Warehouse && cd.DeviceCode == etsk.HallCode);
                    if (hall != null)
                    {
                        ImplementTask halltask = Find(tt => tt.Warehouse == hall.Warehouse && tt.DeviceCode == hall.DeviceCode&&tt.IsComplete==0);
                        if (halltask != null &&
                           (halltask.Status == EnmTaskStatus.OEVDownFinishing ||
                            halltask.Status == EnmTaskStatus.TempEVDownFinishing))
                        {
                            if (etsk.Type == EnmTaskType.GetCar)
                            {
                                halltask.Status = EnmTaskStatus.OWaitforEVUp;
                            }
                            else
                            {
                                halltask.Status = EnmTaskStatus.TempWaitforEVUp;
                            }
                            halltask.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                            halltask.SendDtime = DateTime.Now;
                            manager.Update(halltask);
                        }
                    }
                    else
                    {
                        log.Error("卸载完成，要操作车厅设备时，车厅设备为NULL");
                    }
                    #endregion
                }
                else if (etsk.Type == EnmTaskType.Transpose)
                {
                    frLct = cwlocation.FindLocation(lt => lt.Warehouse == warehouse && lt.Address == fraddrs);
                    toLct = cwlocation.FindLocation(lt => lt.Warehouse == warehouse && lt.Address == toaddrs);
                    if (frLct == null || toLct == null)
                    {
                        log.Error("挪移卸载完成，源车位或目的车位为空，from address-" + etsk.FromLctAddress + "  to address-" + etsk.ToLctAddress);
                    }
                    else
                    {
                        frLct.Status = EnmLocationStatus.Space;
                        WorkTask queue = manager_queue.Find(q => q.ICCardCode == etsk.ICCardCode && q.MasterType == EnmTaskType.Transpose);
                        if (queue != null)
                        {
                            //暂不释放车位
                            frLct.Status = EnmLocationStatus.WillBack;
                        }
                        cwlocation.UpdateLocation(frLct);

                        toLct.Status = EnmLocationStatus.Occupy;
                        cwlocation.UpdateLocation(toLct);
                    }
                }

                etsk.Status = EnmTaskStatus.UnLoadFinishing;
                etsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                etsk.SendDtime = DateTime.Now;
                manager.Update(etsk);
                
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }
       
        /// <summary>
        /// 处理移动完成（1011，1）
        /// </summary>
        /// <param name="etsk"></param>
        public void DealMoveFinishing(ImplementTask etsk)
        {
            Log log = LogFactory.GetLogger("CWTask.DealMoveFinishing");
            try
            {
                etsk.Status = EnmTaskStatus.MoveFinishing;
                etsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                etsk.SendDtime = DateTime.Now;
                manager.Update(etsk);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 完成作业，释放设备
        /// </summary>
        /// <param name="tsk"></param>
        public void DealCompleteTask(ImplementTask tsk)
        {
            Log log = LogFactory.GetLogger("CWTask.DealCompleteTask");
            try
            {               
                CWDevice cwdevice = new CWDevice();
                Device smg = cwdevice.Find(dd => dd.Warehouse == tsk.Warehouse && dd.DeviceCode == tsk.DeviceCode);
                if (smg == null)
                {
                    log.Error("完成作业时，找不到设备号，DeviceCode-" + tsk.DeviceCode);
                }
                smg.TaskID = 0;
                if (smg.Type == EnmSMGType.ETV)
                {
                    List<ImplementTask> remaintask = manager.FindList(tt => tt.ID != tsk.ID &&
                                                        tt.DeviceCode == tsk.DeviceCode &&
                                                        tt.Warehouse == tsk.Warehouse &&
                                                        tt.IsComplete == 0);
                    if (remaintask.Count == 0)
                    {
                        smg.SoonTaskID = 0;
                    }
                    else
                    {
                        //如果当前是避让作业，设备的即将要执行的标志有的话，让其处于执行状态
                        if ((tsk.Type == EnmTaskType.Avoid || tsk.Type == EnmTaskType.Move) &&
                             smg.SoonTaskID != 0)
                        {
                            smg.TaskID = smg.SoonTaskID;
                            smg.SoonTaskID = 0;
                        }
                    }
                }
                else
                {
                    smg.SoonTaskID = 0;
                }
                cwdevice.Update(smg);

                if (smg.Type == EnmSMGType.Hall)
                {
                    if (tsk.Type == EnmTaskType.TempGet)
                    {
                        CWLocation cwlocation = new CWLocation();
                        Location frLct = cwlocation.FindLocation(lt => lt.Warehouse == tsk.Warehouse && lt.Address == tsk.FromLctAddress);
                        if (frLct != null)
                        {
                            //释放车位
                            frLct.WheelBase = 0;
                            frLct.CarSize = "";
                            frLct.ICCode = "";
                            frLct.Status = EnmLocationStatus.Space;
                            frLct.InDate = DateTime.Parse("2017-1-1");

                            cwlocation.UpdateLocation(frLct);
                        }
                    }
                }

                tsk.Status = EnmTaskStatus.Finished;
                tsk.SendStatusDetail = EnmTaskStatusDetail.Asked;
                tsk.SendDtime = DateTime.Now;
                tsk.IsComplete = 1;
                //manager.Update(tsk);
                manager.Delete(tsk.ID);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 完成车厅卸载时处理
        /// </summary>
        public void ODealEVUp(ImplementTask htsk)
        {
            if (htsk.Type == EnmTaskType.TempGet)
            {
                this.AddNofication(htsk.Warehouse,htsk.DeviceCode,"40.wav");
                htsk.Status = EnmTaskStatus.TempOCarOutWaitforDrive;
            }
            else
            {
                htsk.Status = EnmTaskStatus.OCarOutWaitforDriveaway;
                this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "32.wav");
            }
            htsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            htsk.SendDtime = DateTime.Now;
            manager.Update(htsk);
        }

        /// <summary>
        /// 处理第一次刷卡
        /// </summary>
        /// <param name="task"></param>
        /// <param name="iccode"></param>
        public void DealISwipedFirstCard(ImplementTask task,string iccode)
        {
            task.ICCardCode = iccode;
            task.Status = EnmTaskStatus.IFirstSwipedWaitforCheckSize;
            task.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            task.SendDtime = DateTime.Now;
            manager.Update(task);

            this.AddNofication(task.Warehouse, task.DeviceCode, "19.wav");
        }

        /// <summary>
        /// 处理第二次刷卡
        /// </summary>
        /// <param name="task"></param>
        /// <param name="iccode"></param>
        public void DealISwipedSecondCard(ImplementTask task,string iccode)
        {
            task.ICCardCode = iccode;
            task.Status = EnmTaskStatus.ISecondSwipedWaitforCheckSize;
            task.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            task.SendDtime = DateTime.Now;
            manager.Update(task);

            this.AddNofication(task.Warehouse, task.DeviceCode, "21.wav");
        }

        /// <summary>
        /// 处理刷卡取车，只生成队列作业，加入队列列表中
        /// </summary>
        /// <param name="task"></param>
        /// <param name="lct"></param>
        public Response DealOSwipedCard(Device mohall,Location lct)
        {
            Log log = LogFactory.GetLogger("CWTask.DealOSwipedCard");
            Response resp = new Response();
            try
            {
                Device smg = new AllocateTV().Allocate(mohall, lct);
                if (smg == null)
                {
                    //系统故障
                    this.AddNofication(mohall.Warehouse, mohall.DeviceCode, "20.wav");

                    resp.Message = "找不到可用的TV";
                    return resp;
                }
                if (smg.Mode != EnmModel.Automatic)
                {
                    this.AddNofication(mohall.Warehouse, mohall.DeviceCode, "42.wav");

                    resp.Message = "TV的模式不是全自动";
                    return resp;
                }

                lct.Status = EnmLocationStatus.Outing;
                Response respo = new CWLocation().UpdateLocation(lct);
                if (respo.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + " 取车更新车位-" + lct.Address + " 数据，iccode-" + lct.ICCode + " status-" + lct.Status.ToString());
                }

                WorkTask queue = new WorkTask()
                {
                    IsMaster = 2,
                    Warehouse = mohall.Warehouse,
                    DeviceCode = mohall.DeviceCode,
                    MasterType = EnmTaskType.GetCar,
                    TelegramType = 0,
                    SubTelegramType = 0,
                    HallCode = mohall.DeviceCode,
                    FromLctAddress = lct.Address,
                    ToLctAddress = mohall.Address,
                    ICCardCode = lct.ICCode,
                    Distance = lct.WheelBase,
                    CarSize = lct.CarSize,
                    CarWeight = lct.CarWeight
                };
                respo = manager_queue.Add(queue);
                if (respo.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + "  刷卡取车，添加取车队列，存车位-" + lct.Address + "，iccode-" + lct.ICCode);

                    resp.Code = 1;
                    resp.Message = "正在为你取车，请稍后";
                }
        
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return resp;
        }

        /// <summary>
        /// 手动出库
        /// </summary>
        /// <param name="hall"></param>
        /// <param name="lctn"></param>
        /// <param name="iccode"></param>
        public Response ManualGetCar(Device mohall, Location lct)
        {
            Log log = LogFactory.GetLogger("CWTask.ManualGetCar");
            Response resp = new Response();
            try
            {
                Device smg = new AllocateTV().Allocate(mohall, lct);
                if (smg == null)
                {
                    //系统故障
                    resp.Message = "系统故障，找不移动设备。locLayer-" + lct.LocLayer + " warehouse-" + lct.Warehouse;
                    return resp;
                }
                if (smg.Mode != EnmModel.Automatic)
                {
                    resp.Message = "TV-" + smg.DeviceCode + " 不是全自动状态！";
                    return resp;
                }

                lct.Status = EnmLocationStatus.Outing;
                resp = new CWLocation().UpdateLocation(lct);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + " 手动出车更新车位-" + lct.Address + " 数据，iccode-" + lct.ICCode + " status-" + lct.Status.ToString());
                }

                WorkTask queue = new WorkTask()
                {
                    IsMaster = 2,
                    Warehouse = mohall.Warehouse,
                    DeviceCode = mohall.DeviceCode,
                    MasterType = EnmTaskType.GetCar,
                    TelegramType = 0,
                    SubTelegramType = 0,
                    HallCode = mohall.DeviceCode,
                    FromLctAddress = lct.Address,
                    ToLctAddress = mohall.Address,
                    ICCardCode = lct.ICCode,
                    Distance = lct.WheelBase,
                    CarSize = lct.CarSize,
                    CarWeight = lct.CarWeight
                };
                resp = manager_queue.Add(queue);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + "  手动出车，添加取车队列，存车位-" + lct.Address + "，iccode-" + lct.ICCode);
                    resp.Message = "已经加入取车队列，请稍后！";
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }         
            return resp;
        }

        /// <summary>
        /// 取物作业
        /// </summary>
        /// <param name="mohall"></param>
        /// <param name="lct"></param>
        /// <returns></returns>
        public Response TempGetCar(Device mohall,Location lct)
        {
            Log log = LogFactory.GetLogger("CWTask.TempGetCar");
            Response resp = new Response();
            try
            {
                //这里判断是否有可用的TV
                //这里先以平面移动库来做
                Device smg = new AllocateTV().Allocate(mohall, lct);
                if (smg == null)
                {
                    //系统故障
                    resp.Message = "系统故障，找不移动设备。locLayer-" + lct.LocLayer + " warehouse-" + lct.Warehouse;
                    return resp;
                }
                if (smg.Mode != EnmModel.Automatic)
                {
                    resp.Message = "TV-" + smg.DeviceCode + " 不是全自动状态！";
                    return resp;
                }

                lct.Status = EnmLocationStatus.TempGet;
                resp = new CWLocation().UpdateLocation(lct);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + " 取物更新车位-" + lct.Address + " 数据，iccode-" + lct.ICCode + " status-" + lct.Status.ToString());
                }

                WorkTask queue = new WorkTask()
                {
                    IsMaster = 2,
                    Warehouse = mohall.Warehouse,
                    DeviceCode = mohall.DeviceCode,
                    MasterType = EnmTaskType.TempGet,
                    TelegramType = 0,
                    SubTelegramType = 0,
                    HallCode = mohall.DeviceCode,
                    FromLctAddress = lct.Address,
                    ToLctAddress = mohall.Address,
                    ICCardCode = lct.ICCode,
                    Distance = lct.WheelBase,
                    CarSize = lct.CarSize,
                    CarWeight = lct.CarWeight
                };
                resp = manager_queue.Add(queue);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + "  取物操作，添加取物队列，存车位-" + lct.Address + "，iccode-" + lct.ICCode);
                    resp.Message += " 已经加入取车队列，请稍后！";
                }
              
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return resp;
        }

        /// <summary>
        /// 临时取物，查询
        /// </summary>
        /// <param name="iccode"></param>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        /// <param name="locAddress"></param>
        /// <returns></returns>
        public Response TempFindInfo(bool isPlate, string iccode, out int warehouse, out int hallID, out string locAddress)
        {
            warehouse = 0;
            hallID = 0;
            locAddress = "";
            Response _resp = new Response();
            Location lct = null;
            if (isPlate)
            {
                lct = new CWLocation().FindLocation(l => l.PlateNum == iccode);
            }
            else
            {
                lct = new CWLocation().FindLocation(l => l.ICCode == iccode);
            }
            if (lct == null)
            {
                _resp.Message = "该卡没有存车！ICCode-" + iccode;
                return _resp;
            }
            warehouse = lct.Warehouse;
            locAddress = lct.Address;
            if (lct.Type != EnmLocationType.Normal)
            {
                _resp.Message = "车位不可用，Type-" + lct.Type.ToString();
                return _resp;
            }
            if (lct.Status != EnmLocationStatus.Occupy)
            {
                _resp.Message = "车位正在作业，Status-" + lct.Status.ToString();
                return _resp;
            }
            hallID = new CWDevice().AllocateHall(lct, true);
            _resp.Message = "查询成功";
            return _resp;
        }

        /// <summary>
        /// 手动库内搬移
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="fraddrs"></param>
        /// <param name="toaddrs"></param>
        /// <returns></returns>
        public Response TransportLocation(int warehouse,string fraddrs,string toaddrs)
        {
            Log log = LogFactory.GetLogger("CWTask.TransportLocation");
            Response resp = new Response();
            try
            {
                #region
                CWLocation cwloctation = new CWLocation();
                Location frlct = cwloctation.FindLocation(lc => lc.Warehouse == warehouse && lc.Address == fraddrs);
                if (frlct == null)
                {
                    resp.Message = "找不到源地址车位，address-" + fraddrs;
                    return resp;
                }
                Location tolct = cwloctation.FindLocation(lc => lc.Warehouse == warehouse && lc.Address == toaddrs);
                if (tolct == null)
                {
                    resp.Message = "找不到目的地址车位，address-" + toaddrs;
                    return resp;
                }
                if (frlct.Type != EnmLocationType.Normal)
                {
                    resp.Message = "源车位为不可用";
                    return resp;
                }
                if (tolct.Type != EnmLocationType.Normal)
                {
                    resp.Message = "目的车位为不可用";
                    return resp;
                }
                if (frlct.Status != EnmLocationStatus.Occupy)
                {
                    resp.Message = "源车位状态不为占用状态";
                    return resp;
                }
                if (tolct.Status != EnmLocationStatus.Occupy)
                {
                    resp.Message = "目的车位状态不为占用状态";
                    return resp;
                }
                if (string.Compare(frlct.LocSize, tolct.LocSize) > 0)
                {
                    resp.Message = "目标车位的车位尺寸小于源车位尺寸，不允许挪移！";
                    return resp;
                }
                Customer cust = new CWICCard().FindFixLocationByAddress(tolct.Warehouse, tolct.Address);
                if (cust != null)
                {
                    resp.Message = "目标车位是固定车位，不允许挪移！";
                    return resp;
                }
                //是后面车位，则前面保证前面的车位是空闲的
                if (frlct.LocSide == 4)
                {
                    string forward = "2" + frlct.Address.Substring(1);
                    Location loc = cwloctation.FindLocation(l => l.Warehouse == warehouse && l.Address == forward);
                    if (loc != null)
                    {
                        if (loc.Status != EnmLocationStatus.Space)
                        {
                            resp.Message = "源车位是重列位，其前面的车位-" + forward + " 不是空闲的，不允许挪移！";
                            return resp;
                        }
                    }
                }
                if (tolct.LocSide == 4)
                {
                    string forward = "2" + frlct.Address.Substring(1);
                    Location loc = cwloctation.FindLocation(l => l.Warehouse == warehouse && l.Address == forward);
                    if (loc != null)
                    {
                        if (loc.Status != EnmLocationStatus.Space)
                        {
                            resp.Message = "目的车位是重列位，其前面的车位-" + forward + " 不是空闲的，不允许挪移！";
                            return resp;
                        }
                    }
                }

                Device smg = new AllocateTV().TransportToAllocateTV(frlct, tolct);
                if (smg == null)
                {
                    //系统故障
                    resp.Message = "系统故障，挪移时找不移动设备。locLayer-" + frlct.LocLayer + " warehouse-" + frlct.Warehouse;
                    return resp;
                }

                if (smg.Mode != EnmModel.Automatic)
                {
                    resp.Message = "挪移，TV-" + smg.DeviceCode + " 不是全自动状态！";
                    return resp;
                }
                if (smg.TaskID != 0)
                {
                    resp.Message = "挪移，TV-" + smg.DeviceCode + " 正在作业，请等待TV空闲后再进行挪移！";
                    return resp;
                }
                #endregion

                frlct.Status = EnmLocationStatus.Outing;
                tolct.Status = EnmLocationStatus.Entering;
                cwloctation.UpdateLocation(frlct);
                cwloctation.UpdateLocation(tolct);

                WorkTask queue = new WorkTask()
                {
                    IsMaster = 2,
                    Warehouse = warehouse,
                    DeviceCode = 11,
                    MasterType = EnmTaskType.Transpose,
                    TelegramType = 0,
                    SubTelegramType = 0,
                    HallCode = 11,
                    FromLctAddress = frlct.Address,
                    ToLctAddress = tolct.Address,
                    ICCardCode = frlct.ICCode,
                    Distance = frlct.WheelBase,
                    CarSize = frlct.CarSize,
                    CarWeight = frlct.CarWeight
                };
                resp = manager_queue.Add(queue);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + "  添加挪移入队列，源车位-" + frlct.Address + "，目的车位-" + tolct.Address);
                    resp.Message = "已经加入作业队列，请稍后！";
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return resp;
        }

        /// <summary>
        /// 手动移动，直接下发
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="code"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public Response ManualMove(int warehouse, int code, string address)
        {
            Response resp = new Response();
            try
            {
                #region
                Device smg = new CWDevice().Find(d => d.Warehouse == warehouse && d.DeviceCode == code);
                if (smg == null || smg.Type != EnmSMGType.ETV)
                {
                    resp.Message = "请输入正确的库区及设备号！";
                    return resp;
                }
                Location lct = new CWLocation().FindLocation(l => l.Warehouse == warehouse && l.Address == address);
                if (lct == null)
                {
                    resp.Message = "请输入正确的车位地址！";
                    return resp;
                }

                if (smg.Mode != EnmModel.Automatic)
                {
                    resp.Message = "TV不是全自动！";
                    return resp;
                }
                if (smg.IsAble == 0)
                {
                    resp.Message = "TV没有启用！";
                    return resp;
                }
                if (smg.IsAvailabe == 0)
                {
                    resp.Message = "TV不可接收新指令！";
                    return resp;
                }
                if (smg.TaskID != 0)
                {
                    ImplementTask itask = new CWTask().Find(smg.TaskID);
                    if (itask != null)
                    {
                        if (itask.Status != EnmTaskStatus.WillWaitForUnload)
                        {
                            resp.Message = "请等待TV完成作业，再执行移动！";
                            return resp;
                        }
                    }
                    else
                    {
                        resp.Message = "请等待TV完成作业，再执行移动！";
                        return resp;
                    }
                }
                #endregion
                #region
                ImplementTask task = new ImplementTask()
                {
                    Warehouse = warehouse,
                    DeviceCode = smg.DeviceCode,
                    Type = EnmTaskType.Move,
                    Status = EnmTaskStatus.TWaitforMove,
                    SendStatusDetail = EnmTaskStatusDetail.NoSend,
                    SendDtime = DateTime.Now,
                    CreateDate = DateTime.Now,
                    HallCode = 11,
                    FromLctAddress = smg.Address,
                    ToLctAddress = lct.Address,
                    ICCardCode = "",
                    Distance = 0,
                    CarSize = "",
                    CarWeight = 0,
                    IsComplete = 0
                };
                resp = manager.Add(task);
                if (resp.Code == 1)
                {
                    smg.SoonTaskID = smg.TaskID;
                    smg.TaskID = task.ID;
                    new CWDevice().Update(smg);
                    resp.Message = "正在移动，请等待！";
                }
               
                #endregion
            }
            catch (Exception ex)
            {
                clog.Error(ex.ToString());
            }
            return resp;
        }

        /// <summary>
        /// 将队列中的避让作业下发，
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public Response CreateAvoidTaskByQueue(WorkTask queue)
        {
            Log log = LogFactory.GetLogger("CWTask.CreateAvoidTaskByQueue");
            ImplementTask subtask = new ImplementTask()
            {
                Warehouse=queue.Warehouse,
                DeviceCode=queue.DeviceCode,
                Type=queue.MasterType,
                Status=EnmTaskStatus.TWaitforMove,
                SendStatusDetail=EnmTaskStatusDetail.NoSend,
                SendDtime=DateTime.Now,
                HallCode=queue.HallCode,
                FromLctAddress=queue.FromLctAddress,
                ToLctAddress=queue.ToLctAddress,
                ICCardCode=queue.ICCardCode,
                Distance=0,
                CarSize="",
                CarWeight=0,
                IsComplete=0
            };
            Response resp = manager.Add(subtask);
            if (resp.Code == 1)
            {
                Device dev = new CWDevice().Find(d => d.Warehouse == queue.Warehouse && d.DeviceCode == queue.DeviceCode);
                //如果是处于装载完成中，也允许先避让，
                //将当前作业ID加入待发送卸载中，
                //避让优先
                dev.SoonTaskID = dev.TaskID;
                dev.TaskID = subtask.ID;
                resp= new CWDevice().Update(dev);
                log.Info("生成避让，绑定作业，devicecode-"+dev.DeviceCode+" ,TaskID-"+dev.TaskID+" ,SoonTaskID-"+dev.SoonTaskID);
                //删除队列
                resp = manager_queue.Delete(queue.ID);
                log.Info("生成避让，删除队列，Message-" + resp.Message);
            }
            resp.Message = "生成避让，绑定作业成功！DeviceCode-"+subtask.DeviceCode + " ,TaskID-" + subtask.ID;
            
            return resp;
        }

        /// <summary>
        /// 将队列中的报文作业队列下发
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="dev">要执行作业的设备</param>
        /// <returns></returns>
        public Response CreateDeviceTaskByQueue(WorkTask queue,Device dev)
        {
            Log log = LogFactory.GetLogger("CWTask.CreateDeviceTaskByQueue");
            CWLocation cwlctn = new CWLocation();
            CWDevice cwdevice = new CWDevice();

            Response resp = new Response();
            EnmTaskStatus state = EnmTaskStatus.Init;
            #region
            if (dev.Type == EnmSMGType.ETV)
            {
                if (queue.TelegramType == 13 && queue.SubTelegramType == 1)
                {
                    state = EnmTaskStatus.TWaitforLoad;
                }
                else if (queue.TelegramType == 11 && queue.SubTelegramType == 1)
                {
                    state = EnmTaskStatus.TWaitforMove;
                }               
            }
            else if (dev.Type == EnmSMGType.Hall)
            {
                if (queue.TelegramType == 3 && queue.SubTelegramType == 1)
                {
                    state = EnmTaskStatus.OWaitforEVDown;
                }
                else if (queue.TelegramType == 2 && queue.SubTelegramType == 1)
                {
                    state = EnmTaskStatus.TempWaitforEVDown;
                }
            }
            if (state == EnmTaskStatus.Init)
            {
                log.Error("没有办法生成对应的报文状态，TelegramType-" +queue.TelegramType+ "  SubTelegramType-"+queue.SubTelegramType);
                return resp;
            }
            #endregion
            
            if (queue.MasterType == EnmTaskType.Transpose)
            {
                //如果当前要执行的作业是挪移作业
                #region 如果目的车位是2边，则判断后面的车位是否在等待作业，如果是，则不允许下发的
                if (dev.TaskID == 0)
                {
                    Location toLct = cwlctn.FindLocation(lc=>lc.Address==queue.ToLctAddress);
                    if (toLct != null)
                    {
                        if (toLct.LocSide == 2)
                        {
                            Location inner = cwlctn.FindLocation(lc => lc.Address == string.Format((toLct.LocSide + 2).ToString() + toLct.Address.Substring(1)));
                            if (inner != null)
                            {
                                if (inner.Status == EnmLocationStatus.Entering || inner.Status == EnmLocationStatus.Outing)
                                {
                                    resp.Message = "后面车位要进行出入库，暂不允许挪移！";
                                    return resp;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            if (dev.Type == EnmSMGType.ETV)
            {
                #region 生成挪移作业
                string toaddrs = "";
              
                    if (queue.MasterType == EnmTaskType.SaveCar)
                    {
                        toaddrs = queue.ToLctAddress;
                    }
                    else
                    {
                        toaddrs = queue.FromLctAddress;
                    }
               
                int wh = queue.Warehouse;
                Location tolctn = cwlctn.FindLocation(lc => lc.Address == toaddrs && lc.Warehouse == wh);
                if (tolctn != null)
                {
                    if (tolctn.LocSide == 4)
                    {
                        string side = "";
                        side = (tolctn.LocSide - 2).ToString();
                        string forwardaddrs = side + tolctn.Address.Substring(1);
                        Location forwardLctn = cwlctn.FindLocation(lc => lc.Address == forwardaddrs && lc.Warehouse == wh);
                        if (forwardLctn == null)
                        {
                            log.Error("系统错误，目标车位-" + toaddrs + ",库区- " + wh + ", 找不到其前面车位，地址-" + forwardaddrs);
                            return resp;
                        }

                        if (forwardLctn.Status == EnmLocationStatus.Occupy)
                        {
                            #region
                            //找出要挪移的车位
                            Location transLctn = new AllocateTV().AllocateTvNeedTransfer(dev, forwardLctn);
                            if (transLctn == null)
                            {
                                log.Error(string.Format("找不到{0}的挪移车位", forwardLctn));
                                return resp;
                            }
                            #region 更新车位
                            forwardLctn.Status = EnmLocationStatus.Outing;

                            transLctn.Status = EnmLocationStatus.Entering;
                            transLctn.ICCode = forwardLctn.ICCode;

                            cwlctn.UpdateLocation(forwardLctn);
                            cwlctn.UpdateLocation(transLctn);
                            #endregion
                            //生成挪移作业，绑定设备，当前作业暂不执行
                            ImplementTask transtask = new ImplementTask()
                            {
                                Warehouse = queue.Warehouse,
                                DeviceCode = dev.DeviceCode,
                                Type = EnmTaskType.Transpose,
                                Status = EnmTaskStatus.TWaitforLoad,
                                SendStatusDetail = EnmTaskStatusDetail.NoSend,
                                SendDtime = DateTime.Now,
                                CreateDate=DateTime.Now,
                                HallCode = 11,
                                FromLctAddress = forwardLctn.Address,
                                ToLctAddress = transLctn.Address,
                                ICCardCode = forwardLctn.ICCode,
                                Distance = forwardLctn.WheelBase,
                                CarSize = forwardLctn.CarSize,
                                CarWeight = forwardLctn.CarWeight,
                                IsComplete = 0
                            };
                            resp = manager.Add(transtask);
                            if (resp.Code == 1)
                            {
                                dev.SoonTaskID = 0;
                                dev.TaskID = transtask.ID;
                                resp = new CWDevice().Update(dev);
                                log.Info("转化为执行作业，绑定于设备，deviceCode-" + dev.DeviceCode+" ,TaskID-"+dev.TaskID);

                                #region 判断是否生成回挪作业
                                bool isBack = false;
                                Customer cust = new CWICCard().FindFixLocationByAddress(forwardLctn.Warehouse, forwardLctn.Address);
                                if (cust != null)
                                {
                                    isBack = true;
                                }
                                if (isBack || transLctn.Type == EnmLocationType.Temporary)
                                {
                                    WorkTask transback_queue = new WorkTask {
                                        IsMaster = 1,
                                        Warehouse = forwardLctn.Warehouse,
                                        DeviceCode = dev.DeviceCode,
                                        MasterType = EnmTaskType.Transpose,
                                        TelegramType = 13,
                                        SubTelegramType = 1,
                                        HallCode = 11,
                                        FromLctAddress = transLctn.Address,
                                        ToLctAddress = forwardLctn.Address,
                                        ICCardCode = forwardLctn.ICCode,
                                        Distance = forwardLctn.WheelBase,
                                        CarSize = forwardLctn.CarSize,
                                        CarWeight = forwardLctn.CarWeight
                                    };
                                    manager_queue.Add(transback_queue);
                                }
                                #endregion
                            }
                           
                            return resp;
                            #endregion
                        }
                        else if (forwardLctn.Status == EnmLocationStatus.Entering ||
                            forwardLctn.Status == EnmLocationStatus.Outing)
                        {
                            //前面的车位正在出入库，优先让其先完成,当前作业暂不执行
                            return resp;
                        }
                    }
                }
                else
                {
                    log.Error("系统错误，目标车位-" + toaddrs + ",库区- " + wh + ", 找不到其车位");
                    return resp;
                }
                #endregion
            }

            ImplementTask subtask = new ImplementTask()
            {
                Warehouse = queue.Warehouse,
                DeviceCode = queue.DeviceCode,
                Type = queue.MasterType,
                Status = state,
                SendStatusDetail = EnmTaskStatusDetail.NoSend,
                SendDtime = DateTime.Now,
                CreateDate = DateTime.Now,
                HallCode = queue.HallCode,
                FromLctAddress = queue.FromLctAddress,
                ToLctAddress = queue.ToLctAddress,
                ICCardCode = queue.ICCardCode,
                Distance = queue.Distance,
                CarSize = queue.CarSize,
                CarWeight = queue.CarWeight,
                IsComplete = 0
            };
            resp = manager.Add(subtask);
            if (resp.Code == 1)
            {               
                dev.SoonTaskID = 0;
                dev.TaskID = subtask.ID;
                resp = new CWDevice().Update(dev);
                log.Info("转化为执行作业，绑定于设备，devicode-" + dev.DeviceCode+" ,TaskID- "+dev.TaskID);
                //删除队列
                resp = manager_queue.Delete(queue.ID);
                log.Info("转化为执行作业，删除队列，WorkQueue ID-" + queue.ID);
            }
            resp.Message = "转化为执行作业，操作成功！DeviceCode-" + subtask.DeviceCode+ " ,TaskID- " + subtask.ID;
            
            return resp;
        }

        /// <summary>
        /// 处理ETV卸载作业,将队列中的卸载作业删除，
        /// 将原来等待卸载的作业，变为执行作业
        /// </summary>
        /// <param name="unloadQueue"></param>
        /// <param name="smg"></param>
        /// <returns></returns>
        public void DealTVUnloadTask(ImplementTask unloadtask, WorkTask unloadQueue)
        {
            unloadtask.Status = EnmTaskStatus.TWaitforUnload;
            unloadtask.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            unloadtask.SendDtime = DateTime.Now.AddMinutes(-1);
            manager.Update(unloadtask);

            manager_queue.Delete(unloadQueue.ID);
        }

        /// <summary>
        /// 判断作业是否可以实行，要不生成别的TV的避让作业
        /// </summary>
        /// <param name="task"></param>
        /// <param name="dev">ETV</param>
        /// <returns></returns>
        public bool DealAvoid(WorkTask queue,Device smg)
        {
            Log log = LogFactory.GetLogger("CWTask.DealAvoid");
            try
            {
                #region
                CWTask cwtask = new CWTask();

                if (queue.IsMaster == 2)
                {
                    return false;
                }
                int nWarehouse = smg.Warehouse;
                List<Device> Etvs = new CWDevice().FindList(d => d.Type == EnmSMGType.ETV);
                int curEtvCol = BasicClss.GetColumnByAddrs(smg.Address);
                string toAddrs = "";
                if (queue.TelegramType == 13 && queue.SubTelegramType == 1)
                {
                    toAddrs = queue.FromLctAddress;
                }
                else
                {
                    toAddrs = queue.ToLctAddress;
                }
                int curToCol = BasicClss.GetColumnByAddrs(toAddrs);

                int curMax;
                int curMin;
                #region
                if (curEtvCol > curToCol)
                {
                    curMax = curEtvCol;
                    curMin = curToCol - 3;
                    if (curMin < 1)
                    {
                        curMin = 1;
                    }
                }
                else
                {
                    curMax = curToCol + 3;
                    if (curMax > 18)
                    {
                        curMax = 18;
                    }
                    curMin = curEtvCol;
                }
                #endregion
                //对面ETV
                Device otherEtv = null;
                #region
                foreach (Device et in Etvs)
                {
                    if (et.DeviceCode != smg.DeviceCode)
                    {
                        otherEtv = et;
                        break;
                    }
                }
                #endregion
                if (otherEtv == null)
                {
                    return false;
                }
                int otherCol = BasicClss.GetColumnByAddrs(otherEtv.Address);
                #region ETV1与ETV2的位置信号不对，即 列数与实际不合的，先不让其去执行
                bool isTrue = true;
                if (smg.DeviceCode > otherEtv.DeviceCode)
                {
                    if (curEtvCol < otherCol)
                    {
                        isTrue = false;
                    }
                }
                else
                {
                    if (curEtvCol > otherCol)
                    {
                        isTrue = false;
                    }
                }
                if (!isTrue)
                {
                    string msg = String.Format("异常： ETV{0} 当前列{1},ETV{2} 当前列{3}", smg.DeviceCode, curEtvCol, otherEtv.DeviceCode, otherCol);
                    log.Error(msg);
                    return false;
                }
                #endregion
                if (otherEtv.TaskID == 0)
                {
                    #region 另一台ETV空闲的
                    if (curMin < otherCol && otherCol < curMax)
                    {
                        if (otherEtv.IsAble == 0)
                        {
                            return false;
                        }
                        #region 生成避让作业
                        string oList = "";
                        if (curEtvCol > curToCol)     //需向左避让
                        {
                            oList = curMin.ToString().PadLeft(2, '0');
                        }
                        else
                        {
                            oList = curMax.ToString().PadLeft(2, '0');
                        }
                        string toAddress = string.Concat("2", oList, otherEtv.Address.Substring(3));

                        if (otherEtv.IsAvailabe == 1)
                        {
                            #region 直接下发
                            ImplementTask subtask = new ImplementTask()
                            {
                                Warehouse = otherEtv.Warehouse,
                                DeviceCode = otherEtv.DeviceCode,
                                Type = EnmTaskType.Avoid,
                                Status = EnmTaskStatus.TWaitforMove,
                                SendStatusDetail = EnmTaskStatusDetail.NoSend,
                                SendDtime = DateTime.Now,
                                CreateDate=DateTime.Now,
                                HallCode = queue.HallCode,
                                FromLctAddress = otherEtv.Address,
                                ToLctAddress = toAddress,
                                ICCardCode = queue.ICCardCode,
                                Distance = 0,
                                CarSize = "",
                                CarWeight = 0,
                                IsComplete = 0
                            };
                            Response resp = manager.Add(subtask);
                            if (resp.Code == 1)
                            {
                                otherEtv.SoonTaskID = 0;
                                otherEtv.TaskID = subtask.ID;
                                resp = new CWDevice().Update(otherEtv);
                                log.Info("生成避让作业，并绑定于设备 , 避让卡号- " + queue.ICCardCode + " , 目的车位-" + toAddress);                               
                            }
                            return true;
                            #endregion
                        }
                        else
                        {
                            #region 加入队列
                            WorkTask queue_avoid = new WorkTask()
                            {
                                IsMaster = 1,
                                Warehouse = otherEtv.Warehouse,
                                DeviceCode = otherEtv.DeviceCode,
                                MasterType = EnmTaskType.Avoid,
                                TelegramType = 11,
                                SubTelegramType = 1,
                                HallCode = 11,
                                FromLctAddress = otherEtv.Address,
                                ToLctAddress = toAddress,
                                ICCardCode = queue.ICCardCode,
                                Distance = 0,
                                CarSize = "",
                                CarWeight = 0
                            };
                            Response resp = manager_queue.Add(queue_avoid);
                            if (resp.Code == 1)
                            {
                                log.Info("添加避让入队列，避让卡号-" + queue_avoid.ICCardCode + "，目的车位-" + toAddress);
                            }
                            #endregion
                            return true;
                        }
                        #endregion
                    }
                    return true;
                    #endregion
                }
                else  //另一台ETV在忙
                {
                    ImplementTask othertak = cwtask.Find(otherEtv.TaskID);

                    #region 要下发的作业的TV 处于卸载等待时
                    if (smg.TaskID != 0)
                    {
                        //两TV处于等待卸载中
                        ImplementTask itask = cwtask.Find(smg.TaskID);

                        if (itask.Status == EnmTaskStatus.WillWaitForUnload ||
                            othertak.Status == EnmTaskStatus.WillWaitForUnload)
                        {
                            #region 如果是前后排的关系，则在卸载时优先后面的先动作                      
                            string curAddrs = itask.ToLctAddress.Substring(1);
                            string otherAddrs = othertak.ToLctAddress.Substring(1);
                            //终点的坐标一致
                            if (curAddrs == otherAddrs)
                            {
                                //如果是前后排关系
                                int curSide = Convert.ToInt32(itask.ToLctAddress.Substring(0, 1));
                                int otherSide = Convert.ToInt32(othertak.ToLctAddress.Substring(0, 1));
                                //同列卸载，优先前面的作业
                                if (curSide == 3 && otherSide == 1)
                                {
                                    return false;
                                }
                                if (curSide == 4 && otherSide == 2)
                                {
                                    return false;
                                }
                            }
                            #endregion
                        }

                        if (othertak.Status == EnmTaskStatus.WillWaitForUnload)
                        {
                            if (curMin < otherCol && otherCol < curMax)
                            {
                                if (otherEtv.IsAvailabe == 0)
                                {
                                    return false;
                                }
                                #region 生成避让作业    
                                string oList = "";
                                if (curEtvCol > curToCol)     //需向左避让
                                {
                                    oList = curMin.ToString().PadLeft(2, '0');
                                }
                                else
                                {
                                    oList = curMax.ToString().PadLeft(2, '0');
                                }
                                string toAddress = string.Concat("2", oList, otherEtv.Address.Substring(3));

                                ImplementTask subtask = new ImplementTask()
                                {
                                    Warehouse = otherEtv.Warehouse,
                                    DeviceCode = otherEtv.DeviceCode,
                                    Type = EnmTaskType.Avoid,
                                    Status = EnmTaskStatus.TWaitforMove,
                                    SendStatusDetail = EnmTaskStatusDetail.NoSend,
                                    SendDtime = DateTime.Now,
                                    CreateDate=DateTime.Now,
                                    HallCode = queue.HallCode,
                                    FromLctAddress = otherEtv.Address,
                                    ToLctAddress = toAddress,
                                    ICCardCode = queue.ICCardCode,
                                    Distance = 0,
                                    CarSize = "",
                                    CarWeight = 0,
                                    IsComplete = 0
                                };
                                Response resp = manager.Add(subtask);
                                if (resp.Code == 1)
                                {
                                    otherEtv.SoonTaskID = othertak.ID;
                                    otherEtv.TaskID = subtask.ID;
                                    resp = new CWDevice().Update(otherEtv);
                                    log.Info("生成避让作业，并绑定于设备 , 避让卡号- " + queue.ICCardCode + " , 目的车位-" + toAddress);

                                    manager.SaveChanges();

                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                                #endregion
                            }
                            return true;
                        }
                    }
                    #endregion

                    string oToAddrs = "";
                    if (othertak.Status == EnmTaskStatus.WillWaitForUnload ||
                        othertak.Status == EnmTaskStatus.TWaitforUnload)
                    {
                        oToAddrs = othertak.ToLctAddress;
                    }
                    else
                    {
                        oToAddrs = othertak.FromLctAddress;
                    }
                    int toColumn = BasicClss.GetColumnByAddrs(oToAddrs);

                    #region 如果当前TV还没有进行动作，则这里就限制其下发
                    if (otherCol > curMin && otherCol < curMax)
                    {
                        return false;
                    }
                    if (toColumn > curMin && toColumn < curMax)
                    {
                        return false;
                    }
                    if (otherCol > curMax)
                    {
                        if (toColumn < curMax)
                        {
                            return false;
                        }
                    }
                    if (toColumn > curMax)
                    {
                        if (toColumn < curMax)
                        {
                            return false;
                        }
                    }
                    if (otherCol < curMin)
                    {
                        if (toColumn > curMin)
                        {
                            return false;
                        }
                    }
                    if (toColumn < curMin)
                    {
                        if (otherCol > curMin)
                        {
                            return false;
                        }
                    }
                    #endregion
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return false;
            }            
        }

        /// <summary>
        /// 发送车厅报文，组装TV报文
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="lct"></param>
        /// <returns></returns>
        public Response SendHallTelegramAndBuildTV(WorkTask master, Location lct, Device hall)
        {
            Log log = LogFactory.GetLogger("CWTask.SendHallTelegramAndBuildTV");
            #region
            Response resp = new Response();
            CWDevice cwdevice = new CWDevice();
            CWLocation cwlctn = new CWLocation();

            Device tv = new AllocateTV().Allocate(hall, lct);
            if (tv == null)
            {
                log.Error("队列-卡号：" + master.ICCardCode + " 车厅："
                    + master.DeviceCode + " 车位：" + lct.Address + " 在执行时找不到TV！");
                return resp;
            }
            if (tv.Mode != EnmModel.Automatic)
            {
                log.Error("队列-卡号：" + master.ICCardCode + " 车厅："
                    + master.DeviceCode + " 车位：" + lct.Address + " 在执行时TV不是全自动模式！");
                return resp;
            }
            EnmTaskStatus state = EnmTaskStatus.Init;
            if (master.MasterType == EnmTaskType.TempGet)
            {
                state = EnmTaskStatus.TempWaitforEVDown;
            }
            else if (master.MasterType == EnmTaskType.GetCar)
            {
                state = EnmTaskStatus.OWaitforEVDown;
            }
            else
            {
                log.Error("队列-卡号：" + master.ICCardCode + " 车厅："
                   + master.DeviceCode + " 车位：" + lct.Address + " 在执行时,作业类型不对-" + master.MasterType.ToString());
                return resp;
            }

            ImplementTask hallTask = new ImplementTask()
            {
                Warehouse = master.Warehouse,
                DeviceCode = master.DeviceCode,
                Type = master.MasterType,
                Status = state,
                SendStatusDetail = EnmTaskStatusDetail.NoSend,
                SendDtime = DateTime.Now,
                CreateDate=DateTime.Now,
                HallCode = master.HallCode,
                FromLctAddress = master.FromLctAddress,
                ToLctAddress = master.ToLctAddress,
                ICCardCode = master.ICCardCode,
                Distance = master.Distance,
                CarSize = master.CarSize,
                CarWeight = master.CarWeight,
                IsComplete = 0
            };
            resp = manager.Add(hallTask);
            if (resp.Code == 1)
            {
                hall.TaskID = hallTask.ID;
                hall.SoonTaskID = 0;
                cwdevice.Update(hall);
            }
           
            bool isAdd = false;
            if (tv.IsAble == 1 && tv.IsAvailabe == 1)
            {
                if (tv.TaskID == 0)
                {
                    //要判断是否生成倒库作业
                    #region 生成挪移作业
                    string toaddrs = master.ToLctAddress;                   
                    int wh = master.Warehouse;
                    Location tolctn = cwlctn.FindLocation(lc => lc.Address == toaddrs && lc.Warehouse == wh);
                    if (tolctn != null)
                    {
                        if (tolctn.LocSide == 4)
                        {
                            string side = "";
                            side = (tolctn.LocSide - 2).ToString();
                            string forwardaddrs = side + tolctn.Address.Substring(1);
                            Location forwardLctn = cwlctn.FindLocation(lc => lc.Address == forwardaddrs && lc.Warehouse == wh);
                            if (forwardLctn == null)
                            {
                                log.Error("系统错误，目标车位-" + toaddrs + ",库区- " + wh + ", 找不到其前面车位，地址-" + forwardaddrs);
                                return resp;
                            }

                            if (forwardLctn.Status == EnmLocationStatus.Occupy)
                            {
                                #region
                                //找出要挪移的车位
                                Location transLctn = new AllocateTV().AllocateTvNeedTransfer(tv, forwardLctn);
                                if (transLctn == null)
                                {
                                    log.Error(string.Format("找不到{0}的挪移车位", forwardLctn));
                                    return resp;
                                }
                                #region 更新车位
                                forwardLctn.Status = EnmLocationStatus.Outing;

                                transLctn.Status = EnmLocationStatus.Entering;
                                transLctn.ICCode = forwardLctn.ICCode;

                                cwlctn.UpdateLocation(forwardLctn);
                                cwlctn.UpdateLocation(transLctn);
                                #endregion
                                //生成挪移作业，绑定设备，生成当前TV装载作业，加入队列
                                ImplementTask transtask = new ImplementTask()
                                {
                                    Warehouse = master.Warehouse,
                                    DeviceCode = master.DeviceCode,
                                    Type = EnmTaskType.Transpose,
                                    Status = EnmTaskStatus.TWaitforLoad,
                                    SendStatusDetail = EnmTaskStatusDetail.NoSend,
                                    SendDtime = DateTime.Now,
                                    CreateDate = DateTime.Now,
                                    HallCode = 11,
                                    FromLctAddress = forwardLctn.Address,
                                    ToLctAddress = transLctn.Address,
                                    ICCardCode = forwardLctn.ICCode,
                                    Distance = forwardLctn.WheelBase,
                                    CarSize = forwardLctn.CarSize,
                                    CarWeight = forwardLctn.CarWeight,
                                    IsComplete = 0
                                };
                                resp = manager.Add(transtask);
                                if (resp.Code == 1)
                                {
                                    tv.SoonTaskID = 0;
                                    tv.TaskID = transtask.ID;
                                    resp = new CWDevice().Update(tv);
                                    log.Info("转化为执行作业，绑定于设备，Message-" + resp.Message);

                                    #region 生成原来TV装载作业，同时删除该队列

                                    WorkTask waitqueue = new WorkTask()
                                    {
                                        IsMaster = 1,
                                        Warehouse = tv.Warehouse,
                                        DeviceCode = tv.DeviceCode,
                                        MasterType = master.MasterType,
                                        TelegramType = 13,
                                        SubTelegramType = 1,
                                        HallCode = hall.DeviceCode,
                                        FromLctAddress = master.FromLctAddress,
                                        ToLctAddress = master.ToLctAddress,
                                        ICCardCode = master.ICCardCode,
                                        Distance = master.Distance,
                                        CarSize = master.CarSize,
                                        CarWeight = master.CarWeight
                                    };
                                    manager_queue.Add(waitqueue);

                                    //删除队列
                                    resp = manager_queue.Delete(master.ID);

                                    #endregion

                                    #region 判断是否生成回挪作业
                                    bool isBack = false;
                                    Customer cust = new CWICCard().FindFixLocationByAddress(forwardLctn.Warehouse, forwardLctn.Address);
                                    if (cust != null)
                                    {
                                        isBack = true;
                                    }
                                    if (isBack || transLctn.Type == EnmLocationType.Temporary)
                                    {
                                        WorkTask transback_queue = new WorkTask
                                        {
                                            IsMaster = 1,
                                            Warehouse = forwardLctn.Warehouse,
                                            DeviceCode = tv.DeviceCode,
                                            MasterType = EnmTaskType.Transpose,
                                            TelegramType = 13,
                                            SubTelegramType = 1,
                                            HallCode = 11,
                                            FromLctAddress = transLctn.Address,
                                            ToLctAddress = forwardLctn.Address,
                                            ICCardCode = forwardLctn.ICCode,
                                            Distance = forwardLctn.WheelBase,
                                            CarSize = forwardLctn.CarSize,
                                            CarWeight = forwardLctn.CarWeight
                                        };
                                        manager_queue.Add(transback_queue);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    log.Info("提前装载时，生成执行作业，加入队列时异常，Message-" + resp.Message);
                                }

                                return resp;
                                #endregion
                            }
                            else if (forwardLctn.Status == EnmLocationStatus.Entering ||
                                forwardLctn.Status == EnmLocationStatus.Outing)
                            {
                                //前面的车位正在出入库，优先让其先完成,当前作业暂不执行
                                #region 生成原来TV装载作业，加入队列，同时删除该队列

                                WorkTask waitqueue = new WorkTask()
                                {
                                    IsMaster = 1,
                                    Warehouse = tv.Warehouse,
                                    DeviceCode = tv.DeviceCode,
                                    MasterType = master.MasterType,
                                    TelegramType = 13,
                                    SubTelegramType = 1,
                                    HallCode = hall.DeviceCode,
                                    FromLctAddress = master.FromLctAddress,
                                    ToLctAddress = master.ToLctAddress,
                                    ICCardCode = master.ICCardCode,
                                    Distance = master.Distance,
                                    CarSize = master.CarSize,
                                    CarWeight = master.CarWeight
                                };
                                manager_queue.Add(waitqueue);

                                //删除队列
                                resp = manager_queue.Delete(master.ID);
                                
                                #endregion

                                return resp;
                            }
                        }
                    }
                    else
                    {
                        log.Error("系统错误，目标车位-" + toaddrs + ",库区- " + wh + ", 找不到其车位");                       
                    }
                    #endregion

                    ImplementTask TvTask = new ImplementTask()
                    {
                        Warehouse = tv.Warehouse,
                        DeviceCode = tv.DeviceCode,
                        Type = master.MasterType,
                        Status = EnmTaskStatus.TWaitforLoad,
                        SendStatusDetail = EnmTaskStatusDetail.NoSend,
                        SendDtime = DateTime.Now,
                        CreateDate = DateTime.Now,
                        HallCode = master.HallCode,
                        FromLctAddress = master.FromLctAddress,
                        ToLctAddress = master.ToLctAddress,
                        ICCardCode = master.ICCardCode,
                        Distance = master.Distance,
                        CarSize = master.CarSize,
                        CarWeight = master.CarWeight,
                        IsComplete = 0
                    };
                    resp = manager.Add(TvTask);
                    if (resp.Code == 1)
                    {
                        tv.TaskID = TvTask.ID;
                        tv.SoonTaskID = 0;
                        cwdevice.Update(tv);
                    }
                }
                else
                {
                    isAdd = true;
                }
            }
            else
            {
                isAdd = true;
            }

            if (isAdd)
            {
                WorkTask waitqueue = new WorkTask()
                {
                    IsMaster = 1,
                    Warehouse = tv.Warehouse,
                    DeviceCode = tv.DeviceCode,
                    MasterType = master.MasterType,
                    TelegramType = 13,
                    SubTelegramType = 1,
                    HallCode = hall.DeviceCode,
                    FromLctAddress = master.FromLctAddress,
                    ToLctAddress = master.ToLctAddress,
                    ICCardCode = master.ICCardCode,
                    Distance = master.Distance,
                    CarSize = master.CarSize,
                    CarWeight = master.CarWeight
                };
                manager_queue.Add(waitqueue);
            }

            //删除队列
            resp = manager_queue.Delete(master.ID);
            
            #endregion
            return resp;
        }

        /// <summary>
        /// 提前装载，组装车厅报文加入队列
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="lct"></param>
        /// <returns></returns>
        public Response AheadTvTelegramAndBuildHall(WorkTask master,Location lct, Device hall)
        {
            Log log = LogFactory.GetLogger("CWTask.AheadTvTelegramAndBuildHall");
            Response resp = new Response();
            CWDevice cwdevice = new CWDevice();
            CWLocation cwlctn = new CWLocation();
            try
            {
                Device tv = new AllocateTV().Allocate(hall, lct);
                if (tv == null)
                {
                    log.Error("队列-卡号：" + master.ICCardCode + " 车厅："
                        + master.DeviceCode + " 车位：" + lct.Address + " 在执行时找不到TV！");
                    return resp;
                }
                if (tv.Mode != EnmModel.Automatic)
                {
                    log.Error("队列-卡号：" + master.ICCardCode + " 车厅："
                        + master.DeviceCode + " 车位：" + lct.Address + " 在执行时TV不是全自动模式！");
                    return resp;
                }
                if (tv.IsAble == 1 && tv.IsAvailabe == 1)
                {
                    if (tv.TaskID == 0)
                    {
                        bool isCreateITask = true;
                        //要判断是否生成倒库作业
                        #region 生成挪移作业
                        string toaddrs = master.ToLctAddress;
                        int wh = master.Warehouse;
                        Location tolctn = cwlctn.FindLocation(lc => lc.Address == toaddrs && lc.Warehouse == wh);
                        if (tolctn != null)
                        {
                            if (tolctn.LocSide == 4)
                            {
                                string side = "";
                                side = (tolctn.LocSide - 2).ToString();
                                string forwardaddrs = side + tolctn.Address.Substring(1);
                                Location forwardLctn = cwlctn.FindLocation(lc => lc.Address == forwardaddrs && lc.Warehouse == wh);
                                if (forwardLctn == null)
                                {
                                    log.Error("系统错误，目标车位-" + toaddrs + ",库区- " + wh + ", 找不到其前面车位，地址-" + forwardaddrs);
                                    return resp;
                                }

                                if (forwardLctn.Status == EnmLocationStatus.Occupy)
                                {
                                    #region
                                    //找出要挪移的车位
                                    Location transLctn = new AllocateTV().AllocateTvNeedTransfer(tv, forwardLctn);
                                    if (transLctn == null)
                                    {
                                        log.Error(string.Format("找不到{0}的挪移车位", forwardLctn));
                                        return resp;
                                    }
                                    #region 更新车位
                                    forwardLctn.Status = EnmLocationStatus.Outing;

                                    transLctn.Status = EnmLocationStatus.Entering;
                                    transLctn.ICCode = forwardLctn.ICCode;

                                    cwlctn.UpdateLocation(forwardLctn);
                                    cwlctn.UpdateLocation(transLctn);
                                    #endregion
                                    //生成挪移作业，绑定设备，生成当前TV装载作业，加入队列
                                    ImplementTask transtask = new ImplementTask()
                                    {
                                        Warehouse = master.Warehouse,
                                        DeviceCode = master.DeviceCode,
                                        Type = EnmTaskType.Transpose,
                                        Status = EnmTaskStatus.TWaitforLoad,
                                        SendStatusDetail = EnmTaskStatusDetail.NoSend,
                                        SendDtime = DateTime.Now,
                                        CreateDate = DateTime.Now,
                                        HallCode = 11,
                                        FromLctAddress = forwardLctn.Address,
                                        ToLctAddress = transLctn.Address,
                                        ICCardCode = forwardLctn.ICCode,
                                        Distance = forwardLctn.WheelBase,
                                        CarSize = forwardLctn.CarSize,
                                        CarWeight = forwardLctn.CarWeight,
                                        IsComplete = 0
                                    };
                                    resp = manager.Add(transtask);
                                    if (resp.Code == 1)
                                    {
                                        tv.SoonTaskID = 0;
                                        tv.TaskID = transtask.ID;
                                        resp = new CWDevice().Update(tv);
                                        log.Info("转化为执行作业，绑定于设备，Message-" + resp.Message);

                                        isCreateITask = false;
                                        #region 生成原来TV装载作业，同时删除该队列
                                        WorkTask waitqueue = new WorkTask()
                                        {
                                            IsMaster = 1,
                                            Warehouse = tv.Warehouse,
                                            DeviceCode = tv.DeviceCode,
                                            MasterType = master.MasterType,
                                            TelegramType = 13,
                                            SubTelegramType = 1,
                                            HallCode = hall.DeviceCode,
                                            FromLctAddress = master.FromLctAddress,
                                            ToLctAddress = master.ToLctAddress,
                                            ICCardCode = master.ICCardCode,
                                            Distance = master.Distance,
                                            CarSize = master.CarSize,
                                            CarWeight = master.CarWeight
                                        };
                                        manager_queue.Add(waitqueue);
                                        #endregion

                                        #region 判断是否生成回挪作业
                                        bool isBack = false;
                                        Customer cust = new CWICCard().FindFixLocationByAddress(forwardLctn.Warehouse, forwardLctn.Address);
                                        if (cust != null)
                                        {
                                            isBack = true;
                                        }
                                        if (isBack || transLctn.Type == EnmLocationType.Temporary)
                                        {
                                            WorkTask transback_queue = new WorkTask
                                            {
                                                IsMaster = 1,
                                                Warehouse = forwardLctn.Warehouse,
                                                DeviceCode = tv.DeviceCode,
                                                MasterType = EnmTaskType.Transpose,
                                                TelegramType = 13,
                                                SubTelegramType = 1,
                                                HallCode = 11,
                                                FromLctAddress = transLctn.Address,
                                                ToLctAddress = forwardLctn.Address,
                                                ICCardCode = forwardLctn.ICCode,
                                                Distance = forwardLctn.WheelBase,
                                                CarSize = forwardLctn.CarSize,
                                                CarWeight = forwardLctn.CarWeight
                                            };
                                            manager_queue.Add(transback_queue);
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        log.Info("提前装载时，生成执行作业，加入队列时异常，Message-" + resp.Message);
                                    }
                                    #endregion                                    

                                    manager.SaveChanges();
                                }
                                else if (forwardLctn.Status == EnmLocationStatus.Entering ||
                                    forwardLctn.Status == EnmLocationStatus.Outing)
                                {                                  
                                    return resp;
                                }
                            }
                        }
                        else
                        {
                            log.Error("系统错误，目标车位-" + toaddrs + ",库区- " + wh + ", 找不到其车位");
                            return resp;
                        }
                        #endregion

                        if (isCreateITask)
                        {
                            ImplementTask TvTask = new ImplementTask()
                            {
                                Warehouse = tv.Warehouse,
                                DeviceCode = tv.DeviceCode,
                                Type = master.MasterType,
                                Status = EnmTaskStatus.TWaitforLoad,
                                SendStatusDetail = EnmTaskStatusDetail.NoSend,
                                SendDtime = DateTime.Now,
                                CreateDate = DateTime.Now,
                                HallCode = master.HallCode,
                                FromLctAddress = master.FromLctAddress,
                                ToLctAddress = master.ToLctAddress,
                                ICCardCode = master.ICCardCode,
                                Distance = master.Distance,
                                CarSize = master.CarSize,
                                CarWeight = master.CarWeight,
                                IsComplete = 0
                            };
                            resp = manager.Add(TvTask);
                            if (resp.Code == 1)
                            {
                                tv.TaskID = TvTask.ID;
                                tv.SoonTaskID = 0;
                                cwdevice.Update(tv);
                            }
                        }
                        //生成车厅作业，加入队列中
                        int ttype = 0;
                        int subtype = 0;
                        if (master.MasterType == EnmTaskType.GetCar)
                        {
                            ttype = 3;
                            subtype = 1;
                        }
                        else if(master.MasterType == EnmTaskType.TempGet)
                        {
                            ttype = 2;
                            subtype = 1;
                        }
                        else
                        {
                            log.Error("队列-卡号：" + master.ICCardCode + " 车厅：" + master.DeviceCode + " 在执行时,作业类型不对-" + master.MasterType.ToString());                            
                        }
                        //添加车厅作业，加入队列
                        WorkTask waitHallQueue = new WorkTask()
                        {
                            IsMaster = 1,
                            Warehouse = hall.Warehouse,
                            DeviceCode = hall.DeviceCode,
                            MasterType = master.MasterType,
                            TelegramType = ttype,
                            SubTelegramType = subtype,
                            HallCode = hall.DeviceCode,
                            FromLctAddress = master.FromLctAddress,
                            ToLctAddress = master.ToLctAddress,
                            ICCardCode = master.ICCardCode,
                            Distance = master.Distance,
                            CarSize = master.CarSize,
                            CarWeight = master.CarWeight
                        };
                        manager_queue.Add(waitHallQueue);

                        //删除队列
                        manager_queue.Delete(master.ID);
                        
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 没有作业时，查询缓冲车位，如果是占用的，
        /// 则强制生成回挪作业，释放缓存车位
        /// </summary>
        /// <param name="warehouse"></param>
        public void DealTempLocOccupy(int warehouse)
        {
            List<Location> tempOccupyLst = new CWLocation().FindLocationList(loc =>
                                            loc.Type == EnmLocationType.Temporary &&
                                            loc.Status == EnmLocationStatus.Occupy);
            if (tempOccupyLst.Count == 0)
            {
                return;
            }
            #region
            Location frLctn = tempOccupyLst[0];
            Location toLctn;
            Device etv = new AllocateTV().AllocateTvOfTransport(frLctn, out toLctn);
            if (etv == null)
            {
                return;
            }
            if (toLctn == null)
            {
                return;
            }
            if (etv.TaskID==0&&etv.IsAvailabe == 1)
            {
                //下发
                ImplementTask TvTask = new ImplementTask()
                {
                    Warehouse = etv.Warehouse,
                    DeviceCode = etv.DeviceCode,
                    Type = EnmTaskType.Transpose,
                    Status = EnmTaskStatus.TWaitforLoad,
                    SendStatusDetail = EnmTaskStatusDetail.NoSend,
                    SendDtime = DateTime.Now,
                    CreateDate = DateTime.Now,
                    HallCode = 11,
                    FromLctAddress = frLctn.Address,
                    ToLctAddress = toLctn.Address,
                    ICCardCode = frLctn.ICCode,
                    Distance = frLctn.WheelBase,
                    CarSize = frLctn.CarSize,
                    CarWeight = frLctn.CarWeight,
                    IsComplete = 0
                };
                Response resp = manager.Add(TvTask);
                if (resp.Code == 1)
                {
                    etv.TaskID = TvTask.ID;
                    etv.SoonTaskID = 0;
                    new CWDevice().Update(etv);
                }
            }
            else
            {
                //加入队列
                WorkTask tvQueue = new WorkTask()
                {
                    IsMaster = 1,
                    Warehouse = etv.Warehouse,
                    DeviceCode = etv.DeviceCode,
                    MasterType = EnmTaskType.Transpose,
                    TelegramType = 13,
                    SubTelegramType = 1,
                    HallCode = 11,
                    FromLctAddress = frLctn.Address,
                    ToLctAddress = toLctn.Address,
                    ICCardCode =frLctn.ICCode,
                    Distance = frLctn.WheelBase,
                    CarSize = frLctn.CarSize,
                    CarWeight = frLctn.CarWeight
                };
                manager_queue.Add(tvQueue);
            }
            //修改车位状态
            frLctn.Status = EnmLocationStatus.Outing;
            toLctn.Status = EnmLocationStatus.Entering;
            CWLocation cwlctn = new CWLocation();
            cwlctn.UpdateLocation(frLctn);
            cwlctn.UpdateLocation(toLctn);
            #endregion
            
        }


        #region 队列管理
        private static WorkTaskManager manager_queue = new WorkTaskManager();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public WorkTask FindQueue(Expression<Func<WorkTask, bool>> where)
        {
            return manager_queue.Find(where);
        }

        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public Response DeleteQueue(WorkTask queue)
        {
            return manager_queue.Delete(queue.ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public List<WorkTask> FindQueueList(Expression<Func<WorkTask, bool>> where)
        {
            return manager_queue.FindList(where);
        }

        /// <summary>
        /// 依分页条件查询所有
        /// </summary>
        /// <param name="pageWork"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Page<WorkTask> FindPageList(Page<WorkTask> pageWork, OrderParam param)
        {
            if (param == null)
            {
                param = new OrderParam()
                {
                    PropertyName = "ID",
                    Method = OrderMethod.Asc
                };
            }
            Page<WorkTask> page = manager_queue.FindPageList(pageWork, param);
            return page;
        }

        /// <summary>
        /// 依查询条件，查找相应分页信息
        /// </summary>
        /// <param name="pageWork"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Page<WorkTask> FindPagelist(Page<WorkTask> pageWork,Expression<Func<WorkTask,bool>> where, OrderParam param)
        {
            if (param == null)
            {
                param = new OrderParam()
                {
                    PropertyName = "ID",
                    Method = OrderMethod.Asc
                };
            }
            Page<WorkTask> page = manager_queue.FindPageList(pageWork,where,param);
            return page;
        }
        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Response DeleteQueue(int ID)
        {
            CWLocation cwlctn = new CWLocation();
            Response resp = new Response();
            WorkTask queue = manager_queue.Find(ID);
            if (queue == null)
            {
                resp.Code = 0;
                resp.Message = "系统故障，找不到队列，ID-" + ID;
                return resp;
            }
            if (queue.IsMaster == 2)
            {
                Location toLctn = cwlctn.FindLocation(lc=>lc.Warehouse==queue.Warehouse&&lc.Address==queue.FromLctAddress);
                if (toLctn != null)
                {
                    toLctn.Status = EnmLocationStatus.Occupy;
                    cwlctn.UpdateLocation(toLctn);
                }
            }
            else if (queue.IsMaster == 1)
            {
                if (queue.TelegramType == 13 && queue.SubTelegramType == 1)
                {
                    #region
                    if (queue.MasterType == EnmTaskType.SaveCar)
                    {
                        Location toLctn = cwlctn.FindLocation(lc => lc.Warehouse == queue.Warehouse && lc.Address == queue.ToLctAddress);
                        if (toLctn != null)
                        {
                            toLctn.Status = EnmLocationStatus.Occupy;
                            cwlctn.UpdateLocation(toLctn);
                        }
                    }
                    else if (queue.MasterType == EnmTaskType.GetCar)
                    {
                        Location frLctn = cwlctn.FindLocation(lc => lc.Warehouse == queue.Warehouse && lc.Address == queue.FromLctAddress);
                        if (frLctn != null)
                        {
                            frLctn.Status = EnmLocationStatus.Occupy;
                            cwlctn.UpdateLocation(frLctn);
                        }
                    }
                    else if(queue.MasterType == EnmTaskType.Transpose)
                    {
                        Location frLctn = cwlctn.FindLocation(lc => lc.Warehouse == queue.Warehouse && lc.Address == queue.FromLctAddress);                        
                        Location toLctn = cwlctn.FindLocation(lc => lc.Warehouse == queue.Warehouse && lc.Address == queue.ToLctAddress);
                        if (frLctn!=null&&toLctn != null)
                        {
                            frLctn.Status = EnmLocationStatus.Occupy;
                            cwlctn.UpdateLocation(frLctn);

                            toLctn.Status = EnmLocationStatus.Space;
                            cwlctn.UpdateLocation(toLctn);
                        }
                    }
                    #endregion
                }
                else if (queue.TelegramType == 14 && queue.SubTelegramType == 1)
                {
                    resp.Code = 0;
                    resp.Message = "当前队列为卸载队列，不允许删除，ID-" + ID;
                    return resp;
                }
            }
            resp= manager_queue.Delete(ID);
           
            return resp;
        }

        /// <summary>
        /// 获取车厅所有取车数量
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        /// <returns></returns>
        public int GetHallGetCarCount(int warehouse,int hallID)
        {
            List<WorkTask> queues = manager_queue.FindList(qu => qu.Warehouse == warehouse &&
                                                                qu.DeviceCode == hallID &&
                                                                qu.IsMaster == 2 &&
                                                                (qu.MasterType == EnmTaskType.GetCar || qu.MasterType == EnmTaskType.TempGet));
            return queues.Count;
        }

        #endregion
    }
}
