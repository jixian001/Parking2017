using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Data
{
    public class TempUserChargeLog
    {
        public int ID { get; set; }
        /// <summary>
        /// 卡号或指纹编号
        /// </summary>
        public string Proof { get; set; }
        /// <summary>
        /// 车牌
        /// </summary>
        public string Plate { get; set; }
        /// <summary>
        /// 库区
        /// </summary>
        public int Warehouse { get; set; }
        /// <summary>
        /// 存车位
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public string InDate { get; set; }
        /// <summary>
        /// 出库时间
        /// </summary>
        public string OutDate { get; set; }
        /// <summary>
        /// 停车时长
        /// </summary>
        public string SpanTime { get; set; }
        /// <summary>
        /// 应缴费用
        /// </summary>
        public string NeedFee { get; set; }
        /// <summary>
        /// 实收费用
        /// </summary>
        public string ActualFee { get; set; }
        /// <summary>
        /// 找零
        /// </summary>
        public string CoinChange { get; set; }
        /// <summary>
        /// 操作员
        /// </summary>
        public string OprtCode { get; set; }
        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime RecordDTime { get; set; }

    }
}
