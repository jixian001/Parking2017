using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;
using System.Linq.Expressions;

namespace Parking.Core
{
    /// <summary>
    /// ic卡的业务逻辑
    /// </summary>
    public class CWICCard
    {
        private ICCardManager manager = new ICCardManager();

        public CWICCard()
        {
        }

        /// <summary>
        /// 依车位地址查找是否被绑定
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public ICCard FindFixLocationByAddress(int warehouse,string address)
        {
            return manager.Find(iccd => (iccd.Type == EnmICCardType.FixedLocation || iccd.Type == EnmICCardType.VIP) &&
                                              iccd.Warehouse == warehouse && iccd.LocAddress == address);
        }

        public ICCard Find(int id)
        {
            return manager.Find(id);
        }

        public ICCard Find(Expression<Func<ICCard, bool>> where)
        {
            return manager.Find(where);
        }

        public Response Update(ICCard iccd)
        {
            return manager.Update(iccd);
        }

        public List<ICCard> FindIccdList(Expression<Func<ICCard, bool>> where)
        {
            return manager.FindList(where);
        }

        #region 顾客

        private CustomerManager manager_cust = new CustomerManager();

        public Response Add(Customer cust)
        {
            return manager_cust.Add(cust);
        }

        public Response UpdateCust(Customer cust)
        {
            return manager_cust.Update(cust);
        }

        public Response Delete(int ID)
        {

            return manager_cust.Delete(ID);
        }

        public Customer FindCust(int ID)
        {
            return manager_cust.Find(ID);
        }

        public Customer FindCust(Expression<Func<Customer, bool>> where)
        {
            return manager_cust.Find(where);
        }

        public List<Customer> FindCustList(Expression<Func<Customer, bool>> where)
        {
            return manager_cust.FindList(where);
        }

        #endregion
    }
}
