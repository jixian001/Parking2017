using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    /// <summary>
    /// 给云服务发送的停车记录信息
    /// </summary>
    public class ParkingRecord
    {
        /// <summary>
        /// 0-存车，1-取车
        /// </summary>
        public int TaskType { get; set; }
        public string LocAddrs { get; set; }
        public string Proof { get; set; }
        public string PlateNum { get; set; }
        public string InDate { get; set; }
        public int LocSize { get; set; }
        public int CarSize { get; set; }
        public string carpicture { get; set; }

    }
}
