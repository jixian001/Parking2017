using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class FixChargingRuleManager:BaseManager<FixChargingRule>
    {
        public FixChargingRule Find(int ID)
        {
            return _repository.Find(ID);
        }

        public FixChargingRule Find(Expression<Func<FixChargingRule, bool>> where)
        {
            return _repository.Find(where);
        }
    }
}
