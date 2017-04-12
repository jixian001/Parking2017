using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class ICCardManager:BaseManager<ICCard>
    {
        public ICCard Find(int ID)
        {
            return _repository.Find(ID);
        }

        public ICCard Find(Expression<Func<ICCard,bool>> where)
        {
            return _repository.Find(where);
        }

        public List<ICCard> FindList(Expression<Func<ICCard, bool>> where)
        {
            return _repository.FindList(where).ToList();
        }
    }
}
