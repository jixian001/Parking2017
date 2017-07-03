using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 云服务下发的缴费通知
    /// </summary>
    public class RemotePayFeeRcd
    {
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string orderCode { get; set; }
        /// <summary>
        /// 用户卡号
        /// </summary>
        public string strICCardID { get; set; }
        /// <summary>
        /// 车牌号码
        /// </summary>
        public string strPlateNum { get; set; }
        /// <summary>
        /// 存车库区
        /// </summary>
        public int Warehouse { get; set; }
        /// <summary>
        /// 存车车位
        /// </summary>
        public string LocAddress { get; set; }
        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime RecordDtime { get; set; }
    }
}
