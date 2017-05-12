using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 临时卡收费( 共存多个表 )
    ///   1、按次表
    ///   2、按时表
    ///        2、1  时段1表
    ///        2、2  时段2表
    ///        2、3  时段N表
    /// </summary>
    public class TempChargingRule
    {
        public int ID { get; set; }
        public EnmICCardType ICType { get; set; }
        /// <summary>
        /// 按时、按次
        /// </summary>
        public EnmTempChargeType TempChgType { get; set; }
        public int PreChgID { get; set; }       
    }

  
    /// <summary>
    /// 按次收费 表,只有一个记录
    /// </summary>
    public class OrderChargeDetail
    {
        public int ID { get; set; }
        /// <summary>
        /// 临时卡收费ID
        /// </summary>
        public int TempChgID { get; set; }
        /// <summary>
        /// 免费时长
        /// </summary>
        public string OrderFreeTime { get; set; }
        public float Fee { get; set; }

    }   


    /// <summary>
    /// 按时收费 表，只有一个记录,但衍生几个时段表
    /// </summary>
    public class HourChargeDetail
    {
        public int ID { get; set; }
        /// <summary>
        /// 临时卡收费ID
        /// </summary>
        public int TempChgID { get; set; }
        public EnmCycleTime CycleTime { get; set; }
        public EnmStrideDay StrideDay { get; set; }
        /// <summary>
        /// 周期最高计费
        /// </summary>
        public float CycleTopFee { get; set; }
    }

    /// <summary>
    /// 时段详情，一个HourChargeDetail 对应几个时段表
    /// </summary>
    public class HourSectionInfo
    {
        public int ID { get; set; }
        /// <summary>
        /// 按时收费表ID
        /// </summary>
        public int HourChgID { get; set; }
        /// <summary>
        /// 时间段起始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 时间段的终止时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 时段最高计费
        /// </summary>
        public float SectionTopFee { get; set; }
        /// <summary>
        /// 免费时长
        /// </summary>
        public string SectionFreeTime { get; set; }
        /// <summary>
        /// 首段计费时长
        /// </summary>    
        public string FirstVoidTime { get; set; }
        /// <summary>
        /// 首段计费金额
        /// </summary>
        public float FirstVoidFee { get; set; }
        /// <summary>
        /// 间隔计费时长
        /// </summary>      
        public string IntervalVoidTime { get; set; }
        /// <summary>
        /// 间隔计费金额
        /// </summary>
        public float IntervalVoidFee { get; set; }
    }


    #region 枚举
    /// <summary>
    /// 按时、按次
    /// </summary>
    public enum EnmTempChargeType
    {
        Init = 0,
        /// <summary>
        /// 按时
        /// </summary>
        Hour,
        /// <summary>
        /// 按次
        /// </summary>
        Order
    }
   
    /// <summary>
    /// 周期时长
    /// </summary>
    public enum EnmCycleTime
    {
        Init = 0,
        Hour_24,
        Hour_12
    }

    /// <summary>
    /// 跨天计费策略
    /// </summary>
    public enum EnmStrideDay
    {
        Init = 0,
        /// <summary>
        /// 累加计费，
        /// </summary>
        Continue,
        /// <summary>
        /// 重新计费，
        /// 第二天计首段费用
        /// </summary>
        Restart
    }
    #endregion
}
