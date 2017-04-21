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
            //补充车位信息
            lct.WheelBase = distance;
            lct.CarSize = checkCode;
            lct.InDate = DateTime.Now;
            lct.Status = EnmLocationStatus.Entering;
            lct.ICCode = iccd.UserCode;
            Response resp = new CWLocation().UpdateLocation(lct);
            Log log = LogFactory.GetLogger("CWTask IDealCheckedCar");
            if (resp.Code == 1)
            {               
                log.Info(DateTime.Now.ToString()+" 更新车位-"+lct.Address+"数据，iccode-"+lct.ICCode+" status-"+lct.Status.ToString());
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
            if (smg.Type == EnmSMGType.ETV)
            {
                ImplementTask remaintask = Find(tt => tt.ID != tsk.ID &&
                                                    tt.DeviceCode == tsk.DeviceCode &&
                                                    tt.Warehouse == tsk.Warehouse &&
                                                    tt.IsComplete == 0);
                if (remaintask == null)
                {
                    smg.SoonTaskID = 0;
                }
            }
            else
            {
                smg.SoonTaskID = 0;
            }
            smg.TaskID = 0;
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

        #endregion
    }
}
