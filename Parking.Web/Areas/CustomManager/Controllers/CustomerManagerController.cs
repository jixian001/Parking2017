using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using Parking.Web.Areas.CustomManager.Models;

namespace Parking.Web.Areas.CustomManager.Controllers
{
    public class CustomerManagerController : Controller
    {
        // GET: CustomManager/CustomerManager
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        public JsonResult GetSelectName()
        {
            List<SelectItem> items = new List<SelectItem>();
            int id = 1;
            items.Add(new SelectItem() { ID = id++, OptionValue = "UserCode", OptionText = "用户卡号" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "Type", OptionText = "卡类型" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "UserName", OptionText = "用户姓名" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "FamilyAddress", OptionText = "用户住址" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "MobilePhone", OptionText = "手机号" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "LocAddress", OptionText = "车位地址" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "PlateNum", OptionText = "车牌号码" });

            return Json(items,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FindCustomList(int? pageSize, int? pageIndex,
                                           string sortOrder, string sortName,
                                           string warehouse, string code)
        {

            return Json(new { });
        }

    }
}