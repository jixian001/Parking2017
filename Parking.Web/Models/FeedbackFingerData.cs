using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parking.Web.Models
{
    public class FeedbackFingerData
    {
        public string wareHouseName { get; set; }
        public string carBarnd { get; set; }
        public int taskType { get; set; }
        public List<FeedbackQueue> queueList { get; set; }
    }

    public class FeedbackQueue
    {
        public int queueNum { get; set; }
        public string queueCarbrand { get; set; }
        public string queueIcCode { get; set; }
    }
}