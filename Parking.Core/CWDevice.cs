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
            return manager.Find(dev=>dev.Warehouse==warehouse&&dev.DeviceCode==code);
        }

        public Response Update(Device smg)
        {
            return manager.Update(smg);
        }

        public void UpdateSMGStatus(Device smg,int state)
        {
            smg.IsAble = state;
            Update(smg);
        }

        public Device Find(Expression<Func<Device, bool>> where)
        {
            return manager.Find(where);
        }

        public List<Device> FindList(Expression<Func<Device, bool>> where)
        {
            return manager.FindList(where);
        }

        public List<Device> FindList(Expression<Func<Device, bool>> where,OrderParam param)
        {
            return manager.FindList(where, param);
        }

        /// <summary>
        /// 查询，分页显示
        /// </summary>
        /// <param name="pageWork"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Page<Device> FindPageList(Page<Device> pageWork, Expression<Func<Device, bool>> where, OrderParam param)
        {
            if (param == null)
            {
                param = new OrderParam()
                {
                    PropertyName = "ID",
                    Method = OrderMethod.Asc
                };
            }
            Page<Device> page = manager.FindPageList(pageWork, where, param);

            return page;
        }

        #region 报警状态位控制
        private AlarmManager manager_alarm = new AlarmManager();

        public List<Alarm> FindAlarmList(Expression<Func<Alarm, bool>> where)
        {
            return manager_alarm.FindList(where);
        }
        #endregion

    }
}
