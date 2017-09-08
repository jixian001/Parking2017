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
using Parking.Web.Models;

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
            CWICCard cwiccd = new CWICCard();
            //顾客姓名保证唯一的
            Customer other = cwiccd.FindCust(cu=>cu.UserName==model.UserName);
            if (other != null)
            {
                ModelState.AddModelError("", "当前顾客名-  "+model.UserName+" 已被占用，其车牌号- "+other.PlateNum+" ，请输入唯一的用户名！");
                return View(model);
            }
            string plate = "";
            foreach(char vl in model.PlateNum)
            {
                if (Char.IsLetter(vl))
                {
                    plate += char.ToUpper(vl);
                }
                else
                {
                    plate += vl;
                }
            }            

            model.PlateNum = plate;          

            //车牌号码保证唯一的
            other = cwiccd.FindCust(cu => cu.PlateNum == model.PlateNum);
            if (other != null)
            {
                ModelState.AddModelError("", "当前车牌号-  " + model.PlateNum + " 已被绑定，其顾客名- " + other.UserName + " ，请输入正确的车牌号！");
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
               else
                {
                    //固定车位时，当前车位没有存车
                    if (lctn.Status != EnmLocationStatus.Space)
                    {
                        ModelState.AddModelError("", "当前车位：" + lctn.Address + " 已存车，卡号- "+lctn.ICCode+" ,请等待取车完成后再绑定！");
                        return View(model);
                    }
                }
            }
           
            ICCard iccd = null;
            if (!string.IsNullOrEmpty(model.UserCode))
            {
                #region          
                iccd = cwiccd.Find(ic => ic.UserCode == model.UserCode);
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
                        ModelState.AddModelError("", "该卡已被绑定，车主姓名：" + cust.UserName);
                        return View(model);
                    }
                }              
                #endregion
            }

            Customer addcust = new Customer();           
            if (model.Type == EnmICCardType.FixedLocation)
            {               
               Customer cust= cwiccd.FindCust(cc => cc.Warehouse == model.Warehouse && cc.LocAddress == model.LocAddress);
                if (cust != null)
                {
                    ModelState.AddModelError("", "该车位已被其他卡绑定，无法使用该车位- "+model.LocAddress+" ,Warehouse- "+model.Warehouse);
                    return View(model);
                }
                addcust.Warehouse = (int)model.Warehouse;
                addcust.LocAddress = model.LocAddress;
            }
           
            addcust.UserName = model.UserName;
            addcust.PlateNum = model.PlateNum;
            addcust.FamilyAddress = model.FamilyAddress;
            addcust.MobilePhone = model.MobilePhone;
            addcust.Type = EnmICCardType.Temp;
            if ((int)model.Type > 0)
            {
                addcust.Type = model.Type;              
            }
            addcust.StartDTime = DateTime.Parse("2017-1-1");
            addcust.Deadline = DateTime.Parse("2017-1-1");

            Response resp = cwiccd.AddCust(addcust);
            if (resp.Code == 1)
            {
                //如果有卡号，则绑定顾客于卡号
                if (iccd != null)
                {
                    iccd.CustID = addcust.ID;
                    resp = cwiccd.Update(iccd);
                }

                #region 绑定指纹，更新指纹信息
                CWFingerPrint fprint = new CWFingerPrint();
                if (!string.IsNullOrEmpty(model.FingerPrint1))
                {
                    Int32 sn = Convert.ToInt32(model.FingerPrint1);
                    FingerPrint finger = fprint.Find(p=>p.SN_Number==sn);
                    if (finger != null)
                    {
                        finger.CustID = addcust.ID;
                        fprint.Update(finger);
                    }
                }
                if (!string.IsNullOrEmpty(model.FingerPrint2))
                {
                    Int32 sn = Convert.ToInt32(model.FingerPrint2);
                    FingerPrint finger = fprint.Find(p => p.SN_Number == sn);
                    if (finger != null)
                    {
                        finger.CustID = addcust.ID;
                        fprint.Update(finger);
                    }
                }
                if (!string.IsNullOrEmpty(model.FingerPrint3))
                {
                    Int32 sn = Convert.ToInt32(model.FingerPrint3);
                    FingerPrint finger = fprint.Find(p => p.SN_Number == sn);
                    if (finger != null)
                    {
                        finger.CustID = addcust.ID;
                        fprint.Update(finger);
                    }
                }               
                #endregion
            }

            //if (addcust.Type == EnmICCardType.FixedLocation)
            //{
            //    //个推更新固定车位数
            //    MainCallback<Location>.Instance().OnChange(1, null);
            //}
           
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
            List<CustomerModel> models = new List<CustomerModel>();
            foreach (Customer cust in custLst)
            {
                CustomerModel model = new CustomerModel();
                model.ID = cust.ID;
                model.UserName = cust.UserName;
                model.PlateNum = cust.PlateNum;
                model.Type = cust.Type;
                model.Warehouse = cust.Warehouse;
                model.LocAddress = cust.LocAddress;
                model.MobilePhone = cust.MobilePhone;
                model.Deadline = cust.Deadline.ToString();
                model.FamilyAddress = cust.FamilyAddress;

                ICCard iccd = cwiccd.Find(ic=>ic.CustID==cust.ID);
                if (iccd != null)
                {
                    model.UserCode = iccd.UserCode;
                    model.Status = iccd.Status;
                }
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

            Task.Factory.StartNew(() =>
            {
                #region 删除没有绑定用户的指纹(垃圾指纹)
                CWFingerPrint cwfinger = new CWFingerPrint();
                List<FingerPrint> noCustFPrintLst = cwfinger.FindList(fp => fp.CustID == 0);
                foreach (FingerPrint print in noCustFPrintLst)
                {
                    cwfinger.Delete(print.ID, false);
                }
              
                #endregion
            });

            return Json(value);
        }

        public ActionResult Edit(int ID)
        {
            CustomerModel model = new CustomerModel();
            CWICCard cwiccd = new CWICCard();
            Customer cust = cwiccd.FindCust(ID);
            
            model.ID = cust.ID;
            model.UserName = cust.UserName;
            model.FamilyAddress = cust.FamilyAddress;
            model.MobilePhone = cust.MobilePhone;
            model.PlateNum = cust.PlateNum;
            model.Type = cust.Type;
            model.Warehouse = cust.Warehouse;
            model.LocAddress = cust.LocAddress;
            model.Deadline = cust.Deadline.ToString();

            ICCard iccd = cwiccd.Find(ic => ic.CustID == ID);
            if (iccd != null)
            {
                model.UserCode = iccd.UserCode;               
                model.Status = iccd.Status;              
            }

            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var tp in Enum.GetValues(typeof(EnmICCardType)))
            {
                SelectListItem item = new SelectListItem() { Text = tp.ToString(), Value = tp.ToString() };
                items.Add(item);
            }
            ViewData["list"] = items;

            #region 查找指纹
            List<FingerPrint> printList = new CWFingerPrint().FindList(p=>p.CustID==ID);
            if (printList.Count == 1)
            {
                model.FingerPrint1 = printList[0].SN_Number.ToString();
            }
            if (printList.Count == 2)
            {
                model.FingerPrint1 = printList[0].SN_Number.ToString();
                model.FingerPrint2 = printList[1].SN_Number.ToString();
            }
            if (printList.Count == 3)
            {
                model.FingerPrint1 = printList[0].SN_Number.ToString();
                model.FingerPrint2 = printList[1].SN_Number.ToString();
                model.FingerPrint3 = printList[2].SN_Number.ToString();
            }
            #endregion
            
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(CustomerModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            CWICCard cwiccd = new CWICCard();
            #region 验证用户名、车牌号
            //顾客姓名保证唯一的
            Customer other = cwiccd.FindCust(cu => cu.UserName == model.UserName && cu.ID != model.ID);
            if (other != null)
            {
                ModelState.AddModelError("", "当前顾客名-  " + model.UserName +
                    " 已被占用，其车牌号- " + other.PlateNum + " ，请输入唯一的用户名！");
                return View(model);
            }
            //车牌号码保证唯一的
            other = cwiccd.FindCust(cu => cu.PlateNum == model.PlateNum && cu.ID != model.ID);
            if (other != null)
            {
                ModelState.AddModelError("", "当前车牌号-  " + model.PlateNum +
                    " 已被绑定，其顾客名- " + other.UserName + " ，请输入正确的车牌号！");
                return View(model);
            }
            #endregion
            #region
            Customer cust = cwiccd.FindCust(model.ID);
            //是固定卡时
            if (model.Type == EnmICCardType.FixedLocation)
            {
                #region
                if (model.Warehouse == 0 || string.IsNullOrEmpty(model.LocAddress))
                {
                    ModelState.AddModelError("", "固定卡，请指定绑定的库区及车位号！");
                    return View(model);
                }
                Location lctn = new CWLocation().FindLocation(lc => lc.Warehouse == model.Warehouse && lc.Address == model.LocAddress);
                if (lctn == null)
                {
                    ModelState.AddModelError("", "固定卡，请正确的库区及车位地址！");
                    return View(model);
                }
                else
                {
                    //固定车位时，当前车位没有存车
                    if (lctn.Status != EnmLocationStatus.Space)
                    {
                        ModelState.AddModelError("", "当前车位：" + lctn.Address + " 已存车，卡号- " + lctn.ICCode + " ,请等待取车完成后再绑定！");
                        return View(model);
                    }
                }

                Customer custo = cwiccd.FindCust(cc => cc.Warehouse == model.Warehouse && cc.LocAddress == model.LocAddress);
                if (custo != null)
                {
                    if (custo.ID != cust.ID)
                    {
                        ModelState.AddModelError("", "当前车位已被别的用户绑定");
                        return View(model);
                    }
                }
                cust.Type = model.Type;
                cust.Warehouse = (int)model.Warehouse;
                cust.LocAddress = model.LocAddress;
                #endregion
            }
            else
            {
                cust.Type = model.Type;
                cust.Warehouse = 0;
                cust.LocAddress = "";
                cust.StartDTime = DateTime.Parse("2017-1-1");
                cust.Deadline = DateTime.Parse("2017-1-1");
            }

            ICCard oriIccd = cwiccd.Find(ic => ic.CustID == model.ID);
            ICCard newIccd = null;
            if (!string.IsNullOrEmpty(model.UserCode))
            {
                newIccd = cwiccd.Find(ic => ic.UserCode == model.UserCode);
                if (newIccd == null)
                {
                    ModelState.AddModelError("", "当前卡号没有注册！");
                    return View(model);
                }
            }
            if (oriIccd == null)
            {
                //原先没有绑定的
                if (newIccd != null)
                {
                    newIccd.CustID = cust.ID;
                    cwiccd.Update(newIccd);
                }
            }
            else
            {
                if (newIccd == null)
                {
                    //释放原来卡号
                    //释放旧卡                  
                    oriIccd.CustID = 0;
                    cwiccd.Update(oriIccd);
                }
                else //两卡都存在
                {
                    //不是同一张卡
                    if (oriIccd.UserCode != newIccd.UserCode)
                    {
                        #region                  
                        if (newIccd.Status != EnmICCardStatus.Normal)
                        {
                            ModelState.AddModelError("", "卡已挂失或注销，无法绑定用户！");
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
                        //释放旧卡                  
                        oriIccd.CustID = 0;
                        cwiccd.Update(oriIccd);
                        //绑定新卡                  
                        newIccd.CustID = cust.ID;
                        cwiccd.Update(newIccd);
                    }
                }
            }   
           
            //允许更新
            cust.PlateNum = model.PlateNum;
            cust.MobilePhone = model.MobilePhone;
            cust.UserName = model.UserName;
            cust.FamilyAddress = model.FamilyAddress;

            cwiccd.UpdateCust(cust);

           
            #region 更新指纹
            CWFingerPrint cwfprint = new CWFingerPrint();
            if (model.FingerPrint1 != "")
            {
                int fpvalue = Convert.ToInt32(model.FingerPrint1);
                FingerPrint fp = cwfprint.Find(fi=>fi.SN_Number==fpvalue);
                if (fp != null)
                {
                    if (fp.CustID == 0)
                    {
                        fp.CustID = cust.ID;
                        cwfprint.Update(fp);
                    }
                }
            }
            if (model.FingerPrint2 != "")
            {
                int fpvalue = Convert.ToInt32(model.FingerPrint2);
                FingerPrint fp = cwfprint.Find(fi => fi.SN_Number ==fpvalue);
                if (fp != null)
                {
                    if (fp.CustID == 0)
                    {
                        fp.CustID = cust.ID;
                        cwfprint.Update(fp);
                    }
                }
            }
            if (model.FingerPrint3 != "")
            {
                int fpvalue = Convert.ToInt32(model.FingerPrint3);
                FingerPrint fp = cwfprint.Find(fi => fi.SN_Number == fpvalue);
                if (fp != null)
                {
                    if (fp.CustID == 0)
                    {
                        fp.CustID = cust.ID;
                        cwfprint.Update(fp);
                    }
                }
            }
            #endregion

            ////个推更新固定车位数
            //MainCallback<Location>.Instance().OnChange(1, null);

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
                    iccd.CustID = 0;                   
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

                #region 删除相关指纹
                CWFingerPrint fprint = new CWFingerPrint();
                List<FingerPrint> fingerLst = fprint.FindList(p => p.CustID == ID);
                foreach(FingerPrint print in fingerLst)
                {
                    fprint.Delete(print.ID);
                }
                #endregion
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
        /// 新增用户界面，增加指纹，不用后台读指纹
        /// </summary>
        /// <param name="custID"></param>
        /// <returns></returns>
        public ActionResult AddFingerPrint()
        {
            //CWFingerPrint fprint = new CWFingerPrint();
            //Response resp = await fprint.AddFingerPrintAsync(0);
            Response resp = new Response();
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改用户界面，增加指纹，不用后台读指纹
        /// </summary>
        /// <param name="custID"></param>
        /// <returns></returns>
        public ActionResult AddFingerPrintFromModify(int custID)
        {
            //CWFingerPrint fprint = new CWFingerPrint();
            //Response resp = await fprint.AddFingerPrintAsync(custID);
            Response resp = new Response();
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> SubmitFPrint(int custID,string strMBBuf)
        {
            Response resp = new Response();
            if (!string.IsNullOrEmpty(strMBBuf))
            {
                resp =await new CWFingerPrint().SubmitFingerTemplateAsync(custID, strMBBuf);
            }
            else
            {
                resp.Message = "指纹信息为空";
            }
            return Json(resp);
        }

        [HttpGet]
        public ActionResult GetOCXIPAddress()
        {
            string ipaddrs = "";
            string addrs = XMLHelper.GetRootNodeValueByXpath("root", "OCXIPAddrs");
            if (addrs != null)
            {
                ipaddrs = addrs;
            }
            return Content(ipaddrs);
        }

        /// <summary>
        /// 删除指纹
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public ActionResult DeleteFingerPrint(int sn)
        {
            Response resp = new Response();
            CWFingerPrint fpring = new CWFingerPrint();
            FingerPrint finger = fpring.Find(p=>p.SN_Number==sn);
            if (finger != null)
            {
                resp = fpring.Delete(finger.ID);
            }
            else
            {
                resp.Message = "系统异常，找不到SN-"+sn+" 的指纹";
            }
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="custID"></param>
        /// <returns></returns>
        public ActionResult ChangeDeadline(int custID)
        {
            CWICCard cwiccd = new CWICCard();
            Customer cust = cwiccd.FindCust(custID);
            ChangeDeadlineModel deadline = new ChangeDeadlineModel
            {
                ID = cust.ID,
                UserCode = cust.UserName,
                Type = cust.Type,
                OldDeadline = cust.Deadline,
                NewDeadline = DateTime.Parse("2017-1-1")
            };
            return View(deadline);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeDeadline(ChangeDeadlineModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "参数设置不正确");
                return View(model);
            }
            Log log = LogFactory.GetLogger("ChangeDeadline");
            try
            {
                CWICCard cwiccd = new CWICCard();
                Customer cust = await cwiccd.FindCustAsync(model.ID);
                if (cust != null)
                {
                    if ((int)cust.Type < 2)
                    {
                        ModelState.AddModelError("", "临时卡，无法设置使用期限");
                        return View(model);
                    }
                    string olddeadline = cust.Deadline.ToString();

                    cust.Deadline = model.NewDeadline;
                    Response resp = cwiccd.UpdateCust(cust);
                    if (resp.Code == 1)
                    {
                        string oprtname = User.Identity.Name;
                        string utype = "";
                        if (cust.Type == EnmICCardType.FixedLocation)
                        {
                            utype = "固定";
                        }
                        else if (cust.Type == EnmICCardType.Periodical)
                        {
                            utype = "定期";
                        }

                        FixUserChargeLog fixlog = new FixUserChargeLog
                        {
                            UserName = cust.UserName,
                            PlateNum = cust.PlateNum,
                            UserType = utype,
                            Proof = "手动",
                            LastDeadline = olddeadline,
                            CurrDeadline = cust.Deadline.ToString(),
                            FeeType = "",
                            FeeUnit = 0,
                            CurrFee = 0,
                            RecordDTime = DateTime.Now,
                            OprtCode = oprtname
                        };
                        await new CWTariffLog().AddFixLogAsync(fixlog);
                    }
                }
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                log.Error(ex.ToString());

                ModelState.AddModelError("", "系统异常，请联系厂家！");
                return View(model);
            }
            
        }
    }
}