using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Application
{
    interface IPLC
    {
        bool ConnectPLC();

        bool DisConnectPLC();

        bool IsConnected { get; set; }

        object ReadData(string itemName, SocketPlc.VarType varType);

        int WriteData(string itemName,object value);
    }
}
