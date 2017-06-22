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
            IQueryable<OperateLog> iqueryLst = _repository.FindList(where);
            List<OperateLog> allLst = new List<OperateLog>();
            foreach (var tsk in iqueryLst)
            {
                allLst.Add(tsk);
            }
            return allLst;
        }

    }
}
