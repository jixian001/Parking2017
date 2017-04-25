using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;

namespace Parking.Core
{
    /// <summary>
    /// 业务信息中转中心
    /// </summary>
    public class CWTaskTransfer
    {
        private CWTask motsk;
        private Device moHall;
        private int mHallGetCount;

        public CWTaskTransfer()
        {
            motsk = new CWTask();

            string getcount = XMLHelper.GetRootNodeValueByXpath("root", "MaxHallGetCar");
            mHallGetCount = string.IsNullOrEmpty(getcount) ? 0 : Convert.ToInt32(getcount);            
        }

        public CWTaskTransfer(int hallCode,int warehouse) :this()
        {
            moHall = new CWDevice().SelectSMG(hallCode, warehouse);
        }

        /// <summary>
        /// 有车入库
        /// </summary>
        public void DealCarEntrance()
        {
            Log log = LogFactory.GetLogger("CWTaskTransfer.DealCarEntrance");
            try
            {
                if (moHall.Mode != EnmModel.Automatic)
                {
                    //模式不是全自动
                    motsk.AddNofication(moHall.Warehouse, moHall.DeviceCode, "5.wav");
                    return;
                }
                if (moHall.TaskID != 0)
                {
                    //报警- 有故障作业未处理
                    motsk.AddNofication(moHall.Warehouse, moHall.DeviceCode, "39.wav");
                    return;
                }
                if (moHall.HallType == EnmHallType.Exit)
                {
                    //出车厅不允许进车
                    motsk.AddNofication(moHall.Warehouse, moHall.DeviceCode, "7.wav");
                    return;
                }
                //增加车厅作业，绑定车厅作业号
                motsk.DealFirstCarEntrance(moHall);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 外形检测上报处理(1001,101)
        /// </summary>
        /// <param name="tsk"></param>
        /// <param name="distance"></param>
        /// <param name="carSize"></param>
        public void DealICheckCar(ImplementTask tsk,int distance,string carSize,int weight)
        {
            Log log = LogFactory.GetLogger("CWTaskTransfer.DealICheckCar");
            try
            {
                if (tsk.Type == EnmTaskType.TempGet)
                {
                    //注意要刷卡后，将源地址车位与目的地址车位互换下
                    Location tolct = new CWLocation().FindLocation(lc => lc.Warehouse == tsk.Warehouse && lc.Address == tsk.ToLctAddress);
                    if (tolct.Status != EnmLocationStatus.TempGet)
                    {
                        //取物车位不可用，则临时分配
                        motsk.IDealCheckedCar(tsk, moHall.DeviceCode, distance, carSize, weight);
                    }
                    else
                    {
                        motsk.ITempDealCheckCar(tsk, tolct, distance, carSize, weight);
                    }
                }
                else
                {
                    motsk.IDealCheckedCar(tsk, moHall.DeviceCode, distance, carSize, weight);
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 处理车辆离开
        /// </summary>
        /// <param name="task"></param>
        public void DealCarLeave(ImplementTask task)
        {
            Log log = LogFactory.GetLogger("CWTaskTransfer.DealCarLeave");
            try
            {
                if (task.Type == EnmTaskType.SaveCar)
                {
                    motsk.ICancelInAndDeleteTask(task);
                }
                else //取车或取物时
                {
                    motsk.ODealCarDriveaway(task);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 处理刷卡动作
        /// </summary>
        /// <param name="physiccode"></param>
        public void DealICCardMessage(string physiccode)
        {
            Log log = LogFactory.GetLogger("CWTaskTransfer.DealICCardMessage");
            try
            {
                int warehouse = moHall.Warehouse;
                int code = moHall.DeviceCode;
                if (moHall.Mode != EnmModel.Automatic)
                {
                    motsk.AddNofication(warehouse, code, "5.wav");
                    return;
                }
                ICCard iccd = new CWICCard().Find(ic => ic.PhysicCode == physiccode);
                if (iccd != null)
                {
                    motsk.AddNofication(warehouse, code, "6.wav");
                    return;
                }
                if (iccd.Status == EnmICCardStatus.Lost ||
                    iccd.Status == EnmICCardStatus.Disposed)
                {
                    motsk.AddNofication(warehouse, code, "7.wav");
                    return;
                }
                ImplementTask task = motsk.Find(tk => tk.ICCardCode == iccd.UserCode && tk.IsComplete == 0);
                if (task != null)
                {
                    if (task.HallCode != moHall.DeviceCode)
                    {
                        motsk.AddNofication(warehouse, code, "8.wav");
                        return;
                    }
                    if (task.Status != EnmTaskStatus.ICarInWaitFirstSwipeCard &&
                        task.Status != EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                    {
                        motsk.AddNofication(warehouse, code, "9.wav");
                        return;
                    }
                }
                WorkTask queue = motsk.FindQueue(qu => qu.ICCardCode == iccd.UserCode);
                if (queue != null)
                {
                    motsk.AddNofication(warehouse, code, "9.wav");
                    return;
                }
                int getcarCount = motsk.GetHallGetCarCount(warehouse, code);
                Location lct = new CWLocation().FindLocation(lt => lt.ICCode == iccd.UserCode && lt.Type == EnmLocationType.Normal);

                #region 进车厅
                if (moHall.HallType == EnmHallType.Entrance)
                {
                    if (lct != null)
                    {
                        //请到出车厅刷卡取车
                        motsk.AddNofication(warehouse, code, "11.wav");
                        return;
                    }
                    if (moHall.TaskID == 0)
                    {
                        //车厅无车，不能存车
                        motsk.AddNofication(warehouse, code, "10.wav");
                        return;
                    }
                    ImplementTask tsk = motsk.Find(moHall.TaskID);
                    if (tsk == null)
                    {
                        log.Error("依车厅TaskID找不到作业信息，TaskID-"+moHall.TaskID+"  hallCode-"+moHall.DeviceCode);
                        //系统故障
                        motsk.AddNofication(warehouse, code, "20.wav");
                        return;
                    }
                    if (tsk.Status == EnmTaskStatus.ICarInWaitFirstSwipeCard)
                    {
                        //处理刷第一次卡
                        motsk.DealISwipedFirstCard(tsk, iccd.UserCode);
                    }
                    else if(tsk.Status == EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                    {
                        //处理刷第二次卡
                        if (tsk.ICCardCode != iccd.UserCode)
                        {
                            motsk.AddNofication(warehouse, code, "64.wav");
                            return;
                        }
                        motsk.DealISwipedSecondCard(tsk, iccd.UserCode);
                    }
                    else
                    {
                        motsk.AddNofication(warehouse, code, "9.wav");
                        return;
                    }                    
                }
                #endregion
                #region 出车厅
                else if (moHall.HallType == EnmHallType.Exit)
                {
                    if (lct == null)
                    {
                        //该卡没有存车
                        motsk.AddNofication(warehouse, code, "14.wav");
                        return;
                    }
                    if (lct.Status != EnmLocationStatus.Occupy)
                    {
                        //正在作业，
                        motsk.AddNofication(warehouse, code, "65.wav");
                        return;
                    }
                    string isCharge = XMLHelper.GetRootNodeValueByXpath("root", "ChargeEnable");
                    bool isChargeEnable = string.IsNullOrEmpty(isCharge) ? false : (isCharge == "1" ? true : false);
                    if (isChargeEnable)
                    {
                        #region
                        if (iccd.Type == EnmICCardType.Temp)
                        {                           
                            motsk.AddNofication(warehouse, code, "29.wav");
                            return;
                        }
                        else if (iccd.Type == EnmICCardType.Periodical || iccd.Type == EnmICCardType.FixedLocation)
                        {
                            if (DateTime.Compare(iccd.Deadline, DateTime.Now) < 0)
                            {
                                motsk.AddNofication(warehouse, code, "31.wav");
                                return;
                            }
                            if (DateTime.Compare(iccd.Deadline.AddDays(-2), DateTime.Now) < 0)
                            {
                                motsk.AddNofication(warehouse, code, "67.wav");
                            }
                            else if (DateTime.Compare(iccd.Deadline.AddDays(-1), DateTime.Now) < 0)
                            {
                                motsk.AddNofication(warehouse, code, "66.wav");
                            }                            
                        }
                        #endregion
                    }
                    if (getcarCount > mHallGetCount)
                    {
                        motsk.AddNofication(warehouse, code, "12.wav");
                        return;
                    }
                    //生成取车作业，加入队列
                    motsk.DealOSwipedCard(moHall, lct,iccd);
                }
                #endregion
                #region 进出车厅
                else if (moHall.HallType == EnmHallType.EnterOrExit)
                {
                    #region 存车
                    if (lct == null) //是进车状态
                    {
                        if (moHall.TaskID == 0)
                        {
                            //车厅无车，不能存车
                            motsk.AddNofication(warehouse, code, "10.wav");
                            return;
                        }
                        ImplementTask tsk = motsk.Find(moHall.TaskID);
                        if (tsk == null)
                        {
                            log.Error("依车厅TaskID找不到作业信息，TaskID-" + moHall.TaskID + "  hallCode-" + moHall.DeviceCode);
                            //系统故障
                            motsk.AddNofication(warehouse, code, "20.wav");
                            return;
                        }
                        if (tsk.Status == EnmTaskStatus.ICarInWaitFirstSwipeCard)
                        {
                            //处理刷第一次卡
                            motsk.DealISwipedFirstCard(tsk, iccd.UserCode);
                        }
                        else if (tsk.Status == EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                        {
                            //处理刷第二次卡
                            if (tsk.ICCardCode != iccd.UserCode)
                            {
                                motsk.AddNofication(warehouse, code, "64.wav");
                                return;
                            }
                            motsk.DealISwipedSecondCard(tsk, iccd.UserCode);
                        }
                        else
                        {
                            motsk.AddNofication(warehouse, code, "9.wav");
                            return;
                        }
                    }
                    #endregion
                    #region 取车
                    else
                    {
                        if (lct == null)
                        {
                            //该卡没有存车
                            motsk.AddNofication(warehouse, code, "14.wav");
                            return;
                        }
                        if (lct.Status != EnmLocationStatus.Occupy)
                        {
                            //正在作业，
                            motsk.AddNofication(warehouse, code, "65.wav");
                            return;
                        }
                        #region 如果车厅在存车，等存车刷卡了，取车才允许
                        if (moHall.TaskID != 0)
                        {
                            ImplementTask itask = motsk.Find(moHall.TaskID);
                            if (itask != null)
                            {
                                if (itask.Type == EnmTaskType.SaveCar)
                                {
                                    if (itask.Status == EnmTaskStatus.ICarInWaitFirstSwipeCard ||
                                        itask.Status == EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                                    {
                                        motsk.AddNofication(warehouse, code, "68.wav");
                                        return;
                                    }
                                }
                            }
                        }
                        #endregion

                        string isCharge = XMLHelper.GetRootNodeValueByXpath("root", "ChargeEnable");
                        bool isChargeEnable = string.IsNullOrEmpty(isCharge) ? false : (isCharge == "1" ? true : false);
                        if (isChargeEnable)
                        {
                            #region
                            if (iccd.Type == EnmICCardType.Temp)
                            {
                                motsk.AddNofication(warehouse, code, "29.wav");
                                return;
                            }
                            else if (iccd.Type == EnmICCardType.Periodical || iccd.Type == EnmICCardType.FixedLocation)
                            {
                                if (DateTime.Compare(iccd.Deadline, DateTime.Now) < 0)
                                {
                                    motsk.AddNofication(warehouse, code, "31.wav");
                                    return;
                                }
                                if (DateTime.Compare(iccd.Deadline.AddDays(-2), DateTime.Now) < 0)
                                {
                                    motsk.AddNofication(warehouse, code, "67.wav");
                                }
                                else if (DateTime.Compare(iccd.Deadline.AddDays(-1), DateTime.Now) < 0)
                                {
                                    motsk.AddNofication(warehouse, code, "66.wav");
                                }
                            }
                            #endregion
                        }
                        if (getcarCount > mHallGetCount)
                        {
                            motsk.AddNofication(warehouse, code, "12.wav");
                            return;
                        }
                        //生成取车作业，加入队列
                        motsk.DealOSwipedCard(moHall, lct, iccd);
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 手动出库
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="locAddress"></param>
        /// <returns></returns>
        public Response ManualGetCar(int warehouse,string locAddress)
        {
            Response _resp = new Response();
            Log log = LogFactory.GetLogger("CWTaskTransfer.ManualGetCar");
            try
            {
                if (moHall == null || moHall.Type != EnmSMGType.Hall)
                {
                    _resp.Message = "找不到车厅设备，请输入正确的库区及车厅！";
                    return _resp;
                }
                if (moHall.HallType == EnmHallType.Entrance ||
                    moHall.HallType == EnmHallType.Init)
                {
                    _resp.Message = "当前车厅-" + moHall.DeviceCode + ",不是出车厅！";
                    return _resp;
                }
                if (moHall.Mode != EnmModel.Automatic)
                {
                    _resp.Message = "当前车厅-" + moHall.DeviceCode + ",模式不是全自动！";
                    return _resp;
                }
                Location lctn = new CWLocation().FindLocation(l => l.Warehouse == warehouse && l.Address == locAddress);
                if (lctn == null)
                {
                    _resp.Message = "找不到车位，address-" + locAddress;
                    return _resp;
                }
                if (lctn.Type != EnmLocationType.Normal)
                {
                    _resp.Message = "车位不可用，address-" + locAddress;
                    return _resp;
                }
                if (lctn.Status != EnmLocationStatus.Occupy)
                {
                    _resp.Message = "车位不是占用状态，address-" + locAddress;
                    return _resp;
                }
                string iccode = lctn.ICCode;
                ImplementTask itask = motsk.Find(tk => tk.IsComplete == 0 && tk.ICCardCode == iccode);
                if (itask != null)
                {
                    _resp.Message = "正在作业，请稍后！";
                    return _resp;
                }
                WorkTask queue = motsk.FindQueue(qu => qu.ICCardCode == iccode);
                if (queue != null)
                {
                    _resp.Message = "正在作业，请稍后！";
                    return _resp;
                }

                return motsk.ManualGetCar(moHall, lctn);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return _resp;
        }

        /// <summary>
        /// 临时取物
        /// </summary>
        /// <param name="iccode"></param>
        /// <returns></returns>
        public Response TempGetCar(string iccode)
        {
            Response _resp = new Response();
            Log log = LogFactory.GetLogger("CWTaskTransfer.TempGetCar");
            try
            {
                if (moHall == null || moHall.Type != EnmSMGType.Hall)
                {
                    _resp.Message = "找不到车厅设备，请输入正确的库区及车厅！";
                    return _resp;
                }
                if (moHall.HallType != EnmHallType.EnterOrExit)
                {
                    _resp.Message = "当前车厅-" + moHall.DeviceCode + ",不是进出车厅！";
                    return _resp;
                }
                if (moHall.Mode != EnmModel.Automatic)
                {
                    _resp.Message = "当前车厅-" + moHall.DeviceCode + ",模式不是全自动！";
                    return _resp;
                }
                ICCard iccd = new CWICCard().Find(ic => ic.UserCode == iccode);
                if (iccd == null)
                {
                    _resp.Message = "找不到当前卡信息！ICCode-" + iccode;
                    return _resp;
                }
                if (iccd.Status == EnmICCardStatus.Disposed ||
                    iccd.Status == EnmICCardStatus.Lost)
                {
                    _resp.Message = "卡已挂失或注销！status-" + iccd.Status.ToString();
                    return _resp;
                }
                Location lct = new CWLocation().FindLocation(l => l.ICCode == iccode);
                if (lct == null)
                {
                    _resp.Message = "该卡没有存车！ICCode-" + iccode;
                    return _resp;
                }
                if (lct.Type != EnmLocationType.Normal)
                {
                    _resp.Message = "车位不可用，Type-" + lct.Type.ToString();
                    return _resp;
                }
                if (lct.Status != EnmLocationStatus.Occupy)
                {
                    _resp.Message = "车位正在作业，Status-" + lct.Status.ToString();
                    return _resp;
                }
                ImplementTask itask = motsk.Find(tk => tk.IsComplete == 0 && tk.ICCardCode == iccode);
                if (itask != null)
                {
                    _resp.Message = "正在作业，请稍后！";
                    return _resp;
                }
                WorkTask queue = motsk.FindQueue(qu => qu.ICCardCode == iccode);
                if (queue != null)
                {
                    _resp.Message = "正在作业，请稍后！";
                    return _resp;
                }

                return motsk.TempGetCar(moHall, lct);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return _resp;
        }

        

    }
}
