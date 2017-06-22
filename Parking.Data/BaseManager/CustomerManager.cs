using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class CustomerManager:BaseManager<Customer>
    {
        public Customer Find(int ID)
        {
            return _repository.Find(ID);
        }

        public Customer Find(Expression<Func<Customer, bool>> where)
        {
            return _repository.Find(where);
        }

        public List<Customer> FindList(Expression<Func<Customer,bool>> where)
        {
            IQueryable<Customer> iqueryLst = _repository.FindList(where);
            List<Customer> allLst = new List<Customer>();
            foreach (var tsk in iqueryLst)
            {
                allLst.Add(tsk);
            }           
            return allLst;
        }
    }
}
