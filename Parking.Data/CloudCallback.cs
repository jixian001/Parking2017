using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;

namespace Parking.Data
{
    public delegate void ParkingRcdEventHandler(ParkingRecord record);
    public delegate void WorkTaskEventHandler(int type, WorkTask mtask);
    public delegate void ImpTaskEventHandler(int type, ImplementTask itask);
    public delegate void SendSMSEventHandler(SMSInfo sms);

    /// <summary>
    /// 给云服务推送的
    /// </summary>
    public class CloudCallback
    {
        public event ParkingRcdEventHandler ParkingRcdWatchEvent;
        public event WorkTaskEventHandler MasterTaskWatchEvent;
        public event ImpTaskEventHandler ImpTaskWatchEvent;
        public event SendSMSEventHandler SendSMSWatchEvent;

        /// <summary>
        /// 停车记录推送
        /// </summary>
        /// <param name="record"></param>
        public void WatchParkingRcd(ParkingRecord record)
        {
            if (ParkingRcdWatchEvent != null)
            {
                ParkingRcdWatchEvent(record);
            }
        }

        /// <summary>
        /// 队列的添加，及更新(这里更新时，暂不推送给云端，不关注车厅号发生变化)
        /// </summary>
        /// <param name="type">1-添加，2-更新</param>
        /// <param name="mtsk"></param>
        public void WatchWorkTask(int type, WorkTask mtsk)
        {
            if (MasterTaskWatchEvent != null)
            {
                MasterTaskWatchEvent(type, mtsk);
            }
        }

        /// <summary>
        /// 执行作业的操作推送，
        /// </summary>
        /// <param name="type">1-添加，2-更新，3-删除</param>
        /// <param name="subtask"></param>
        public void WatchImpTask(int type,ImplementTask subtask)
        {
            if (ImpTaskWatchEvent != null)
            {
                ImpTaskWatchEvent(type, subtask);
            }
        }

        /// <summary>
        /// 发短信息，给云服务
        /// </summary>
        /// <param name="sms"></param>
        public void SendSMSToCloud(SMSInfo sms)
        {
            if (SendSMSWatchEvent != null)
            {
                SendSMSWatchEvent(sms);
            }
        }

        //定义单例模式,延时初始化
        private static CloudCallback _singleton;
        public static CloudCallback Instance()
        {
            return _singleton ?? (_singleton = new CloudCallback());
        }
    }
}
