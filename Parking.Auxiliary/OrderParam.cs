using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    public class OrderParam
    {
        public string PropertyName { get; set; }
        public OrderMethod Method { get; set; }
    }

    public enum OrderMethod
    {
        /// <summary>
        /// 正序
        /// </summary>
        Asc,
        /// <summary>
        /// 倒序
        /// </summary>
        Desc
    }
}
