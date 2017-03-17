using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 工作队列
    /// </summary>
    public class WorkTask
    {
        public int ID { get; set; }
        /// <summary>
        /// 1- 子作业，是报文
        /// 2- 主作业，暂没有报文，待分解的        
        /// </summary>
        public int IsMaster { get; set; }
        public EnmTaskType MasterType { get; set; }
        public int TelegramType { get; set; }
        public int SubTelegramType { get; set; }
        public int DeviceCode { get; set; }
        [StringLength(10)]
        public string FromLctAddress { get; set; }
        [StringLength(10)]
        public string ToLctAddress { get; set; }
        [StringLength(10)]
        public string ICCardCode { get; set; }
        /// <summary>
        /// 轴距
        /// </summary>
        public int Distance { get; set; }
        [StringLength(10)]
        public string CarSize { get; set; }
        public int CarWeight { get; set; }        

    }

}
