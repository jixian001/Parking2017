using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class HourSectionInfoManager:BaseManager<HourSectionInfo>
    {
        public HourSectionInfo Find(int ID)
        {
            return _repository.Find(ID);
        }

        public HourSectionInfo Find(Expression<Func<HourSectionInfo, bool>> where)
        {
            return _repository.Find(where);
        }

        public List<HourSectionInfo> FindList(Expression<Func<HourSectionInfo, bool>> where)
        {
            IQueryable<HourSectionInfo> iqueryLst = _repository.FindList(where);
            List<HourSectionInfo> allLst = new List<HourSectionInfo>();
            foreach (var tsk in iqueryLst)
            {
                allLst.Add(tsk);
            }
            return allLst;
           
        }

    }
}
