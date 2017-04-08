using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class AlarmManager:BaseManager<Alarm>
    {
        public List<Alarm> FindList(Expression<Func<Alarm, bool>> where)
        {
            return _repository.FindList(where).ToList();
        }
    }
}
