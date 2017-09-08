using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 报文记录
    /// </summary>
    public class TelegramLog
    {
        [Key]
        public int ID { get; set; }
        public DateTime RecordDtime { get; set; }
        /// <summary>
        /// 日志类型- 1：发送，2：接收
        /// </summary>
        public int Type { get; set; }
        public int Warehouse { get; set; }      
        public int DeviceCode { get; set; }
        public string Telegram { get; set; }
        /// <summary>
        /// 车牌或卡号，不注册的，以车牌为准
        /// </summary>
        public string ICCode { get; set; }
        public string CarInfo { get; set; }
        [StringLength(10)]
        public string FromAddress { get; set; }
        [StringLength(10)]
        public string ToAddress { get; set; }        
        public int TelegramID { get; set; }
        
    }
}
