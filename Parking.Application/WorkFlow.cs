using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Core;

namespace Parking.Application
{
    public class WorkFlow
    {
        private int warehouse;
        private string ipAddrs;
        private SocketPlc plcAccess;
        
        private WorkFlow()
        {
        }
        public WorkFlow(string ipaddrs,int wh)
        {
            ipAddrs = ipaddrs;
            warehouse = wh;
            plcAccess = new SocketPlc(ipAddrs);
        }

        public void ConnectPLC()
        {
            if (plcAccess != null)
            {
                plcAccess.ConnectPLC();
            }
        }

        public void DisConnect()
        {
            if (plcAccess != null)
            {
                plcAccess.DisConnectPLC();
            }
        }

        public void TaskAssign()
        {

        }

        public void SendMessage()
        {

        }

        public void ReceiveMessage()
        {

        }

        public void DealAlarmInfo()
        {

        }

    }
}
