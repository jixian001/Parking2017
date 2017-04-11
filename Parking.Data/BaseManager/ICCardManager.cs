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

        public ICCard FindICCard(Expression<Func<ICCard,bool>> where)
        {
            return _repository.Find(where);
        }

        
    }
}
