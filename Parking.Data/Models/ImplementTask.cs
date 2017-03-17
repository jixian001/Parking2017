using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.Data
{
    /// <summary>
    /// 执行中的任务,一个设备只能允许有一个
    /// </summary>
    public class ImplementTask
    {
        [Key]
        public int ID { get; set; }
        public int Warehouse { get; set; }
        public int DeviceCode { get; set; }
        public EnmTaskType Type { get; set; }
        public EnmTaskStatus Status { get; set; }
        /// <summary>
        /// 不要求写入数据库
        /// </summary>
        [NotMapped]
        public EnmTaskStatusDetail SendStatusDetail { get; set; }
        public DateTime CreateDate { get; set; }
        public int HallCode { get; set; }
        [StringLength(10)]
        public string FromLctAddress { get; set; }
        [StringLength(10)]
        public string ToLctAddress { get; set; }
        [StringLength(10)]
        public string ICCardCode { get; set; }
        public int Distance { get; set; }
        public string CarSize { get; set; }
        public int CarWeight { get; set; }
        /// <summary>
        /// 判断当前作业是否完成，
        /// 如果完成了，则只在系统里做个记录
        /// </summary>
        public int IsComplete { get; set; }
        //如果有其他报文需求信息，再增加
        
    }

    public enum EnmTaskType
    {
        Init=0,
        SaveCar,
        GetCar,
        Transpose,
        Move,
        TempGet,
        Avoid,
        RetrySend
    }

    public enum EnmTaskStatusDetail
    {
        NoSend=0,
        SendWaitAsk,
        Asked
    }

    public enum EnmTaskStatus
    {

    }

}
