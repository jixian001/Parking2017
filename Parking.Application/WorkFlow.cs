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
        private string[] s7_Connection_Items = null;
        
        private WorkFlow()
        {
        }
        public WorkFlow(string ipaddrs,int wh)
        {
            ipAddrs = ipaddrs;
            warehouse = wh;
            plcAccess = new SocketPlc(ipAddrs);
        }

        public string[] S7_Connection_Items
        {
            get { return s7_Connection_Items; }
            set { s7_Connection_Items = value; }
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
