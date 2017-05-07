using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Parking.Auxiliary;

namespace Parking.Data
{
    public class PreChargingManager:BaseManager<PreCharging>
    {
        public PreCharging Find(int ID)
        {
            return _repository.Find(ID);
        }

        public PreCharging Find(Expression<Func<PreCharging, bool>> where)
        {
            return _repository.Find(where);
        }


    }
}
