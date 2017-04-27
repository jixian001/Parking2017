using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class TelegramLogManager:BaseManager<TelegramLog>
    {
        public List<TelegramLog> FindList(Expression<Func<TelegramLog, bool>> where)
        {
            return _repository.FindList(where).ToList();
        }
    }
}
