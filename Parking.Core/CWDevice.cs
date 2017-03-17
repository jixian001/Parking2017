using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;

namespace Parking.Core
{
    /// <summary>
    /// 设备的业务逻辑
    /// </summary>
    public class CWDevice
    {    
        private DeviceManager manager;

        public CWDevice()
        {
            manager = new DeviceManager();
            
        }



    }
}
