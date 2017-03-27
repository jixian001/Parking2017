using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;

namespace Parking.Data
{
    public class CurrentTaskManager:BaseManager<ImplementTask>
    {
       public List<ImplementTask> GetCurrentTaskList()
        {
            IQueryable<ImplementTask> itaskLst= _repository.FindList(tsk => tsk.IsComplete == 0);
            return itaskLst.ToList();
        }

        public ImplementTask GetTaskBySmgID(int code,int warehouse)
        {
            return _repository.Find(tsk => tsk.DeviceCode == code && tsk.Warehouse == warehouse);
        }

    }
}
