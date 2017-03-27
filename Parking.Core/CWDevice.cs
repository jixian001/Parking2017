using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;
using System.Linq.Expressions;

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

        public Response UpdateSMG(Device smg)
        {
            return manager.Update(smg);
        }

        public List<Device> FindList(Expression<Func<Device, bool>> where)
        {
            return manager.FindList(where);
        }

        public List<Device> FindList(Expression<Func<Device, bool>> where,OrderParam param)
        {
            return manager.FindList(where, param);
        }


    }
}
