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
            IQueryable<WorkTask> iqueryLst = _repository.FindList(where);
            List<WorkTask> allLst = new List<WorkTask>();
            foreach (var tsk in iqueryLst)
            {
                allLst.Add(tsk);
            }
            return allLst;
        }

        public Page<WorkTask> FindPageList(Page<WorkTask> workPage, Expression<Func<WorkTask, bool>> where,OrderParam oparam)
        {
            int totalNum = 0;
            IQueryable<WorkTask> iqueryLst = _repository.FindPageList(workPage.PageSize, workPage.PageIndex, out totalNum, where, new OrderParam[] { oparam });
            List<WorkTask> allLst = new List<WorkTask>();
            foreach (var tsk in iqueryLst)
            {
                allLst.Add(tsk);
            }
            workPage.ItemLists = allLst;
            workPage.TotalNumber = totalNum;
            return workPage;
        }
    }
}
