using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 用于保存存车凭证，
    /// 由于需要对用户卡片进行强制注册或指纹进行强制注册，
    /// 故需要此来保存存车时的物理卡号或指纹模板值
    /// </summary>
    public class SaveCertificate
    {
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// 1 - 指纹 ，2 - 物理卡号
        /// </summary>
        [Required]
        public int IsFingerPrint { get; set; }

        /// <summary>
        /// 如果是指纹的，则以base64编码保存
        /// </summary>
        [Required]        
        public string Proof { get; set; }

        /// <summary>
        /// 编号，22000 至 32000
        /// </summary>
        [Required]
        public int SNO { get; set; }

        /// <summary>
        /// 如果是注册用户的，则记录用户ID
        /// </summary>
        public int CustID { get; set; }

    }
}
