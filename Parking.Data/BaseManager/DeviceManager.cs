using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;

namespace Parking.Data
{
    public class DeviceManager:BaseManager<Device>
    {
        /// <summary>
        /// 添加设备记录，暂不用，待后续用
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override Response Add(Device entity)
        {
            Response _resp = new Response();
            if (IsExist(entity.DeviceCode,entity.Warehouse))
            {
                _resp.Code = 2;
                _resp.Message = "当前设备Code已存在";
            }
            else
            {
                _resp = base.Add(entity);
            }
            return _resp;
        } 

        public bool IsExist(int code,int warehouse)
        {
            return base._repository.IsContains(dev => dev.DeviceCode == code&&dev.Warehouse==warehouse);
        }

        public Device GetDeviceByCode(int code,int warehouse)
        {
            return base._repository.Find(dev => dev.Warehouse == warehouse && dev.DeviceCode == code);
        }
       
    }
}
