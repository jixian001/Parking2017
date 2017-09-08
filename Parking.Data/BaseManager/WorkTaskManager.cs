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
        public Page<WorkTask> FindPageList(Page<WorkTask> workPage, Expression<Func<WorkTask, bool>> where,OrderParam oparam)
        {
            int totalNum = 0;
            List<WorkTask> allLst = _repository.FindPageList(workPage.PageSize, workPage.PageIndex, out totalNum, where, new OrderParam[] { oparam }).ToList<WorkTask>();
            
            workPage.ItemLists = allLst;
            workPage.TotalNumber = totalNum;
            return workPage;
        }
    }
}
