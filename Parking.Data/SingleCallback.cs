using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;

namespace Parking.Data
{    
    public delegate void ICCardEventHandler(string msg);
    public delegate void FaultsEventHandler(int warehouse, int code,List<BackAlarm> alarmLst);
    public delegate void PlateDisplayEventtHandler(PlateDisplay plate);
    /// <summary>
    /// 个推
    /// </summary>
    public class SingleCallback
    {       
        public event ICCardEventHandler ICCardWatchEvent;
        public event FaultsEventHandler FaultsWatchEvent;
        public event PlateDisplayEventtHandler PlateWatchEvent;
        /// <summary>
        /// 读到卡号时，引发
        /// </summary>
        /// <param name="physc"></param>
        public void WatchICCard(string physc)
        {
            if (ICCardWatchEvent != null)
            {
                ICCardWatchEvent(physc);
            }
        }

        /// <summary>
        /// 更新设备状态或报警
        /// </summary>
        /// <param name="needUpdateLst"></param>
        public void WatchFaults(int warehouse, int code, List<BackAlarm> alarmLst)
        {
            if (FaultsWatchEvent != null)
            {
                FaultsWatchEvent(warehouse,code,alarmLst);
            }
        }

        /// <summary>
        /// 车牌识别识别到车牌时回调页面显示
        /// </summary>
        /// <param name="platenum"></param>
        /// <param name="headpath">这个是虚拟路径+文件名</param>
        /// <param name="dtime"></param>
        public void WatchPlateInfo(PlateDisplay plate)
        {
            if (PlateWatchEvent != null)
            {
                PlateWatchEvent(plate);
            }
        }

        //定义单例模式,延时初始化
        private static SingleCallback _singleton;
        public static SingleCallback Instance()
        {
            return _singleton ?? (_singleton = new SingleCallback());
        }
    }
}
