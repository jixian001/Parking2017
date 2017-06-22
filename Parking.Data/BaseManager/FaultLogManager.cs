using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class FaultLogManager:BaseManager<FaultLog>
    {

        public List<FaultLog> FindList(Expression<Func<FaultLog, bool>> where)
        {
            IQueryable<FaultLog> iqueryLst = _repository.FindList(where);
            List<FaultLog> allLst = new List<FaultLog>();
            foreach (var tsk in iqueryLst)
            {
                allLst.Add(tsk);
            }
            return allLst;            
        }
    }
}
