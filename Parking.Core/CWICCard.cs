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
            return manager.FindICCard(iccd => (iccd.Type == EnmICCardType.FixedLocation || iccd.Type == EnmICCardType.VIP) &&
                                              iccd.Warehouse == warehouse && iccd.LocAddress == address);
        }

        public ICCard Find(Expression<Func<ICCard, bool>> where)
        {
            return manager.FindICCard(where);
        }

        public Response Update(ICCard iccd)
        {
            return manager.Update(iccd);
        }

    }
}
