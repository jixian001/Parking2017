using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 固定卡、定期卡收费策略
    /// </summary>
    public class FixChargingRule
    {
        public int ID { get; set; }
        public EnmICCardType ICType { get; set; }
        public EnmFeeUnit Unit { get; set; }
        public float Fee { get; set; }
        public int? PreChgID { get; set; }
    }

    /// <summary>
    /// 固定类，按月季年收费
    /// </summary>
    public enum EnmFeeUnit
    {
        Init=0,
        Month,
        Season,
        Year
    }

}
