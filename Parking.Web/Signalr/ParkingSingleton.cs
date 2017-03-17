using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Parking.Data;
using Parking.Application;
using Parking.Core;

namespace Parking.Web.Signalr
{
    public class ParkingSingleton
    {
        // signleton instance
        private readonly static Lazy<ParkingSingleton> _instance = new Lazy<ParkingSingleton>(
            ()=>new ParkingSingleton(GlobalHost.ConnectionManager.GetHubContext<ParkingHub>().Clients));

        private ServeStatus _state=ServeStatus.Close;

        private EqpService service;       

        public ParkingSingleton(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;            
            //订阅事件
            MainCallback.FileWatch.LctnWatchEvent += BroadcastLocation;
            MainCallback.FileWatch.DeviceWatchEvent += BroadcastDevice;
            MainCallback.FileWatch.FaultWatchEvent += BroadcastFault;

            service = new EqpService();
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
        /// 车位信息发生变化时推送车位信息
        /// </summary>
        /// <param name="lctn"></param>
        public void BroadcastLocation(Location lctn)
        {
            //推送对象
            Clients.All.feedbackLocInfo(lctn);
        }

        /// <summary>
        /// 推送设备信息
        /// </summary>
        /// <param name="dev"></param>
        public void BroadcastDevice(Device dev)
        {
            Clients.All.feedbackDeviceInfo(dev);
        }

        /// <summary>
        /// 推送报警及状态位信息
        /// </summary>
        /// <param name="fault"></param>
        public void BroadcastFault(Alarm fault)
        {
            Clients.All.feedbackFault(fault);
        }

    }
}