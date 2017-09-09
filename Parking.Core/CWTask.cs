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
        private Log clog = null;
        public CWTask()
        {
            clog = LogFactory.GetLogger("CWTask");
        }

        /// <summary>
        /// 禁止存车
        /// </summary>
        private static bool isSaveLimit = false;
        /// <summary>
        /// 存车操作
        /// </summary>
        public static bool SaveLimit
        {
            get { return isSaveLimit; }
            set { isSaveLimit = value; }
        }

        /// <summary>
        /// 禁止存取
        /// </summary>
        private static bool isGarageLimit = false;
        /// <summary>
        /// 禁库操作
        /// </summary>
        public static bool GarageLimit
        {
            get { return isGarageLimit; }
            set { isGarageLimit = value; }
        }

        /// <summary>
        /// 添加声音文件
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        /// <param name="soundFile"></param>
        public void AddNofication(int warehouse, int hallID, string soundFile)
        {
            lock (soundsList)
            {
                string sfile = warehouse.ToString() + ";" + hallID.ToString() + ";" + soundFile;
                if (!soundsList.Contains(sfile))
                {
                    soundsList.Add(sfile);
                }
                clog.Info(DateTime.Now.ToString() + "  warehouse-" + warehouse + "   hallID-" + hallID + " make sound, file name-" + soundFile);
            }
        }

        /// <summary>
        /// 移除声音
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        public void ClearNotification(int warehouse, int hallID)
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
                lock (soundsList)
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
            }
            catch (Exception ex)
            {
                clog.Error(ex.ToString());
            }
            return "null";
        }
        /// <summary>
        /// 依ID获取任务
        /// </summary>
        public ImplementTask FindITask(int ID)
        {
            return manager.Find(ID);
        }

        public async Task<ImplementTask> FindITaskAsync(int ID)
        {
            return await manager.FindAsync(ID);
        }

        /// <summary>
        /// 依ID获取任务
        /// </summary>
        public async Task<ImplementTask> FindAsync(int ID)
        {
            return await manager.FindAsync(ID);
        }

        public ImplementTask FindITask(Expression<Func<ImplementTask, bool>> where)
        {
            return manager.Find(where);
        }

        public async Task<ImplementTask> FindITaskAsync(Expression<Func<ImplementTask, bool>> where)
        {
            return await manager.FindAsync(where);
        }

        public ImplementTask Find(int ID)
        {
            return manager.Find(ID);
        }

        public ImplementTask Find(Expression<Func<ImplementTask, bool>> where)
        {
            return FindITask(where);
        }

        public async Task<ImplementTask> FindAsync(Expression<Func<ImplementTask, bool>> where)
        {
            return await manager.FindAsync(where);
        }

        public Response AddITask(ImplementTask itask)
        {
            Response resp = manager.Add(itask);
            if (resp.Code == 1)
            {
                MainCallback<ImplementTask>.Instance().OnChange(1, itask);
            }
            return resp;
        }

        public async Task<Response> AddITaskAsync(ImplementTask itask)
        {
            Response resp = await manager.AddAsync(itask);
            if (resp.Code == 1)
            {
                MainCallback<ImplementTask>.Instance().OnChange(1, itask);
            }
            return resp;
        }

        public Response UpdateITask(ImplementTask itask)
        {
            Response resp = manager.Update(itask);
            if (resp.Code == 1)
            {
                MainCallback<ImplementTask>.Instance().OnChange(2, itask);
            }
            return resp;
        }

        public async Task<Response> UpdateITaskAsync(ImplementTask itask)
        {
            Response resp = await manager.UpdateAsync(itask);
            if (resp.Code == 1)
            {
                MainCallback<ImplementTask>.Instance().OnChange(2, itask);
            }
            return resp;
        }

        public Response DeleteITask(ImplementTask itask)
        {
            ImplementTask copy = new ImplementTask
            {
                ID = itask.ID,
                Warehouse = itask.Warehouse,
                DeviceCode = itask.DeviceCode,
                Type = itask.Type,
                Status = itask.Status,
                SendStatusDetail = itask.SendStatusDetail
            };
            Response resp = manager.Delete(itask.ID);

            if (resp.Code == 1)
            {
                MainCallback<ImplementTask>.Instance().OnChange(3, copy);
            }
            return resp;
        }

        public async Task<Response> DeleteITaskAsync(ImplementTask itask)
        {
            ImplementTask copy = new ImplementTask
            {
                ID = itask.ID,
                Warehouse = itask.Warehouse,
                DeviceCode = itask.DeviceCode,
                Type = itask.Type,
                Status = itask.Status,
                SendStatusDetail = itask.SendStatusDetail
            };
            Response resp = await manager.DeleteAsync(itask);

            if (resp.Code == 1)
            {
                MainCallback<ImplementTask>.Instance().OnChange(3, copy);
            }
            return resp;
        }

        public List<ImplementTask> FindITaskLst()
        {
            List<ImplementTask> retLst = manager.FindList();
            return retLst;
        }

        public async Task<List<ImplementTask>> FindITaskLstAsync()
        {
            List<ImplementTask> retLst = await manager.FindListAsync();
            return retLst;
        }

        public List<ImplementTask> FindITaskLst(Expression<Func<ImplementTask, bool>> where)
        {
            return manager.FindList(where);
        }

        public async Task<List<ImplementTask>> FindITaskLstAsync(Expression<Func<ImplementTask, bool>> where)
        {
            return await manager.FindListAsync(where);
        }

        /// <summary>
        /// 更新作业报文的发送状态
        /// </summary>
        /// <param name="task"></param>
        /// <param name="detail"></param>
        public Response UpdateSendStatusDetail(ImplementTask task, EnmTaskStatusDetail detail)
        {
            task.SendStatusDetail = detail;
            task.SendDtime = DateTime.Now;
            Response resp = UpdateITask(task);
            return resp;
        }

        /// <summary>
        /// 更新作业状态
        /// </summary>
        /// <param name="task"></param>
        /// <param name="status"></param>
        public Response DealUpdateTaskStatus(ImplementTask task, EnmTaskStatus status)
        {
            task.Status = status;
            task.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            task.SendDtime = DateTime.Now;
            Response resp = UpdateITask(task);
            return resp;
        }


        /// <summary>
        /// 有车入库
        /// </summary>
        /// <param name="hall"></param>
        public void DealFirstCarEntrance(Device hall)
        {
            try
            {
                CWDevice cwdevice = new CWDevice();

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
                task.LocSize = "";
                #region
                string strPlateNum = "";
                #region 获取车牌识别车牌信息
                PlateMappingDev device_plate = new CWPlate().FindPlate(hall.Warehouse, hall.DeviceCode);
                if (device_plate != null)
                {
                    if (!string.IsNullOrEmpty(device_plate.PlateNum))
                    {
                        strPlateNum = device_plate.PlateNum;
                    }
                }
                #endregion
                #endregion
                task.PlateNum = strPlateNum;

                Response _resp = AddITask(task);
                if (_resp.Code == 1)
                {
                    hall.TaskID = task.ID;
                    cwdevice.Update(hall);
                }
                #region 增加调度，如果区域内的ETV为空闲，则让其移动至车厅门，等待接车
                Device etv = cwdevice.Find(e => e.Region == hall.Region && e.Type == EnmSMGType.ETV);
                if (etv != null)
                {
                    if (etv.IsAble == 1 && etv.TaskID == 0 && etv.SoonTaskID == 0)
                    {
                        if (etv.IsAvailabe == 1)
                        {
                            //生成作业，绑定于etv中
                            ImplementTask etask = new ImplementTask
                            {
                                Warehouse = etv.Warehouse,
                                DeviceCode = etv.DeviceCode,
                                Type = EnmTaskType.Move,
                                Status = EnmTaskStatus.TWaitforMove,
                                SendStatusDetail = EnmTaskStatusDetail.NoSend,
                                CreateDate = DateTime.Now,
                                SendDtime = DateTime.Now.AddMinutes(-1),
                                HallCode = hall.DeviceCode,
                                FromLctAddress = etv.Address,
                                ToLctAddress = hall.Address,
                                ICCardCode = "",
                                Distance = 0,
                                CarSize = "",
                                IsComplete = 0,
                                LocSize = "",
                                PlateNum = ""
                            };
                            _resp = AddITask(etask);
                            if (_resp.Code == 1)
                            {
                                etv.TaskID = etask.ID;
                                cwdevice.Update(etv);
                            }
                            MainCallback<ImplementTask>.Instance().OnChange(1, etask);
                        }
                    }
                }

                #endregion

                //显示页面用
                MainCallback<ImplementTask>.Instance().OnChange(1, task);
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
        public void IDealCheckedCar(ImplementTask htsk, int hallID, int distance, string checkCode, int weight)
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
                    UpdateITask(htsk);
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "60.wav");
                    return;
                }
                #endregion

                Device hall = new CWDevice().Find(t => t.DeviceCode == hallID && t.Warehouse == htsk.Warehouse);
                string strPlateNum = "";
                string strImgPath = "";
                #region 获取车牌识别车牌信息
                PlateMappingDev device_plate = new CWPlate().FindPlate(hall.Warehouse, hall.DeviceCode);
                if (device_plate != null)
                {
                    if (!string.IsNullOrEmpty(device_plate.PlateNum) &&
                        DateTime.Compare(DateTime.Now, device_plate.InDate.AddMinutes(10)) < 0)
                    {
                        strPlateNum = device_plate.PlateNum;
                        strImgPath = device_plate.HeadImagePath;
                    }
                }
                #endregion

                Customer cust = null;
                #region 获取顾客信息
                #region
                //if (Convert.ToInt32(htsk.ICCardCode) >= 10000) //是指纹激活的
                //{
                //    int prf = Convert.ToInt32(htsk.ICCardCode);
                //    FingerPrint print = new CWFingerPrint().Find(p => p.SN_Number == prf);
                //    if (print == null)
                //    {
                //        //上位控制系统故障
                //        this.AddNofication(htsk.Warehouse, hallID, "20.wav");
                //        return;
                //    }
                //    cust = new CWICCard().FindCust(print.CustID);
                //    if (cust == null)
                //    {
                //        //上位控制系统故障
                //        this.AddNofication(htsk.Warehouse, hallID, "20.wav");
                //        return;
                //    }
                //}
                //else
                //{
                //    ICCard iccd = new CWICCard().Find(ic => ic.UserCode == htsk.ICCardCode);
                //    if (iccd == null)
                //    {
                //        //上位控制系统故障
                //        this.AddNofication(htsk.Warehouse, hallID, "20.wav");
                //        return;
                //    }
                //    if (iccd.CustID != 0)
                //    {
                //        cust = new CWICCard().FindCust(iccd.CustID);
                //    }
                //}
                #endregion
                int cno = Convert.ToInt32(htsk.ICCardCode);
                SaveCertificate scert = new CWSaveProof().Find(s => s.SNO == cno);
                if (scert == null)
                {
                    //系统异常
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "20.wav");
                    log.Error(htsk.ICCardCode + " 在存车指纹库中找不到对应记录！");
                    return;
                }
                //先依车牌号查找顾客，以确认是不是固定用户，
                //如果找不到，则依存车凭证查找用户
                if (!string.IsNullOrEmpty(strPlateNum))
                {
                    cust = new CWICCard().FindCust(d => d.PlateNum == strPlateNum);
                }
                if (cust == null)
                {
                    if (scert.CustID != 0)
                    {
                        cust = new CWICCard().FindCust(scert.CustID);
                    }
                }
                #endregion                             
                #region 如果没有车牌识别，则从顾客登记的信息中给车牌号，以作为后续界面取车用
                if (string.IsNullOrEmpty(strPlateNum))
                {
                    if (cust != null)
                    {
                        if (!string.IsNullOrEmpty(cust.PlateNum))
                        {
                            strPlateNum = cust.PlateNum;
                        }
                    }
                }
                #endregion
                if (cust == null)
                {
                    cust = new Customer();
                    cust.UserName = "TempAdd";
                    cust.Type = EnmICCardType.Temp;
                    cust.PlateNum = strPlateNum;
                }
                int tvID = 0;
                Location lct = new AllocateLocation().IAllocateLocation(checkCode, cust, hall, out tvID);
                if (lct == null)
                {
                    htsk.Status = EnmTaskStatus.ISecondSwipedWaitforCarLeave;
                    //更新任务信息
                    UpdateITask(htsk);
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "62.wav");
                    return;
                }
                if (tvID == 0)
                {
                    htsk.Status = EnmTaskStatus.ISecondSwipedWaitforCarLeave;
                    UpdateITask(htsk);
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "42.wav");
                    return;
                }
                //再判断下车位尺寸
                if (string.Compare(lct.LocSize, checkCode) < 0)
                {
                    htsk.Status = EnmTaskStatus.ISecondSwipedWaitforCarLeave;
                    UpdateITask(htsk);
                    this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "63.wav");
                    return;
                }

                lct.PlateNum = strPlateNum;
                lct.ImagePath = strImgPath;
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
                        device_plate.PlateNum = "";
                        device_plate.InDate = DateTime.Parse("2017-1-1");
                        new CWPlate().UpdatePlate(device_plate);
                    }
                    #endregion
                }

                htsk.ToLctAddress = lct.Address;
                htsk.LocSize = lct.LocSize;
                htsk.Status = EnmTaskStatus.ISecondSwipedWaitforEVDown;

                resp = UpdateITask(htsk);
                //添加TV的存车装载，将其加入队列中
                WorkTask queue = new WorkTask()
                {
                    IsMaster = 1,
                    Warehouse = lct.Warehouse,
                    DeviceCode = tvID,
                    MasterType = EnmTaskType.SaveCar,
                    TelegramType = 13,
                    SubTelegramType = 1,
                    HallCode = hallID,
                    FromLctAddress = hall.Address,
                    ToLctAddress = lct.Address,
                    ICCardCode = htsk.ICCardCode,
                    Distance = distance,
                    CarSize = checkCode,
                    CarWeight = weight
                };
                resp = AddQueue(queue);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + " 队列中添加TV装载作业，存车位-" + lct.Address + "，iccode-" + lct.ICCode);
                }

                this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "26.wav");

                #region 推送存车记录给云平台
                ParkingRecord pkRecord = new ParkingRecord
                {
                    TaskType = 0,
                    LocAddrs = lct.Address,
                    Proof = lct.ICCode,
                    PlateNum = lct.PlateNum,
                    carpicture = lct.ImagePath,
                    CarSize = Convert.ToInt32(lct.CarSize),
                    LocSize = Convert.ToInt32(lct.LocSize),
                    InDate = lct.InDate.ToString()
                };
                CloudCallback.Instance().WatchParkingRcd(pkRecord);
                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 临时取物刷卡转存时，处理外形检测上报
        /// </summary>
        public void ITempDealCheckCar(ImplementTask htsk, Location lct, int distance, string carsize, int weight)
        {
            Log log = LogFactory.GetLogger("CWTask ITempDealCheckCar");
            try
            {
                Device hall = new CWDevice().Find(t => t.DeviceCode == htsk.HallCode && t.Warehouse == htsk.Warehouse);
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
                lct.PlateNum = htsk.PlateNum;
                htsk.LocSize = lct.LocSize;
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
                resp = UpdateITask(htsk);

                //添加TV的存车装载，将其加入队列中
                WorkTask queue = new WorkTask()
                {
                    IsMaster = 1,
                    Warehouse = lct.Warehouse,
                    DeviceCode = smg.DeviceCode,
                    MasterType = EnmTaskType.SaveCar,
                    TelegramType = 13,
                    SubTelegramType = 1,
                    HallCode = hall.DeviceCode,
                    FromLctAddress = htsk.FromLctAddress,
                    ToLctAddress = lct.Address,
                    ICCardCode = htsk.ICCardCode,
                    Distance = distance,
                    CarSize = carsize,
                    CarWeight = weight
                };
                resp = AddQueue(queue);
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

        public Page<ImplementTask> FindPageList(Page<ImplementTask> pageTask, OrderParam param)
        {
            lock (manager)
            {
                if (param == null)
                {
                    param = new OrderParam()
                    {
                        PropertyName = "ID",
                        Method = OrderMethod.Asc
                    };
                }
                Page<ImplementTask> page = manager.FindPageList(pageTask, param);
                return page;
            }
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
                ImplementTask itask = FindITask(tid);
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

                string rcdMessage = "手动完成 - 卡号：" + itask.ICCardCode + " 类型：" + itask.Type.ToString() + " 设备：" + dev.DeviceCode + " 源地址：" + itask.FromLctAddress + " 目的地址：" + itask.ToLctAddress;

                //获取相关联的作业
                string iccode = itask.ICCardCode;
                ImplementTask relatetask = manager.Find(tsk => tsk.ICCardCode == iccode && tsk.ID != tid && tsk.Type != EnmTaskType.Avoid && tsk.IsComplete == 0);
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
                    else if (etvtask.Type == EnmTaskType.GetCar)
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
                            toLct.PlateNum = "";
                            toLct.ImagePath = "";

                            cwlctn.UpdateLocation(toLct);
                        }

                        #region 删除存车指纹库
                        if (!string.IsNullOrEmpty(etvtask.ICCardCode))
                        {
                            CWSaveProof cwsaveproof = new CWSaveProof();
                            int sno = Convert.ToInt32(etvtask.ICCardCode);
                            SaveCertificate scert = cwsaveproof.Find(s => s.SNO == sno);
                            if (scert != null)
                            {
                                cwsaveproof.Delete(scert.ID);
                            }
                        }
                        #endregion

                    }
                    else if (etvtask.Type == EnmTaskType.TempGet)
                    {
                        //没有转存的，当前是取物至车厅动作的，则取源地址
                        if ((int)itask.Status > (int)EnmTaskStatus.ISecondSwipedWaitforCheckSize)
                        {
                            Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.FromLctAddress);
                            if (toLct != null)
                            {
                                toLct.Status = EnmLocationStatus.Space;
                                toLct.InDate = DateTime.Parse("2017-1-1");
                                toLct.ICCode = "";
                                toLct.WheelBase = 0;
                                toLct.CarSize = "";
                                toLct.PlateNum = "";
                                toLct.ImagePath = "";
                                cwlctn.UpdateLocation(toLct);
                            }
                            #region 删除存车指纹库
                            if (!string.IsNullOrEmpty(etvtask.ICCardCode))
                            {
                                CWSaveProof cwsaveproof = new CWSaveProof();
                                int sno = Convert.ToInt32(etvtask.ICCardCode);
                                SaveCertificate scert = cwsaveproof.Find(s => s.SNO == sno);
                                if (scert != null)
                                {
                                    cwsaveproof.Delete(scert.ID);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            //这时就是转存的，源地址与目的地址已经交换了
                            Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.ToLctAddress);
                            if (toLct != null)
                            {
                                toLct.Status = EnmLocationStatus.Occupy;
                                toLct.ICCode = etvtask.ICCardCode;
                                toLct.WheelBase = etvtask.Distance;
                                toLct.CarSize = etvtask.CarSize;
                                cwlctn.UpdateLocation(toLct);
                            }
                        }

                    }
                    else if (etvtask.Type == EnmTaskType.Transpose)
                    {
                        Location frLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.FromLctAddress);
                        Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.ToLctAddress);
                        if (frLct != null && toLct != null)
                        {
                            toLct.Status = EnmLocationStatus.Occupy;
                            toLct.ICCode = etvtask.ICCardCode;
                            toLct.InDate = frLct.InDate;
                            toLct.WheelBase = etvtask.Distance;
                            toLct.CarSize = etvtask.CarSize;
                            toLct.CarWeight = frLct.CarWeight;
                            toLct.PlateNum = frLct.PlateNum;
                            toLct.ImagePath = frLct.ImagePath;

                            cwlctn.UpdateLocation(toLct);

                            frLct.Status = EnmLocationStatus.Space;
                            frLct.InDate = DateTime.Parse("2017-1-1");
                            frLct.ICCode = "";
                            frLct.WheelBase = 0;
                            frLct.CarSize = "";
                            frLct.CarWeight = 0;
                            frLct.PlateNum = "";
                            frLct.ImagePath = "";

                            cwlctn.UpdateLocation(frLct);
                        }

                    }
                }
                else
                {
                    //取物时，车厅内手动完成作业，则也要处于是对应车位
                    if (itask.Type == EnmTaskType.TempGet)
                    {
                        Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == itask.Warehouse && lc.Address == itask.FromLctAddress);
                        if (toLct != null)
                        {
                            toLct.Status = EnmLocationStatus.Occupy;
                            toLct.ICCode = itask.ICCardCode;
                            toLct.WheelBase = itask.Distance;
                            cwlctn.UpdateLocation(toLct);
                        }
                    }

                    #region 只有车厅作业时，也删除存车指纹库
                    if (!string.IsNullOrEmpty(iccode))
                    {
                        CWSaveProof cwsaveproof = new CWSaveProof();
                        int sno = Convert.ToInt32(iccode);
                        SaveCertificate scert = cwsaveproof.Find(s => s.SNO == sno);
                        if (scert != null)
                        {
                            cwsaveproof.Delete(scert.ID);
                        }
                    }
                    #endregion
                }
                #endregion

                #region 删除相关队列
                List<WorkTask> queueLst = manager_queue.FindList(wk => wk.ICCardCode == iccode);
                for (int i = 0; i < queueLst.Count; i++)
                {
                    WorkTask wtsk = queueLst[i];
                    DeleteQueue(wtsk.ID);
                }

                #endregion

                //删除作业
                if (relatetask != null)
                {
                    relatetask.Status = EnmTaskStatus.Finished;
                    relatetask.IsComplete = 1;
                    DeleteITask(relatetask);
                }
                itask.Status = EnmTaskStatus.Finished;
                itask.IsComplete = 1;
                resp = DeleteITask(itask);

                if (resp.Code == 1)
                {
                    resp.Message = "手动完成作业成功,ID-" + tid;
                }

                OperateLog olog = new OperateLog
                {
                    Description = rcdMessage,
                    CreateDate = DateTime.Now,
                    OptName = ""
                };

                new CWOperateRecordLog().AddOperateLog(olog);
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
                ImplementTask itask = FindITask(tid);
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

                string rcdMessage = "手动复位- 卡号：" + itask.ICCardCode + " 类型：" + itask.Type.ToString() + " 设备：" + dev.DeviceCode + " 源地址：" + itask.FromLctAddress + " 目的地址：" + itask.ToLctAddress;

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
                            #region 推送停车记录给云平台
                            ParkingRecord pkRecord = new ParkingRecord
                            {
                                TaskType = 1,
                                LocAddrs = toLct.Address,
                                Proof = toLct.ICCode,
                                PlateNum = toLct.PlateNum,
                                carpicture = toLct.ImagePath,
                                CarSize = Convert.ToInt32(toLct.CarSize),
                                LocSize = Convert.ToInt32(toLct.LocSize),
                                InDate = toLct.InDate.ToString()
                            };
                            CloudCallback.Instance().WatchParkingRcd(pkRecord);
                            #endregion

                            toLct.Status = EnmLocationStatus.Space;
                            toLct.InDate = DateTime.Parse("2017-1-1");
                            toLct.ICCode = "";
                            toLct.WheelBase = 0;
                            toLct.CarSize = "";
                            toLct.CarWeight = 0;
                            toLct.PlateNum = "";
                            toLct.ImagePath = "";

                            cwlctn.UpdateLocation(toLct);
                        }
                        #region 删除存车指纹库
                        if (!string.IsNullOrEmpty(etvtask.ICCardCode))
                        {
                            CWSaveProof cwsaveproof = new CWSaveProof();
                            int sno = Convert.ToInt32(etvtask.ICCardCode);
                            SaveCertificate scert = cwsaveproof.Find(s => s.SNO == sno);
                            if (scert != null)
                            {
                                cwsaveproof.Delete(scert.ID);
                            }
                        }
                        #endregion

                    }
                    else if (etvtask.Type == EnmTaskType.GetCar)
                    {
                        Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.FromLctAddress);
                        if (toLct != null)
                        {
                            toLct.Status = EnmLocationStatus.Occupy;
                            toLct.ICCode = etvtask.ICCardCode;
                            toLct.WheelBase = etvtask.Distance;
                            toLct.CarSize = etvtask.CarSize;
                            cwlctn.UpdateLocation(toLct);

                            #region 推送停车记录给云平台
                            ParkingRecord pkRecord = new ParkingRecord
                            {
                                TaskType = 0,
                                LocAddrs = toLct.Address,
                                Proof = toLct.ICCode,
                                PlateNum = toLct.PlateNum,
                                carpicture = toLct.ImagePath,
                                CarSize = Convert.ToInt32(toLct.CarSize),
                                LocSize = Convert.ToInt32(toLct.LocSize),
                                InDate = toLct.InDate.ToString()
                            };
                            CloudCallback.Instance().WatchParkingRcd(pkRecord);
                            #endregion
                        }

                    }
                    else if (etvtask.Type == EnmTaskType.TempGet)
                    {
                        //没有转存的，当前是取物至车厅动作的，则取源地址
                        if ((int)itask.Status > (int)EnmTaskStatus.ISecondSwipedWaitforCheckSize)
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
                        else
                        {
                            //这时就是转存的，源地址与目的地址已经交换了
                            Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == etvtask.Warehouse && lc.Address == etvtask.ToLctAddress);
                            if (toLct != null)
                            {
                                toLct.Status = EnmLocationStatus.Space;
                                toLct.InDate = DateTime.Parse("2017-1-1");
                                toLct.ICCode = "";
                                toLct.WheelBase = 0;
                                toLct.CarSize = "";
                                toLct.PlateNum = "";
                                toLct.ImagePath = "";

                                cwlctn.UpdateLocation(toLct);
                            }
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
                            toLct.PlateNum = "";
                            toLct.ImagePath = "";

                            cwlctn.UpdateLocation(toLct);
                        }

                    }
                }
                else
                {
                    //取物时，车厅内手动完成作业，则也要复位对应车位
                    if (itask.Type == EnmTaskType.TempGet)
                    {
                        Location toLct = cwlctn.FindLocation(lc => lc.Warehouse == itask.Warehouse && lc.Address == itask.FromLctAddress);
                        //第三次刷卡后，源地址与目的地址都互换了
                        if (itask.Status == EnmTaskStatus.ISecondSwipedWaitforCheckSize)
                        {
                            toLct = cwlctn.FindLocation(lc => lc.Warehouse == itask.Warehouse && lc.Address == itask.ToLctAddress);
                        }
                        if (toLct != null)
                        {
                            toLct.Status = EnmLocationStatus.Space;
                            toLct.InDate = DateTime.Parse("2017-1-1");
                            toLct.ICCode = "";
                            toLct.WheelBase = 0;
                            toLct.CarSize = "";
                            toLct.CarWeight = 0;
                            toLct.PlateNum = "";
                            toLct.ImagePath = "";

                            cwlctn.UpdateLocation(toLct);
                        }
                    }
                    #region 只有车厅作业时，也删除存车指纹库
                    if (!string.IsNullOrEmpty(iccode))
                    {
                        CWSaveProof cwsaveproof = new CWSaveProof();
                        int sno = Convert.ToInt32(iccode);
                        SaveCertificate scert = cwsaveproof.Find(s => s.SNO == sno);
                        if (scert != null)
                        {
                            cwsaveproof.Delete(scert.ID);
                        }
                    }
                    #endregion
                }
                #endregion            

                //删除作业
                if (relatetask != null)
                {
                    relatetask.Status = EnmTaskStatus.Finished;
                    relatetask.IsComplete = 1;
                    DeleteITask(relatetask);
                }
                itask.Status = EnmTaskStatus.Finished;
                itask.IsComplete = 1;
                resp = DeleteITask(itask);
                if (resp.Code == 1)
                {
                    resp.Message = "手动复位作业成功,ID-" + tid;
                }

                OperateLog olog = new OperateLog
                {
                    Description = rcdMessage,
                    CreateDate = DateTime.Now,
                    OptName = ""
                };

                new CWOperateRecordLog().AddOperateLog(olog);
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

                #region 清除存车指纹库记录
                if (!string.IsNullOrEmpty(task.ICCardCode))
                {
                    CWSaveProof cwsaveproof = new CWSaveProof();
                    int cno = Convert.ToInt32(task.ICCardCode);
                    SaveCertificate scert = cwsaveproof.Find(sa => sa.SNO == cno);
                    if (scert != null)
                    {
                        cwsaveproof.Delete(scert.ID);
                    }
                }
                #endregion

                task.Status = EnmTaskStatus.Finished;
                task.IsComplete = 1;
                DeleteITask(task);

                CWPlate cwplate = new CWPlate();
                PlateMappingDev mp = cwplate.FindPlate(hall.Warehouse, hall.DeviceCode);
                if (mp != null && !string.IsNullOrEmpty(mp.PlateNum))
                {
                    mp.PlateNum = "";
                    mp.HeadImagePath = "";
                    mp.InDate = DateTime.Parse("2017-1-1");

                    cwplate.UpdatePlate(mp);
                }
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
                    #region 释放车位
                    Location frlct = new CWLocation().FindLocation(l => l.Warehouse == task.Warehouse && l.Address == task.FromLctAddress);
                    if (frlct != null && frlct.Type == EnmLocationType.Normal)
                    {
                        if (frlct.Status == EnmLocationStatus.Entering ||
                            frlct.Status == EnmLocationStatus.Outing)
                        {
                        }
                        else
                        {
                            #region 推送停车记录给云平台
                            ParkingRecord pkRecord = new ParkingRecord
                            {
                                TaskType = 1,
                                LocAddrs = frlct.Address,
                                Proof = frlct.ICCode,
                                PlateNum = frlct.PlateNum,
                                carpicture = frlct.ImagePath,
                                CarSize = string.IsNullOrEmpty(frlct.CarSize) ? 0 : Convert.ToInt32(frlct.CarSize),
                                LocSize = string.IsNullOrEmpty(frlct.LocSize) ? 0 : Convert.ToInt32(frlct.LocSize),
                                InDate = frlct.InDate == null ? "2017-1-1" : frlct.InDate.ToString()
                            };
                            CloudCallback.Instance().WatchParkingRcd(pkRecord);
                            #endregion

                            frlct.Status = EnmLocationStatus.Space;
                            frlct.ICCode = "";
                            frlct.PlateNum = "";
                            new CWLocation().UpdateLocation(frlct);
                        }
                    }

                    Location tolct = new CWLocation().FindLocation(l => l.Warehouse == task.Warehouse && l.Address == task.ToLctAddress);
                    if (tolct != null && tolct.Type == EnmLocationType.Normal)
                    {
                        if (tolct.Status == EnmLocationStatus.Entering ||
                          tolct.Status == EnmLocationStatus.Outing)
                        {
                        }
                        else
                        {
                            #region 推送停车记录给云平台
                            ParkingRecord pkRecord = new ParkingRecord
                            {
                                TaskType = 1,
                                LocAddrs = tolct.Address,
                                Proof = tolct.ICCode,
                                PlateNum = tolct.PlateNum,
                                carpicture = tolct.ImagePath,
                                CarSize = string.IsNullOrEmpty(tolct.CarSize) ? 0 : Convert.ToInt32(tolct.CarSize),
                                LocSize = string.IsNullOrEmpty(tolct.LocSize) ? 0 : Convert.ToInt32(tolct.LocSize),
                                InDate = tolct.InDate == null ? "2017-1-1" : tolct.InDate.ToString()
                            };
                            CloudCallback.Instance().WatchParkingRcd(pkRecord);
                            #endregion

                            tolct.Status = EnmLocationStatus.Space;
                            tolct.ICCode = "";
                            tolct.PlateNum = "";
                            new CWLocation().UpdateLocation(tolct);
                        }
                    }
                    #endregion
                }
                CWDevice cwdevice = new CWDevice();
                Device hall = cwdevice.Find(dev => dev.Warehouse == task.Warehouse && dev.DeviceCode == task.DeviceCode);
                if (hall != null)
                {
                    hall.TaskID = 0;
                    new CWDevice().Update(hall);
                }

                #region 清除存车指纹库记录
                if (!string.IsNullOrEmpty(task.ICCardCode))
                {
                    CWSaveProof cwsaveproof = new CWSaveProof();
                    int cno = Convert.ToInt32(task.ICCardCode);
                    SaveCertificate scert = cwsaveproof.Find(sa => sa.SNO == cno);
                    if (scert != null)
                    {
                        cwsaveproof.Delete(scert.ID);
                    }
                }
                #endregion

                task.Status = EnmTaskStatus.Finished;
                task.IsComplete = 1;
                DeleteITask(task);

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
        public void DealLoadFinishing(ImplementTask etsk, int distance)
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
                        ImplementTask halltask = FindITask(tt => tt.Warehouse == hall.Warehouse && tt.DeviceCode == hall.DeviceCode);
                        if (halltask != null)
                        {
                            halltask.Status = EnmTaskStatus.Finished;
                            halltask.IsComplete = 1;
                            //manager.Update(halltask);
                            DeleteITask(halltask);
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

                    #region  添加存车记录                 
                    string rcdmsg = "存车，ICCode - " + etsk.ICCardCode + " ,车位 - " + etsk.ToLctAddress + " ,库区 - " + etsk.Warehouse + " ,车厅 - " + etsk.HallCode + " ,轴距 - " + etsk.Distance + " ,外形 - " + etsk.CarSize;
                    OperateLog olog = new OperateLog
                    {
                        Description = rcdmsg,
                        CreateDate = DateTime.Now,
                        OptName = "system"
                    };
                    new CWOperateRecordLog().AddOperateLog(olog);
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
                        if (etsk.Type == EnmTaskType.TempGet)
                        {
                            frLct.Status = EnmLocationStatus.TempGet;
                        }
                        cwlocation.UpdateLocation(frLct);
                    }
                    else
                    {
                        log.Error("存车装载完成，要更新存车位信息，但车位为NULL，address-" + etsk.ToLctAddress);
                    }
                    #endregion

                    #region  添加取车记录 
                    string rcdtype = "";
                    if (etsk.Type == EnmTaskType.TempGet)
                    {
                        rcdtype = "取物";
                    }
                    else
                    {
                        rcdtype = "取车";
                    }
                    string rcdmsg = rcdtype + "，ICCode - " + etsk.ICCardCode + " ,车位 - " + etsk.ToLctAddress + " ,库区 - " + etsk.Warehouse + " ,车厅 - " + etsk.HallCode + " ,轴距 - " + etsk.Distance + " ,外形 - " + etsk.CarSize;
                    OperateLog olog = new OperateLog
                    {
                        Description = rcdmsg,
                        CreateDate = DateTime.Now,
                        OptName = "system"
                    };
                    new CWOperateRecordLog().AddOperateLog(olog);
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
                        toLct.ImagePath = frLct.ImagePath;
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
                UpdateITask(etsk);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 为了节约时间，在下发（13，51）时就打包好（14，1）加入队列
        /// </summary>
        /// <param name="tsk"></param>
        public async Task<int> UnpackUnloadOrderAsync(ImplementTask tsk)
        {
            WorkTask exitQueue = await manager_queue.FindAsync(m => m.IsMaster == 1 && m.DeviceCode == tsk.DeviceCode && m.ICCardCode == tsk.ICCardCode && m.TelegramType == 14);
            if (exitQueue == null)
            {
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
                AddQueue(queue);
            }
            return 1;
        }

        /// <summary>
        /// 收到（13/43，51，9999）
        /// 将存车装载完成
        /// </summary>
        /// <param name="tsk"></param>
        public async Task DealLoadFinishedAsync(ImplementTask tsk)
        {
            Log log = LogFactory.GetLogger("CWTask.DealLoadFinished");
            try
            {
                //将当前装载作业置完成，
                tsk.Status = EnmTaskStatus.WillWaitForUnload;
                tsk.SendStatusDetail = EnmTaskStatusDetail.Asked;
                tsk.SendDtime = DateTime.Now;
                tsk.IsComplete = 0;
                await UpdateITaskAsync(tsk);
            }
            catch (Exception ex)
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
                        frLct.PlateNum = "";
                        frLct.ImagePath = "";

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
                        ImplementTask halltask = FindITask(tt => tt.Warehouse == hall.Warehouse && tt.DeviceCode == hall.DeviceCode && tt.IsComplete == 0);
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
                            UpdateITask(halltask);
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
                        cwlocation.UpdateLocation(toLct);

                        frLct.Status = EnmLocationStatus.Space;
                        WorkTask queue = manager_queue.Find(q => q.ICCardCode == etsk.ICCardCode && q.MasterType == EnmTaskType.Transpose);
                        if (queue != null)
                        {
                            //暂不释放车位
                            frLct.Status = EnmLocationStatus.WillBack;
                        }
                        frLct.ICCode = "";
                        frLct.PlateNum = "";
                        frLct.ImagePath = "";

                        cwlocation.UpdateLocation(frLct);

                    }
                }

                etsk.Status = EnmTaskStatus.UnLoadFinishing;
                etsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                etsk.SendDtime = DateTime.Now;
                UpdateITask(etsk);

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
                UpdateITask(etsk);
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
                #region 取车出去时，删除对应的存车指纹库记录
                if (tsk.Status == EnmTaskStatus.OHallFinishing ||
                    tsk.Status == EnmTaskStatus.TempHallFinishing)
                {
                    if (!string.IsNullOrEmpty(tsk.ICCardCode))
                    {
                        CWSaveProof cwsaveprooft = new CWSaveProof();
                        int sno = Convert.ToInt32(tsk.ICCardCode);
                        SaveCertificate scert = cwsaveprooft.Find(s => s.SNO == sno);
                        if (scert != null)
                        {
                            cwsaveprooft.Delete(scert.ID);
                        }
                    }
                }
                #endregion

                tsk.Status = EnmTaskStatus.Finished;
                tsk.IsComplete = 1;
                DeleteITask(tsk);
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
                this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "40.wav");
                htsk.Status = EnmTaskStatus.TempOCarOutWaitforDrive;
            }
            else
            {
                htsk.Status = EnmTaskStatus.OCarOutWaitforDriveaway;
                this.AddNofication(htsk.Warehouse, htsk.DeviceCode, "32.wav");
            }
            htsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            htsk.SendDtime = DateTime.Now;
            UpdateITask(htsk);
        }

        /// <summary>
        /// 处理第一次刷卡
        /// </summary>
        /// <param name="task"></param>
        /// <param name="iccode"></param>
        public void DealISwipedFirstCard(ImplementTask task, string iccode)
        {
            task.ICCardCode = iccode;
            task.Status = EnmTaskStatus.IFirstSwipedWaitforCheckSize;
            task.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            task.SendDtime = DateTime.Now;
            UpdateITask(task);

            this.AddNofication(task.Warehouse, task.DeviceCode, "19.wav");
        }

        /// <summary>
        /// 处理第二次刷卡
        /// </summary>
        /// <param name="task"></param>
        /// <param name="iccode"></param>
        public void DealISwipedSecondCard(ImplementTask task, string iccode)
        {
            task.ICCardCode = iccode;
            task.Status = EnmTaskStatus.ISecondSwipedWaitforCheckSize;
            task.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            task.SendDtime = DateTime.Now;
            UpdateITask(task);

            this.AddNofication(task.Warehouse, task.DeviceCode, "21.wav");
        }

        /// <summary>
        /// 处理取物刷卡
        /// </summary>
        /// <param name="task"></param>
        /// <param name="iccode"></param>
        public int DealISWipeThreeCard(ImplementTask task, string iccode)
        {
            if (task.Type != EnmTaskType.TempGet)
            {
                this.AddNofication(task.Warehouse, task.DeviceCode, "14.wav");
                return 0;
            }

            task.Status = EnmTaskStatus.IFirstSwipedWaitforCheckSize;
            string frLct = task.FromLctAddress;
            string toLct = task.ToLctAddress;

            task.FromLctAddress = toLct;
            task.ToLctAddress = frLct;
            task.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            task.SendDtime = DateTime.Now;
            Response resp = UpdateITask(task);

            this.AddNofication(task.Warehouse, task.DeviceCode, "19.wav");

            return resp.Code;
        }


        /// <summary>
        /// 处理刷卡取车，只生成队列作业，加入队列列表中
        /// </summary>
        /// <param name="task"></param>
        /// <param name="lct"></param>
        public Response DealOSwipedCard(Device mohall, Location lct)
        {
            Log log = LogFactory.GetLogger("CWTask.DealOSwipedCard");
            Response resp = new Response();
            try
            {
                Device smg = new AllocateTV().Allocate(mohall, lct);
                //Device smg = new CWDevice().Find(d => d.Region == lct.Region);
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
                respo = AddQueue(queue);
                if (respo.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + "  刷卡取车，添加取车队列，存车位-" + lct.Address + "，iccode-" + lct.ICCode);

                    resp.Code = 1;
                    resp.Message = "正在为你取车，请稍后";

                    this.AddNofication(mohall.Warehouse, mohall.DeviceCode, "28.wav");

                    #region 推送停车记录给云平台
                    ParkingRecord pkRecord = new ParkingRecord
                    {
                        TaskType = 1,
                        LocAddrs = lct.Address,
                        Proof = lct.ICCode,
                        PlateNum = lct.PlateNum,
                        carpicture = lct.ImagePath,
                        CarSize = Convert.ToInt32(lct.CarSize),
                        LocSize = Convert.ToInt32(lct.LocSize),
                        InDate = lct.InDate.ToString()
                    };
                    CloudCallback.Instance().WatchParkingRcd(pkRecord);
                    #endregion
                }
                else
                {
                    resp.Message = "加入队列失败";
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
                //Device smg = new CWDevice().Find(d => d.Region == lct.Region);
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
                resp = AddQueue(queue);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + "  手动出车，添加取车队列，存车位-" + lct.Address + "，iccode-" + lct.ICCode);
                    resp.Message = "已经加入取车队列，请稍后！";

                    #region 推送停车记录给云平台
                    ParkingRecord pkRecord = new ParkingRecord
                    {
                        TaskType = 1,
                        LocAddrs = lct.Address,
                        Proof = lct.ICCode,
                        PlateNum = lct.PlateNum,
                        carpicture = lct.ImagePath,
                        CarSize = Convert.ToInt32(lct.CarSize),
                        LocSize = Convert.ToInt32(lct.LocSize),
                        InDate = lct.InDate.ToString()
                    };
                    CloudCallback.Instance().WatchParkingRcd(pkRecord);
                    #endregion
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
        public Response TempGetCar(Device mohall, Location lct)
        {
            Log log = LogFactory.GetLogger("CWTask.TempGetCar");
            Response resp = new Response();
            try
            {
                //这里判断是否有可用的TV
                //这里先以平面移动库来做
                Device smg = new AllocateTV().Allocate(mohall, lct);
                //Device smg = new CWDevice().Find(d => d.Region == lct.Region);
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
                resp = AddQueue(queue);
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
            _resp.Code = 1;
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
        public Response TransportLocation(int warehouse, string fraddrs, string toaddrs)
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
                if (tolct.Status != EnmLocationStatus.Space)
                {
                    resp.Message = "目的车位状态不为占用状态";
                    return resp;
                }
                if (string.Compare(frlct.LocSize, tolct.LocSize) > 0)
                {
                    resp.Message = "目标车位的车位尺寸小于源车位尺寸，不允许挪移！";
                    return resp;
                }
                if (frlct.LocColumn == tolct.LocColumn)
                {
                    resp.Message = "同列车位暂不允许手动挪移，目标地址请输入别的！";
                    return resp;
                }
                //if (frlct.Region != tolct.Region)
                //{
                //    resp.Message = "源地址与目的地址不同层，不允许挪移！";
                //    return resp;
                //}
                Customer cust = new CWICCard().FindFixLocationByAddress(tolct.Warehouse, tolct.Address);
                if (cust != null)
                {
                    resp.Message = "目标车位是固定车位，不允许挪移！";
                    return resp;
                }
                //是后面车位，则前面保证前面的车位是空闲的
                if (frlct.NeedBackup == 1)
                {
                    string forward = (frlct.LocSide - 2).ToString();
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
                if (tolct.NeedBackup == 1)
                {
                    string forward = (frlct.LocSide - 2).ToString();
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
                //Device smg = new CWDevice().Find(d => d.Region == frlct.Region);
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
                    IsMaster = 1,
                    Warehouse = warehouse,
                    DeviceCode = smg.DeviceCode,
                    MasterType = EnmTaskType.Transpose,
                    TelegramType = 13,
                    SubTelegramType = 1,
                    HallCode = 11,
                    FromLctAddress = frlct.Address,
                    ToLctAddress = tolct.Address,
                    ICCardCode = frlct.ICCode,
                    Distance = frlct.WheelBase,
                    CarSize = frlct.CarSize,
                    CarWeight = frlct.CarWeight
                };
                resp = AddQueue(queue);
                if (resp.Code == 1)
                {
                    log.Info(DateTime.Now.ToString() + "  添加挪移入队列，源车位-" + frlct.Address + "，目的车位-" + tolct.Address);
                    resp.Message = "已经加入作业队列，请稍后！";

                    //一个信息入库，一个信息的出库
                    #region 推送停车记录给云平台
                    ParkingRecord pkRecord = new ParkingRecord
                    {
                        TaskType = 1,
                        LocAddrs = frlct.Address,
                        Proof = frlct.ICCode,
                        PlateNum = frlct.PlateNum,
                        carpicture = frlct.ImagePath,
                        CarSize = string.IsNullOrEmpty(frlct.CarSize) ? 0 : Convert.ToInt32(frlct.CarSize),
                        LocSize = string.IsNullOrEmpty(frlct.LocSize) ? 0 : Convert.ToInt32(frlct.LocSize),
                        InDate = frlct.InDate.ToString()
                    };
                    CloudCallback.Instance().WatchParkingRcd(pkRecord);

                    ParkingRecord inRecord = new ParkingRecord
                    {
                        TaskType = 0,
                        LocAddrs = tolct.Address,
                        Proof = frlct.ICCode,
                        PlateNum = frlct.PlateNum,
                        carpicture = frlct.ImagePath,
                        CarSize = string.IsNullOrEmpty(tolct.CarSize) ? 0 : Convert.ToInt32(tolct.CarSize),
                        LocSize = string.IsNullOrEmpty(tolct.LocSize) ? 0 : Convert.ToInt32(frlct.LocSize),
                        InDate = frlct.InDate.ToString()
                    };
                    CloudCallback.Instance().WatchParkingRcd(inRecord);
                    #endregion
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
                string toaddrs = address;
                Location lct = new CWLocation().FindLocation(l => l.Warehouse == warehouse && l.Address == toaddrs);
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
                    ImplementTask itask = FindITask(smg.TaskID);
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
                    IsComplete = 0,
                    LocSize = "",
                    PlateNum = ""
                };
                resp = AddITask(task);
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
        public async Task<Response> CreateAvoidTaskByQueueAsync(int queueID)
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("CWTask.CreateAvoidTaskByQueue");
            try
            {
                WorkTask queue = await FindQueueAsync(queueID);
                if (queue == null)
                {
                    log.Error("生成可执行的避让作业时，依ID号找不到队列，ID - " + queueID);
                    return resp;
                }
                ImplementTask subtask = new ImplementTask()
                {
                    Warehouse = queue.Warehouse,
                    DeviceCode = queue.DeviceCode,
                    Type = queue.MasterType,
                    Status = EnmTaskStatus.TWaitforMove,
                    SendStatusDetail = EnmTaskStatusDetail.NoSend,
                    SendDtime = DateTime.Now,
                    HallCode = queue.HallCode,
                    FromLctAddress = queue.FromLctAddress,
                    ToLctAddress = queue.ToLctAddress,
                    ICCardCode = queue.ICCardCode,
                    Distance = 0,
                    CarSize = "",
                    CarWeight = 0,
                    IsComplete = 0,
                    LocSize = "",
                    PlateNum = ""
                };
                resp = AddITask(subtask);
                if (resp.Code == 1)
                {
                    Device dev = await new CWDevice().FindAsync(d => d.Warehouse == queue.Warehouse && d.DeviceCode == queue.DeviceCode);
                    //如果是处于装载完成中，也允许先避让，
                    //将当前作业ID加入待发送卸载中，
                    //避让优先
                    dev.SoonTaskID = dev.TaskID;
                    dev.TaskID = subtask.ID;
                    resp = new CWDevice().Update(dev);
                    log.Info("生成避让，绑定作业，devicecode-" + dev.DeviceCode + " ,TaskID-" + dev.TaskID + " ,SoonTaskID-" + dev.SoonTaskID);
                    //删除队列
                    resp = manager_queue.Delete(queue.ID);

                    log.Info("生成避让，删除队列，Message-" + resp.Message);
                }
                resp.Message = "生成避让，绑定作业成功！DeviceCode-" + subtask.DeviceCode + " ,TaskID-" + subtask.ID;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return resp;
        }

        /// <summary>
        /// 将队列中的报文作业队列下发
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="dev">要执行作业的设备</param>
        /// <returns></returns>
        public async Task<Response> CreateDeviceTaskByQueueAsync(WorkTask queue, Device dev)
        {
            Log log = LogFactory.GetLogger("CWTask.CreateDeviceTaskByQueue");
            CWLocation cwlctn = new CWLocation();
            CWDevice cwdevice = new CWDevice();

            Response resp = new Response();
            EnmTaskStatus state = EnmTaskStatus.Init;
            string locSize = "";
            string plateNum = "";
            #region
            if (dev.Type == EnmSMGType.ETV)
            {
                if (queue.TelegramType == 13 && queue.SubTelegramType == 1)
                {
                    state = EnmTaskStatus.TWaitforLoad;
                    #region
                    if (queue.MasterType == EnmTaskType.SaveCar)
                    {
                        Location loc = await cwlctn.FindLocationAsync(l => l.Address == queue.ToLctAddress);
                        if (loc != null && loc.Type == EnmLocationType.Normal)
                        {
                            locSize = loc.LocSize;
                            plateNum = loc.PlateNum;
                        }
                    }
                    else
                    {
                        Location loc = await cwlctn.FindLocationAsync(l => l.Address == queue.FromLctAddress);
                        if (loc != null && loc.Type == EnmLocationType.Normal)
                        {
                            locSize = loc.LocSize;
                            plateNum = loc.PlateNum;
                        }
                    }
                    #endregion
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
                #region
                Location loc = await cwlctn.FindLocationAsync(l => l.Address == queue.ToLctAddress);
                if (loc != null && loc.Type == EnmLocationType.Normal)
                {
                    locSize = loc.LocSize;
                    plateNum = loc.PlateNum;
                }
                #endregion
            }
            if (state == EnmTaskStatus.Init)
            {
                log.Error("没有办法生成对应的报文状态，TelegramType-" + queue.TelegramType + "  SubTelegramType-" + queue.SubTelegramType);
                return resp;
            }
            #endregion

            if (queue.MasterType == EnmTaskType.Transpose)
            {
                #region 修改源车位、目的车位的状态
                Location frLoc = await cwlctn.FindLocationAsync(l => l.Warehouse == queue.Warehouse && l.Address == queue.FromLctAddress);
                Location toLoc = await cwlctn.FindLocationAsync(l => l.Warehouse == queue.Warehouse && l.Address == queue.ToLctAddress);
                if (frLoc != null && toLoc != null)
                {
                    frLoc.Status = EnmLocationStatus.Outing;
                    toLoc.Status = EnmLocationStatus.Entering;

                    cwlctn.UpdateLocation(frLoc);
                    cwlctn.UpdateLocation(toLoc);
                }
                #endregion

                //如果当前要执行的作业是挪移作业
                #region 如果目的车位是1边，则判断后面的车位是否在等待作业，如果是，则不允许下发的
                if (dev.TaskID == 0)
                {
                    Location toLct = await cwlctn.FindLocationAsync(lc => lc.Address == queue.ToLctAddress);
                    if (toLct != null)
                    {
                        //后面有车位，且是重列的
                        if (toLct.NeedBackup == 0)
                        {
                            string backaddrs = string.Format((toLct.LocSide + 2).ToString() + toLct.Address.Substring(1));
                            Location inner = await cwlctn.FindLocationAsync(lc => lc.Address == backaddrs);
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
                Location tolctn = await cwlctn.FindLocationAsync(lc => lc.Address == toaddrs && lc.Warehouse == wh);
                if (tolctn != null)
                {
                    if (tolctn.NeedBackup == 1)
                    {
                        string side = "";
                        //前面车位
                        side = (tolctn.LocSide - 2).ToString();
                        string forwardaddrs = side + tolctn.Address.Substring(1);
                        Location forwardLctn = await cwlctn.FindLocationAsync(lc => lc.Address == forwardaddrs && lc.Warehouse == wh);
                        if (forwardLctn == null)
                        {
                            log.Error("系统错误，目标车位-" + toaddrs + ",库区- " + wh + ", 找不到其前面车位，地址-" + forwardaddrs);
                            return resp;
                        }

                        if (forwardLctn.Status == EnmLocationStatus.Occupy)
                        {
                            //要先判断是否需要生成避让作业
                            bool isAvoid = true;
                            #region
                            isAvoid = JudgeAvoidOfToAddrs(forwardLctn.Address, dev);
                            #endregion

                            if (isAvoid)
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
                                    CreateDate = DateTime.Now,
                                    HallCode = 11,
                                    FromLctAddress = forwardLctn.Address,
                                    ToLctAddress = transLctn.Address,
                                    ICCardCode = forwardLctn.ICCode,
                                    Distance = forwardLctn.WheelBase,
                                    CarSize = forwardLctn.CarSize,
                                    CarWeight = forwardLctn.CarWeight,
                                    IsComplete = 0,
                                    LocSize = forwardLctn.LocSize,
                                    PlateNum = forwardLctn.PlateNum
                                };
                                resp = AddITask(transtask);
                                if (resp.Code == 1)
                                {
                                    dev.SoonTaskID = 0;
                                    dev.TaskID = transtask.ID;
                                    resp = new CWDevice().Update(dev);
                                    log.Info("将挪移转化为执行作业，绑定于设备，deviceCode-" + dev.DeviceCode + " ,TaskID-" + dev.TaskID + " ,ICCode - " + forwardLctn.ICCode);

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
                                        AddQueue(transback_queue);
                                        log.Info("生成回挪队列，deviceCode-" + dev.DeviceCode + " ,ID-" + transback_queue.ID + " ,ICCode - " + forwardLctn.ICCode);
                                    }
                                    #endregion
                                }

                                return resp;

                                #endregion
                            }
                            else
                            {
                                resp.Code = 0;
                                resp.Message = "需求避让，不允许下发";
                                return resp;
                            }
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
                IsComplete = 0,
                LocSize = locSize,
                PlateNum = plateNum
            };
            resp = AddITask(subtask);
            if (resp.Code == 1)
            {
                dev.SoonTaskID = 0;
                dev.TaskID = subtask.ID;
                resp = new CWDevice().Update(dev);
                log.Info("转化为执行作业，绑定于设备，devicode-" + dev.DeviceCode + " ,TaskID- " + dev.TaskID);
                //删除队列
                resp = DeleteQueue(queue.ID);
                log.Info("转化为执行作业，删除队列，WorkQueue ID-" + queue.ID);
            }
            resp.Message = "转化为执行作业，操作成功！DeviceCode-" + subtask.DeviceCode + " ,TaskID- " + subtask.ID;

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
            unloadtask.SendDtime = DateTime.Now;
            UpdateITask(unloadtask);

            DeleteQueue(unloadQueue.ID);
        }

        /// <summary>
        /// 判断作业是否可以实行，要不生成别的TV的避让作业
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="dev">ETV</param>
        /// <returns></returns>
        public async Task<Response> DealAvoidAsync(int queueID, int warehouse, int code)
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("CWTask.DealAvoid");
            try
            {
                WorkTask queue = await FindQueueAsync(queueID);
                if (queue == null)
                {
                    log.Error("依ID号找不到队列，ID - " + queueID);
                    resp.Message = "找不到队列";
                    return resp;
                }
                Device smg = await new CWDevice().FindAsync(d => d.Warehouse == warehouse && d.DeviceCode == code);
                if (smg == null)
                {
                    log.Error("依设备号找不到设备，code - " + code);
                    resp.Message = "找不到设备";
                    return resp;
                }

                #region
                CWTask cwtask = new CWTask();
                int nWarehouse = smg.Warehouse;
                List<Device> Etvs = new CWDevice().FindList(d => d.Type == EnmSMGType.ETV);
                int curEtvCol = BasicClss.GetColumnByAddrs(smg.Address);
                string toAddrs = "";
                if (queue.IsMaster == 1)
                {
                    if (queue.TelegramType == 13 && queue.SubTelegramType == 1)
                    {
                        toAddrs = queue.FromLctAddress;
                    }
                    else
                    {
                        toAddrs = queue.ToLctAddress;
                    }
                }
                else if (queue.IsMaster == 2)
                {
                    toAddrs = queue.FromLctAddress;
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
                    log.Error("系统故障，找不到对面的ETV，不允许下发指令");
                    return resp;
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
                    return resp;
                }
                #endregion
                if (otherEtv.TaskID == 0)
                {
                    #region 另一台ETV空闲的
                    if (curMin < otherCol && otherCol < curMax)
                    {
                        if (otherEtv.IsAble == 0)
                        {
                            log.Error("对面的ETV没有启用，不允许下发指令, other code- " + otherEtv.DeviceCode);
                            return resp;
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
                                CreateDate = DateTime.Now,
                                HallCode = queue.HallCode,
                                FromLctAddress = otherEtv.Address,
                                ToLctAddress = toAddress,
                                ICCardCode = queue.ICCardCode,
                                Distance = 0,
                                CarSize = "",
                                CarWeight = 0,
                                IsComplete = 0
                            };
                            resp = manager.Add(subtask);
                            if (resp.Code == 1)
                            {
                                otherEtv.SoonTaskID = 0;
                                otherEtv.TaskID = subtask.ID;
                                resp = new CWDevice().Update(otherEtv);
                                log.Info("生成避让作业，并绑定于设备 , 避让卡号- " + queue.ICCardCode + " , 目的车位-" + toAddress);
                            }
                            return resp;
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
                            resp = manager_queue.Add(queue_avoid);
                            if (resp.Code == 1)
                            {
                                log.Info("添加避让入队列，避让卡号-" + queue_avoid.ICCardCode + "，目的车位-" + toAddress);
                            }
                            #endregion
                            return resp;
                        }
                        #endregion
                    }
                    resp.Code = 1;
                    resp.Message = "允许下发";
                    return resp;
                    #endregion
                }
                else  //另一台ETV在忙
                {
                    ImplementTask othertak = FindITask(otherEtv.TaskID);

                    #region 要下发的作业的TV 处于卸载等待时
                    if (smg.TaskID != 0)
                    {
                        //两TV处于等待卸载中
                        ImplementTask itask = FindITask(smg.TaskID);

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
                                //同列卸载，优先后面的作业
                                if (curSide == 1 && otherSide == 3)
                                {
                                    resp.Code = 0;
                                    return resp;
                                }
                                if (curSide == 2 && otherSide == 4)
                                {
                                    resp.Code = 0;
                                    return resp;
                                }
                            }
                            #endregion                                                        
                        }

                        if (othertak.Status == EnmTaskStatus.WillWaitForUnload)
                        {
                            #region
                            if (curMin < otherCol && otherCol < curMax)
                            {
                                if (otherEtv.IsAvailabe == 0)
                                {
                                    resp.Code = 0;
                                    return resp;
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
                                    CreateDate = DateTime.Now,
                                    HallCode = queue.HallCode,
                                    FromLctAddress = otherEtv.Address,
                                    ToLctAddress = toAddress,
                                    ICCardCode = queue.ICCardCode,
                                    Distance = 0,
                                    CarSize = "",
                                    CarWeight = 0,
                                    IsComplete = 0
                                };
                                resp = manager.Add(subtask);
                                if (resp.Code == 1)
                                {
                                    otherEtv.SoonTaskID = othertak.ID;
                                    otherEtv.TaskID = subtask.ID;
                                    resp = new CWDevice().Update(otherEtv);
                                    log.Info("生成避让作业，并绑定于设备 , 避让卡号- " + queue.ICCardCode + " , 目的车位-" + toAddress);

                                    return resp;
                                }
                                else
                                {
                                    resp.Code = 0;
                                    return resp;
                                }
                                #endregion
                            }
                            resp.Code = 1;
                            return resp;
                            #endregion
                        }

                        //如果当前要卸载，则要判断下其后排车位是不是在进行作业，如果是，则不允许动作
                        if (itask.Status == EnmTaskStatus.WillWaitForUnload)
                        {
                            if (itask.Type == EnmTaskType.SaveCar)
                            {
                                Location toLocation = new CWLocation().FindLocation(l => l.Warehouse == smg.Warehouse && l.Address == itask.ToLctAddress);
                                if (toLocation != null)
                                {
                                    if (toLocation.LocSide == 2)
                                    {
                                        //如果后面的车位在作业，优先其动作
                                        string bck = (toLocation.LocSide + 2).ToString() + toLocation.Address.Substring(1);
                                        Location nbackLctn = new CWLocation().FindLocation(l => l.Warehouse == smg.Warehouse && l.Address == bck);
                                        if (nbackLctn != null)
                                        {
                                            if (nbackLctn.Status == EnmLocationStatus.Entering ||
                                                nbackLctn.Status == EnmLocationStatus.Outing)
                                            {
                                                resp.Code = 0;
                                                return resp;
                                            }
                                        }
                                    }
                                }
                            }
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
                        resp.Code = 0;
                        return resp;
                    }
                    if (toColumn > curMin && toColumn < curMax)
                    {
                        resp.Code = 0;
                        return resp;
                    }
                    if (otherCol > curMax)
                    {
                        if (toColumn < curMax)
                        {
                            resp.Code = 0;
                            return resp;
                        }
                    }
                    if (toColumn > curMax)
                    {
                        if (toColumn < curMax)
                        {
                            resp.Code = 0;
                            return resp;
                        }
                    }
                    if (otherCol < curMin)
                    {
                        if (toColumn > curMin)
                        {
                            resp.Code = 0;
                            return resp;
                        }
                    }
                    if (toColumn < curMin)
                    {
                        if (otherCol > curMin)
                        {
                            resp.Code = 0;
                            return resp;
                        }
                    }
                    #endregion
                }
                #endregion
                resp.Code = 1;
                return resp;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Code = 0;
                resp.Message = "系统异常";
            }
            return resp;
        }

        public Response DealAvoid(int queueID, int warehouse, int code)
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("CWTask.DealAvoid");
            try
            {
                WorkTask queue = FindQueue(queueID);
                if (queue == null)
                {
                    log.Error("依ID号找不到队列，ID - " + queueID);
                    resp.Message = "找不到队列";
                    return resp;
                }
                Device smg = new CWDevice().Find(d => d.Warehouse == warehouse && d.DeviceCode == code);
                if (smg == null)
                {
                    log.Error("依设备号找不到设备，code - " + code);
                    resp.Message = "找不到设备";
                    return resp;
                }

                #region
                CWTask cwtask = new CWTask();
                int nWarehouse = smg.Warehouse;
                List<Device> Etvs = new CWDevice().FindList(d => d.Type == EnmSMGType.ETV);
                int curEtvCol = BasicClss.GetColumnByAddrs(smg.Address);
                string toAddrs = "";
                if (queue.IsMaster == 1)
                {
                    if (queue.TelegramType == 13 && queue.SubTelegramType == 1)
                    {
                        toAddrs = queue.FromLctAddress;
                    }
                    else
                    {
                        toAddrs = queue.ToLctAddress;
                    }
                }
                else if (queue.IsMaster == 2)
                {
                    toAddrs = queue.FromLctAddress;
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
                    log.Error("系统故障，找不到对面的ETV，不允许下发指令");
                    return resp;
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
                    return resp;
                }
                #endregion
                if (otherEtv.TaskID == 0)
                {
                    #region 另一台ETV空闲的
                    if (curMin < otherCol && otherCol < curMax)
                    {
                        if (otherEtv.IsAble == 0)
                        {
                            log.Error("对面的ETV没有启用，不允许下发指令, other code- " + otherEtv.DeviceCode);
                            return resp;
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
                                CreateDate = DateTime.Now,
                                HallCode = queue.HallCode,
                                FromLctAddress = otherEtv.Address,
                                ToLctAddress = toAddress,
                                ICCardCode = queue.ICCardCode,
                                Distance = 0,
                                CarSize = "",
                                CarWeight = 0,
                                IsComplete = 0
                            };
                            resp = manager.Add(subtask);
                            if (resp.Code == 1)
                            {
                                otherEtv.SoonTaskID = 0;
                                otherEtv.TaskID = subtask.ID;
                                resp = new CWDevice().Update(otherEtv);
                                log.Info("生成避让作业，并绑定于设备 , 避让卡号- " + queue.ICCardCode + " , 目的车位-" + toAddress);
                            }
                            return resp;
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
                            resp = manager_queue.Add(queue_avoid);
                            if (resp.Code == 1)
                            {
                                log.Info("添加避让入队列，避让卡号-" + queue_avoid.ICCardCode + "，目的车位-" + toAddress);
                            }
                            #endregion
                            return resp;
                        }
                        #endregion
                    }
                    resp.Code = 1;
                    resp.Message = "允许下发";
                    return resp;
                    #endregion
                }
                else  //另一台ETV在忙
                {
                    ImplementTask othertak = FindITask(otherEtv.TaskID);

                    #region 要下发的作业的TV 处于卸载等待时
                    if (smg.TaskID != 0)
                    {
                        //两TV处于等待卸载中
                        ImplementTask itask = FindITask(smg.TaskID);

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
                                //同列卸载，优先后面的作业
                                if (curSide == 1 && otherSide == 3)
                                {
                                    resp.Code = 0;
                                    return resp;
                                }
                                if (curSide == 2 && otherSide == 4)
                                {
                                    resp.Code = 0;
                                    return resp;
                                }
                            }
                            #endregion                                                        
                        }

                        if (othertak.Status == EnmTaskStatus.WillWaitForUnload)
                        {
                            #region
                            if (curMin < otherCol && otherCol < curMax)
                            {
                                if (otherEtv.IsAvailabe == 0)
                                {
                                    resp.Code = 0;
                                    return resp;
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
                                    CreateDate = DateTime.Now,
                                    HallCode = queue.HallCode,
                                    FromLctAddress = otherEtv.Address,
                                    ToLctAddress = toAddress,
                                    ICCardCode = queue.ICCardCode,
                                    Distance = 0,
                                    CarSize = "",
                                    CarWeight = 0,
                                    IsComplete = 0
                                };
                                resp = manager.Add(subtask);
                                if (resp.Code == 1)
                                {
                                    otherEtv.SoonTaskID = othertak.ID;
                                    otherEtv.TaskID = subtask.ID;
                                    resp = new CWDevice().Update(otherEtv);
                                    log.Info("生成避让作业，并绑定于设备 , 避让卡号- " + queue.ICCardCode + " , 目的车位-" + toAddress);

                                    return resp;
                                }
                                else
                                {
                                    resp.Code = 0;
                                    return resp;
                                }
                                #endregion
                            }
                            resp.Code = 1;
                            return resp;
                            #endregion
                        }

                        //如果当前要卸载，则要判断下其后排车位是不是在进行作业，如果是，则不允许动作
                        if (itask.Status == EnmTaskStatus.WillWaitForUnload)
                        {
                            if (itask.Type == EnmTaskType.SaveCar)
                            {
                                Location toLocation = new CWLocation().FindLocation(l => l.Warehouse == smg.Warehouse && l.Address == itask.ToLctAddress);
                                if (toLocation != null)
                                {
                                    if (toLocation.LocSide == 2)
                                    {
                                        //如果后面的车位在作业，优先其动作
                                        string bck = (toLocation.LocSide + 2).ToString() + toLocation.Address.Substring(1);
                                        Location nbackLctn = new CWLocation().FindLocation(l => l.Warehouse == smg.Warehouse && l.Address == bck);
                                        if (nbackLctn != null)
                                        {
                                            if (nbackLctn.Status == EnmLocationStatus.Entering ||
                                                nbackLctn.Status == EnmLocationStatus.Outing)
                                            {
                                                resp.Code = 0;
                                                return resp;
                                            }
                                        }
                                    }
                                }
                            }
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
                        resp.Code = 0;
                        return resp;
                    }
                    if (toColumn > curMin && toColumn < curMax)
                    {
                        resp.Code = 0;
                        return resp;
                    }
                    if (otherCol > curMax)
                    {
                        if (toColumn < curMax)
                        {
                            resp.Code = 0;
                            return resp;
                        }
                    }
                    if (toColumn > curMax)
                    {
                        if (toColumn < curMax)
                        {
                            resp.Code = 0;
                            return resp;
                        }
                    }
                    if (otherCol < curMin)
                    {
                        if (toColumn > curMin)
                        {
                            resp.Code = 0;
                            return resp;
                        }
                    }
                    if (toColumn < curMin)
                    {
                        if (otherCol > curMin)
                        {
                            resp.Code = 0;
                            return resp;
                        }
                    }
                    #endregion
                }
                #endregion
                resp.Code = 1;
                return resp;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Code = 0;
                resp.Message = "系统异常";
            }
            return resp;
        }

        /// <summary>
        ///  依终点坐标判断是否需要生成对应的避让作业
        /// </summary>
        /// <param name="toAddrs"></param>
        /// <param name="smg"></param>
        /// <returns></returns>
        public bool JudgeAvoidOfToAddrs(string toAddrs, Device smg)
        {
            Log log = LogFactory.GetLogger("JudgeAvoidOfToAddrs");
            try
            {
                List<Device> Etvs = new CWDevice().FindList(d => d.Type == EnmSMGType.ETV);
                int curEtvCol = BasicClss.GetColumnByAddrs(smg.Address);

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
                    log.Error("系统故障，找不到对面的ETV，不允许下发指令");
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
                            log.Error("对面的ETV在路径内，但没有启用，不允许下发指令, other code- " + otherEtv.DeviceCode);
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
                                CreateDate = DateTime.Now,
                                HallCode = 11,
                                FromLctAddress = otherEtv.Address,
                                ToLctAddress = toAddress,
                                ICCardCode = "9999",
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
                                log.Info("生成避让作业，并绑定于设备 - " + otherEtv.DeviceCode + " , 请求避让车位地址" + toAddrs + " ,避让的目的车位-" + toAddress);
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
                                ICCardCode = "9999",
                                Distance = 0,
                                CarSize = "",
                                CarWeight = 0
                            };
                            Response resp1 = manager_queue.Add(queue_avoid);
                            if (resp1.Code == 1)
                            {
                                log.Info("添加避让入队列，目的车位-" + toAddress);
                            }
                            #endregion
                            return true;
                        }
                        #endregion
                    }
                    else
                    {
                        //不在范围内，不请求避让
                        return true;
                    }
                    #endregion
                }
                else  //另一台ETV在忙
                {
                    ImplementTask othertak = FindITask(otherEtv.TaskID);

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

            #region 生成车厅作业，并绑定于车厅上
            ImplementTask hallTask = new ImplementTask()
            {
                Warehouse = master.Warehouse,
                DeviceCode = master.DeviceCode,
                Type = master.MasterType,
                Status = state,
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
                IsComplete = 0,
                LocSize = lct.LocSize,
                PlateNum = lct.PlateNum
            };
            resp = AddITask(hallTask);
            if (resp.Code == 1)
            {
                hall.TaskID = hallTask.ID;
                hall.SoonTaskID = 0;
                cwdevice.Update(hall);
            }
            #endregion

            bool isAdd = false;
            if (tv.IsAble == 1 && tv.IsAvailabe == 1)
            {
                if (tv.TaskID == 0)
                {
                    #region 判断是否要生成避让  
                    Response _resp = DealAvoid(master.ID, tv.Warehouse, tv.DeviceCode);
                    if (_resp.Code == 1)
                    {
                        //要判断是否生成倒库作业
                        #region 生成挪移作业
                        string toaddrs = master.FromLctAddress;
                        int wh = master.Warehouse;
                        Location tolctn = cwlctn.FindLocation(lc => lc.Address == toaddrs && lc.Warehouse == wh);
                        if (tolctn != null)
                        {
                            if (tolctn.NeedBackup == 1)
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
                                    //Location transLctn = new AllocateTV().PPYAllocateLctnNeedTransfer(forwardLctn);
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
                                        Warehouse = tv.Warehouse,
                                        DeviceCode = tv.DeviceCode,
                                        Type = EnmTaskType.Transpose,
                                        Status = EnmTaskStatus.TWaitforLoad,
                                        SendStatusDetail = EnmTaskStatusDetail.NoSend,
                                        SendDtime = DateTime.Now,
                                        CreateDate = DateTime.Now,
                                        HallCode = hall.DeviceCode,
                                        FromLctAddress = forwardLctn.Address,
                                        ToLctAddress = transLctn.Address,
                                        ICCardCode = forwardLctn.ICCode,
                                        Distance = forwardLctn.WheelBase,
                                        CarSize = forwardLctn.CarSize,
                                        CarWeight = forwardLctn.CarWeight,
                                        IsComplete = 0,
                                        LocSize = forwardLctn.LocSize,
                                        PlateNum = forwardLctn.PlateNum
                                    };
                                    resp = AddITask(transtask);
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
                                        AddQueue(waitqueue);

                                        //删除队列
                                        resp = DeleteQueue(master.ID);

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
                                                Warehouse = tv.Warehouse,
                                                DeviceCode = tv.DeviceCode,
                                                MasterType = EnmTaskType.Transpose,
                                                TelegramType = 13,
                                                SubTelegramType = 1,
                                                HallCode = hall.DeviceCode,
                                                FromLctAddress = transLctn.Address,
                                                ToLctAddress = forwardLctn.Address,
                                                ICCardCode = forwardLctn.ICCode,
                                                Distance = forwardLctn.WheelBase,
                                                CarSize = forwardLctn.CarSize,
                                                CarWeight = forwardLctn.CarWeight
                                            };
                                            AddQueue(transback_queue);
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
                                    AddQueue(waitqueue);

                                    //删除队列
                                    resp = DeleteQueue(master.ID);

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
                            IsComplete = 0,
                            LocSize = lct.LocSize,
                            PlateNum = lct.PlateNum
                        };
                        resp = AddITask(TvTask);
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
                    #endregion
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
                AddQueue(waitqueue);
            }

            //删除队列
            resp = DeleteQueue(master.ID);

            #endregion
            return resp;
        }

        /// <summary>
        /// 提前装载，组装车厅报文加入队列
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="lct"></param>
        /// <returns></returns>
        public Response AheadTvTelegramAndBuildHall(WorkTask master, Location lct, Device hall)
        {
            Log log = LogFactory.GetLogger("CWTask.AheadTvTelegramAndBuildHall");
            Response resp = new Response();
            CWDevice cwdevice = new CWDevice();
            CWLocation cwlctn = new CWLocation();
            try
            {
                Device tv = new AllocateTV().Allocate(hall, lct);
                //Device tv = cwdevice.Find(d => d.Region == lct.Region);
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
                        //判断是否允许下发,是否需求避让
                        Response _resp = DealAvoid(master.ID, tv.Warehouse, tv.DeviceCode);
                        if (_resp.Code == 1)
                        {
                            #region
                            bool isCreateITask = true;
                            //要判断是否生成倒库作业
                            #region 生成挪移作业
                            string toaddrs = master.FromLctAddress;
                            int wh = master.Warehouse;
                            Location tolctn = cwlctn.FindLocation(lc => lc.Address == toaddrs && lc.Warehouse == wh);
                            if (tolctn != null)
                            {
                                if (tolctn.NeedBackup == 1)
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
                                        //Location transLctn = new AllocateTV().PPYAllocateLctnNeedTransfer(forwardLctn);
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
                                            Warehouse = tv.Warehouse,
                                            DeviceCode = tv.DeviceCode,
                                            Type = EnmTaskType.Transpose,
                                            Status = EnmTaskStatus.TWaitforLoad,
                                            SendStatusDetail = EnmTaskStatusDetail.NoSend,
                                            SendDtime = DateTime.Now,
                                            CreateDate = DateTime.Now,
                                            HallCode = hall.DeviceCode,
                                            FromLctAddress = forwardLctn.Address,
                                            ToLctAddress = transLctn.Address,
                                            ICCardCode = forwardLctn.ICCode,
                                            Distance = forwardLctn.WheelBase,
                                            CarSize = forwardLctn.CarSize,
                                            CarWeight = forwardLctn.CarWeight,
                                            IsComplete = 0,
                                            LocSize = forwardLctn.LocSize,
                                            PlateNum = forwardLctn.PlateNum
                                        };
                                        resp = AddITask(transtask);
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
                                            AddQueue(waitqueue);
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
                                                    Warehouse = tv.Warehouse,
                                                    DeviceCode = tv.DeviceCode,
                                                    MasterType = EnmTaskType.Transpose,
                                                    TelegramType = 13,
                                                    SubTelegramType = 1,
                                                    HallCode = hall.DeviceCode,
                                                    FromLctAddress = transLctn.Address,
                                                    ToLctAddress = forwardLctn.Address,
                                                    ICCardCode = forwardLctn.ICCode,
                                                    Distance = forwardLctn.WheelBase,
                                                    CarSize = forwardLctn.CarSize,
                                                    CarWeight = forwardLctn.CarWeight
                                                };
                                                AddQueue(transback_queue);
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            log.Info("提前装载时，生成执行作业，加入队列时异常，Message-" + resp.Message);
                                        }
                                        #endregion
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
                                    IsComplete = 0,
                                    LocSize = lct.LocSize,
                                    PlateNum = lct.PlateNum
                                };
                                resp = AddITask(TvTask);
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
                            else if (master.MasterType == EnmTaskType.TempGet)
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
                            AddQueue(waitHallQueue);

                            //删除队列
                            DeleteQueue(master.ID);

                            #endregion
                        }
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
        public async Task<int> DealTempLocOccupy(int warehouse)
        {
            Log log = LogFactory.GetLogger("DealTempLocOccupy");
            try
            {
                List<Location> tempOccupyLst = await new CWLocation().FindLocationListAsync(loc =>
                                                 loc.Type == EnmLocationType.Temporary &&
                                                 loc.Status == EnmLocationStatus.Occupy);
                if (tempOccupyLst.Count == 0)
                {
                    return 0;
                }
                #region
                Location frLctn = tempOccupyLst[0];
                Location toLctn;
                Device etv = new AllocateTV().AllocateTvOfTransport(frLctn, out toLctn);
                if (etv == null)
                {
                    return 0;
                }
                if (toLctn == null)
                {
                    return 0;
                }
                if (etv.TaskID == 0 && etv.IsAvailabe == 1)
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
                        IsComplete = 0,
                        LocSize = frLctn.LocSize,
                        PlateNum = frLctn.PlateNum
                    };
                    Response resp = AddITask(TvTask);
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
                        ICCardCode = frLctn.ICCode,
                        Distance = frLctn.WheelBase,
                        CarSize = frLctn.CarSize,
                        CarWeight = frLctn.CarWeight
                    };
                    AddQueue(tvQueue);
                }
                //修改车位状态
                frLctn.Status = EnmLocationStatus.Outing;
                toLctn.Status = EnmLocationStatus.Entering;
                CWLocation cwlctn = new CWLocation();
                cwlctn.UpdateLocation(frLctn);
                cwlctn.UpdateLocation(toLctn);
                #endregion

                return 1;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return 0;
        }

        /// <summary>
        /// 完成作业，删除任务
        /// </summary>
        /// <param name="itask"></param>
        public void DeleteSubTask(ImplementTask itask)
        {
            itask.Status = EnmTaskStatus.Finished;
            itask.IsComplete = 1;
            DeleteITask(itask);
        }

        /// <summary>
        /// 设备上绑定有作业号，但找不到该作业的，则清空该作业信息
        /// </summary>
        /// <param name="warehouse"></param>
        public async Task<int> ReleaseDeviceTaskIDButNoTaskAsync(int warehouse)
        {
            CWDevice cwdevice = new CWDevice();
            List<Device> devsLst = await new CWDevice().FindListAsync(d => d.Warehouse == warehouse);
            for (int i = 0; i < devsLst.Count; i++)
            {
                Device smg = devsLst[i];
                if (smg.TaskID != 0)
                {
                    ImplementTask itask = await manager.FindAsync(smg.TaskID);
                    if (itask == null)
                    {
                        smg.TaskID = 0;
                        cwdevice.Update(smg);
                    }
                }
            }
            return 1;
        }

        /// <summary>
        /// 移动设备，MURO继续
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public Response MUROTask(int ids)
        {
            Response resp = new Response();
            #region
            Log log = LogFactory.GetLogger("MUROTask");
            CWDevice cwdevice = new CWDevice();
            try
            {
                ImplementTask itask = manager.Find(ids);
                if (itask == null)
                {
                    resp.Message = "依ID号找不到对应的作业号";
                    return resp;
                }
                if (itask.Status != EnmTaskStatus.TMURO)
                {
                    resp.Message = "作业状态不是处于故障中";
                    return resp;
                }
                Device smg = cwdevice.Find(d => d.TaskID == itask.ID);
                if (smg == null)
                {
                    resp.Message = "没有找到对应的设备";
                    return resp;
                }
                if (smg.IsAble == 0)
                {
                    resp.Message = "设备ETV- " + smg.DeviceCode + " 没有启用！";
                    return resp;
                }
                if (smg.IsAvailabe == 0)
                {
                    resp.Message = "设备ETV- " + smg.DeviceCode + " 不可接收新指令！";
                    return resp;
                }
                int nback = cwdevice.JudgeTVHasCar(smg.Warehouse, smg.DeviceCode);
                //首先删除队列中的卸载作业
                WorkTask mtask = FindQueue(mn => mn.ICCardCode == itask.ICCardCode &&
                                                mn.DeviceCode < 10 &&
                                                mn.MasterType == itask.Type &&
                                                mn.TelegramType == 14 &&
                                                mn.SubTelegramType == 1);
                if (mtask != null)
                {
                    DeleteQueue(mtask.ID);
                }
                //搬运器上有车，则下步执行卸载动作
                if (nback == 10)
                {
                    itask.Status = EnmTaskStatus.TMUROWaitforUnload;
                    itask.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                    itask.SendDtime = DateTime.Now;
                    UpdateITask(itask);
                    resp.Message = "发送卸载指令";
                }
                //搬运器上无车，则下步执行装载动作
                else if (nback == 20)
                {
                    itask.Status = EnmTaskStatus.TMUROWaitforLoad;
                    itask.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                    itask.SendDtime = DateTime.Now;
                    UpdateITask(itask);
                    resp.Message = "发送装载指令";
                }
                else
                {
                    resp.Message = "搬运器状态异常！";
                }

                string rcdmsg = "MURO继续，卡号 - " + itask.ICCardCode + " " + resp.Message + " 源车位 - " + itask.FromLctAddress + " 目的车位 - " + itask.ToLctAddress;
                OperateLog olog = new OperateLog
                {
                    Description = rcdmsg,
                    CreateDate = DateTime.Now,
                    OptName = ""
                };
                new CWOperateRecordLog().AddOperateLog(olog);

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Message = "系统异常";
            }
            #endregion
            return resp;
        }

        public async Task ResetHallOnlyHasTaskAsync(int warehouse, int hallID)
        {
            CWDevice cwdevice = new CWDevice();
            Device smg = await cwdevice.FindAsync(d => d.Warehouse == warehouse && d.DeviceCode == hallID);
            if (smg != null && smg.Type == EnmSMGType.Hall)
            {
                ImplementTask itask = FindITask(smg.TaskID);
                if (itask != null && itask.Type != EnmTaskType.TempGet)
                {
                    ImplementTask etask = FindITask(ts => ts.ICCardCode == itask.ICCardCode && ts.ID != itask.ID);
                    //只有车厅作业的，可以复位该车厅
                    if (etask == null)
                    {
                        string msg = "模式切换时， 强制复位车厅作业,HallID - " + smg.DeviceCode + " Task Type - " + itask.Type + " Status - " + itask.Status + " Iccard - " + itask.ICCardCode + " FromLoc - " + itask.FromLctAddress + " ToLoc - " + itask.ToLctAddress;

                        #region 清除存车指纹库记录
                        if (!string.IsNullOrEmpty(itask.ICCardCode))
                        {
                            Location loc = await new CWLocation().FindLocationAsync(it => it.ICCode == itask.ICCardCode);
                            if (loc == null)
                            {
                                CWSaveProof cwsaveproof = new CWSaveProof();
                                int cno = Convert.ToInt32(itask.ICCardCode);
                                SaveCertificate scert = cwsaveproof.Find(sa => sa.SNO == cno);
                                if (scert != null)
                                {
                                    cwsaveproof.Delete(scert.ID);
                                }
                            }
                        }
                        #endregion

                        smg.TaskID = 0;
                        cwdevice.Update(smg);
                        DeleteSubTask(itask);

                        OperateLog log = new OperateLog()
                        {
                            Description = msg,
                            CreateDate = DateTime.Now,
                            OptName = "system"
                        };
                        new CWOperateRecordLog().AddOperateLog(log);
                    }
                }
            }
        }

        #region 队列管理
        private WorkTaskManager manager_queue = new WorkTaskManager();

        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public Response AddQueue(WorkTask queue)
        {
            Log log = LogFactory.GetLogger("AddQueue");
            Response resp = new Response();
            try
            {
                resp = manager_queue.Add(queue);
                if (resp.Code == 1)
                {
                    MainCallback<WorkTask>.Instance().OnChange(1, queue);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Message = "系统异常";
            }
            return resp;
        }

        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public Response DeleteQueue(int tid)
        {
            Log log = LogFactory.GetLogger("DeleteQueue");
            Response resp = new Response();
            try
            {
                WorkTask mtsk = manager_queue.Find(tid);
                WorkTask copy = new WorkTask
                {
                    ID = mtsk.ID,
                    IsMaster = mtsk.IsMaster,
                    Warehouse = mtsk.Warehouse,
                    DeviceCode = mtsk.DeviceCode,
                    MasterType = mtsk.MasterType,
                    TelegramType = 0,
                    SubTelegramType = 0,
                    HallCode = mtsk.HallCode,
                    FromLctAddress = mtsk.FromLctAddress,
                    ToLctAddress = mtsk.ToLctAddress,
                    ICCardCode = mtsk.ICCardCode,
                    Distance = mtsk.Distance,
                    CarSize = mtsk.CarSize,
                    CarWeight = 0
                };
                resp = manager_queue.Delete(tid);
                if (resp.Code == 1)
                {
                    MainCallback<WorkTask>.Instance().OnChange(3, copy);
                }
            }
            catch (Exception ex)
            {

                log.Error(ex.ToString());
            }
            return resp;
        }

        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public async Task<Response> DeleteQueueAsync(int tid)
        {
            Log log = LogFactory.GetLogger("DeleteQueue");
            Response resp = new Response();
            try
            {
                WorkTask mtsk = await manager_queue.FindAsync(tid);
                WorkTask copy = new WorkTask
                {
                    ID = mtsk.ID,
                    IsMaster = mtsk.IsMaster,
                    Warehouse = mtsk.Warehouse,
                    DeviceCode = mtsk.DeviceCode,
                    MasterType = mtsk.MasterType,
                    TelegramType = 0,
                    SubTelegramType = 0,
                    HallCode = mtsk.HallCode,
                    FromLctAddress = mtsk.FromLctAddress,
                    ToLctAddress = mtsk.ToLctAddress,
                    ICCardCode = mtsk.ICCardCode,
                    Distance = mtsk.Distance,
                    CarSize = mtsk.CarSize,
                    CarWeight = 0
                };
                resp = manager_queue.Delete(tid);
                if (resp.Code == 1)
                {
                    MainCallback<WorkTask>.Instance().OnChange(3, copy);
                }
            }
            catch (Exception ex)
            {

                log.Error(ex.ToString());
            }
            return resp;
        }

        public WorkTask FindQueue(int id)
        {
            return manager_queue.Find(id);
        }

        public async Task<WorkTask> FindQueueAsync(int id)
        {
            return await manager_queue.FindAsync(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public WorkTask FindQueue(Expression<Func<WorkTask, bool>> where)
        {
            return manager_queue.Find(where);
        }

        public async Task<WorkTask> FindQueueAsync(Expression<Func<WorkTask, bool>> where)
        {
            return await manager_queue.FindAsync(where);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<WorkTask> FindQueueLst()
        {
            return manager_queue.FindList();
        }

        public async Task<List<WorkTask>> FindQueueLstAsync()
        {
            return await manager_queue.FindListAsync();
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

        public async Task<List<WorkTask>> FindQueueListAsync(Expression<Func<WorkTask, bool>> where)
        {
            return await manager_queue.FindListAsync(where);
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
        public Page<WorkTask> FindPagelist(Page<WorkTask> pageWork, Expression<Func<WorkTask, bool>> where, OrderParam param)
        {

            if (param == null)
            {
                param = new OrderParam()
                {
                    PropertyName = "ID",
                    Method = OrderMethod.Asc
                };
            }
            Page<WorkTask> page = manager_queue.FindPageList(pageWork, where, param);
            return page;

        }
        /// <summary>
        /// 页面删除队列
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Response ManualDeleteQueue(int ID)
        {
            CWLocation cwlctn = new CWLocation();
            Response resp = new Response();
            WorkTask queue = FindQueue(ID);
            if (queue == null)
            {
                resp.Code = 0;
                resp.Message = "系统故障，找不到队列，ID-" + ID;
                return resp;
            }
            if (queue.IsMaster == 2)
            {
                Location toLctn = cwlctn.FindLocation(lc => lc.Warehouse == queue.Warehouse && lc.Address == queue.FromLctAddress);
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
                    else if (queue.MasterType == EnmTaskType.Transpose)
                    {
                        Location frLctn = cwlctn.FindLocation(lc => lc.Warehouse == queue.Warehouse && lc.Address == queue.FromLctAddress);
                        Location toLctn = cwlctn.FindLocation(lc => lc.Warehouse == queue.Warehouse && lc.Address == queue.ToLctAddress);
                        if (frLctn != null && toLctn != null)
                        {
                            frLctn.Status = EnmLocationStatus.Occupy;
                            cwlctn.UpdateLocation(frLctn);

                            toLctn.Status = EnmLocationStatus.Space;
                            cwlctn.UpdateLocation(toLctn);
                        }
                    }
                    #endregion
                }
            }

            string rcdmsg = "删除队列，卡号 - " + queue.ICCardCode + " 源车位 - " + queue.FromLctAddress + " 目的车位- " + queue.ToLctAddress + " Type-" + queue.MasterType.ToString() + "  报文类型- " + queue.TelegramType + " 子类型- " + queue.SubTelegramType;

            resp = DeleteQueue(ID);

            OperateLog olog = new OperateLog
            {
                Description = rcdmsg,
                CreateDate = DateTime.Now,
                OptName = ""
            };
            new CWOperateRecordLog().AddOperateLog(olog);
            return resp;

        }

        /// <summary>
        /// 获取车厅所有取车数量
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        /// <returns></returns>
        public int GetHallGetCarCount(int warehouse, int hallID)
        {
            List<WorkTask> queues = manager_queue.FindList(qu => qu.Warehouse == warehouse &&
                                                                qu.DeviceCode == hallID &&
                                                                qu.IsMaster == 2 &&
                                                                (qu.MasterType == EnmTaskType.GetCar || qu.MasterType == EnmTaskType.TempGet));
            return queues.Count;
        }

        #endregion

        public async Task DealCarDriveOffTracingAsync(int warehouse, int devicecode)
        {
            Log log = LogFactory.GetLogger("DealCarDriveOffTracing");
            try
            {
                Device hall = await new CWDevice().FindAsync(d => d.Warehouse == warehouse && d.DeviceCode == devicecode);
                if (hall == null)
                {
                    return;
                }
                if (hall.Type != EnmSMGType.Hall)
                {
                    return;
                }
                if (hall.TaskID == 0)
                {
                    return;
                }

                ImplementTask itask = Find(hall.TaskID);
                if (itask.Status != EnmTaskStatus.ISecondSwipedWaitforCheckSize)
                {
                    return;
                }
                this.AddNofication(warehouse, devicecode, "44.wav");

                if (itask.Type == EnmTaskType.TempGet)
                {
                    itask.Status = EnmTaskStatus.IFirstSwipedWaitforCheckSize;
                    itask.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                    itask.SendDtime = DateTime.Now;

                    UpdateITask(itask);
                }
                else
                {
                    itask.IsComplete = 1;
                    DeleteITask(itask);

                    hall.TaskID = 0;
                    new CWDevice().Update(hall);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 车厅装载时高度复检不通过，释放原来车位，重新分配车位
        /// </summary>       
        /// <returns></returns>
        public async Task ReCheckCarWithLoadAsync(int taskid, int distance, string checkcode)
        {
            Log log = LogFactory.GetLogger("ReCheckCarWithLoad");
            try
            {
                ImplementTask etsk = await FindAsync(taskid);
                if (etsk == null)
                {
                    log.Error("系统异常，依ID - " + taskid + " 找不到作业！");
                    return;
                }
                if (checkcode.Length != 3)
                {
                    log.Error("上报外形尺寸不正确，CheckCode - " + checkcode);
                    return;
                }
                CWDevice cwdevice = new CWDevice();
                CWLocation cwlctn = new CWLocation();

                Device etv = await cwdevice.FindAsync(d => d.Warehouse == etsk.Warehouse && d.DeviceCode == etsk.DeviceCode);
                if (etv == null)
                {
                    log.Error("找不到设备，DeviceCode - " + etsk.DeviceCode);
                    return;
                }
                //只接收存车的
                if (etsk.Type == EnmTaskType.SaveCar)
                {
                    #region 将车厅作业完成
                    Device hall = cwdevice.Find(cd => cd.Warehouse == etsk.Warehouse && cd.DeviceCode == etsk.HallCode);
                    if (hall != null)
                    {
                        ImplementTask halltask = FindITask(tt => tt.Warehouse == hall.Warehouse && tt.DeviceCode == hall.DeviceCode);
                        if (halltask != null)
                        {
                            halltask.Status = EnmTaskStatus.Finished;
                            halltask.IsComplete = 1;
                            DeleteITask(halltask);
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

                    Location toLct = await cwlctn.FindLocationAsync(l => l.Address == etsk.ToLctAddress && l.Warehouse == etsk.Warehouse);

                    //查找车位，如果查找得到，则复位原来的车位，同时生成卸载作业；
                    //如果查找不到，则不做任何处理
                    Location loc = new AllocateLocBySecond().AllocateLocOfEtvScope(etv, checkcode);
                    if (loc == null)
                    {
                        log.Info("复检尺寸不通过时，二次分配车位，找不到合适车位，checkcode - " + checkcode + ",原来存车位 - " + toLct.Address + " 尺寸 - " + toLct.LocSize);
                        return;
                    }
                    loc.Status = EnmLocationStatus.Entering;
                    loc.InDate = toLct.InDate;
                    loc.WheelBase = distance;
                    loc.CarSize = checkcode;
                    loc.PlateNum = toLct.PlateNum;
                    loc.ImagePath = toLct.ImagePath;
                    loc.ICCode = etsk.ICCardCode;
                    await cwlctn.UpdateLocationAsync(loc);

                    toLct.Status = EnmLocationStatus.Space;
                    toLct.InDate = DateTime.Parse("2017-1-1");
                    toLct.WheelBase = 0;
                    toLct.CarSize = "";
                    toLct.ICCode = "";
                    toLct.PlateNum = "";
                    toLct.ImagePath = "";
                    await cwlctn.UpdateLocationAsync(toLct);

                    etsk.ToLctAddress = loc.Address;
                    etsk.Distance = distance;
                    etsk.CarSize = checkcode;
                    etsk.LocSize = loc.LocSize;
                    etsk.Status = EnmTaskStatus.ReCheckInLoad;
                    etsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                    etsk.SendDtime = DateTime.Now;

                    await UpdateITaskAsync(etsk);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 车位卸载时,车位上有车，则重新分配车位
        /// </summary>
        public async Task DealUnloadButHasCarBlock(int taskid)
        {
            Log log = LogFactory.GetLogger("DealUnloadButHasCarBlock");
            try
            {
                #region
                CWLocation cwlctn = new CWLocation();
                CWDevice cwdevice = new CWDevice();

                ImplementTask etask = await FindAsync(taskid);
                if (etask == null)
                {
                    log.Error("系统异常，依ID - " + taskid + " 找不到作业！");
                    return;
                }
                if (etask.Type == EnmTaskType.GetCar)
                {
                    log.Error("当前作业是取车，为非法报文！");
                    return;
                }
                Device etv = await cwdevice.FindAsync(d => d.Warehouse == etask.Warehouse && d.DeviceCode == etask.DeviceCode);
                if (etv == null)
                {
                    log.Error("系统异常，找不到移动设备 devicecode - " + etask.DeviceCode);
                    return;
                }
                Location unloadLctn = await cwlctn.FindLocationAsync(l => l.Warehouse == etask.Warehouse && l.Address == etask.ToLctAddress);
                if (unloadLctn == null)
                {
                    log.Error("系统异常，找不到卸载车位 - " + etask.ToLctAddress);
                    return;
                }
                if (unloadLctn.Type != EnmLocationType.Normal)
                {
                    log.Error("卸载车位 - " + etask.ToLctAddress + " 不是正常车位，可能是车厅！");
                    return;
                }
                if (etask.IsComplete == 11)
                {
                    log.Info("已经进行分配过车位，不再进行分配车位");
                    return;
                }
                Location transLctn = new AllocateLocBySecond().AllocateLocOfEtvScope(etv, unloadLctn.CarSize);
                if (transLctn == null)
                {
                    log.Info("卸载车位上有车时，二次分配车位，找不到合适车位，checkcode - " + unloadLctn.CarSize + ",卸载车位 - " + unloadLctn.Address);
                    return;
                }
                //将原车位数据搬移，同时将其类型设为系统禁用，修改作业状态
                transLctn.Status = EnmLocationStatus.Entering;
                transLctn.ICCode = unloadLctn.ICCode;
                transLctn.InDate = unloadLctn.InDate;
                transLctn.WheelBase = unloadLctn.WheelBase;
                transLctn.CarSize = unloadLctn.CarSize;
                transLctn.PlateNum = unloadLctn.PlateNum;
                transLctn.ImagePath = unloadLctn.ImagePath;

                await cwlctn.UpdateLocationAsync(transLctn);

                //禁用原车位
                unloadLctn.Type = EnmLocationType.HasCarLocker;
                unloadLctn.Status = EnmLocationStatus.Space;
                unloadLctn.InDate = DateTime.Parse("2017-1-1");
                unloadLctn.WheelBase = 0;
                unloadLctn.CarSize = "";
                unloadLctn.ICCode = "";
                unloadLctn.PlateNum = "";
                unloadLctn.ImagePath = "";

                await cwlctn.UpdateLocationAsync(transLctn);

                etask.Status = EnmTaskStatus.WaitforDeleteTask;
                etask.SendStatusDetail = EnmTaskStatusDetail.NoSend;
                etask.SendDtime = DateTime.Now;
                etask.ToLctAddress = transLctn.Address;
                etask.LocSize = transLctn.LocSize;

                await UpdateITaskAsync(etask);
                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

    }
}
