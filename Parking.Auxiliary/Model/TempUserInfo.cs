using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    public class TempUserInfo
    {
        public string CCode { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public string InDate { get; set; }
        /// <summary>
        /// 停车时长
        /// </summary>
        public string SpanTime { get; set; }
        /// <summary>
        /// 应缴费用
        /// </summary>
        public string NeedFee { get; set; }
        /// <summary>
        /// 库区
        /// </summary>
        public int Warehouse { get; set; }
        /// <summary>
        /// 出库车厅
        /// </summary>
        public int HallID { get; set; }
    }
}
