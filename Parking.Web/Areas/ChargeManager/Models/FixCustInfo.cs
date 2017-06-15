using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parking.Web.Areas.ChargeManager.Models
{
    public class FixCustInfo
    {
        public int CustID { get; set; }
        public string Proof { get; set; }
        public int ICType { get; set; }
        public string LastDeadline { get; set; }
        public string CurrDeadline { get; set; }
        /// <summary>
        /// 默认，返回月卡费用
        /// </summary>
        public float MonthFee { get; set; }

    }
}