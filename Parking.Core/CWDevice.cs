using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;
using System.Linq.Expressions;
using System.Web;
using System.Web.Caching;

namespace Parking.Core
{
    /// <summary>
    /// 设备的业务逻辑
    /// </summary>
    public class CWDevice
    {
        private DeviceManager manager = new DeviceManager();       

        public CWDevice()
        {
        }

        public Response Update(Device smg)
        {
            Response resp = manager.Update(smg);
            if (resp.Code == 1)
            {
                //回调设备状态
                MainCallback<Device>.Instance().OnChange(2, resp.Data);
            }
            #region 记录设备状态
            string addrs = "";
            if (smg.Type == EnmSMGType.ETV)
            {
                addrs = smg.Address;
            }

            DeviceInfoLog devlog = new DeviceInfoLog
            {
                Warehouse = smg.Warehouse,
                DeviceCode = smg.DeviceCode,
                RecordDtime = DateTime.Now,
                Mode =ConvertMode(smg.Mode),
                IsAble = smg.IsAble,
                IsAvailabe = smg.IsAvailabe,
                RunStep = smg.RunStep,
                InStep = smg.InStep,
                OutStep = smg.OutStep,
                Address = addrs,
                TaskID = smg.TaskID
            };
            new CWDeviceStatusLog().AddLog(devlog);
            #endregion
            return resp;
        }

        public async Task<Response> UpdateAsync(Device smg)
        {
            Response resp = await manager.UpdateAsync(smg);
            if (resp.Code == 1)
            {
                //回调设备状态
                MainCallback<Device>.Instance().OnChange(2, resp.Data);
            }
            return resp;
        }

        public int UpdateSMGStatus(Device smg, int state)
        {
            smg.IsAble = state;
            Response resp = Update(smg);
            return resp.Code;
        }

        public Device Find(Expression<Func<Device, bool>> where)
        {
            return manager.Find(where);
        }

        public async Task<Device> FindAsync(Expression<Func<Device, bool>> where)
        {
            return await manager.FindAsync(where);
        }

        public List<Device> FindList()
        {
            return manager.FindList();
        }

        public async Task<List<Device>> FindListAsync()
        {
            return await manager.FindListAsync();
        }

        public List<Device> FindList(Expression<Func<Device, bool>> where)
        {
            List<Device> devsLst = manager.FindList(where);
            return devsLst;
        }

        public async Task<List<Device>> FindListAsync(Expression<Func<Device, bool>> where)
        {
            return await manager.FindListAsync(where);
        }

        public List<Device> FindList(Expression<Func<Device, bool>> where, OrderParam param)
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
        public int AllocateHall(Location lct, bool isTemp)
        {
            List<Device> hallsList = FindList(d => d.Type == EnmSMGType.Hall);
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
            Device first = avaibleHalls.Find(h => h.TaskID == 0);
            if (first != null)
            {
                return first.DeviceCode;
            }
            Dictionary<int, int> _dicHallTaskCount = new Dictionary<int, int>();
            List<WorkTask> queueList = new CWTask().FindQueueList(q => true);
            foreach (Device dev in avaibleHalls)
            {
                int count = 0;
                foreach (WorkTask wt in queueList)
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

        /// <summary>
        /// 判断搬运器上有车否
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public int JudgeTVHasCar(int warehouse, int code)
        {
            List<Alarm> alarmsLst = FindAlarmList(a => a.Warehouse == warehouse && a.DeviceCode == code);
            if (alarmsLst == null || alarmsLst.Count == 0)
            {
                return 0;
            }
            #region
            int hascaraddr = 0;
            int nocaraddr = 0;
            int partaholdaddr = 0;
            int partbholdaddr = 0;
            int partareleaseaddr = 0;
            int partbreleaseaddr = 0;
            try
            {
                string hascar = XMLHelper.GetRootNodeValueByXpath("root", "CarrierHasCar");
                int.TryParse(hascar, out hascaraddr);

                string nocar = XMLHelper.GetRootNodeValueByXpath("root", "CarrierNoCar");
                int.TryParse(nocar, out nocaraddr);

                string partahold = XMLHelper.GetRootNodeValueByXpath("root", "PartAHold");
                int.TryParse(hascar, out hascaraddr);

                string partbhold = XMLHelper.GetRootNodeValueByXpath("root", "PartBHold");
                int.TryParse(partbhold, out partbholdaddr);

                string partarelease = XMLHelper.GetRootNodeValueByXpath("root", "PartARelease");
                int.TryParse(partarelease, out partareleaseaddr);

                string partbrelease = XMLHelper.GetRootNodeValueByXpath("root", "PartBRelease");
                int.TryParse(partbrelease, out partbreleaseaddr);
            }
            catch (Exception ex)
            {
                Log log = LogFactory.GetLogger("JudgeTVHasCar");
                log.Error(ex.ToString());
            }
            #endregion
            //搬运器上有车           
            Alarm hasCar = alarmsLst.Find(a => a.Address == hascaraddr);
            if (hasCar == null)
            {
                return 1;
            }
            //A夹持
            Alarm partA_j = alarmsLst.Find(a => a.Address == partaholdaddr);
            //B夹持
            Alarm partB_j = alarmsLst.Find(a => a.Address == partbholdaddr);
            if (partA_j == null || partB_j == null)
            {
                return 2;
            }
            //搬运器上无车           
            Alarm noCar = alarmsLst.Find(a => a.Address == nocaraddr);
            if (hasCar == null)
            {
                return 3;
            }
            //A松开
            Alarm partA_r = alarmsLst.Find(a => a.Address == partareleaseaddr);
            //B松开
            Alarm partB_r = alarmsLst.Find(a => a.Address == partbreleaseaddr);
            if (partA_j == null || partB_j == null)
            {
                return 4;
            }
            //有车判断
            if (hasCar.Value == 1 && partA_j.Value == 1 && partB_j.Value == 1)
            {
                return 10;
            }
            //无车判断
            if (noCar.Value == 1 && partA_r.Value == 1 && partB_r.Value == 1)
            {
                return 20;
            }
            return 0;
        }

        #region 报警状态位控制
        private AlarmManager manager_alarm = new AlarmManager();

        public Alarm FindAlarm(Expression<Func<Alarm, bool>> where)
        {
            Log log = LogFactory.GetLogger("FindAlarm");
            try
            {
                return manager_alarm.Find(where);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return null;
        }

        public List<Alarm> FindAlarmList(Expression<Func<Alarm, bool>> where)
        {
            return manager_alarm.FindList(where);
        }

        public async Task<List<Alarm>> FindAlarmListAsync(Expression<Func<Alarm, bool>> where)
        {
            return await manager_alarm.FindListAsync(where);
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="alarmLst"></param>
        /// <returns></returns>
        public async Task<Response> UpdateAlarmListAsync(List<Alarm> alarmLst)
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("UpdateAlarmList");
            try
            {
                resp = await manager_alarm.UpdateAlarmListAsync(alarmLst);

                #region 报警记录写入数据库              
                List<Alarm> faultLst = alarmLst.FindAll(f => f.Color == EnmAlarmColor.Red);
                if (faultLst.Count > 0)
                {
                    new CWFaultLog().AddFaultRecord(faultLst);
                }
                #endregion

                #region 写状态位记录入数据库
                List<Alarm> statusLst = alarmLst.FindAll(f => f.Color == EnmAlarmColor.Green);
                if (faultLst.Count > 0)
                {
                    new CWStatusLog().AddStateRecord(statusLst);
                }
                #endregion

                #region 推送至显示
                int warehouse = alarmLst.First().Warehouse;
                int devicecode = alarmLst.First().DeviceCode;
                List<BackAlarm> backlst = new List<BackAlarm>();

                List<Alarm> hasValueLst = manager_alarm.FindList(al => al.Warehouse == warehouse && al.DeviceCode == devicecode && al.Value == 1);
                foreach (Alarm ar in hasValueLst)
                {
                    int type = 0;
                    if (ar.Color == EnmAlarmColor.Green)
                    {
                        type = 1;
                    }
                    else if (ar.Color == EnmAlarmColor.Red)
                    {
                        type = 2;
                    }

                    BackAlarm back = new BackAlarm
                    {
                        Type = type,
                        Description = ar.Description
                    };

                    backlst.Add(back);
                }

                SingleCallback.Instance().WatchFaults(warehouse, devicecode, backlst);
                #endregion              
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return resp;
        }

        #endregion

        private string ConvertMode(EnmModel model)
        {
            string md = "";
            switch (model)
            {
                case EnmModel.Automatic:
                    md = "全自动";
                    break;
                case EnmModel.StandAlone:
                    md = "单机";
                    break;
                case EnmModel.Manual:
                    md = "手动";
                    break;
                case EnmModel.Maintance:
                    md = "维修";
                    break;
                default:
                    md = model.ToString();
                    break;
            }
            return md;
        }

    }
}
