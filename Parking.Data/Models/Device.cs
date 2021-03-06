﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Parking.Data
{
    /// <summary>
    /// 设备信息
    /// </summary>
    public class Device
    {
        [Key]
        public int ID { get; set; }
        public int Warehouse { get; set; }
        public int DeviceCode { get; set; }
        public EnmSMGType Type { get; set; }
        public EnmHallType HallType { get; set; }
        [StringLength(10)]
        public string Address { get; set; }
        public int Layer { get; set; }
        public int Region { get; set; }
        public EnmModel Mode { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public int IsAble { get; set; }
        /// <summary>
        /// 是否可接收新指令
        /// </summary>
        public int IsAvailabe { get; set; }        
        public int RunStep { get; set; }
        /// <summary>
        /// 入库步进、装载步进
        /// </summary>
        public int InStep { get; set; }
        /// <summary>
        /// 出库步进、卸载步进
        /// </summary>
        public int OutStep { get; set; }
        /// <summary>
        /// 用于存放当前正在执行的作业
        /// </summary>
        public int TaskID { get; set; }
        /// <summary>
        /// 即将要执行的作业ID，也在作业也在执行队列中
        /// 目的：多TV时，用于装载完成后，如果执行避让作业，
        ///       则将当前作业ID放至这个字段，TaskID存放避让作业ID
        /// </summary>
        public int SoonTaskID { get; set; }
    }
    #region 枚举类型
    public enum EnmSMGType
    {
        Init=0,
        Hall,
        ETV
    }

    public enum EnmHallType
    {
        Init=0,
        Entrance,
        Exit,
        EnterOrExit
    }

    public enum EnmModel
    {
        Init=0,
        Maintance,
        Manual,
        StandAlone,
        /// <summary>
        /// 全自动
        /// </summary>
        Automatic
    }


    #endregion
}
