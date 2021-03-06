﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core;
using Parking.Web.Areas.SystemManager.Models;

namespace Parking.Web.Areas.SystemManager.Controllers
{    
    public class MSConfigController : Controller
    {
        // GET: SystemManager/MSConfig
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetSelectItemName(int warehouse)
        {
            List<SelectItem> items = new List<SelectItem>();
            //先以一个库算，后面做联动查询的
            List<Device> devLst = new CWDevice().FindList(d=>d.Warehouse==warehouse);
            foreach(Device dev in devLst)
            {
                if (dev.DeviceCode > 10)
                {
                    items.Add(new SelectItem { OptionValue = dev.DeviceCode.ToString(), OptionText = "车厅 " + (dev.DeviceCode - 10).ToString() });
                }
                else
                {
                    items.Add(new SelectItem { OptionValue = dev.DeviceCode.ToString(), OptionText = "TV" + dev.DeviceCode.ToString() });
                }
            }
            return Json(items, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult FindDeviceList(int? pageSize, int? pageIndex,
                                           string sortOrder, string sortName,
                                           string warehouse, string code)
        {
            CWDevice cwdevice = new CWDevice();
            Page<Device> page = new Page<Device>();
            if (pageSize != null)
            {
                page.PageSize = (int)pageSize;
            }
            if (pageIndex != null)
            {
                page.PageIndex = (int)pageIndex;
            }
            OrderParam orderParam = null;
            if (!string.IsNullOrEmpty(sortName))
            {
                orderParam = new OrderParam();
                orderParam.PropertyName = sortName;
                if (!string.IsNullOrEmpty(sortOrder))
                {
                    orderParam.Method = sortOrder.ToLower() == "asc" ? OrderMethod.Asc : OrderMethod.Desc;
                }
                else
                {
                    orderParam.Method = OrderMethod.Asc;
                }
            }
            if (warehouse != "0" && code != "0")
            {
                int wh = Convert.ToInt32(warehouse);
                int smg = Convert.ToInt32(code);

                page = cwdevice.FindPageList(page, (dev => dev.Warehouse == wh && dev.DeviceCode == smg), orderParam);
                var data = new
                {
                    total = page.TotalNumber,
                    rows = page.ItemLists
                };
                return Json(data);
            }
            page = cwdevice.FindPageList(page, (dev => true), orderParam);
            var value = new
            {
                total = page.TotalNumber,
                rows = page.ItemLists
            };
            return Json(value);
        }

        public ActionResult Detail(int? ID)
        {
            Device smg = new CWDevice().Find(dev => dev.ID == ID);
            return View(smg);
        }

        public ActionResult Edit(int? ID)
        {
            Device smg = new CWDevice().Find(dev => dev.ID == ID);
            return View(smg);
        }

        [HttpPost]
        public ActionResult Edit(int ID,int IsAble, string HallType)
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("POST Edit");
            try
            {
                Device smg = new CWDevice().Find(dev => dev.ID == ID);
                if (smg == null)
                {
                    return RedirectToAction("Index");
                }
                if (IsAble > 1)
                {
                    ModelState.AddModelError("", "<可用性> 只允许输入：0、1  两种");
                    return View(smg);
                }
                smg.IsAble = IsAble;
                if (smg.Type == EnmSMGType.Hall)
                {
                    EnmHallType htype = EnmHallType.Init;
                    bool nback = Enum.TryParse(HallType, out htype);
                    if (nback)
                    {
                        smg.HallType = htype;
                    }
                    else
                    {
                        ModelState.AddModelError("", " <车厅类型> 不正确，只允许输入：Entrance、Exit、EnterOrExit 三种");
                        return View(smg);
                    }
                }
                resp = new CWDevice().Update(smg);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return RedirectToAction("Index");          
        }

    }
}