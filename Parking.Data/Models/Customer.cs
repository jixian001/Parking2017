using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 顾客信息,实行一车牌一车主一卡，
    /// 绑定用户类型，释放用户卡的固定类型，
    /// 统一到用户这里绑定
    /// </summary>
    public class Customer
    {
        [Key]
        public int ID { get; set; }
        [StringLength(20)]
        public string UserName { get; set; }
        [Required]
        [StringLength(20)]
        public string PlateNum { get; set; }
        public string FamilyAddress { get; set; }
        [StringLength(25)]
        public string MobilePhone { get; set; }
        [StringLength(25)]
        public string Telephone { get; set; }        
        /// <summary>
        /// 顾客照片
        /// </summary>
        public byte[] HeadShot { get; set; }        
        /// <summary>
        /// 车辆图片路径
        /// </summary>
        public string ImagePath { get; set; }
        /// <summary>
        /// 车辆图片
        /// </summary>
        public byte[] ImageData { get; set; }

        #region 明确用户类型及绑定对应车位，规定收费期限
        /*
         * 以下 顾客信息
         */
        public EnmICCardType Type { get; set; }
        /// <summary>
        /// 最近的缴费日期
        /// </summary>
        public DateTime StartDTime { get; set; }
        /// <summary>
        /// 卡的使用截止期限
        /// </summary>
        public DateTime Deadline { get; set; }
        /// <summary>
        /// 库区
        /// </summary>
        public int Warehouse { get; set; }
        [StringLength(10)]
        /// <summary>
        ///  绑定车位
        /// </summary>
        public string LocAddress { get; set; }
        #endregion
    }
}
