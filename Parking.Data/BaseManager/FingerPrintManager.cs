using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class FingerPrintManager:BaseManager<FingerPrint>
    {
        public FingerPrint Find(Expression<Func<FingerPrint, bool>> where)
        {
            return _repository.Find(where);
        }

        public List<FingerPrint> FindList(Expression<Func<FingerPrint, bool>> where)
        {
            return _repository.FindList(where).ToList();
        }
    }
}
