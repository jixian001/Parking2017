using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 报警信息
    /// </summary>
    public class Alarm
    {
        [Key]
        public int ID { get; set; }
        public int Warehouse { get; set; }
        public int DeviceCode { get; set; }
        public string Description { get; set; }
        public int Address { get; set; }
        public EnmAlarmColor Color { get; set; }        
    }

    public enum EnmAlarmColor
    {
        Init=0,
        Green,
        Red
    }
}
