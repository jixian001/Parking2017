using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    public class BackAlarm
    {
        /// <summary>
        /// 1-状态，2-报警
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}
