using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 预付类
    /// </summary>
    public class PreCharging
    {
        public int ID { get; set; }
        /// <summary>
        /// 充值周期
        /// </summary>
        public int CycleNum { get; set; }
        /// <summary>
        /// 周期单位
        /// </summary>
        public EnmCycleUnit CycleUnit { get; set; }
        /// <summary>
        /// 收费金额
        /// </summary>
        public float Fee { get; set; }
    }

    /// <summary>
    /// 预付类的周期单位
    /// </summary>
    public enum EnmCycleUnit
    {
        [Display(Name = "请选择")]
        Init =0,
        [Display(Name = "天")]
        Day,
        [Display(Name = "个月")]
        Month,
        [Display(Name = "个季度")]
        Season
    }
}
