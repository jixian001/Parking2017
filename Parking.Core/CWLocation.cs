using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;
using System.Linq.Expressions;

namespace Parking.Core
{
    /// <summary>
    /// 车位信息的业务逻辑
    /// </summary>
    public class CWLocation
    {
        private LocationManager manager = new LocationManager();

        public Location SelectLocByAddress(int warehouse,string addrs)
        {
            return manager.FindLocation(loc => loc.Warehouse == warehouse && loc.Address == addrs);
        }

        /// <summary>
        /// 存车时获取效车位
        /// </summary>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public List<Location> FindLocationList(Expression<Func<Location, bool>> where)
        {
            return manager.FindLocationList(where, null);
        }
    }
}
