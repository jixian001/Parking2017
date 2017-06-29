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
        private void FileWatch_LctnWatchEvent(Location loca)
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
        private void FileWatch_DeviceWatchEvent(Device smg)
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
        private void FileWatch_IMPTaskWatchEvent(ImplementTask itask)
        {            
            string desp = itask.Warehouse.ToString() + itask.DeviceCode.ToString();
            string type = PlusCvt.ConvertTaskType(itask.Type);
            string status = PlusCvt.ConvertTaskStatus(itask.Status, itask.SendStatusDetail);
            DeviceTaskDetail detail = new DeviceTaskDetail
            {
                DevDescp = desp,
                TaskType = type,
                Status = status,
                Proof = itask.ICCardCode
            };
            //作业要删除时
            if (itask.IsComplete == 1)
            {
                detail.TaskType = "";
                detail.Status = "";
                detail.Proof = "";
            }
            Clients.All.feedbackImpTask(detail);
        }

        /// <summary>
        /// 用于处理有报警时给主界面显示红色，
        /// 同时可用于云服务，发送错误短信
        /// </summary>
        /// <param name="fault"></param>
        public void FileWatch_FaultWatchEvent(Alarm fault)
        {


        }

        /// <summary>
        /// 队列信息回调
        /// </summary>
        /// <param name="entity"></param>
        private void ParkingSingleton_WatchEvent(WorkTask entity)
        {
            Clients.All.feedbackWorkTask(entity);
        }
    }
}