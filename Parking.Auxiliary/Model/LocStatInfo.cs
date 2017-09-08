using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    public class LocStatInfo
    {
        public int Total { get; set; }
        public int Occupy { get; set; }
        public int FixLoc { get; set; }
        public int Space { get; set; }
        public int BigSpace { get; set; }
        public int SmallSpace { get; set; }
    }
}
