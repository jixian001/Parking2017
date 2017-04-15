using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Parking.Web
{
    /// <summary>
    /// 使用骆驼命名法
    /// </summary>
    [HubName("parkingHub")]
    public class ParkingHub : Hub
    {
        private readonly ParkingSingleton _instance;        

        public ParkingHub() :
            this(ParkingSingleton.Instance)
        { }

        public ParkingHub(ParkingSingleton instance)
        {
            _instance = instance;
        }

        public string getStatus()
        {
            return _instance.ServerState.ToString();
        }

        public void openServe()
        {
            _instance.openServe();
        }

        public void closeServe()
        {
            _instance.closeServe();
        }
    }
}