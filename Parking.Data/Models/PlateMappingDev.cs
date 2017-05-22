using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Data
{
    /// <summary>
    /// 入库时，车牌识别最终结果写入，
    /// 供后续分配到车位后，绑定到车位上
    /// </summary>
    public class PlateMappingDev
    {
        public int ID { get; set; }
        public int Warehouse { get; set; }
        public int DeviceCode { get; set; }
        public string PlateNum { get; set; }
        /// <summary>
        /// 车头图片路径
        /// </summary>
        public string HeadImagePath { get; set; }
        /// <summary>
        /// 车牌图片路径(备用)
        /// </summary>
        public string PlateImagePath { get; set; }
        public DateTime InDate { get; set; }

    }
}
