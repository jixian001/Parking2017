using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    public class FaultLog
    {
        [Key]
        public int ID { get; set; }
        public int Warehouse { get; set; }
        public int DeviceCode { get; set; }
        public int RunStep { get; set; }
        public int InStep { get; set; }
        public int OutStep { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
