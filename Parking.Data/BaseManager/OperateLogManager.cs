using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class OperateLogManager:BaseManager<OperateLog>
    {
        public List<OperateLog> FindList(Expression<Func<OperateLog, bool>> where)
        {
            return _repository.FindList(where).ToList();
        }

    }
}
