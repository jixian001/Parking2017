using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class PlateInfoManager:BaseManager<PlateMappingDev>
    {
        public PlateMappingDev Find(Expression<Func<PlateMappingDev,bool>> where)
        {
            return _repository.Find(where);
        }


    }
}
