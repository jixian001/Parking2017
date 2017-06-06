using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parking.Web.Models
{
    /// <summary>
    /// 统计信息
    /// </summary>
    public class StatisInfo
    {
        public int Total { get; set; }
        public int Occupy { get; set; }
        public int FixLoc { get; set; }
        public int Space { get; set; }
        public int BigSpace { get; set; }
        public int SmallSpace { get; set; }
    }
}