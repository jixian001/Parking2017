using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 用户卡号
    /// </summary>
    public class ICCard
    {
        [Key]
        public int ID { get; set; }
        [StringLength(20)]
        /// <summary>
        /// 物理卡号
        /// </summary>
        public string PhysicCode { get; set; }
        [StringLength(10)]
        /// <summary>
        /// 用户卡号
        /// </summary>
        public string UserCode { get; set; }
        public EnmICCardType Type { get; set; }
        public EnmICCardStatus Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LossDate { get; set; }
        public DateTime LogoutDate { get; set; }

        /*
         * 以下 顾客信息
         */

         /// <summary>
         /// 最近的缴费日期
         /// </summary>
        public DateTime StartDTime { get; set; }
        /// <summary>
        /// 卡的使用截止期限
        /// </summary>
        public DateTime Deadline { get; set; }
        
        /// <summary>
        /// 顾客ID
        /// </summary>     
        public int CustID { get; set; }
        /// <summary>
        /// 库区
        /// </summary>
        public int Warehouse { get; set; }
        [StringLength(10)]
        /// <summary>
        ///  绑定车位
        /// </summary>
        public string LocAddress { get; set; }
    }

    public enum EnmICCardType
    {
        [Display(Name ="请选择")]
        Init = 0,  //初始
        [Display(Name ="临时卡")]
        Temp,    //临时
        [Display(Name = "定期卡")]
        Periodical,   //定期
        [Display(Name = "固定卡")]
        FixedLocation,  //固定卡
        VIP,     //贵宾卡，不需要充值，直接放行
    }
    public enum EnmICCardStatus
    {
        Init = 0,       
        Lost,     //挂失       
        Normal,   //正常      
        Disposed  //注销
    }
}
