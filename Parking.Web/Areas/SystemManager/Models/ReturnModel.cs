using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parking.Web.Areas.SystemManager.Models
{
    public class ReturnModel
    {
        /// <summary>
        /// 返回代码，1-成功，0-失败
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 返回要显示的信息
        /// </summary>
        public string message { get; set; }
        #region 可选项
        public int warehouse { get; set; }
        public int hallID { get; set; }
        public string locaddrs { get; set; }
        public string iccode { get; set; }
        #endregion
    }
}