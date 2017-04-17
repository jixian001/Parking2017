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

        public Response DisableLocation(int warehouse,string addrs,bool isDis)
        {
            Response _resp = new Response();
            Location loc = new CWLocation().FindLocation(lc => lc.Warehouse == warehouse && lc.Address == addrs);
            if (loc == null)
            {
                _resp.Code = 0;
                _resp.Message = "找不到车位-" + addrs;
                return _resp;
            }
            if (loc.Type == EnmLocationType.Invalid ||
                loc.Type == EnmLocationType.Hall ||
                loc.Type == EnmLocationType.ETV)
            {
                _resp.Code = 0;
                _resp.Message = "当前车位-" + addrs + "无效，不允许操作！";
                return _resp;
            }          
            if (isDis)
            {
                loc.Type = EnmLocationType.Disable;
            }
            else
            {
                loc.Type = EnmLocationType.Normal;
            }
            _resp=  manager.Update(loc);            
            return _resp;
        }

        public Response UpdateLocation(Location loc)
        {
            Response resp = manager.Update(loc);
            return resp;
        }

    }
}
