using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parking.Web.Areas.ExternalManager.Models
{
    public class TelegramRecord
    {
        public int Type { get; set; }
        public short[] Telegram { get; set; }
    }
}