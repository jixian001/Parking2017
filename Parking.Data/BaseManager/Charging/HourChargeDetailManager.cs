using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class HourChargeDetailManager:BaseManager<HourChargeDetail>
    {
        public HourChargeDetail Find(Expression<Func<HourChargeDetail,bool>> where)
        {
            return _repository.Find(where);
        }

        public HourChargeDetail Find(int ID)
        {
            return _repository.Find(ID);
        }

    }
}
