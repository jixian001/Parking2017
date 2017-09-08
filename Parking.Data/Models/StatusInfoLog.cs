using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    public class StatusInfoLog
    {
        [Key]
        public int ID { get; set; }
        public int Warehouse { get; set; }
        public int DeviceCode { get; set; }
        public string Description { get; set; }
        public DateTime RcdDtime { get; set; }
    }
}
