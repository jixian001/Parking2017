#region
using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Parking.Data;
using Parking.Application;
using Parking.Core;
using Parking.Web.Models;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Newtonsoft.Json;
#endregion

namespace Parking.Web
{
    public class ParkingSingleton
    {
        // signleton instance
        private readonly static Lazy<ParkingSingleton> _instance = new Lazy<ParkingSingleton>(
            () => new ParkingSingleton(GlobalHost.ConnectionManager.GetHubContext<ParkingHub>().Clients));

        private ServeStatus _state=ServeStatus.Close;

        private EqpService service;
        private Log log;      

        private Dictionary<string, string> dicCurrClients = new Dictionary<string, string>();     

        private ParkingSingleton(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;          
            service = new EqpService();
            log = LogFactory.GetLogger("ParkingSingleton");

            MainCallback<Location>.Instance().WatchEvent += FileWatch_LctnWatchEvent;
            MainCallback<Device>.Instance().WatchEvent += FileWatch_DeviceWatchEvent;
            MainCallback<Alarm>.Instance().WatchEvent+= FileWatch_FaultWatchEvent;
            MainCallback<ImplementTask>.Instance().WatchEvent += FileWatch_IMPTaskWatchEvent;
            MainCallback<WorkTask>.Instance().WatchEvent += ParkingSingleton_WatchEvent;

            SingleCallback.Instance().ManualOprtEvent += ParkingSingleton_ManualOprtEvent;
        }

        public IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public static ParkingSingleton Instance
        {
            get
            {
                return _instance.Value;
            }
            private set {; }
        }

        public enum ServeStatus
        {
            Close=0,
            Open
        }

        public ServeStatus ServerState
        {
            get
            {
                //去查询下状态，再给值
                _state = service.RunState ? ServeStatus.Open : ServeStatus.Close;

                return _state;
            }
            private set {; }
        }       

        public void openServe()
        {
            if (ServerState != ServeStatus.Open)
            {
                //这里要引用到application层
                service.OnStart();

                _state = ServeStatus.Open;              
            }
            else
            {
                //关闭再重新打开
                service.OnStop();               
            }
            broadcastStateChange(_state);
        }

        public void closeServe()
        {
            //这里要引用到application层
            service.OnStop();

            _state = ServeStatus.Close;
            broadcastStateChange(_state);
        }

        public void register(string client,string connID)
        {
            lock (dicCurrClients)
            {
                if (!dicCurrClients.ContainsKey(connID))
                {
                    dicCurrClients.Add(connID, client);
                }
            }
            Clients.All.nowUsers(dicCurrClients);
        }

        public void removeClient(string connID)
        {
            lock (dicCurrClients)
            {
                if (dicCurrClients.ContainsKey(connID))
                {
                    dicCurrClients.Remove(connID);
                }
            }
            Clients.All.nowUsers(dicCurrClients);
        }

        private void broadcastStateChange(ServeStatus marketState)
        {
            switch (marketState)
            {
                case ServeStatus.Open:
                    Clients.All.serveOpened();
                    break;
                case ServeStatus.Close:
                    Clients.All.serveClosed();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 推送车位信息
        /// </summary>
        /// <param name="loc"></param>
        private void FileWatch_LctnWatchEvent(int type, Location loca)
        {
            #region
            int total = 0;
            int occupy = 0;
            int space = 0;
            int fix = 0;
            int bspace = 0;
            int sspace = 0;
            List<Location> locLst = new CWLocation().FindLocationList(lc => lc.Type != EnmLocationType.Invalid && lc.Type != EnmLocationType.Hall);
            total = locLst.Count;
            CWICCard cwiccd = new CWICCard();
            foreach (Location loc in locLst)
            {
                #region
                if (loc.Type == EnmLocationType.Normal)
                {
                    if (cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null)
                    {
                        if (loc.Type == EnmLocationType.Normal)
                        {
                            if (loc.Status == EnmLocationStatus.Space)
                            {
                                space++;
                                if (loc.LocSize.Length == 3)
                                {
                                    string last = loc.LocSize.Substring(2);
                                    if (last == "1")
                                    {
                                        sspace++;
                                    }
                                    else if (last == "2")
                                    {
                                        bspace++;
                                    }
                                }
                            }
                            else if (loc.Status == EnmLocationStatus.Occupy)
                            {
                                occupy++;
                            }
                        }
                    }
                    else
                    {
                        fix++;
                    }
                }
                #endregion
            }
            StatisInfo info = new StatisInfo
            {
                Total = total,
                Occupy = occupy,
                Space = space,
                SmallSpace = sspace,
                BigSpace = bspace,
                FixLoc = fix
            };
            #endregion

            Clients.All.feedbackLocInfo(loca);

            Clients.All.feedbackStatistInfo(info);           
        }

        /// <summary>
        /// 推送设备信息
        /// </summary>
        /// <param name="entity"></param>
        private void FileWatch_DeviceWatchEvent(int type, Device smg)
        {
            if (log != null)
            {
                log.Debug("  warehouse- " + smg.Warehouse + " ,devicecode-" + smg.DeviceCode);
            }

            Clients.All.feedbackDevice(smg);          
        }

        /// <summary>
        /// 推送执行作业信息
        /// </summary>
        /// <param name="itask"></param>
        private void FileWatch_IMPTaskWatchEvent(int type, ImplementTask itask)
        {            
            string desp = itask.Warehouse.ToString() + itask.DeviceCode.ToString();
            string ctype = PlusCvt.ConvertTaskType(itask.Type);
            string status = PlusCvt.ConvertTaskStatus(itask.Status, itask.SendStatusDetail);
            DeviceTaskDetail detail = new DeviceTaskDetail
            {
                DevDescp = desp,
                TaskType = ctype,
                Status = status,
                Proof = itask.ICCardCode
            };
            //作业要删除时
            if (itask.IsComplete == 1||type==3)
            {
                detail.TaskType = "";
                detail.Status = "";
                detail.Proof = "";
            }
            //给界面用
            Clients.All.feedbackImpTask(detail);

            //给云服务处理用
            Clients.All.feedbackSubTask(itask);
        }

        private static Dictionary<Device, SMSInfo> dicSMSInfo = new Dictionary<Device, SMSInfo>();
        /// <summary>
        /// 用于处理有报警时给主界面显示红色，
        /// 同时可用于云服务，发送错误短信
        /// </summary>
        /// <param name="fault"></param>
        public void FileWatch_FaultWatchEvent(int type, Alarm fault)
        {
            #region 用于报警画面更新

            #endregion
            #region 用于发送SMS用
            if (fault.Color == EnmAlarmColor.Red)
            {
                //初步分析，跟当前保存上一个记录是不是重复
                Device smg = new CWDevice().Find(d => d.Warehouse == fault.Warehouse && d.DeviceCode == fault.DeviceCode);
                if (smg != null)
                {
                    bool isSend = true;
                    if (dicSMSInfo.ContainsKey(smg))
                    {
                        SMSInfo lastSMS = dicSMSInfo[smg];
                        if (lastSMS != null)
                        {
                            //自动步没有变化，则表示卡在这一步，则不会再次下发报警
                            if (lastSMS.AutoStep == smg.RunStep)
                            {
                                isSend = false;
                            }
                            //上次发送时间距
                            if (DateTime.Compare(DateTime.Now, lastSMS.RcdTime.AddMinutes(20)) < 0)
                            {
                                isSend = false;
                            }
                        }
                        //非全自动下，也不发送
                        if (smg.Mode != EnmModel.Automatic)
                        {
                            isSend = false;
                        }
                    }

                    if (isSend)
                    {
                        //本次要报警的卡号，与原先发送的卡号一致的,时间差小的，则当前的也不发送
                        CWTask cwtask = new CWTask();
                        foreach (KeyValuePair<Device, SMSInfo> pair in dicSMSInfo)
                        {
                            SMSInfo sms = pair.Value;
                            ImplementTask itask = cwtask.Find(smg.TaskID);
                            if (itask != null)
                            {
                                if (sms.ICCode == itask.ICCardCode && DateTime.Compare(DateTime.Now, sms.RcdTime.AddMinutes(5)) < 0)
                                {
                                    isSend = false;
                                    break;
                                }
                            }
                        }
                    }

                    //推送至接口中
                    if (isSend)
                    {
                        string iccode = "";
                        ImplementTask itask = new CWTask().Find(smg.TaskID);
                        if (itask != null)
                        {
                            iccode = itask.ICCardCode;
                        }
                        SMSInfo currSMS = new SMSInfo {
                            warehouse = smg.Warehouse,
                            DeviceCode = smg.DeviceCode,
                            AutoStep=smg.RunStep,
                            Message=fault.Description,
                            RcdTime=DateTime.Now
                        };
                        Clients.All.feedbackSMSInfo(currSMS);
                        //更新数据字典
                        lock (dicSMSInfo)
                        {
                            if (dicSMSInfo.ContainsKey(smg))
                            {
                                dicSMSInfo.Remove(smg);
                            }
                            dicSMSInfo.Add(smg, currSMS);
                        }
                    }

                }

            }
            #endregion
        }

        /// <summary>
        /// 队列信息回调
        /// </summary>
        /// <param name="entity"></param>
        private void ParkingSingleton_WatchEvent(int type, WorkTask entity)
        {
            List<WorkTask> mtskLst = new CWTask().FindQueueList(d => true);
            //删除
            if (type == 3)
            {
                if (mtskLst.Contains(entity))
                {
                    mtskLst.Remove(entity);
                }
            }
            string jsonStr = JsonConvert.SerializeObject(mtskLst);

            Clients.All.feedbackWorkTask(jsonStr);
        }

        /// <summary>
        /// 手动入出库时，上报车辆出入库信息
        /// </summary>
        /// <param name="loc"></param>
        private void ParkingSingleton_ManualOprtEvent(int type, Location loc)
        {            
            var data = new {
                Type=type,
                Data=loc
            };
            string jsonstr = JsonConvert.SerializeObject(data); 
            Clients.All.feedbackSinglePkRecord(jsonstr);
        }

    }
}