using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;

namespace Parking.Core
{
    /// <summary>
    /// 取车、取物分配TV，这里只以考虑一个巷道一台移动设备
    /// </summary>
    public class AllocateTV
    {
        public AllocateTV()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hall"></param>
        /// <param name="lctn"></param>
        /// <returns></returns>
        public Device Allocate(Device hall,Location lctn)
        {
            Device smg = new CWDevice().Find(d=>d.Type==EnmSMGType.ETV&&d.Layer==lctn.LocLayer);
            return smg;
        }

    }
}
