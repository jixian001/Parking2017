using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parking.Web.Areas.ExternalManager.Models
{
    public class ParkingFeeInfo
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public float Fee { get; set; }
        public string InDtime { get; set; }
        public string OutDtime { get; set; }

        public ParkingFeeInfo()
        {
            Code = 0;
        }
    }
}