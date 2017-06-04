using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    public class BasicClss
    {
        /// <summary>
        /// 依地址获取列数
        /// </summary>
        /// <param name="addrs"></param>
        /// <returns></returns>
        public static int GetColumnByAddrs(string addrs)
        {
            if (addrs.Length < 3)
            {
                return 0;
            }
            string col = addrs.Substring(1, 2);
            int column = Convert.ToInt32(col);
            if (column > 0 && column < 19)
            {
                return column;
            }
            return 0;
        }

    }
}
