using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parking.Web.Areas.CustomManager.Models
{
    /// <summary>
    /// 下拉列表
    /// </summary>
    public class SelectItem
    {
        public int ID { get; set; }
        /// <summary>
        /// 下拉列表 option value值
        /// </summary>    
        public string OptionValue { get; set; }
        /// <summary>
        /// 下拉列表 option text值
        /// </summary>   
        public string OptionText { get; set; }
    }
}