using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;

namespace Parking.Core.Schedule
{
    /// <summary>
    /// 当前作业范围
    /// </summary>
    public class RealScope
    {
        /// <summary>
        /// 安全列
        /// </summary>
        private readonly int safeDistCol = 3;

        private Device cEtv;
        private IList<Device> cEtvLst;
        private CScope cPhysScope;

        public RealScope(Device pEtv,IList<Device> pEtvLst,CScope physScope)
        {
            cEtv = pEtv;
            cEtvLst = pEtvLst;
            cPhysScope = physScope;
        }

        /// <summary>
        /// 获取最大可达的左侧范围
        /// </summary>
        /// <returns></returns>
        public int GetMaxLeftScope()
        {
            string addrs = cEtv.Address;
            if (string.IsNullOrEmpty(addrs) || addrs.Length < 3)
            {
                return 0;
            }
            int realColmn = Convert.ToInt32(addrs.Substring(1,2));
            int disableCol = PhysicLeftCol(cPhysScope.LeftCol, realColmn);
            if (disableCol != cPhysScope.LeftCol)
            {
                return disableCol + safeDistCol;
            }
            return cPhysScope.LeftCol;
        }

        /// <summary>
        /// 获取最大可达的右侧范围
        /// </summary>
        /// <returns></returns>
        public int GetMaxRightScope()
        {
            string addrs = cEtv.Address;
            if (string.IsNullOrEmpty(addrs) || addrs.Length < 3)
            {
                return 0;
            }
            int realColmn = Convert.ToInt32(addrs.Substring(1, 2));
            int disableCol = PhysicLeftCol(cPhysScope.LeftCol, realColmn);
            if (disableCol != cPhysScope.RightCol)
            {
                return disableCol - safeDistCol;
            }
            return cPhysScope.RightCol;
        }

        /// <summary>
        /// 物理左范围
        /// </summary>
        /// <param name="pLeft"></param>
        /// <param name="pRight"></param>
        /// <returns></returns>
        private int PhysicLeftCol(int pLeft,int pRight)
        {
            int leftCol = pLeft;
            foreach(Device smg in cEtvLst)
            {
                string addr = smg.Address;
                if (string.IsNullOrEmpty(addr))
                {
                    continue;
                }
                bool isAble = smg.IsAble == 1 ? true : false;
                int etvCol = Convert.ToInt32(addr.Substring(1,2));
                if (!isAble && (etvCol > pLeft && etvCol < pRight))
                {
                    leftCol = etvCol;
                }
            }
            return leftCol;
        }

        /// <summary>
        /// 物理右范围
        /// </summary>
        /// <param name="pLeft"></param>
        /// <param name="pRight"></param>
        /// <returns></returns>
        private int PhysicRightCol(int pLeft, int pRight)
        {
            int rightCol = pLeft;
            foreach (Device smg in cEtvLst)
            {
                string addr = smg.Address;
                if (string.IsNullOrEmpty(addr))
                {
                    continue;
                }
                bool isAble = smg.IsAble == 1 ? true : false;
                int etvCol = Convert.ToInt32(addr.Substring(1, 2));
                if (!isAble && (etvCol > pLeft && etvCol < pRight))
                {
                    rightCol = etvCol;
                }
            }
            return rightCol;
        }
    }
}
