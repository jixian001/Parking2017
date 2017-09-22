using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Parking.Data;

namespace Parking.Web.Models
{
    public class LocsMapping
    {
        public int Warehouse { get; set; }
        public string Address { get; set; }
        public int LocSide { get; set; }
        public int LocColumn { get; set; }
        public int LocLayer { get; set; }
        public EnmLocationType Type { get; set; }
        public EnmLocationStatus Status { get; set; }
        public string LocSize { get; set; }
        public string ICCode { get; set; }
        public int WheelBase { get; set; }
        public int CarWeight { get; set; }
        public string CarSize { get; set; }
        public string InDate { get; set; }
        public string PlateNum { get; set; }
        /// <summary>
        /// 是否是固定用户
        /// </summary>
        public int IsFixLoc { get; set; }
        /// <summary>
        /// 车主姓名
        /// </summary>
        public string CustName { get; set; }
        /// <summary>
        /// 登记车牌
        /// </summary>
        public string RcdPlate { get; set; }
        /// <summary>
        /// 使用期限
        /// </summary>
        public string Deadline { get; set; }
    }
}