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
        public ImplementTask Find(int id)
        {
            return _repository.Find(id);
        }

        public ImplementTask Find(Expression<Func<ImplementTask,bool>> where)
        {
            return _repository.Find(where);
        }

        public List<ImplementTask> FindList(Expression<Func<ImplementTask, bool>> where)
        {
            IQueryable<ImplementTask> itaskLst= _repository.FindList(where);
            List<ImplementTask> taskLst = new List<ImplementTask>();
            foreach(ImplementTask tsk in itaskLst)
            {
                taskLst.Add(tsk);
            }
            return taskLst;
        }
    }
}
