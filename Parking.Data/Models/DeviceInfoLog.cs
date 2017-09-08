using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    public class DeviceInfoLog
    {
        [Key]
        public int ID { get; set; }
        public int Warehouse { get; set; }
        public int DeviceCode { get; set; }
        public DateTime RecordDtime { get; set; }
        public string Mode { get; set; }       
        public int IsAble { get; set; }      
        public int IsAvailabe { get; set; }
        public int RunStep { get; set; }       
        public int InStep { get; set; }       
        public int OutStep { get; set; }
        public string Address { get; set; }
        //作业ID
        public int TaskID { get; set; }
    }
}
