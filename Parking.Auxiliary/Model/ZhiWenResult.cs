using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    public class ZhiWenResult
    {
        /// <summary>
        /// 0-存车，1-取车
        /// </summary>
        public int IsTakeCar { get; set; }
        public string PlateNum { get; set; }
        public string Sound { get; set; }        
    }
}
