using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Data
{
    public class FixUserChargeLog
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string PlateNum { get; set; }
        /// <summary>
        /// 用户类型
        /// </summary>
        public string UserType { get; set; }
        /// <summary>
        /// 缴费凭证
        /// </summary>
        public string Proof { get; set; }
      
        /// <summary>
        /// 缴费前期限
        /// </summary>
        public string LastDeadline { get; set; }
        /// <summary>
        /// 本次期限
        /// </summary>
        public string CurrDeadline { get; set; }
        /// <summary>
        /// 收费类型
        /// </summary>
        public string FeeType { get; set; }
        /// <summary>
        /// 收费单元
        /// </summary>
        public float FeeUnit { get; set; }
        /// <summary>
        /// 本次缴费
        /// </summary>
        public float CurrFee { get; set; }
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
