using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using Parking.Web.Areas.CustomManager.Models;
using System.Threading.Tasks;

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

        [HttpPost]
        public ActionResult Add(CustomerModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            #region
            if (model.Type==EnmICCardType.FixedLocation)
            {
                if (model.Warehouse == 0||string.IsNullOrEmpty(model.LocAddress))
                {
                    ModelState.AddModelError("","固定车位卡，请指定绑定车位！");
                    return View(model);
                }
                Location lctn = new CWLocation().FindLocation(lc => lc.Warehouse == model.Warehouse && lc.Address == model.LocAddress);
                if (lctn == null)
                {
                    ModelState.AddModelError("", "绑定车位不存在，地址-" + model.LocAddress);
                    return View(model);
                }
            }
            CWICCard cwiccd = new CWICCard();
            ICCard iccd = cwiccd.Find(ic=>ic.UserCode==model.UserCode);
            if (iccd == null)
            {
                ModelState.AddModelError("", "当前用户卡号没有注册，请确保该卡已完成制卡！");
                return View(model);
            }
            if (iccd.Status != EnmICCardStatus.Normal)
            {
                ModelState.AddModelError("", "该卡已挂失或注销，无法完成操作！");
                return View(model);
            }
            if (iccd.CustID != 0)
            {
                Customer cust = cwiccd.FindCust(iccd.CustID);
                if (cust != null)
                {
                    ModelState.AddModelError("", "该卡已被绑定，车主姓名："+cust.UserName);
                    return View(model);
                }
            }
            Location loc = new CWLocation().FindLocation(lc=>lc.ICCode==iccd.UserCode);
            if (loc != null)
            {
                ModelState.AddModelError("", "该卡已存车，请等待出车完成后，再绑定！");
                return View(model);
            }
            if (model.Type == EnmICCardType.FixedLocation)
            {               
                ICCard fixiccd = cwiccd.Find(ic => ic.Warehouse == model.Warehouse && ic.LocAddress == model.LocAddress);
                if (fixiccd != null)
                {
                    ModelState.AddModelError("", "该车位已被其他卡绑定，无法使用该车位-"+model.LocAddress);
                    return View(model);
                }               
            }
            Customer addcust = new Customer();
            addcust.UserName = model.UserName;
            addcust.FamilyAddress = model.FamilyAddress;
            addcust.MobilePhone = model.MobilePhone;
            addcust.PlateNum = model.PlateNum;
           
            Response resp = cwiccd.Add(addcust);
            if (resp.Code == 1)
            {
                if ((int)model.Type > 0)
                {
                    iccd.Type = model.Type;
                    if(model.Type == EnmICCardType.FixedLocation)
                    {
                        iccd.Warehouse = (int)model.Warehouse;
                        iccd.LocAddress = model.LocAddress;
                    }
                }
                else
                {
                    iccd.Type = EnmICCardType.Temp;
                }
                iccd.CustID = addcust.ID;
                resp= cwiccd.Update(iccd);
                if (resp.Code == 1)
                {
                    //记录
                }
                else
                {
                    //记录
                }
            }
            else
            {
                //记录
            }
            #endregion
            return RedirectToAction("Index");
        }

        public JsonResult GetSelectName()
        {
            List<SelectItem> items = new List<SelectItem>();
            #region
            int id = 1;
            items.Add(new SelectItem() { ID = id++, OptionValue = "UserCode", OptionText = "用户卡号" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "Type", OptionText = "卡类型" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "UserName", OptionText = "用户姓名" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "FamilyAddress", OptionText = "用户住址" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "MobilePhone", OptionText = "手机号" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "LocAddress", OptionText = "车位地址" });
            items.Add(new SelectItem() { ID = id++, OptionValue = "PlateNum", OptionText = "车牌号码" });
            #endregion
            return Json(items,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FindCustomList(int? pageSize, int? pageIndex,
                                           string sortOrder, string sortName,
                                           string queryName, string queryValue)
        {
            #region
            CWICCard cwiccd = new CWICCard();
            List<Customer> custLst = cwiccd.FindCustList(cust=>true);
            List<ICCard> iccdLst = cwiccd.FindIccdList(iccd => iccd.CustID != 0);
            var query = from cu in custLst
                        join ic in iccdLst on cu.ID equals ic.CustID into temp
                        from tt in temp.DefaultIfEmpty()
                        select new
                        {
                            cu.ID,
                            cu.UserName,
                            UserCode=(tt==null?"":tt.UserCode),
                            Type= (tt == null ? 0 : tt.Type),
                            Status= (tt == null ? 0 : tt.Status),
                            Warehouse= (tt == null ? 0 : tt.Warehouse),
                            LocAddress= (tt == null ? "" : tt.LocAddress),
                            Deadline= (tt == null ? DateTime.Parse("2017-1-1") : tt.Deadline),
                            cu.MobilePhone,
                            cu.PlateNum,
                            cu.FamilyAddress
                        };           
            List<CustomerModel> models = new List<CustomerModel>();
            foreach(var obj in query)
            {
                CustomerModel model = new CustomerModel
                {
                    ID=obj.ID,
                    UserName=obj.UserName,
                    UserCode=obj.UserCode,
                    Type=obj.Type,
                    Status=obj.Status,
                    Warehouse=obj.Warehouse,
                    LocAddress=obj.LocAddress,
                    Deadline=obj.Deadline,
                    MobilePhone=obj.MobilePhone,
                    PlateNum=obj.PlateNum,
                    FamilyAddress=obj.FamilyAddress
                };
                models.Add(model);
            }

            List<CustomerModel> firstQuery = new List<CustomerModel>();
            if (queryName != "0"&&!string.IsNullOrEmpty(queryValue))
            {
                #region
                if (queryName == "UserCode")
                {
                    firstQuery = models.Where(md => md.UserCode.Contains(queryValue)).ToList();
                }
                else if (queryName == "Type")
                {
                    EnmICCardType type = EnmICCardType.Init;
                    if (queryValue.Contains("临时"))
                    {
                        type = EnmICCardType.Temp;
                    }
                    else if (queryValue.Contains("定期"))
                    {
                        type = EnmICCardType.Periodical;
                    }
                    else if (queryValue.Contains("固定"))
                    {
                        type = EnmICCardType.FixedLocation;
                    }
                    else if (queryValue.ToUpper().Contains("VIP"))
                    {
                        type = EnmICCardType.VIP;
                    }
                    firstQuery = models.Where(md => md.Type == type).ToList();
                }
                else if (queryName == "UserName")
                {
                    firstQuery = models.Where(md => md.UserName.Contains(queryValue)).ToList();
                }
                else if (queryName == "FamilyAddress")
                {
                    firstQuery = models.Where(md => md.FamilyAddress.Contains(queryValue)).ToList();
                }
                else if (queryName == "MobilePhone")
                {
                    firstQuery = models.Where(md => md.MobilePhone.Contains(queryValue)).ToList();
                }
                else if (queryName == "LocAddress")
                {
                    firstQuery = models.Where(md => md.LocAddress==queryValue).ToList();
                }
                else if (queryName == "PlateNum")
                {
                    firstQuery = models.Where(md => md.PlateNum.Contains(queryValue)).ToList();
                }
                #endregion
            }
            else
            {
                firstQuery.AddRange(models);
            }
            #region 排序 只允许几个字段可以排序
            List<CustomerModel> sortList = new List<CustomerModel>();
            if (!string.IsNullOrEmpty(sortName))
            {
                if (sortName == "ID")
                {
                    if (sortOrder.ToLower() == "asc")
                    {
                        var sort = from cu in firstQuery
                                   orderby cu.ID ascending
                                   select cu;
                        sortList.AddRange(sort);
                    }
                    else
                    {
                        var sort = from cu in firstQuery
                                   orderby cu.ID descending
                                   select cu;
                        sortList.AddRange(sort);
                    }
                }
                else if (sortName == "Type")
                {
                    if (sortOrder.ToLower() == "asc")
                    {
                        var sort = from cu in firstQuery
                                   orderby (int)cu.Type ascending
                                   select cu;
                        sortList.AddRange(sort);
                    }
                    else
                    {
                        var sort = from cu in firstQuery
                                   orderby (int)cu.Type descending
                                   select cu;
                        sortList.AddRange(sort);
                    }
                }
                else if (sortName == "LocAddress")
                {
                    if (sortOrder.ToLower() == "asc")
                    {
                        var sort = from cu in firstQuery
                                   orderby cu.LocAddress ascending
                                   select cu;
                        sortList.AddRange(sort);
                    }
                    else
                    {
                        var sort = from cu in firstQuery
                                   orderby cu.LocAddress descending
                                   select cu;
                        sortList.AddRange(sort);
                    }
                }
                else if (sortName == "UserCode")
                {
                    if (sortOrder.ToLower() == "asc")
                    {
                        var sort = from cu in firstQuery
                                   orderby cu.UserCode ascending
                                   select cu;
                        sortList.AddRange(sort);
                    }
                    else
                    {
                        var sort = from cu in firstQuery
                                   orderby cu.UserCode descending
                                   select cu;
                        sortList.AddRange(sort);
                    }
                }
                else
                {
                    sortList.AddRange(firstQuery);
                }
            }
            else
            {
                sortList.AddRange(firstQuery);
            }
            #endregion

            #endregion
            #region 分页
            int index = 1;
            int size = 10;
            if (pageSize != null)
            {
                size = (int)pageSize;
            }
            if (pageIndex != null)
            {
                index = (int)pageIndex;
            }
            int total = firstQuery.Count;
            List<CustomerModel> last = sortList.Skip((index - 1) * size).Take(size).ToList();
            #endregion
            var value = new
            {
                total = total,
                rows = last
            };
            return Json(value);
        }

        public ActionResult Edit(int ID)
        {
            CustomerModel model = new CustomerModel();
            CWICCard cwiccd = new CWICCard();
            Customer cust = cwiccd.FindCust(ID);
            ICCard iccd = cwiccd.Find(ic=>ic.CustID==ID);

            model.ID = cust.ID;
            model.UserName = cust.UserName;
            model.FamilyAddress = cust.FamilyAddress;
            model.MobilePhone = cust.MobilePhone;
            model.PlateNum = cust.PlateNum;
            if (iccd != null)
            {
                model.UserCode = iccd.UserCode;
                model.Type = iccd.Type;
                model.Status = iccd.Status;
                model.Warehouse = iccd.Warehouse;
                model.LocAddress = iccd.LocAddress;
                model.Deadline = iccd.Deadline;
            }

            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var tp in Enum.GetValues(typeof(EnmICCardType)))
            {
                SelectListItem item = new SelectListItem() { Text = tp.ToString(), Value = tp.ToString() };
                items.Add(item);
            }
            ViewData["list"] = items;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(CustomerModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            #region
            CWICCard cwiccd = new CWICCard();
            Customer cust = cwiccd.FindCust(model.ID);         
            ICCard oriIccd = cwiccd.Find(ic=>ic.CustID==model.ID);
            ICCard newIccd = cwiccd.Find(ic => ic.UserCode == model.UserCode);
            if (newIccd == null)
            {
                ModelState.AddModelError("", "当前卡号没有注册！");
                return View(model);
            }
            if (oriIccd != null && newIccd != null)
            {
                //不是同一张卡
                if (oriIccd.UserCode != newIccd.UserCode)
                {
                    #region                  
                    if (newIccd.Status != EnmICCardStatus.Normal)
                    {
                        ModelState.AddModelError("","卡已挂失或注销，无法绑定用户！");
                        return View(model);
                    }
                    if (newIccd.CustID != 0)
                    {
                        Customer oricust = cwiccd.FindCust(newIccd.CustID);
                        if (oricust != null)
                        {
                            ModelState.AddModelError("", "该卡已被绑定，车主姓名：" + oricust.UserName);
                            return View(model);
                        }
                    }
                    #endregion
                }
                //是固定卡时
                if (model.Type == EnmICCardType.FixedLocation)
                {
                    #region
                    if (model.Warehouse == 0 || string.IsNullOrEmpty(model.LocAddress))
                    {
                        ModelState.AddModelError("", "固定卡，请指定绑定的库区及车位号！");
                        return View(model);
                    }
                    Location lctn = new CWLocation().FindLocation(lc=>lc.Warehouse==model.Warehouse&&lc.Address==model.LocAddress);
                    if (lctn == null)
                    {
                        ModelState.AddModelError("", "固定卡，请正确的库区及车位地址！");
                        return View(model);
                    }
                    ICCard iccd = cwiccd.Find(ic=>ic.Warehouse==model.Warehouse&&ic.LocAddress==model.LocAddress);
                    if (iccd != null)
                    {
                        if (iccd.UserCode != newIccd.UserCode)
                        {
                            ModelState.AddModelError("", "当前车位已被别的卡绑定，卡号-" + iccd.UserCode);
                            return View(model);
                        }
                    }
                    #endregion
                }
                Location loc = new CWLocation().FindLocation(lc=>lc.ICCode==newIccd.UserCode);
                if (loc != null)
                {
                    ModelState.AddModelError("", "当前卡号已存车，车位："+loc.Address+",请等待取车完成后绑定！");
                    return View(model);
                }
                if(oriIccd.UserCode != newIccd.UserCode)
                {
                    //释放旧卡
                    oriIccd.Type = EnmICCardType.Temp;
                    oriIccd.CustID = 0;
                    oriIccd.Deadline = DateTime.Parse("2017-1-1");
                    oriIccd.Warehouse = 0;
                    oriIccd.LocAddress = "";
                    cwiccd.Update(oriIccd);
                    //绑定新卡
                    newIccd.Type = model.Type;
                    newIccd.Warehouse = 0;
                    newIccd.LocAddress = "";
                    if (model.Type == EnmICCardType.FixedLocation)
                    {
                        newIccd.Warehouse = (int)model.Warehouse;
                        newIccd.LocAddress = model.LocAddress;
                    }
                    newIccd.CustID = cust.ID;
                    cwiccd.Update(newIccd);
                }
                else
                {
                    newIccd.Type = model.Type;
                    newIccd.Warehouse = 0;
                    newIccd.LocAddress = "";
                    if (model.Type == EnmICCardType.FixedLocation)
                    {
                        newIccd.Warehouse = (int)model.Warehouse;
                        newIccd.LocAddress = model.LocAddress;
                    }
                    newIccd.CustID = cust.ID;
                    cwiccd.Update(newIccd);
                }
            }            
            //允许更新
            cust.PlateNum = model.PlateNum;
            cust.MobilePhone = model.MobilePhone;
            cust.UserName = model.UserName;
            cust.FamilyAddress = model.FamilyAddress;
            cwiccd.UpdateCust(cust);
            #endregion
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int ID)
        {
            #region
            CWICCard cwiccd = new CWICCard();
            Customer cust = cwiccd.FindCust(ID);
            if (cust != null)
            {
                ICCard iccd = cwiccd.Find(ic => ic.CustID == ID);
                if (iccd != null)
                {
                    Location lct = new CWLocation().FindLocation(l=>l.ICCode==iccd.UserCode);
                    if (lct != null)
                    {
                        var data = new
                        {
                            code = 1,
                            message = "当前卡号已存车，请等待取车结算后，再删除该用户"
                        };
                        return Json(data,JsonRequestBehavior.AllowGet);
                    }
                    iccd.CustID = 0;
                    iccd.Type = EnmICCardType.Temp;
                    iccd.Warehouse = 0;
                    iccd.LocAddress = "";
                    iccd.Deadline = DateTime.Parse("2017-1-1");
                    Response _resp= cwiccd.Update(iccd);
                    if (_resp.Code == 0)
                    {
                        //记录

                    }
                }
                Response resp= cwiccd.Delete(ID);
                if (resp.Code == 0)
                {
                    var data = new
                    {
                        code = 1,
                        message = "删除用户失败，请联系管理员！"
                    };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            #endregion
            var nback = new
            {
                code = 2,
                message = "删除成功！"
            };
            return Json(nback, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 绑定指纹
        /// </summary>
        /// <param name="custID"></param>
        /// <returns></returns>
        public async Task<ActionResult> ReadFingerPrint(int custID)
        {
            CWFingerPrint fprint = new CWFingerPrint();
            Response resp = await fprint.FindFingerPrintAsync(custID);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }


    }
}