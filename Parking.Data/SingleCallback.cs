using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Data
{
    public delegate void ManualEventHandler(int type, Location loc);
    /// <summary>
    /// 个推
    /// </summary>
    public class SingleCallback
    {
        public event ManualEventHandler ManualOprtEvent;

        /// <summary>
        /// 手动入库、手动出库时，发送下入出库信息给到云服务
        /// </summary>
        /// <param name="type">1、手动入库。 2、手动出库</param>
        /// <param name="loc"></param>
        public void OnChange(int type, Location loc)
        {
            if (ManualOprtEvent != null)
            {
                ManualOprtEvent(type, loc);
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
