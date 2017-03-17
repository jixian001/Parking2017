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
        public int Type { get; set; }
        public int Warehouse { get; set; }
        public string Telegram { get; set; }
        public int DeviceCode { get; set; }
        [StringLength(10)]
        public string ICCode { get; set; }
        public string CarInfo { get; set; }
        [StringLength(10)]
        public string FromAddress { get; set; }
        [StringLength(10)]
        public string ToAddress { get; set; }
        public int TelegramID { get; set; }
    }
}
