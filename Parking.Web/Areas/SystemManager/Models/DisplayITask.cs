using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parking.Web.Areas.SystemManager.Models
{
    public class DisplayITask
    {
        public int ID { get; set; }
        public int Warehouse { get; set; }
        public int DeviceCode { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }       
        public string SendStatusDetail { get; set; }
        public string CreateDate { get; set; }       
        public string SendDtime { get; set; }
        public int HallCode { get; set; }        
        public string FromLctAddress { get; set; }       
        public string ToLctAddress { get; set; }        
        public string ICCardCode { get; set; }
        public int Distance { get; set; }
        public string CarSize { get; set; }
        public int CarWeight { get; set; }    
    }
}