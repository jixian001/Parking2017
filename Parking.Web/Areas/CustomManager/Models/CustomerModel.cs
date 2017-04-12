using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Parking.Data;
using System.ComponentModel.DataAnnotations;

namespace Parking.Web.Areas.CustomManager.Models
{
    public class CustomerModel
    {
        public int ID { get; set; }  
        [Required]
        [StringLength(20)] 
        [Display(Name ="顾客姓名")]    
        public string UserName { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "注册卡号")]
        public string UserCode { get; set; }
        [Display(Name = "卡类型")]
        public  EnmICCardType Type { get; set; }
        [Display(Name = "卡状态")]
        public EnmICCardStatus Status { get; set; }
        [Display(Name = "库区")]
        public int? Warehouse { get; set; }
        [Display(Name = "绑定车位")]
        public string LocAddress { get; set; }
        [Display(Name = "截止日期")]
        public DateTime? Deadline { get; set; }
        [Display(Name = "手机")]
        public string MobilePhone { get; set; }
        [Display(Name = "车牌号码")]
        public string PlateNum { get; set; }
        [Display(Name = "家庭住址")]
        public string FamilyAddress { get; set; }
       
    }
}