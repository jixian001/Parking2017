using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class WorkTaskManager:BaseManager<WorkTask>
    {
        public WorkTask Find(int ID)
        {
            return _repository.Find(ID);
        }

        public WorkTask Find(Expression<Func<WorkTask, bool>> where)
        {
            return _repository.Find(where);
        }

        public List<WorkTask> FindList(Expression<Func<WorkTask,bool>> where)
        {
            return _repository.FindList(where).ToList();
        }

        public Page<WorkTask> FindPageList(Page<WorkTask> workPage, Expression<Func<WorkTask, bool>> where,OrderParam oparam)
        {
            int totalNum = 0;
            workPage.ItemLists = _repository.FindPageList(workPage.PageSize, workPage.PageIndex, out totalNum, where, new OrderParam[] { oparam }).ToList();
            workPage.TotalNumber = totalNum;
            return workPage;
        }
    }
}
