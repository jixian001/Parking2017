using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Parking.Web.Areas.CustomManager.Models
{
    public class ChangeDeadlineModel
    {
        public int ID { get; set; }
        [Required]
        [StringLength(8)]
        [Display(Name ="用户卡号")]
        public string ICCode { get; set; }
        public int Type { get; set; }
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