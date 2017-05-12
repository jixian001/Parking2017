using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class OrderChargeDetailManager:BaseManager<OrderChargeDetail>
    {
        public OrderChargeDetail Find(Expression<Func<OrderChargeDetail, bool>> where)
        {
            return _repository.Find(where);
        }
    }
}
