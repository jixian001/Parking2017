using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 设备模式更改了也记录在此
    /// </summary>
    public class OperateLog
    {
        [Key]
        public int ID { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        [StringLength(20)]
        public string OptName { get; set; }
    }
}
