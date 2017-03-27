using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;

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
        /// 依用户卡号查询卡
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ICCard SelectICCdByUserCode(string code)
        {
            return manager.FindICCard(iccd => iccd.UserCode == code);
        }

        /// <summary>
        /// 依物理卡号查询卡
        /// </summary>
        /// <param name="physc"></param>
        /// <returns></returns>
        public ICCard SelectICCdByPhyscard(string physc)
        {
            return manager.FindICCard(iccd => iccd.PhysicCode==physc);
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
    }
}
