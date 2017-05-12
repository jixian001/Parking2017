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
        public List<PreCharging> FindPreChargeList(Expression<Func<PreCharging,bool>> where)
        {
            return preChgManager.FindList().ToList();
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
            return tempManager.FindList().ToList();
        }
        /// <summary>
        /// 分页查询
        /// </summary>
        public Page<TempChargingRule> FindPageTempRuleList(int pageSize, int pageIndex)
        {
            Page<TempChargingRule> page = new Page<TempChargingRule>();
            page.PageIndex = pageIndex;
            page.PageSize = pageSize;

            OrderParam orderParam = new OrderParam();
            orderParam.PropertyName = "ID";
            orderParam.Method = OrderMethod.Asc;

            page = tempManager.FindPageList(page, orderParam);
            return page;
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
        /// 添加临时类收费记录，
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
            return orderDetailManager.FindList().ToList();
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
            return hourDetailManager.FindList().ToList();
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

        #region 时间段
        /// <summary>
        /// 查找时间段列表
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public List<HourSectionInfo> FindHourSectionList(Expression<Func<HourSectionInfo,bool>> where)
        {
            return hourSectionManager.FindList(where);
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

        public FixChargingRule FindFixCharge(Expression<Func<FixChargingRule, bool>> where)
        {
            return fixManager.Find(where);
        }

        public Response UpdateFixCharge(FixChargingRule fix)
        {
            return fixManager.Update(fix);
        }
        #endregion
    }
}
