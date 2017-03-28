using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class CurrentTaskManager:BaseManager<ImplementTask>
    {
       public List<ImplementTask> GetCurrentTaskList()
        {
            IQueryable<ImplementTask> itaskLst= _repository.FindList(tsk => tsk.IsComplete == 0);
            return itaskLst.ToList();
        }

        public ImplementTask Find(Expression<Func<ImplementTask,bool>> where)
        {
            return _repository.Find(where);
        }

        public List<ImplementTask> FindList(Expression<Func<ImplementTask, bool>> where)
        {
            return _repository.FindList(where).ToList();
        }
    }
}
