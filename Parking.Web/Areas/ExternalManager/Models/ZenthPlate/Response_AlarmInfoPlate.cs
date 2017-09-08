using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parking.Web.Areas.ExternalManager.Models
{
    public class CResponse_AlarmInfoPlate
    {
        public string info { get; set; }
        public int channelNum { get; set; }
        public string manualTigger { get; set; }
        public ZTriggerImage TriggleImage { get; set; }
        public string is_pay { get; set; }
        public List<PortData> serialData { get; set; }
    }

    public class ZTriggerImage
    {
        public int port { get; set; }
        public string snapImageRelativeUrl { get; set; }
        public string snapImageAbsolutelyUrl { get; set; }
    }

    public class PortData
    {
        public int serialChannel { get; set; }
        public string data { get; set; }
        public int dataLen { get; set; }
    }
}