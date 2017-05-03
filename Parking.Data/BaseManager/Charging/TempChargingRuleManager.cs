using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class TempChargingRuleManager:BaseManager<TempChargingRule>
    {
        public TempChargingRule Find(int ID)
        {
            return _repository.Find(ID);
        }

        public TempChargingRule Find(Expression<Func<TempChargingRule, bool>> where)
        {
            return _repository.Find(where);
        }

    }
}
