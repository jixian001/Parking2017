using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;

namespace Parking.Data
{
    public delegate void ParkingRcdEventHandler(ParkingRecord record);
    /// <summary>
    /// 给云服务推送的
    /// </summary>
    public class CloudCallback
    {
        public event ParkingRcdEventHandler ParkingRcdWatchEvent;

        public void WatchParkingRcd(ParkingRecord record)
        {
            if (ParkingRcdWatchEvent != null)
            {
                ParkingRcdWatchEvent(record);
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
