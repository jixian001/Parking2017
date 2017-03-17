using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    public class Page<TEntity> where TEntity:class
    {
        public Page()
        {
            PageIndex = 1;
            PageSize = 20;
        }
        /// <summary>
        /// 当前页，从1计数
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalNumber { get; set; }
        /// <summary>
        /// 记录列表
        /// </summary>
        public List<TEntity> ItemLists { get; set; }
    }
}
