using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Parking.Data;

namespace Parking.Web.Areas.CustomManager.Models
{
    public class ChangeDeadlineModel
    {
        public int ID { get; set; }
        [Required]
        [StringLength(8)]
        [Display(Name ="顾客姓名")]
        public string UserCode { get; set; }
        [Display(Name = "用户类型")]
        public EnmICCardType Type { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name ="原来期限")]
        public DateTime OldDeadline { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "修改期限")]
        public DateTime NewDeadline { get; set; }

    }
}