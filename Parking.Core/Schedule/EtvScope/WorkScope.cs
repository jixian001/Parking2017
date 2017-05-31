using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;

namespace Parking.Core.Schedule
{
    public class WorkScope
    {
        private readonly int etv1LefPhysCol = 1;
        private readonly int etv1RightPhysCol = 15;
        private readonly int etv2LefPhysCol = 4;
        private readonly int etv2RightPhysCol = 18;
        private IList<Device> mEtvList;

        public WorkScope(IList<Device> etvs)
        {
            mEtvList = etvs;
        }

        /// <summary>
        /// 获取ETV最大作业范围
        /// </summary>
        /// <param name="smg"></param>
        /// <returns></returns>
        public CScope GetEtvScope(Device smg)
        {
            foreach (KeyValuePair<Device, CScope> pair in DicMaxWork)
            {
                if (pair.Key.DeviceCode == smg.DeviceCode)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 最大物理范围
        /// </summary>
        public Dictionary<Device,CScope> DicMaxWork
        {
            get
            {
                Dictionary<Device, CScope> maxwork = new Dictionary<Device, CScope>();
                foreach(Device smg in mEtvList)
                {
                    Dictionary<Device, CScope> physcScope = GetPhyscScope();
                    RealScope maxscope = new RealScope(smg, mEtvList, physcScope[smg]);

                    CScope cs = new CScope();
                    cs.LeftCol = maxscope.GetMaxLeftScope();
                    cs.RightCol = maxscope.GetMaxRightScope();
                    maxwork.Add(smg, cs);
                }
                return maxwork;
            }
        }

        /// <summary>
        /// 原始物理工作范围
        /// </summary>
        /// <returns></returns>
        private Dictionary<Device, CScope> GetPhyscScope()
        {
            Dictionary<Device, CScope> dicPhyscScope = new Dictionary<Device, CScope>();
            foreach (Device smg in mEtvList)
            {
                CScope physScope;
                if (smg.Region == 1)  //ETV1
                {
                    physScope = new CScope(etv1LefPhysCol, etv1RightPhysCol);
                }
                else  //ETV2
                {
                    physScope = new CScope(etv2LefPhysCol, etv2RightPhysCol);
                }
                dicPhyscScope.Add(smg, physScope);
            }
            return dicPhyscScope;
        }

    }
}
