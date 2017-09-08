using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Core
{
    /// <summary>
    /// 收费相关
    /// </summary>
    public class CWTariff
    {
        #region manager类
        //预付类
        private PreChargingManager preChgManager = new PreChargingManager();
        //固定类
        private FixChargingRuleManager fixManager = new FixChargingRuleManager();
        //临时类
        private TempChargingRuleManager tempManager = new TempChargingRuleManager();
        //按次计费
        private OrderChargeDetailManager orderDetailManager = new OrderChargeDetailManager();
        //按时计费
        private HourChargeDetailManager hourDetailManager = new HourChargeDetailManager();
        private HourSectionInfoManager hourSectionManager = new HourSectionInfoManager();
        #endregion

        #region 预付类
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public List<PreCharging> FindPreChargeList(Expression<Func<PreCharging, bool>> where)
        {
            List<PreCharging> allLst = preChgManager.FindList(where);
            return allLst;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        public Page<PreCharging> FindPreRulePageList(int pageSize, int pageIndex, string sortOrder, string sortName)
        {
            Page<PreCharging> page = new Page<PreCharging>();
            page.PageIndex = pageIndex;
            page.PageSize = pageSize;

            OrderParam orderParam = new OrderParam();
            if (!string.IsNullOrEmpty(sortName))
            {
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
            else
            {
                orderParam.PropertyName = "ID";
                orderParam.Method = OrderMethod.Asc;
            }

            page = preChgManager.FindPageList(page, orderParam);
            return page;
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        public Response AddPreCharge(PreCharging pre)
        {
            return preChgManager.Add(pre);
        }
        /// <summary>
        /// 查找
        /// </summary>
        public PreCharging FindPreCharge(int ID)
        {
            return preChgManager.Find(ID);
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public PreCharging FindPreCharge(Expression<Func<PreCharging, bool>> where)
        {
            return preChgManager.Find(where);
        }

        /// <summary>
        ///  更新
        /// </summary>
        /// <param name="chg"></param>
        /// <returns></returns>
        public Response UpdatePreCharge(PreCharging chg)
        {
            return preChgManager.Update(chg);
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        public Response DeletePreCharge(int id)
        {
            return preChgManager.Delete(id);
        }
        #endregion

        #region 临时类卡
        /// <summary>
        /// 临时类，只允许有一个记录吧
        /// </summary>
        /// <returns></returns>
        public List<TempChargingRule> GetTempChgRuleList()
        {
            List<TempChargingRule> allLst = tempManager.FindList();
            return allLst;
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TempChargingRule FindTempChgRule(int id)
        {
            return tempManager.Find(id);
        }
        /// <summary>
        /// 查找
        /// </summary>
        public TempChargingRule FindTempChgRule(Expression<Func<TempChargingRule, bool>> where)
        {
            return tempManager.Find(where);
        }
        /// <summary>
        /// 添加临时类收费规则
        /// </summary>
        /// <param name="temprule"></param>
        /// <returns></returns>
        public Response AddTempChgRule(TempChargingRule temprule)
        {
            return tempManager.Add(temprule);
        }
        /// <summary>
        /// 更新临时类收费记录
        /// </summary>
        /// <param name="temprule"></param>
        /// <returns></returns>
        public Response UpdateTempChgRule(TempChargingRule temprule)
        {
            return tempManager.Update(temprule);
        }
        /// <summary>
        /// 删除临时类卡
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Response DeleteTempChgRule(int id)
        {
            return tempManager.Delete(id);
        }

        #region 按次
        /// <summary>
        /// 获取按次收费项，只允许有一个的
        /// </summary>
        /// <returns></returns>
        public List<OrderChargeDetail> GetOrderDetailList()
        {
            List<OrderChargeDetail> allLst = orderDetailManager.FindList();

            return allLst;
        }

        /// <summary>
        /// 查找按次收费记录
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public OrderChargeDetail FindOrderDetail(Expression<Func<OrderChargeDetail, bool>> where)
        {
            return orderDetailManager.Find(where);
        }

        /// <summary>
        /// 查找按次收费记录
        /// </summary>
        public OrderChargeDetail FindOrderDetail(int oid)
        {
            return orderDetailManager.Find(oid);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Response AddOrderDetail(OrderChargeDetail order)
        {
            return orderDetailManager.Add(order);
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Response UpdateOrderDetail(OrderChargeDetail order)
        {
            return orderDetailManager.Update(order);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Response DeleteOrderDetail(int id)
        {
            return orderDetailManager.Delete(id);
        }

        #endregion

        #region 按时
        /// <summary>
        /// 获取按时收费项，只允许有一个的
        /// </summary>
        /// <returns></returns>
        public List<HourChargeDetail> FindHourChgDetailList()
        {
            List<HourChargeDetail> allLst = hourDetailManager.FindList();
            return allLst;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public Response AddHourChgDetail(HourChargeDetail detail)
        {
            return hourDetailManager.Add(detail);
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public Response UpdateHourChgDetail(HourChargeDetail detail)
        {
            return hourDetailManager.Update(detail);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Response DeleteHourChgDetail(int ID)
        {
            return hourDetailManager.Delete(ID);
        }
        /// <summary>
        /// 查找
        /// </summary>
        public HourChargeDetail FindHourChgDetail(Expression<Func<HourChargeDetail, bool>> where)
        {
            return hourDetailManager.Find(where);
        }
        /// <summary>
        /// 查找
        /// </summary>
        public HourChargeDetail FindHourChgDetail(int ID)
        {
            return hourDetailManager.Find(ID);
        }

        #region 时间段
        /// <summary>
        /// 查找时间段列表
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public List<HourSectionInfo> FindHourSectionList(Expression<Func<HourSectionInfo, bool>> where)
        {
            return hourSectionManager.FindList(where);
        }
        /// <summary>
        /// 分页查询时间段
        /// </summary>
        public Page<HourSectionInfo> FindPageHourRuleList(int pageSize, int pageNumber)
        {
            Page<HourSectionInfo> page = new Page<HourSectionInfo>();
            page.PageIndex = pageNumber;
            page.PageSize = pageSize;

            OrderParam orderParam = new OrderParam();
            orderParam.PropertyName = "ID";
            orderParam.Method = OrderMethod.Asc;

            page = hourSectionManager.FindPageList(page, orderParam);
            return page;
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public HourSectionInfo FindHourSection(int ID)
        {
            return hourSectionManager.Find(ID);
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public HourSectionInfo FindHourSection(Expression<Func<HourSectionInfo, bool>> where)
        {
            return hourSectionManager.Find(where);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Response AddHourSection(HourSectionInfo section)
        {
            return hourSectionManager.Add(section);
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Response UpdateHourSection(HourSectionInfo section)
        {
            return hourSectionManager.Update(section);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Response DeleteHourSection(int ID)
        {
            return hourSectionManager.Delete(ID);
        }
        #endregion
        #endregion

        #endregion

        #region 固定类
        public FixChargingRule FindFixCharge(int ID)
        {
            return fixManager.Find(ID);
        }

        public Response AddFixRule(FixChargingRule rule)
        {
            return fixManager.Add(rule);
        }

        public FixChargingRule FindFixCharge(Expression<Func<FixChargingRule, bool>> where)
        {
            return fixManager.Find(where);
        }

        public Response UpdateFixCharge(FixChargingRule fix)
        {
            return fixManager.Update(fix);
        }

        public Response DeleteFixRule(int ID)
        {
            return fixManager.Delete(ID);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public Page<FixChargingRule> FindPageFixRuleLst(int pageSize, int pageNumber)
        {
            Page<FixChargingRule> page = new Page<FixChargingRule>();
            page.PageIndex = pageNumber;
            page.PageSize = pageSize;

            OrderParam orderParam = new OrderParam();
            orderParam.PropertyName = "ID";
            orderParam.Method = OrderMethod.Asc;

            page = fixManager.FindPageList(page, orderParam);
            return page;
        }
        #endregion
     
        /// <summary>
        /// 查询临时用户费用
        /// </summary>
        public Response GetTempUserInfo(string iccode, bool isPlate)
        {
            Log log = LogFactory.GetLogger("CWTariff.GetTempUserInfo");
            Response resp = new Response();
            CWICCard cwiccd = new CWICCard();
            CWLocation cwlctn = new CWLocation();
            try
            {
                Location loc = null;
                TempUserInfo info = new TempUserInfo();
                #region 暂不用
                //if (!isPlate)
                //{
                //    #region
                //    ICCard iccd = cwiccd.Find(ic=>ic.UserCode==iccode);
                //    if (iccd == null)
                //    {
                //        resp.Message = "不是本系统卡！";
                //        return resp;
                //    }
                //    if (iccd.CustID != 0)
                //    {
                //        Customer cust = cwiccd.FindCust(iccd.CustID);
                //        if (cust != null)
                //        {
                //            if (cust.Type != EnmICCardType.Temp)
                //            {
                //                resp.Message = "该用户不是临时用户！";
                //                return resp;
                //            }
                //        }
                //    }
                //    loc = cwlctn.FindLocation(lc=>lc.ICCode==iccode);
                //    if (loc == null)
                //    {
                //        resp.Message = "当前卡号没有存车！";
                //        return resp;
                //    }
                //    #endregion
                //}
                //else
                //{
                //    #region
                //    loc = cwlctn.FindLocation(l=>l.PlateNum==iccode);
                //    if (loc == null)
                //    {
                //        resp.Message = "当前输入车牌没有存车！";
                //        return resp;
                //    }
                //    string proof = loc.ICCode;
                //    Customer cust = null;
                //    #region
                //    if (Convert.ToInt32(proof) >= 10000) //是指纹激活的
                //    {
                //        int sno = Convert.ToInt32(proof);
                //        FingerPrint print = new CWFingerPrint().Find(p => p.SN_Number == sno);
                //        if (print == null)
                //        {
                //            //上位控制系统故障
                //            resp.Message = "找不到注册指纹，系统异常！";
                //            return resp;
                //        }
                //        cust = new CWICCard().FindCust(print.CustID);
                //        if (cust == null)
                //        {
                //            //上位控制系统故障
                //            resp.Message = "指纹没有绑定用户，系统异常！";
                //            return resp;
                //        }
                //    }
                //    else
                //    {
                //        ICCard iccd = new CWICCard().Find(ic => ic.UserCode == proof);
                //        if (iccd == null)
                //        {
                //            //上位控制系统故障
                //            resp.Message = "上位控制系统异常，找不到卡号！";
                //            return resp;
                //        }
                //        if (iccd.CustID != 0)
                //        {
                //            cust = new CWICCard().FindCust(iccd.CustID);
                //        }
                //    }
                //    #endregion
                //    if (cust != null)
                //    {
                //        if (cust.Type != EnmICCardType.Temp)
                //        {
                //            resp.Message = "该用户不是临时用户！";
                //            return resp;
                //        }
                //    }
                //    #endregion
                //}
                #endregion
                if (isPlate)
                {
                    //是车牌
                    loc = cwlctn.FindLocation(l => l.PlateNum == iccode);
                }
                else
                {
                    loc = cwlctn.FindLocation(l => l.ICCode == iccode);
                }
                if (loc == null)
                {
                    resp.Message = "当前车牌没有存车！Proof - " + iccode;
                    return resp;
                }
                int sno = Convert.ToInt32(loc.ICCode);
                SaveCertificate scert = new CWSaveProof().Find(s => s.SNO == sno);
                if (scert != null)
                {
                    Customer cust = new CWICCard().FindCust(scert.CustID);
                    if (cust != null)
                    {
                        if (cust.Type != EnmICCardType.Temp)
                        {
                            resp.Message = "该用户不是临时用户！";
                            return resp;
                        }
                    }
                }

                CWTask cwtask = new CWTask();
                ImplementTask itask = cwtask.FindITask(tk => tk.ICCardCode == loc.ICCode && tk.IsComplete == 0);
                if (itask != null)
                {
                    resp.Message = "正在作业，无法查询！";
                    return resp;
                }
                WorkTask queue = cwtask.FindQueue(q => q.ICCardCode == loc.ICCode);
                if (queue != null)
                {
                    resp.Message = "已经加入取车队列，无法查询！";
                    return resp;
                }

                info.CCode = iccode;
                info.InDate = loc.InDate.ToString();
                info.OutDate = DateTime.Now.ToString();
                TimeSpan span = DateTime.Now - loc.InDate;
                info.SpanTime = (span.Days > 0 ? span.Days + "天" : " ") + (span.Hours > 0 ? span.Hours + "小时" : " ") +
                        (span.Minutes >= 0 ? span.Minutes + "分" : " ") + (span.Seconds >= 0 ? span.Seconds + "秒" : " ");
                float fee = 0;
                resp = this.CalculateTempFee(loc.InDate, DateTime.Now, out fee);
                if (resp.Code == 0)
                {
                    return resp;
                }
                info.NeedFee = fee.ToString();
                info.Warehouse = loc.Warehouse;

                int hallID = new CWDevice().AllocateHall(loc, false);
                info.HallID = hallID;

                resp.Code = 1;
                resp.Message = "查询成功";
                resp.Data = info;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return resp;
        }

        /// <summary>
        /// 计算临时用户费用
        /// </summary>
        /// <param name="indate"></param>
        /// <param name="outdate"></param>
        /// <returns></returns>
        public Response CalculateTempFee(DateTime indate, DateTime outdate, out float fee)
        {
            fee = 0;
            Response resp = new Response();
            List<TempChargingRule> tempChgRuleLst = tempManager.FindList().ToList();
            if (tempChgRuleLst.Count == 0)
            {
                resp.Message = "未定义临时类计费规则";
                return resp;
            }
            TempChargingRule globalRule = tempChgRuleLst[0];
            if (globalRule.TempChgType == EnmTempChargeType.Order)
            {
                #region 按次
                OrderChargeDetail orderdetail = orderDetailManager.Find(globalRule.ID);
                if (orderdetail == null)
                {
                    resp.Message = "按次计费类型，但是没有找到按次计费规则表！";
                    return resp;
                }
                TimeSpan ts = outdate - indate;
                double totalMinutes = Math.Ceiling(ts.TotalMinutes);
                TimeSpan span = TimeSpan.Parse(orderdetail.OrderFreeTime.Trim());
                if (span.TotalMinutes > totalMinutes)
                {
                    fee = orderdetail.Fee;
                }
                resp.Code = 1;
                resp.Message = "按次费用，查询成功";
                #endregion
            }
            else if (globalRule.TempChgType == EnmTempChargeType.Hour)
            {
                #region 按时
                HourChargeDetail hourdetail = hourDetailManager.Find(globalRule.ID);
                if (hourdetail == null)
                {
                    resp.Message = "没有找到 按时计费 规则记录！";
                    return resp;
                }
                List<HourSectionInfo> sectionInfoLst = hourSectionManager.FindList(hs => hs.HourChgID == hourdetail.ID);
                if (sectionInfoLst.Count == 0)
                {
                    resp.Message = "没有找到 时段 规则记录";
                    return resp;
                }
                //这里都是24小时制，延续计费的方式进行，后面的，再增加
                //排序
                List<HourSectionInfo> timeslotLst = sectionInfoLst.OrderBy(sc => sc.StartTime).ToList();

                if (hourdetail.CycleTopFee > 0)
                {
                    #region 设置了周期最高计费
                    TimeSpan ts = outdate - indate;
                    int days = ts.Days;
                    bool isInit = true;
                    DateTime newIndate = indate;
                    //计算超过天数的停车费用
                    float daysFee = 0;
                    if (days > 0)
                    {
                        daysFee = days * hourdetail.CycleTopFee;
                        isInit = false;

                        newIndate = indate.AddDays(days);
                    }
                    float otherHoursFee = calcuteHoursFeeHasLimit(newIndate, outdate, timeslotLst, isInit);
                    if (otherHoursFee > 10000)
                    {
                        resp.Message = "系统异常,Fee - " + otherHoursFee;
                        return resp;
                    }
                    if (otherHoursFee > hourdetail.CycleTopFee)
                    {
                        otherHoursFee = hourdetail.CycleTopFee;
                    }
                    fee = daysFee + otherHoursFee;
                    #endregion
                }
                else
                {
                    //没有周期最高限额限制
                    fee = calcuteHoursFeeNoLimit(indate, outdate, timeslotLst);
                }
                resp.Code = 1;
                resp.Message = "查询费用成功";
                #endregion
            }
            else
            {
                resp.Message = "未定义计费类型，请核查！";
            }

            return resp;
        }

        #region 临时卡费用计算

        /// <summary>
        /// 临时卡计费规则(有 周期最高限额 )
        /// </summary>
        /// <param name="Indate"></param>
        /// <param name="Outdate"></param>
        /// <param name="timeslotLst"></param>
        /// <param name="isInit"></param>
        /// <returns></returns>
        private float calcuteHoursFeeHasLimit(DateTime Indate, DateTime Outdate, List<HourSectionInfo> timeslotLst, bool isInit)
        {
            string onlydate = Indate.ToShortDateString();

            foreach (HourSectionInfo timeslot in timeslotLst)
            {
                DateTime st = DateTime.Parse(onlydate + " " + timeslot.StartTime.ToLongTimeString());
                DateTime end = DateTime.Parse(onlydate + " " + (timeslot.EndTime.AddSeconds(-1)).ToLongTimeString());
                //没有跨0点
                if (DateTime.Compare(st, end) < 0)
                {
                    #region 起点落在 不是跨0点的时间段里面
                    if (DateTime.Compare(st, Indate) <= 0 && DateTime.Compare(end, Indate) >= 0)
                    {
                        //入库时间落在时间段内
                        TimeSpan ts = Outdate - Indate;
                        double totalMinutes = Math.Ceiling(ts.TotalMinutes);
                        #region 停车时间小于免费时长的
                        if (isInit)
                        {
                            TimeSpan freetime = TimeSpan.Parse(timeslot.SectionFreeTime.Trim());
                            if (totalMinutes <= freetime.TotalMinutes)
                            {
                                return 0;
                            }
                        }
                        #endregion
                        //出车时间也在该时段里面
                        if (DateTime.Compare(end, Outdate) >= 0)
                        {
                            #region 出车时间也落在当前时间段内
                            //如果在首段时间内，依首段时间计费
                            if (totalMinutes <= TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes)
                            {
                                return timeslot.FirstVoidFee;
                            }
                            //减去首段时间，
                            int remainMinutes = (int)totalMinutes - (int)TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes;
                            //整数部分
                            int integarPart = remainMinutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                            //余数部分
                            int remainer = remainMinutes % (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                            //不足一个间隔时长的，以一个时长计算（例：不足15分钟的以15分钟计算）
                            if (remainer > 0)
                            {
                                ++integarPart;
                            }
                            //返回费用
                            float sectFee = timeslot.FirstVoidFee + integarPart * timeslot.IntervalVoidFee;
                            //本段是否有收费限额
                            if (timeslot.SectionTopFee > 0)
                            {
                                if (sectFee > timeslot.SectionTopFee)
                                {
                                    return timeslot.SectionTopFee;
                                }
                            }
                            return sectFee;
                            #endregion
                        }
                        else
                        {
                            #region 出车时间出现跨时间段( 即 出现在下一个时间段)
                            //获取剩余的时间段
                            List<HourSectionInfo> strideTimeslotLst = timeslotLst.FindAll(tm => tm.ID != timeslot.ID);
                            if (strideTimeslotLst.Count == 0)
                            {
                                strideTimeslotLst = timeslotLst;
                            }

                            #region 本段时间内的时间 计算在本段内的费用
                            TimeSpan frontTS = end - Indate;
                            int frontMinutes = (int)Math.Ceiling(frontTS.TotalMinutes);
                            float sectFee = 0;
                            if (frontMinutes <= TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes)
                            {
                                sectFee = timeslot.FirstVoidFee;
                            }
                            else
                            {
                                //减去首段时间，
                                int remainMinutes = (int)frontMinutes - (int)TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes;
                                //整数部分
                                int integarPart = remainMinutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                                //余数部分
                                int remainer = remainMinutes % (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                                //不足一个间隔时长的，以一个时长计算（例：不足15分钟的以15分钟计算）
                                if (remainer > 0)
                                {
                                    ++integarPart;
                                }
                                sectFee = integarPart * timeslot.IntervalVoidFee;
                            }

                            if (timeslot.SectionTopFee > 0)
                            {
                                if (sectFee > timeslot.SectionTopFee)
                                {
                                    sectFee = timeslot.SectionTopFee;
                                }
                            }
                            #endregion

                            //起始时间从本段终点时间开始
                            DateTime strideStart = DateTime.Parse(onlydate + " " + timeslot.EndTime.ToLongTimeString());

                            //计算在另一段的费用
                            return sectFee + calcuteStrideSection(strideStart, Outdate, strideTimeslotLst, timeslotLst);
                            #endregion
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 起点落在 跨0点 时间段
                    DateTime nextEnd = end.AddDays(1);

                    DateTime newIndate = Indate;
                    DateTime newOutdate = Outdate;

                    //如果落到跨0点的时段上时
                    if (DateTime.Compare(st, newIndate) <= 0 && DateTime.Compare(nextEnd, newIndate) >= 0)
                    {
                        TimeSpan ts = newOutdate - newIndate;
                        double totalMinutes = Math.Ceiling(ts.TotalMinutes);
                        #region
                        //是从这个时间段开始入库的
                        if (isInit)
                        {
                            if (totalMinutes <= TimeSpan.Parse(timeslot.SectionFreeTime.Trim()).TotalMinutes)
                            {
                                //如果在免费时长内，则不收费
                                return 0;
                            }
                        }
                        //在时间段内停车
                        if (DateTime.Compare(nextEnd, newOutdate) >= 0)
                        {
                            #region

                            //如果在首段时间内，依首段时间计费
                            if (totalMinutes <= TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes)
                            {
                                return timeslot.FirstVoidFee;
                            }
                            //减去首段时间，
                            int remainMinutes = (int)totalMinutes - (int)TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes;
                            //整数部分
                            int integarPart = remainMinutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                            //余数部分
                            int remainer = remainMinutes % (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                            //不足一个间隔时长的，以一个时长计算（例：不足15分钟的以15分钟计算）
                            if (remainer > 0)
                            {
                                ++integarPart;
                            }
                            //返回费用
                            float sectFee = timeslot.FirstVoidFee + integarPart * timeslot.IntervalVoidFee;
                            //本段是否有收费限额
                            if (timeslot.SectionTopFee > 0)
                            {
                                if (sectFee > timeslot.SectionTopFee)
                                {
                                    return timeslot.SectionTopFee;
                                }
                            }
                            return sectFee;

                            #endregion
                        }
                        else //跨时间段停车
                        {
                            #region
                            //计算下些时段集合
                            List<HourSectionInfo> strideTimeslotLst = timeslotLst.FindAll(ti => ti.StartTime != timeslot.StartTime);
                            //本段时间内的时间
                            TimeSpan frontTS = nextEnd - newIndate;
                            int frontMinutes = (int)Math.Ceiling(frontTS.TotalMinutes);

                            float sectFee = 0;
                            if (frontMinutes <= TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes)
                            {
                                sectFee = timeslot.FirstVoidFee;
                            }
                            else
                            {
                                //减去首段时间，
                                int remainMinutes = (int)frontMinutes - (int)TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes;
                                //整数部分
                                int integarPart = remainMinutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                                //余数部分
                                int remainer = remainMinutes % (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                                //不足一个间隔时长的，以一个时长计算（例：不足15分钟的以15分钟计算）
                                if (remainer > 0)
                                {
                                    ++integarPart;
                                }
                                //跨周期是延续计费时时，则不包含首段时间收费
                                sectFee = integarPart * timeslot.IntervalVoidFee;
                            }

                            if (timeslot.SectionTopFee > 0)
                            {
                                if (sectFee > timeslot.SectionTopFee)
                                {
                                    sectFee = timeslot.SectionTopFee;
                                }
                            }

                            //起始时间从本段终点时间开始
                            DateTime strideStart = nextEnd;

                            //计算在另一段的费用
                            return sectFee + calcuteStrideSection(strideStart, Outdate, strideTimeslotLst, timeslotLst);

                            #endregion
                        }
                        #endregion
                    }
                    #endregion
                }
            }

            //都没有办法找到相应的区间的，则说明入出库时间都在跨0点后的0点时间段中
            foreach (HourSectionInfo timeslot in timeslotLst)
            {
                #region
                DateTime st = DateTime.Parse(onlydate + " " + timeslot.StartTime.ToLongTimeString());
                DateTime end = DateTime.Parse(onlydate + " " + (timeslot.EndTime.AddSeconds(-1)).ToLongTimeString());
                //跨O点
                if (DateTime.Compare(st, end) > 0)
                {
                    #region
                    DateTime nextEnd = end.AddDays(1);
                    //那这个入出库时间都应加上1天吧，不然基准点都不对了
                    DateTime newIndate = Indate.AddDays(1);
                    DateTime newOutdate = Outdate.AddDays(1);
                    //如果落到跨0点的时段上时
                    if (DateTime.Compare(st, newIndate) <= 0 && DateTime.Compare(nextEnd, newIndate) >= 0)
                    {
                        TimeSpan ts = newOutdate - newIndate;
                        double totalMinutes = Math.Ceiling(ts.TotalMinutes);
                        #region
                        if (isInit)
                        {
                            TimeSpan freetime = TimeSpan.Parse(timeslot.SectionFreeTime.Trim());
                            if (totalMinutes <= freetime.TotalMinutes)
                            {
                                return 0;
                            }
                        }
                        //在时间段内停车
                        if (DateTime.Compare(nextEnd, newOutdate) >= 0)
                        {
                            #region 出车时间也落在当前时间段内
                            //如果在首段时间内，依首段时间计费
                            if (totalMinutes <= TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes)
                            {
                                return timeslot.FirstVoidFee;
                            }
                            //减去首段时间，
                            int remainMinutes = (int)totalMinutes - (int)TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes;
                            //整数部分
                            int integarPart = remainMinutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                            //余数部分
                            int remainer = remainMinutes % (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                            //不足一个间隔时长的，以一个时长计算（例：不足15分钟的以15分钟计算）
                            if (remainer > 0)
                            {
                                ++integarPart;
                            }
                            //返回费用
                            float sectFee = timeslot.FirstVoidFee + integarPart * timeslot.IntervalVoidFee;
                            //本段是否有收费限额
                            if (timeslot.SectionTopFee > 0)
                            {
                                if (sectFee > timeslot.SectionTopFee)
                                {
                                    return timeslot.SectionTopFee;
                                }
                            }
                            return sectFee;
                            #endregion
                        }
                        else //跨时间段停车
                        {
                            #region 出车时间出现跨时间段( 即 出现在下一个时间段)
                            //获取剩余的时间段
                            List<HourSectionInfo> strideTimeslotLst = timeslotLst.FindAll(tm => tm.ID != timeslot.ID);
                            if (strideTimeslotLst.Count == 0)
                            {
                                strideTimeslotLst = timeslotLst;
                            }

                            #region 本段时间内的时间 计算在本段内的费用
                            TimeSpan frontTS = nextEnd - newIndate;
                            int frontMinutes = (int)Math.Ceiling(frontTS.TotalMinutes);
                            float sectFee = 0;
                            if (frontMinutes <= TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes)
                            {
                                sectFee = timeslot.FirstVoidFee;
                            }
                            else
                            {
                                //减去首段时间，
                                int remainMinutes = (int)frontMinutes - (int)TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes;
                                //整数部分
                                int integarPart = remainMinutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                                //余数部分
                                int remainer = remainMinutes % (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                                //不足一个间隔时长的，以一个时长计算（例：不足15分钟的以15分钟计算）
                                if (remainer > 0)
                                {
                                    ++integarPart;
                                }
                                sectFee = integarPart * timeslot.IntervalVoidFee;
                            }

                            if (timeslot.SectionTopFee > 0)
                            {
                                if (sectFee > timeslot.SectionTopFee)
                                {
                                    sectFee = timeslot.SectionTopFee;
                                }
                            }
                            #endregion

                            //起始时间从本段终点时间开始
                            DateTime strideStart = nextEnd;

                            //计算在另一段的费用
                            return sectFee + calcuteStrideSection(strideStart, newOutdate, strideTimeslotLst, timeslotLst);
                            #endregion
                        }
                        #endregion
                    }


                    #endregion
                }

                #endregion
            }

            return 41000;
        }

        /// <summary>
        /// 停车终点出现跨段的
        /// </summary>
        private float calcuteStrideSection(DateTime indate, DateTime outdate, List<HourSectionInfo> timeslotLst, List<HourSectionInfo> allTimeSlots)
        {
            if (timeslotLst.Count == 0)
            {
                return 40001;
            }
            //找出 indate 落在里面的时间段
            HourSectionInfo timeslot = timeslotLst.First(ti =>
               DateTime.Compare(ti.StartTime, DateTime.Parse(ti.StartTime.ToShortDateString() + " " + indate.ToShortTimeString())) == 0 ||
               DateTime.Compare(ti.StartTime, DateTime.Parse(ti.StartTime.ToShortDateString() + " " + indate.AddMinutes(1).ToShortTimeString())) == 0
                                                       );
            if (timeslot == null)
            {
                return 40002;
            }
            string onlydate = indate.ToShortDateString();
            DateTime st = DateTime.Parse(onlydate + " " + timeslot.StartTime.ToShortTimeString() + ":00");
            DateTime end = DateTime.Parse(onlydate + " " + (timeslot.EndTime.AddSeconds(-1)).ToShortTimeString() + ":00");
            //没有出现跨0点
            if (DateTime.Compare(st, end) < 0)
            {
                #region
                if (DateTime.Compare(end, outdate) >= 0)
                {
                    #region
                    TimeSpan ts = outdate - st;
                    int minutes = (int)Math.Ceiling(ts.TotalMinutes);
                    //整数部分
                    int integarPart = minutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                    //余数部分
                    int remainer = minutes % (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                    //不足一个间隔时长的，以一个时长计算（例：不足15分钟的以15分钟计算）
                    if (remainer > 0)
                    {
                        ++integarPart;
                    }
                    //返回费用
                    float sectFee = integarPart * timeslot.IntervalVoidFee;
                    //本段是否有收费限额
                    if (timeslot.SectionTopFee > 0)
                    {
                        if (sectFee > timeslot.SectionTopFee)
                        {
                            return timeslot.SectionTopFee;
                        }
                    }
                    //没有出现跨段的直接返回值
                    return sectFee;
                    #endregion
                }
                else //又出现跨段，使用迭代，继续累加
                {
                    #region
                    //计算下些时段集合
                    List<HourSectionInfo> strideTimeslotLst = timeslotLst.FindAll(ti => ti.ID != timeslot.ID);

                    //计算本段内的停车费用，时起始时间及终点时间，以时间段的时间为段
                    TimeSpan ts = end - st;
                    int minutes = (int)Math.Ceiling(ts.TotalMinutes);
                    //整数部分
                    int integarPart = minutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;

                    //返回费用
                    float sectFee = integarPart * timeslot.IntervalVoidFee;
                    //本段是否有收费限额
                    if (timeslot.SectionTopFee > 0)
                    {
                        if (sectFee > timeslot.SectionTopFee)
                        {
                            sectFee = timeslot.SectionTopFee;
                        }
                    }
                    //累加下一个时间段的费用
                    return sectFee + this.calcuteStrideSection(timeslot.EndTime, outdate, strideTimeslotLst, timeslotLst);
                    #endregion
                }
                #endregion
            }
            else //跨0点
            {
                #region
                DateTime nextEnd = end.AddDays(1); //跨时间段，则当前终点加1天
                if (DateTime.Compare(nextEnd, outdate) >= 0)
                {
                    #region
                    TimeSpan ts = outdate - st;
                    int minutes = (int)Math.Ceiling(ts.TotalMinutes);
                    //整数部分
                    int integarPart = minutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                    //余数部分
                    int remainer = minutes % (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                    //不足一个间隔时长的，以一个时长计算（例：不足15分钟的以15分钟计算）
                    if (remainer > 0)
                    {
                        ++integarPart;
                    }
                    //返回费用
                    float sectFee = integarPart * timeslot.IntervalVoidFee;
                    //本段是否有收费限额
                    if (timeslot.SectionTopFee > 0)
                    {
                        if (sectFee > timeslot.SectionTopFee)
                        {
                            return timeslot.SectionTopFee;
                        }
                    }
                    return sectFee;
                    #endregion
                }
                else //又出现跨段，使用迭代，继续累加
                {
                    #region
                    //计算下些时段集合
                    List<HourSectionInfo> strideTimeslotLst = timeslotLst.FindAll(ti => ti.StartTime != timeslot.StartTime);
                    if (strideTimeslotLst == null || !strideTimeslotLst.Exists(ti => ti.StartTime == timeslot.EndTime))
                    {
                        strideTimeslotLst = allTimeSlots.FindAll(ti => ti.StartTime != timeslot.StartTime);
                    }

                    TimeSpan ts = nextEnd - st;
                    int minutes = (int)Math.Ceiling(ts.TotalMinutes);
                    //整数部分
                    int integarPart = minutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;

                    //返回费用
                    float sectFee = integarPart * timeslot.IntervalVoidFee;
                    //本段是否有收费限额
                    if (timeslot.SectionTopFee > 0)
                    {
                        if (sectFee > timeslot.SectionTopFee)
                        {
                            sectFee = timeslot.SectionTopFee;
                        }
                    }

                    //累加下一个时间段的费用
                    return sectFee + this.calcuteStrideSection(nextEnd, outdate, strideTimeslotLst, allTimeSlots);

                    #endregion
                }
                #endregion
            }
        }

        /// <summary>
        /// 临时卡计费规则(没有 周期最高限额 )
        /// </summary>
        /// <param name="Indate"></param>
        /// <param name="Outdate"></param>
        /// <param name="timeslotLst"></param>
        /// <param name="hourChgDetail"></param>
        /// <returns></returns>
        private float calcuteHoursFeeNoLimit(DateTime Indate, DateTime Outdate, List<HourSectionInfo> timeslotLst)
        {
            //先计算一个周期的总费用，再将时间减去，最后调用 《calcuteHoursFeeHasLimit》有周期限制的
            float cyclefee = 0;
            foreach (HourSectionInfo timeslot in timeslotLst)
            {
                #region
                //本段时间内的时间
                TimeSpan frontTS = timeslot.EndTime - timeslot.StartTime;
                int frontMinutes = (int)Math.Ceiling(frontTS.TotalMinutes);

                float sectFee = 0;
                if (frontMinutes <= TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes)
                {
                    sectFee = timeslot.FirstVoidFee;
                }
                else
                {
                    //减去首段时间，
                    int remainMinutes = (int)frontMinutes - (int)TimeSpan.Parse(timeslot.FirstVoidTime.Trim()).TotalMinutes;
                    //整数部分
                    int integarPart = remainMinutes / (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                    //余数部分
                    int remainer = remainMinutes % (int)TimeSpan.Parse(timeslot.IntervalVoidTime.Trim()).TotalMinutes;
                    //不足一个间隔时长的，以一个时长计算（例：不足15分钟的以15分钟计算）
                    if (remainer > 0)
                    {
                        ++integarPart;
                    }
                    //首段费用+间隔费用
                    sectFee = timeslot.FirstVoidFee + integarPart * timeslot.IntervalVoidFee;
                }

                if (timeslot.SectionTopFee > 0)
                {
                    if (sectFee > timeslot.SectionTopFee)
                    {
                        sectFee = timeslot.SectionTopFee;
                    }
                }

                cyclefee += sectFee;
                #endregion
            }

            DateTime newIndate = Indate;
            TimeSpan ts = Outdate - Indate;
            if (ts.Days > 0)
            {
                newIndate = Indate.AddDays(ts.Days);
            }

            return cyclefee + calcuteHoursFeeHasLimit(newIndate, Outdate, timeslotLst, false);

        }

        #endregion

    }
}
