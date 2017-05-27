﻿using System;
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
        private CurrentTaskManager manager = new CurrentTaskManager();

        private static List<string> soundsList = new List<string>();

        public CWTask()
        {
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
            //做下记录吧
            Log log = LogFactory.GetLogger("CWTask AddNofication");
            log.Info(DateTime.Now.ToString() + "  warehouse-" + warehouse + "   hallID-" + hallID + " make sound, file name-" + soundFile);
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
            ImplementTask task = new ImplementTask();
            task.Warehouse = hall.Warehouse;
            task.DeviceCode = hall.DeviceCode;
            task.Type = EnmTaskType.SaveCar;
            task.Status = EnmTaskStatus.ICarInWaitFirstSwipeCard;
            task.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            task.CreateDate = DateTime.Now;
            #region 发送时间没有暂为空，看看有没有异常出现
            task.SendDtime = DateTime.Parse("2017-1-1");
            #endregion
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

        /// <summary>
        /// 外形检测上报处理
        /// </summary>
        /// <param name="hallID"></param>
        /// <param name="distance"></param>
        /// <param name="carSize"></param>
        public void IDealCheckedCar(ImplementTask htsk, int hallID,int distance,string checkCode,int weight)
        {
            htsk.CarSize = checkCode;
            htsk.Distance = distance;
            htsk.SendDtime = DateTime.Now;
            htsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;

            Log log = LogFactory.GetLogger("CWTask IDealCheckedCar");

            #region 上报外形数据不正确
            if (checkCode.Length != 3)
            {
                htsk.Status =EnmTaskStatus.ISecondSwipedWaitforCarLeave;
                //更新任务信息
                manager.Update(htsk);
                this.AddNofication(htsk.Warehouse,htsk.DeviceCode, "60.wav");
                return;
            }
            #endregion
            
            //暂以卡号为准
            ICCard iccd = new CWICCard().Find(ic=>ic.UserCode==htsk.ICCardCode);
            if (iccd == null)
            {
                //上位控制系统故障
                this.AddNofication(htsk.Warehouse,hallID,"20.wav");
                this.AddNofication(htsk.Warehouse, hallID, "6.wav");
                return;
            }
            Device hall = new CWDevice().SelectSMG(hallID, htsk.Warehouse);
            int tvID=0;
            Location lct = new AllocateLocation().IAllocateLocation(checkCode, iccd, hall, out tvID);
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

            //补充车位信息
            lct.WheelBase = distance;
            lct.CarSize = checkCode;
            lct.InDate = DateTime.Now;
            lct.Status = EnmLocationStatus.Entering;
            lct.ICCode = iccd.UserCode;
            lct.CarWeight = weight;
            Response resp = new CWLocation().UpdateLocation(lct);
           
            if (resp.Code == 1)
            {               
                log.Info(DateTime.Now.ToString()+" 更新车位-"+lct.Address+"数据，iccode-"+lct.ICCode+" status-"+lct.Status.ToString());

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
            WorkTask queue = new WorkTask() {
                IsMaster=1,
                Warehouse=lct.Warehouse,
                DeviceCode=tvID,
                MasterType=EnmTaskType.SaveCar,
                TelegramType=13,
                SubTelegramType=1,
                FromLctAddress=hall.Address,
                ToLctAddress=lct.Address,
                ICCardCode=iccd.UserCode,
                Distance=distance,
                CarSize=checkCode,
                CarWeight=weight
            };
            resp = manager_queue.Add(queue);
            if (resp.Code == 1)
            {
                log.Info(DateTime.Now.ToString() + " 队列中添加TV装载作业，存车位-" + lct.Address + "，iccode-" + lct.ICCode);
            }
        }

        /// <summary>
        /// 临时取物刷卡转存时，处理外形检测上报
        /// </summary>
        public void ITempDealCheckCar(ImplementTask htsk,Location lct, int distance,string carsize, int weight)
        {
            Log log = LogFactory.GetLogger("CWTask ITempDealCheckCar");
            //平面移动的
            Device smg = new CWDevice().Find(de=>de.Warehouse==lct.Warehouse&&de.Layer==lct.LocLayer);
            if (smg == null)
            {
                this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "42.wav");
                log.Error("系统故障，找不到TV");
                return;
            }
            ICCard iccd = new CWICCard().Find(ic=>ic.UserCode==htsk.ICCardCode);
            if (iccd == null)
            {
                log.Error("系统故障，找不到卡号-"+htsk.ICCardCode);
                return;
            }
            //补充车位信息
            lct.WheelBase = distance;
            lct.CarSize = carsize;
            lct.InDate = DateTime.Now;
            lct.ICCode = iccd.UserCode;
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
                FromLctAddress = htsk.FromLctAddress,
                ToLctAddress = lct.Address,
                ICCardCode = iccd.UserCode,
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
        /// 手动完成作业
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public Response ManualCompleteTask(int tid)
        {
            //车位信息完善

            //设备信息完善
            

            manager.Delete(tid);
            Response _resp = new Response()
            {
                Code = 1,
                Message = "手动完成作业成功,ID-" + tid
            };
            return _resp;
        }

        /// <summary>
        /// 手动复位作业
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public Response ManualResetTask(int tid)
        {
            //车位信息完善

            //设备信息完善

            manager.Delete(tid);
            Response _resp = new Response()
            {
                Code = 1,
                Message = "手动复位作业成功,ID-"+tid
            };
            return _resp;
        }

        /// <summary>
        /// 中途退出，任务完成
        /// </summary>
        /// <param name="task"></param>
        public void ICancelInAndDeleteTask(ImplementTask task)
        {
            CWDevice cwdevice = new CWDevice();
            Device hall = cwdevice.Find(dev => dev.Warehouse == task.Warehouse && dev.DeviceCode == task.DeviceCode);
            if (hall != null)
            {
                hall.TaskID = 0;
                new CWDevice().Update(hall);
            }
            task.Status = EnmTaskStatus.Finished;
            task.IsComplete = 1;
            manager.Update(task);
        }

        /// <summary>
        /// 处理取车、取物车辆离开
        /// </summary>
        /// <param name="task"></param>
        public void ODealCarDriveaway(ImplementTask task)
        {
            if (task.Type == EnmTaskType.TempGet)
            {
                //释放车位
                Location lct = new CWLocation().FindLocation(l=>l.Warehouse==task.Warehouse&&l.Address==task.FromLctAddress);
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
            manager.Update(task);
        }

        /// <summary>
        /// 处理装载完成（1013，1）
        /// </summary>
        /// <param name="etsk"></param>
        /// <param name="distance"></param>
        public void DealLoadFinishing(ImplementTask etsk,int distance)
        {
            Log log = LogFactory.GetLogger("CWTask.DealLoadFinishing");
            CWLocation cwlocation = new CWLocation();
            CWDevice cwdevice = new CWDevice();
            
            Location frLct = null;
            Location toLct = null;
            #region 存车
            if (etsk.Type == EnmTaskType.SaveCar)
            {
                #region 将车厅作业完成
                Device hall = cwdevice.Find(cd=>cd.Warehouse==etsk.Warehouse&&cd.DeviceCode==etsk.HallCode);
                if (hall != null)
                {
                    ImplementTask halltask = Find(tt=>tt.Warehouse==hall.Warehouse&&tt.DeviceCode==hall.DeviceCode);
                    if (halltask != null)
                    {
                        halltask.Status = EnmTaskStatus.Finished;
                        halltask.IsComplete = 1;
                        manager.Update(halltask);
                    }
                    hall.TaskID = 0;
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
                    log.Error("存车装载完成，要更新存车位信息，但车位为NULL，address-"+etsk.ToLctAddress);
                }
                #endregion
            }
            #endregion
            #region 取车 取物
            else if (etsk.Type == EnmTaskType.GetCar||
                     etsk.Type==EnmTaskType.TempGet)
            {              
                #region 更新下车位信息
                frLct = cwlocation.FindLocation(l => l.Address == etsk.FromLctAddress && l.Warehouse == etsk.Warehouse);
                if (toLct != null)
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
                frLct = cwlocation.FindLocation(lt=>lt.Warehouse==warehouse&&lt.Address==fradrs);
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

        /// <summary>
        /// 收到（13，51，9999）
        /// 将存车装载完成，生成卸载指令，加入队列中
        /// </summary>
        /// <param name="tsk"></param>
        public void DealLoadFinished(ImplementTask tsk)
        {
            Log log = LogFactory.GetLogger("CWTask.DealLoadFinished");
            //将当前装载作业置完成，
            tsk.Status = EnmTaskStatus.WillWaitForUnload;
            tsk.SendStatusDetail = EnmTaskStatusDetail.Asked;
            tsk.SendDtime = DateTime.Now;
            tsk.IsComplete = 0;

            manager.Update(tsk);
            //生成卸载指令，加入队列
            WorkTask queue = new WorkTask() {
                IsMaster=1,
                Warehouse=tsk.Warehouse,
                DeviceCode=tsk.DeviceCode,
                MasterType=tsk.Type,
                TelegramType=14,
                SubTelegramType=1,
                HallCode=tsk.HallCode,
                FromLctAddress=tsk.FromLctAddress,
                ToLctAddress=tsk.ToLctAddress,
                ICCardCode=tsk.ICCardCode,
                Distance=tsk.Distance,
                CarSize=tsk.CarSize,
                CarWeight=tsk.CarWeight
            };
            manager_queue.Add(queue);  
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

            int warehouse = etsk.Warehouse;
            string fraddrs = etsk.FromLctAddress;
            string toaddrs = etsk.ToLctAddress;

            Location frLct = null;
            Location toLct = null;

            if (etsk.Type == EnmTaskType.SaveCar)
            {
                toLct = cwlocation.FindLocation(l => l.Warehouse == warehouse &&l.Address==toaddrs);
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
                frLct = cwlocation.FindLocation(l=>l.Warehouse==warehouse&&l.Address==fraddrs);
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
                    ImplementTask halltask = Find(tt => tt.Warehouse == hall.Warehouse && tt.DeviceCode == hall.DeviceCode);
                    if (halltask != null)
                    {
                        halltask.Status = EnmTaskStatus.OWaitforEVUp;
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
                    toLct.Status = EnmLocationStatus.Occupy;
                    frLct.Status = EnmLocationStatus.Space;                   
                    cwlocation.UpdateLocation(toLct);
                    cwlocation.UpdateLocation(frLct);
                }
            }

            etsk.Status = EnmTaskStatus.UnLoadFinishing;
            etsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            etsk.SendDtime = DateTime.Now;
            manager.Update(etsk);
        }
       
        /// <summary>
        /// 处理移动完成（1011，1）
        /// </summary>
        /// <param name="etsk"></param>
        public void DealMoveFinishing(ImplementTask etsk)
        {
            Log log = LogFactory.GetLogger("CWTask.DealMoveFinishing");
            etsk.Status = EnmTaskStatus.MoveFinishing;
            etsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            etsk.SendDtime = DateTime.Now;
            manager.Update(etsk);
        }

        /// <summary>
        /// 完成作业，释放设备
        /// </summary>
        /// <param name="tsk"></param>
        public void DealCompleteTask(ImplementTask tsk)
        {
            Log log = LogFactory.GetLogger("CWTask.DealCompleteTask");
            tsk.Status = EnmTaskStatus.Finished;
            tsk.SendStatusDetail = EnmTaskStatusDetail.Asked;
            tsk.SendDtime = DateTime.Now;
            tsk.IsComplete = 1;
            manager.Update(tsk);

            CWDevice cwdevice = new CWDevice();
            Device smg =cwdevice.Find(dd => dd.Warehouse == tsk.Warehouse && dd.DeviceCode == tsk.DeviceCode);
            if (smg == null)
            {
                log.Error("完成作业时，找不到设备号，DeviceCode-"+tsk.DeviceCode);
            }
            smg.TaskID = 0;
            if (smg.Type == EnmSMGType.ETV)
            {
                List<ImplementTask> remaintask = manager.FindList(tt => tt.ID != tsk.ID &&
                                                    tt.DeviceCode == tsk.DeviceCode &&
                                                    tt.Warehouse == tsk.Warehouse &&
                                                    tt.IsComplete == 0);
                if (remaintask.Count==0)
                {
                    smg.SoonTaskID = 0;
                }
                else
                {
                    //如果当前是避让作业，设备的即将要执行的标志有的话，让其处于执行状态
                    if (tsk.Type == EnmTaskType.Avoid && smg.SoonTaskID != 0)
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
                    Location frLct = cwlocation.FindLocation(lt=>lt.Warehouse==tsk.Warehouse&&lt.Address==tsk.FromLctAddress);
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
        public void DealOSwipedCard(Device mohall,Location lct,ICCard iccd)
        {
            Log log = LogFactory.GetLogger("CWTask.DealOSwipedCard");
            //这里判断是否有可用的TV
            //这里先以平面移动库来做
            Device smg = new CWDevice().Find(dev=>dev.Region==lct.LocLayer&&dev.Warehouse==lct.Warehouse);
            if (smg == null)
            {
                //系统故障
                this.AddNofication(mohall.Warehouse, mohall.DeviceCode, "20.wav");
                return;
            }
            if (smg.Mode != EnmModel.Automatic)
            {
                this.AddNofication(mohall.Warehouse, mohall.DeviceCode, "42.wav");
                return;
            }

            lct.Status = EnmLocationStatus.Outing;
            Response resp= new CWLocation().UpdateLocation(lct);
            if (resp.Code == 1)
            {
                log.Info(DateTime.Now.ToString() + " 取车更新车位-" + lct.Address + " 数据，iccode-" + lct.ICCode + " status-" + lct.Status.ToString());
            }

            WorkTask queue = new WorkTask() {
                IsMaster=2,
                Warehouse=mohall.Warehouse,
                DeviceCode=mohall.DeviceCode,
                MasterType=EnmTaskType.GetCar,
                TelegramType=0,
                SubTelegramType=0,
                HallCode=mohall.DeviceCode,
                FromLctAddress=lct.Address,
                ToLctAddress=mohall.Address,
                ICCardCode=lct.ICCode,
                Distance=lct.WheelBase,
                CarSize=lct.CarSize,
                CarWeight=lct.CarWeight
            };
            resp = manager_queue.Add(queue);
            if (resp.Code == 1)
            {
                log.Info(DateTime.Now.ToString() + "  刷卡取车，添加取车队列，存车位-" + lct.Address + "，iccode-" + lct.ICCode);
            }

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
            resp.Code = 0;
            //这里判断是否有可用的TV
            //这里先以平面移动库来做
            Device smg = new CWDevice().Find(dev => dev.Region == lct.LocLayer && dev.Warehouse == lct.Warehouse);
            if (smg == null)
            {
                //系统故障
                resp.Message = "系统故障，找不移动设备。locLayer-"+lct.LocLayer+" warehouse-"+lct.Warehouse;
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
            resp.Code = 0;
            //这里判断是否有可用的TV
            //这里先以平面移动库来做
            Device smg = new CWDevice().Find(dev => dev.Region == lct.LocLayer && dev.Warehouse == lct.Warehouse);
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
                resp.Message = "已经加入取车队列，请稍后！";
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
        public Response TempFindInfo(string iccode, out int warehouse, out int hallID, out string locAddress)
        {
            warehouse = 0;
            hallID = 0;
            locAddress = "";
            Response _resp = new Response();

            Location lct = new CWLocation().FindLocation(l => l.ICCode == iccode);
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
            #region
            CWLocation cwloctation = new CWLocation();
            Location frlct = cwloctation.FindLocation(lc=>lc.Warehouse==warehouse&&lc.Address==fraddrs);
            if (frlct == null)
            {
                resp.Message="找不到源地址车位，address-"+fraddrs;
                return resp;
            }
            Location tolct = cwloctation.FindLocation(lc => lc.Warehouse == warehouse && lc.Address ==toaddrs);
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
            if (frlct.LocLayer != tolct.LocLayer)
            {
                resp.Message = "目标车位与源车位不在同一层，不允许挪移！";
                return resp;
            }
           
            Device smg = new CWDevice().Find(dev => dev.Region == frlct.LocLayer && dev.Warehouse == warehouse);
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
            #region
            Device smg = new CWDevice().Find(d => d.Warehouse == warehouse && d.DeviceCode == code);
            if (smg == null || smg.Type != EnmSMGType.ETV)
            {
                resp.Message = "请输入正确的库区及设备号！";
                return resp;
            }
            Location lct = new CWLocation().FindLocation(l=>l.Warehouse==warehouse&&l.Address==address);
            if (lct == null)
            {
                resp.Message = "请输入正确的车位地址！";
                return resp;
            }
            if (smg.Layer != lct.LocLayer)
            {
                resp.Message = "TV与车位不同层，无法移动！";
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
                resp.Message = "请等待TV完成作业，再执行移动！";
                return resp;
            }
            #endregion
            #region
            ImplementTask task = new ImplementTask() {
                Warehouse = warehouse,
                DeviceCode = smg.DeviceCode,
                Type = EnmTaskType.Move,
                Status = EnmTaskStatus.TWaitforMove,
                SendStatusDetail = EnmTaskStatusDetail.NoSend,
                SendDtime = DateTime.Now,
                CreateDate = DateTime.Now,
                HallCode=11,
                FromLctAddress=smg.Address,
                ToLctAddress=lct.Address,
                ICCardCode="",
                Distance=0,
                CarSize="",
                CarWeight=0,
                IsComplete=0
            };
            resp= manager.Add(task);
            if (resp.Code == 1)
            {
                smg.TaskID = task.ID;
                new CWDevice().Update(smg);
                resp.Message = "正在移动，请等待！";
            }
            #endregion
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
                log.Info("生成避让，绑定作业，Message-"+resp.Message);
                //删除队列
                resp = manager_queue.Delete(queue.ID);
                log.Info("生成避让，删除队列，Message-" + resp.Message);
            }
            resp.Message = "生成避让，绑定作业成功！DeviceCode-"+subtask.DeviceCode;
            return resp;
        }

        /// <summary>
        /// 将队列中的报文作业队列下发
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public Response CreateDeviceTaskByQueue(WorkTask queue,Device dev)
        {
            Log log = LogFactory.GetLogger("CWTask.CreateHallTaskByQueue");
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
                else if (queue.TelegramType == 14 && queue.SubTelegramType == 1)
                {
                    state = EnmTaskStatus.TWaitforUnload;
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
            ImplementTask subtask = new ImplementTask()
            {
                Warehouse = queue.Warehouse,
                DeviceCode = queue.DeviceCode,
                Type = queue.MasterType,
                Status = state,
                SendStatusDetail = EnmTaskStatusDetail.NoSend,
                SendDtime = DateTime.Now,
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
                log.Info("转化为执行作业，绑定于设备，Message-" + resp.Message);
                //删除队列
                resp = manager_queue.Delete(queue.ID);
                log.Info("转化为执行作业，删除队列，Message-" + resp.Message);
            }
            resp.Message = "转化为执行作业，操作成功！DeviceCode-" + subtask.DeviceCode;
            return resp;
        }

        /// <summary>
        /// 判断作业是否可以实行，要不生成别的TV的避让作业
        /// </summary>
        /// <param name="task"></param>
        /// <param name="dev"></param>
        /// <returns></returns>
        public bool DealAvoid(WorkTask queue,Device dev)
        {
            return true;
        }

        /// <summary>
        /// 发送车厅报文，组装TV报文
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="lct"></param>
        /// <returns></returns>
        public Response SendHallTelegramAndBuildTV(WorkTask master,Location lct,Device hall)
        {
            Log log = LogFactory.GetLogger("CWTask.SendHallTelegramAndBuildTV");
            #region
            Response resp = new Response();
            CWDevice cwdevice = new CWDevice();

            Device tv = new AllocateTV().Allocate(hall,lct);
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
                    ImplementTask TvTask = new ImplementTask()
                    {
                        Warehouse = tv.Warehouse,
                        DeviceCode = tv.DeviceCode,
                        Type = master.MasterType,
                        Status = EnmTaskStatus.TWaitforLoad,
                        SendStatusDetail = EnmTaskStatusDetail.NoSend,
                        SendDtime = DateTime.Now,
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
                    IsMaster=1,
                    Warehouse=tv.Warehouse,
                    DeviceCode=tv.DeviceCode,
                    MasterType=master.MasterType,
                    TelegramType=13,
                    SubTelegramType=1,
                    HallCode=hall.DeviceCode,
                    FromLctAddress=master.FromLctAddress,
                    ToLctAddress=master.ToLctAddress,
                    ICCardCode=master.ICCardCode,
                    Distance=master.Distance,
                    CarSize=master.CarSize,
                    CarWeight=master.CarWeight
                };
                manager_queue.Add(waitqueue);
            }

            //删除队列
            resp= manager_queue.Delete(master.ID);

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
                        ImplementTask TvTask = new ImplementTask()
                        {
                            Warehouse = tv.Warehouse,
                            DeviceCode = tv.DeviceCode,
                            Type = master.MasterType,
                            Status = EnmTaskStatus.TWaitforLoad,
                            SendStatusDetail = EnmTaskStatusDetail.NoSend,
                            SendDtime = DateTime.Now,
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


        #region 队列管理
        private WorkTaskManager manager_queue = new WorkTaskManager();

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
            return manager_queue.Delete(ID);
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
