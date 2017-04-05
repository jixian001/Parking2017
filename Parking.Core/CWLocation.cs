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

        public Location FindLocation(Expression<Func<Location, bool>> where)
        {
            return manager.FindLocation(where);
        }

        public int TransportLoc(Location fromLoc,Location toLoc)
        {
            toLoc.Status = EnmLocationStatus.Occupy;
            toLoc.CarSize = fromLoc.CarSize;
            toLoc.WheelBase = fromLoc.WheelBase;
            toLoc.ICCode = fromLoc.ICCode;
            toLoc.InDate = fromLoc.InDate;
            manager.Update(toLoc);

            fromLoc.Status = EnmLocationStatus.Space;
            fromLoc.CarSize = "";
            fromLoc.WheelBase = 0;
            fromLoc.ICCode = "";
            fromLoc.InDate = DateTime.Parse("2017-1-1");
            manager.Update(fromLoc);

            return 1;
        }

        public int DisableLocation(Location loc,bool isDis)
        {
            if (isDis)
            {
                loc.Type = EnmLocationType.Disable;
            }
            else
            {
                loc.Type = EnmLocationType.Normal;
            }
            manager.Update(loc);

            return 1;
        }

        public Response UpdateLocation(Location loc)
        {
            Response resp = manager.Update(loc);
            return resp;
        }

    }
}
