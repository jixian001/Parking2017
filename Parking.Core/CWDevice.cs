using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;

namespace Parking.Core
{
    /// <summary>
    /// 设备的业务逻辑
    /// </summary>
    public class CWDevice
    {    
        private DeviceManager manager=new DeviceManager();

        public CWDevice()
        {
           
        }
        /// <summary>
        /// 依设备号查找设备
        /// </summary>
        /// <param name="code"></param>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public Device SelectSMG(int code,int warehouse)
        {
            return manager.GetDeviceByCode(code,warehouse);
        }

    }
}
