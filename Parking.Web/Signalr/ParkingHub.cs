using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="clientName"></param>
        public void register(string clientName)
        {
            string connID = Context.ConnectionId;
            _instance.register(clientName, connID);
        }
        

        public void openServe()
        {           
        }

        public void closeServe()
        {           
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string connID = Context.ConnectionId;
            _instance.removeClient(connID);
            stopCalled = true;
            return base.OnDisconnected(stopCalled);
        }
    }
}