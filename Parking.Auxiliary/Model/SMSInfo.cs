using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    public class SMSInfo
    {       
        public int warehouse { get; set; }
        public int DeviceCode { get; set; }
        public string Plate { get; set; }
        public int AutoStep { get; set; }
        public string Message { get; set; }
        public string RcdTime { get; set; }
    }
}
