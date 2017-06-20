using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Parking.Auxiliary
{
    public class FiPrintMatch
    {
        /// <summary>
        /// 指纹比对,0 -- 成功，小于0--失败
        /// </summary>
        /// <param name="psMB">指纹模板数据</param>
        /// <param name="psTZ">指纹特性数据</param>
        /// <param name="iLevel">安全等级</param>
        /// <returns></returns>
        [DllImport("JZTAlg30Dll.dll")]
        public static extern int FPIMatch(byte[] psMB, byte[] psTZ, int iLevel);

    }
}
