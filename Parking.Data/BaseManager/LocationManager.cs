using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class LocationManager:BaseManager<Location>
    {
        /// <summary>
        /// 查找车位
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public Location FindLocation(Expression<Func<Location, bool>> where)
        {
            return _repository.Find(where);
        }

        public List<Location> FindLocationList(Expression<Func<Location, bool>> where, OrderParam[] orderParams)
        {
            if (orderParams == null)
            {
                List<Location> allLst = _repository.FindList(where).ToList<Location>();
                
                return allLst;               
            }
            else
            {
                List<Location> allLst = _repository.FindList(where, orderParams, 0).ToList<Location>();               
                return allLst;
            }
        }

    }
}
