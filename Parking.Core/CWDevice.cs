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

        /// <summary>
        /// 分配车厅
        /// </summary>
        /// <param name="lct"></param>
        /// <param name="isTemp"></param>
        /// <returns></returns>
        public int AllocateHall(Location lct,bool isTemp)
        {            
            List<Device> hallsList = manager.FindList(d=>d.Type==EnmSMGType.Hall);
            var query = from hall in hallsList
                        where hall.Mode == EnmModel.Automatic &&
                              hall.IsAble == 1 &&
                             (isTemp ? hall.HallType == EnmHallType.EnterOrExit : hall.HallType != EnmHallType.Entrance)
                        orderby Math.Abs(Convert.ToInt16(hall.Address.Substring(1, 2)) - lct.LocColumn) ascending
                        select hall;

            List<Device> avaibleHalls = query.ToList();
            if (avaibleHalls.Count == 0)
            {
                return 0;
            }
            if (avaibleHalls.Count == 1)
            {
                return avaibleHalls[0].DeviceCode;
            }
            Device first = avaibleHalls.Find(h=>h.TaskID==0);
            if (first != null)
            {
                return first.DeviceCode;
            }
            Dictionary<int, int> _dicHallTaskCount = new Dictionary<int, int>();
            List<WorkTask> queueList = new CWTask().FindQueueList(q => true);
            foreach(Device dev in avaibleHalls)
            {
                int count = 0;
                foreach(WorkTask wt in queueList)
                {
                    if (dev.DeviceCode == wt.DeviceCode)
                    {
                        count++;
                    }
                }
                _dicHallTaskCount.Add(dev.DeviceCode, count);
            }
            Dictionary<int, int> dicHallOrder = _dicHallTaskCount.OrderBy(d => d.Value).ToDictionary(o => o.Key, p => p.Value);

            return dicHallOrder.FirstOrDefault().Key;
        }






        #region 报警状态位控制
        private AlarmManager manager_alarm = new AlarmManager();

        public List<Alarm> FindAlarmList(Expression<Func<Alarm, bool>> where)
        {
            return manager_alarm.FindList(where);
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="alarmLst"></param>
        /// <returns></returns>
        public Response UpdateAlarmList(List<Alarm> alarmLst)
        {
            return manager_alarm.UpdateAlarmList(alarmLst);
        }

        #endregion

    }
}
