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
        private static DeviceManager manager = new DeviceManager();
        private Log log;

        public CWDevice()
        {
            log = LogFactory.GetLogger("CWDevice");
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

        /// <summary>
        /// 强制更新
        /// </summary>
        /// <param name="smg"></param>
        /// <param name="isSave"></param>
        /// <returns></returns>
        public Response Update(Device smg,bool isSave)
        {
            return manager.Update(smg,isSave);
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
            List<Device> devsLst = new List<Device>();
            try
            {
                devsLst = manager.FindList(where);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return devsLst;
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
        /// <param name="isTemp">是否是临时取物</param>
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
        private static AlarmManager manager_alarm = new AlarmManager();

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

        #region 车牌识别映射到车厅设备表
        private static PlateInfoManager plateManager = new PlateInfoManager();

        /// <summary>
        /// 查找车厅车牌
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public PlateMappingDev FindPlateInfo(Expression<Func<PlateMappingDev, bool>> where)
        {
            return plateManager.Find(where);
        }

        /// <summary>
        /// 更新对应的车厅车牌信息
        /// 主要是清空
        /// </summary>
        /// <param name="dev"></param>
        /// <returns></returns>
        public Response UpdatePlateInfo(PlateMappingDev dev)
        {
            return plateManager.Update(dev);
        }

        /// <summary>
        /// 更新对应的车厅车牌信息       
        /// </summary>
        public Response UpdatePlateInfo(PlateMappingDev dev,bool isSave)
        {
            return plateManager.Update(dev,isSave);
        }
        #endregion

    }
}
