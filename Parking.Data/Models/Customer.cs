using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 顾客信息
    /// </summary>
    public class Customer
    {
        [Key]
        public int ID { get; set; }
        [StringLength(20)]
        public string UserName { get; set; }
        public string FamilyAddress { get; set; }
        [StringLength(25)]
        public string MobilePhone { get; set; }
        [StringLength(25)]
        public string Telephone { get; set; }
        [StringLength(20)]
        public string PlateNum { get; set; }
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
    }
}
