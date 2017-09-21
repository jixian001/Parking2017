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
        private static int mHallGetCount = 1;

        public CWTaskTransfer()
        {
            motsk = new CWTask();
            try
            {
                string getcount = XMLHelper.GetRootNodeValueByXpath("root", "MaxHallGetCar");
                mHallGetCount = string.IsNullOrEmpty(getcount) ? mHallGetCount : Convert.ToInt32(getcount);
            }
            catch(Exception ex)
            {
                Log log = LogFactory.GetLogger("CWTaskTransfer init");
                log.Error(ex.ToString());
            }
        }

        public CWTaskTransfer(int hallCode, int warehouse) : this()
        {
            moHall = new CWDevice().Find(d => d.Warehouse == warehouse && d.DeviceCode == hallCode);
        }

        /// <summary>
        /// 有车入库
        /// </summary>
        public async Task DealCarEntranceAsync()
        {
            Log log = LogFactory.GetLogger("CWTaskTransfer.DealCarEntrance");
            try
            {
                if (moHall == null)
                {
                    log.Error("车厅设备为空！");
                    return;
                }

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
                ImplementTask itask =await motsk.FindITaskAsync(d => d.DeviceCode == moHall.DeviceCode && d.Warehouse == moHall.Warehouse);
                if (itask != null)
                {
                    //报警- 有故障作业未处理
                    motsk.AddNofication(moHall.Warehouse, moHall.DeviceCode, "39.wav");
                    //强制将其删除
                    motsk.DeleteITask(itask);
                    log.Error("入库时，执行作业内有车厅作业，强制将其删除，ICCard - " + itask.ICCardCode + " platenum - " + itask.PlateNum);
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
                motsk.AddNofication(moHall.Warehouse, moHall.DeviceCode, "18.wav");
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
        public async Task DealICheckCarAsync(int tskID, int distance, string carSize, int weight)
        {
            Log log = LogFactory.GetLogger("CWTaskTransfer.DealICheckCar");
            try
            {
                ImplementTask tsk =await motsk.FindAsync(tskID);
                if (tsk == null)
                {
                    log.Error("tsk 为空, id-" + tsk);
                    return;
                }
                if (tsk.Type == EnmTaskType.TempGet)
                {
                    //注意要刷卡后，将源地址车位与目的地址车位互换下
                    Location tolct =await new CWLocation().FindLocationAsync(lc => lc.Warehouse == tsk.Warehouse && lc.Address == tsk.ToLctAddress);
                    if (tolct != null)
                    {
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
                }
                else
                {
                    motsk.IDealCheckedCar(tsk, moHall.DeviceCode, distance, carSize, weight);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 处理车辆离开
        /// </summary>
        /// <param name="task"></param>
        public async Task DealCarLeaveAsync(int taskid)
        {
            Log log = LogFactory.GetLogger("CWTaskTransfer.DealCarLeave");
            try
            {
                ImplementTask task =await motsk.FindAsync(taskid);
                if (task == null)
                {
                    return;
                }
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
                if (moHall == null)
                {
                    log.Error("moHall 为 空");
                    return;
                }
                int warehouse = moHall.Warehouse;
                int code = moHall.DeviceCode;
                if (moHall.Mode != EnmModel.Automatic)
                {
                    motsk.AddNofication(warehouse, code, "5.wav");
                    return;
                }
                ICCard iccd = new CWICCard().Find(ic => ic.PhysicCode == physiccode);
                if (iccd == null)
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
                    //取物动作
                    if (task.Type == EnmTaskType.TempGet)
                    {
                        if (task.Status == EnmTaskStatus.TempOCarOutWaitforDrive)
                        {
                            //取物处理第一次刷卡
                            motsk.DealISWipeThreeCard(task, iccd.UserCode);
                            return;
                        }

                        if (task.Status == EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                        {
                            motsk.DealISwipedSecondCard(task, iccd.UserCode);
                            return;
                        }
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

                    if (getcarCount > mHallGetCount)
                    {
                        motsk.AddNofication(warehouse, code, "12.wav");
                        return;
                    }
                    //判断是否已缴费
                    if (!this.JudgeIsChargeAndAllowOut(iccd.CustID, lct))
                    {
                        return;
                    }

                    //生成取车作业，加入队列
                    motsk.DealOSwipedCard(moHall, lct);
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

                        if (getcarCount > mHallGetCount)
                        {
                            motsk.AddNofication(warehouse, code, "12.wav");
                            return;
                        }

                        //判断是否已缴费
                        if (!this.JudgeIsChargeAndAllowOut(iccd.CustID, lct))
                        {
                            return;
                        }

                        //生成取车作业，加入队列
                        motsk.DealOSwipedCard(moHall, lct);
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
        public Response ManualGetCar(int warehouse, string locAddress)
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
        /// <param name="iccode">用户凭证，可能是卡号，可能是指纹编号</param>
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
                if (Convert.ToInt32(iccode) < 10000) //是卡号，则判断下，是指纹，就不用判断了
                {
                    #region 判断卡的有效性
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
                    #endregion
                }
                else
                {
                    _resp.Message = "车辆是刷指纹存车的！";
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

        /// <summary>
        /// 固定收费界面，确认出车
        /// </summary>
        /// <param name="lct"></param>
        /// <returns></returns>
        public Response FixGUIGetCar(Location lct)
        {
            Response _resp = new Response();
            Log log = LogFactory.GetLogger("CWTaskTransfer.FixGUIGetCar");
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
                string iccode = lct.ICCode;
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

                return motsk.ManualGetCar(moHall, lct);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return _resp;
        }

        /// <summary>
        /// 处理指纹信息上报
        /// 指纹存车，只需采集一次就下发（1，9）
        /// </summary>
        /// <param name="psTZ"></param>
        /// <returns></returns>
        public async Task<Response> DealFingerPrintMessageAsync(byte[] psTZ)
        {           
            Log log = LogFactory.GetLogger("CWTasksfer.DealFingerPrintMessage");
            Response resp = new Response();
            if (moHall == null)
            {
                resp.Message = "找不到车厅设备！";
                return resp;
            }
            int warehouse = moHall.Warehouse;
            int code = moHall.DeviceCode;
            int iLevel = 3;

            #region 由于不需要强制比对注册指纹库，这段暂不用
            //CWFingerPrint fingerprint = new CWFingerPrint();           
            //List<FingerPrint> FingerList = fingerprint.FindList(p=>true).ToList();

            //foreach (FingerPrint fp in FingerList)
            //{              
            //    byte[] orig = FPrintBase64.Base64FingerDataToHex(fp.FingerInfo);
            //    if (orig == null)
            //    {
            //        log.Debug("指纹-" + fp.FingerInfo + " ,转化为Byte失败！");
            //    }

            //    byte[] psMB = orig;
            //    int nback = FiPrintMatch.FPIMatch(psMB, psTZ, iLevel);               
            //    //比对成功
            //    if (nback == 0)
            //    {
            //        log.Debug("指纹对比，成功，SN- " + fp.SN_Number);                   
            //        print = fp;
            //        break;
            //    }
            //    else
            //    {
            //        log.Warn("指纹对比失败！");
            //    }              
            //}
            //if (print == null)
            //{
            //    motsk.AddNofication(warehouse, code, "80.wav");
            //    resp.Message = "指纹库内未找到匹配模板";
            //    log.Debug("指纹对比，失败，找不到对应模板！");
            //    return resp;
            //}
            #endregion

            CWSaveProof cwsaveproof = new CWSaveProof();
            #region 先判断存车指纹库内是否存在匹配指纹，如果存在，则提示当前指纹已存车
            SaveCertificate sproof = null;
            List<SaveCertificate> proofLst =await cwsaveproof.FindListAsync(p => p.IsFingerPrint == 1);
            foreach (SaveCertificate cert in proofLst)
            {
                byte[] orig = FPrintBase64.Base64FingerDataToHex(cert.Proof);
                if (orig == null)
                {
                    log.Debug("指纹 - " + cert.SNO + " ,转化为Byte失败！");
                    continue;
                }
                byte[] psMB = orig;
                int nback = FiPrintMatch.FPIMatch(psMB, psTZ, iLevel);
                //比对成功
                if (nback == 0)
                {
                    log.Debug("指纹对比，成功，SN- " + cert.SNO);
                    sproof = cert;
                    break;
                }
                else
                {
                    log.Warn("匹配 - " + cert.SNO + " 失败！");
                }
            }
            #endregion

            string SNO = "CCC";
            if (sproof != null)
            {
                SNO = sproof.SNO.ToString();
            }

            ImplementTask task =await motsk.FindAsync(tk => tk.ICCardCode == SNO && tk.IsComplete == 0);
            if (task != null)
            {
                if (task.HallCode != moHall.DeviceCode)
                {
                    motsk.AddNofication(warehouse, code, "8.wav");
                    resp.Message = "正在作业，请勿重复";
                    return resp;
                }

                //取物动作
                if (task.Type == EnmTaskType.TempGet)
                {
                    if (task.Status == EnmTaskStatus.TempOCarOutWaitforDrive)
                    {
                        //取物处理第一次刷卡
                        motsk.DealISWipeThreeCard(task, SNO);

                        //指纹只刷一次
                        motsk.DealISwipedSecondCard(task, SNO);
                        resp.Code = 1;
                        resp.Message = "下发(1,9)";
                        return resp;
                    }
                }

                if (task.Status != EnmTaskStatus.ICarInWaitFirstSwipeCard)
                {
                    motsk.AddNofication(warehouse, code, "9.wav");
                    resp.Message = "正在作业";
                    return resp;
                }
            }
            WorkTask queue =await motsk.FindQueueAsync(qu => qu.ICCardCode == SNO);
            if (queue != null)
            {
                motsk.AddNofication(warehouse, code, "9.wav");
                resp.Message = "正在作业";
                return resp;
            }
            int getcarCount = motsk.GetHallGetCarCount(warehouse, code);
            Location lct =await new CWLocation().FindLocationAsync(lt => lt.ICCode == SNO && lt.Type == EnmLocationType.Normal);

            if (moHall.Mode != EnmModel.Automatic)
            {
                motsk.AddNofication(warehouse, code, "5.wav");
                resp.Message = "已转为人工作业";
                return resp;
            }

            #region 进车厅
            if (moHall.HallType == EnmHallType.Entrance)
            {
                if (lct != null)
                {
                    //请到出车厅刷卡取车
                    motsk.AddNofication(warehouse, code, "11.wav");
                    resp.Message = "已存车，请到出车厅取车";
                    return resp;
                }
                if (moHall.TaskID == 0)
                {
                    //车厅无车，不能存车
                    motsk.AddNofication(warehouse, code, "10.wav");
                    resp.Message = "车厅无车，不能存车";
                    return resp;
                }
                ImplementTask tsk =await motsk.FindAsync(moHall.TaskID);
                if (tsk == null)
                {
                    log.Error("依车厅TaskID找不到作业信息，TaskID-" + moHall.TaskID + "  hallCode-" + moHall.DeviceCode);
                    //系统故障
                    motsk.AddNofication(warehouse, code, "20.wav");
                    resp.Message = "系统故障";
                    return resp;
                }
                if (tsk.Status == EnmTaskStatus.ICarInWaitFirstSwipeCard)
                {
                    //获取最大编码，会赋给相应指纹的
                    SNO = cwsaveproof.GetMaxSNO().ToString();

                    motsk.DealISwipedSecondCard(tsk, SNO);
                    resp.Code = 1;
                    resp.Message = "指纹识别成功";
                    string brand = "";
                    #region 获取车厅内识别车牌
                    PlateMappingDev device_plate = new CWPlate().FindPlate(moHall.Warehouse, moHall.DeviceCode);
                    if (device_plate != null)
                    {
                        if (!string.IsNullOrEmpty(device_plate.PlateNum))
                        {
                            brand = device_plate.PlateNum;
                        }
                    }
                    #endregion

                    ZhiWenResult result = new ZhiWenResult
                    {
                        IsTakeCar = 0,
                        PlateNum = brand,
                        Sound = ""
                    };
                    resp.Data = result;

                    #region 允许存车时，才将当前指纹信息保存到指纹库中,先查询注册指纹库内是否有匹配模板
                    SaveCertificate scert = new SaveCertificate();

                    FingerPrint print = null;
                    CWFingerPrint fingerprint = new CWFingerPrint();
                    List<FingerPrint> FingerList = fingerprint.FindList(p => true).ToList();
                    foreach (FingerPrint fp in FingerList)
                    {
                        byte[] orig = FPrintBase64.Base64FingerDataToHex(fp.FingerInfo);
                        if (orig == null)
                        {
                            log.Debug("指纹-" + fp.FingerInfo + " ,转化为Byte失败！");
                        }

                        byte[] psMB = orig;
                        int nback = FiPrintMatch.FPIMatch(psMB, psTZ, iLevel);
                        //比对成功
                        if (nback == 0)
                        {
                            log.Debug("下发（1，9），指纹对比，成功，SN- " + fp.SN_Number);
                            print = fp;
                            break;
                        }
                    }
                    //没有注册指纹
                    if (print == null)
                    {
                        log.Debug("下发（1，9）后，指纹库内没有对应模板！");
                        //保存当前指纹
                        string base64Print = FPrintBase64.FingerDataBytesToBase64Str(psTZ);
                        scert.Proof = base64Print;
                        scert.CustID = 0;
                    }
                    else
                    {
                        scert.Proof = print.FingerInfo;
                        scert.CustID = print.CustID;
                    }
                    scert.IsFingerPrint = 1;
                    scert.SNO = Convert.ToInt32(SNO);
                    //添加凭证到存车指纹库中
                    Response respe = cwsaveproof.Add(scert);
                    if (respe.Code == 1)
                    {
                        log.Debug("存车按指纹，保存至存车指纹库成功，SNO - " + SNO);
                    }

                    //在存车指纹库中，存在其记录，则将其删除
                    if (sproof != null)
                    {
                        cwsaveproof.Delete(sproof.ID);
                    }

                    #endregion
                }
                else if (tsk.ICCardCode == SNO)
                {
                    motsk.AddNofication(warehouse, code, "9.wav");
                    resp.Message = "正在作业，请稍后";
                }
                else
                {
                    //没有存车
                    motsk.AddNofication(warehouse, code, "14.wav");
                    resp.Message = "该卡没有存车";
                    return resp;
                }
                return resp;
            }
            #endregion
            #region 出车厅
            else if (moHall.HallType == EnmHallType.Exit)
            {
                if (sproof == null || lct == null)
                {
                    //该卡没有存车
                    motsk.AddNofication(warehouse, code, "14.wav");
                    resp.Message = "该卡没有存车";
                    return resp;
                }
                if (lct.Status != EnmLocationStatus.Occupy)
                {
                    //正在作业，
                    motsk.AddNofication(warehouse, code, "65.wav");
                    resp.Message = "车位状态不是占用状态";
                    return resp;
                }
                if (getcarCount >= mHallGetCount)
                {
                    motsk.AddNofication(warehouse, code, "12.wav");
                    resp.Message = "取车队列已满，请稍后取车";
                    return resp;
                }

                #region 收费判断               
                //判断是否已缴费
                if (!this.JudgeIsChargeAndAllowOut(sproof.CustID, lct))
                {
                    resp.Message = "请确认缴费后出车";
                    return resp;
                }
                #endregion

                //生成取车作业，加入队列
                motsk.DealOSwipedCard(moHall, lct);
                resp.Code = 1;
                resp.Message = "已经加入取车队列，请稍后";

                ZhiWenResult result = new ZhiWenResult
                {
                    IsTakeCar = 1,
                    PlateNum = lct.PlateNum,
                    Sound = ""
                };
                resp.Data = result;

                return resp;
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
                        resp.Message = "车厅无车，不能存车";
                        return resp;
                    }
                    ImplementTask tsk = motsk.Find(moHall.TaskID);
                    if (tsk == null)
                    {
                        log.Error("依车厅TaskID找不到作业信息，TaskID-" + moHall.TaskID + "  hallCode-" + moHall.DeviceCode);
                        //系统故障
                        motsk.AddNofication(warehouse, code, "20.wav");
                        resp.Message = "系统故障";
                        return resp;
                    }
                    if (tsk.Status == EnmTaskStatus.ICarInWaitFirstSwipeCard)
                    {
                        SNO = cwsaveproof.GetMaxSNO().ToString();

                        motsk.DealISwipedSecondCard(tsk, SNO);
                        resp.Code = 1;
                        resp.Message = "指纹识别成功";
                        string brand = "";
                        #region 获取车厅内识别车牌
                        PlateMappingDev device_plate = new CWPlate().FindPlate(moHall.Warehouse, moHall.DeviceCode);
                        if (device_plate != null)
                        {
                            if (!string.IsNullOrEmpty(device_plate.PlateNum) &&
                                DateTime.Compare(DateTime.Now, device_plate.InDate.AddMinutes(8)) < 0)
                            {
                                brand = device_plate.PlateNum;
                            }
                        }
                        #endregion

                        ZhiWenResult result = new ZhiWenResult
                        {
                            IsTakeCar = 0,
                            PlateNum = brand,
                            Sound = ""
                        };
                        resp.Data = result;
                        #region 允许存车时，才将当前指纹信息保存到指纹库中,先查询注册指纹库内是否有匹配模板
                        SaveCertificate scert = new SaveCertificate();

                        FingerPrint print = null;
                        CWFingerPrint fingerprint = new CWFingerPrint();
                        List<FingerPrint> FingerList = fingerprint.FindList(p => true).ToList();
                        foreach (FingerPrint fp in FingerList)
                        {
                            byte[] orig = FPrintBase64.Base64FingerDataToHex(fp.FingerInfo);
                            if (orig == null)
                            {
                                log.Debug("指纹-" + fp.FingerInfo + " ,转化为Byte失败！");
                            }

                            byte[] psMB = orig;
                            int nback = FiPrintMatch.FPIMatch(psMB, psTZ, iLevel);
                            //比对成功
                            if (nback == 0)
                            {
                                log.Debug("下发（1，9），指纹对比，成功，SN- " + fp.SN_Number);
                                print = fp;
                                break;
                            }
                        }
                        //没有注册指纹
                        if (print == null)
                        {
                            log.Debug("下发（1，9）后，指纹库内没有对应模板！");
                            //保存当前指纹
                            string base64Print = FPrintBase64.FingerDataBytesToBase64Str(psTZ);
                            scert.Proof = base64Print;
                        }
                        else
                        {
                            scert.Proof = print.FingerInfo;
                            scert.CustID = print.CustID;
                        }
                        scert.IsFingerPrint = 1;
                        scert.SNO = Convert.ToInt32(SNO);
                        //添加凭证到存车指纹库中
                        Response respe = cwsaveproof.Add(scert);
                        if (respe.Code == 1)
                        {
                            log.Debug("存车按指纹，保存至存车指纹库成功，SNO - " + SNO);
                        }

                        //在存车指纹库中，存在其记录，则将其删除
                        if (sproof != null)
                        {
                            cwsaveproof.Delete(sproof.ID);
                        }
                        #endregion
                    }
                    else if (tsk.ICCardCode == SNO)
                    {
                        motsk.AddNofication(warehouse, code, "9.wav");
                        resp.Message = "正在作业，请稍后";
                    }
                    else
                    {
                        //没有存车
                        motsk.AddNofication(warehouse, code, "14.wav");
                        resp.Message = "该卡没有存车";
                        return resp;
                    }
                    return resp;
                }
                #endregion
                #region 取车
                else
                {
                    if (sproof == null)
                    {
                        //该卡没有存车
                        motsk.AddNofication(warehouse, code, "14.wav");
                        resp.Message = "该卡没有存车";
                        return resp;
                    }
                    if (lct.Status != EnmLocationStatus.Occupy)
                    {
                        //正在作业，
                        motsk.AddNofication(warehouse, code, "65.wav");
                        resp.Message = "车位状态不是占用状态";
                        return resp;
                    }
                    if (getcarCount >= mHallGetCount)
                    {
                        motsk.AddNofication(warehouse, code, "12.wav");
                        resp.Message = "取车队列已满，请稍后取车";
                        return resp;
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
                                    resp.Message = "请等待存车刷卡时，再进行刷卡取车";
                                    return resp;
                                }
                            }
                        }
                    }
                    #endregion

                    #region 缴费出车判断                   
                    //判断是否已缴费
                    if (!this.JudgeIsChargeAndAllowOut(sproof.CustID, lct))
                    {
                        resp.Message = "请确认缴费后出车";
                        return resp;
                    }
                    #endregion

                    //生成取车作业，加入队列
                    motsk.DealOSwipedCard(moHall, lct);
                    resp.Code = 1;
                    resp.Message = "已经加入取车队列，请稍后";

                    ZhiWenResult result = new ZhiWenResult
                    {
                        IsTakeCar = 1,
                        PlateNum = lct.PlateNum,
                        Sound = ""
                    };
                    resp.Data = result;

                    return resp;
                }
                #endregion
            }
            #endregion

            return resp;
        }

        /// <summary>
        /// 处理指纹机刷卡动作
        /// </summary>
        /// <param name="physiccode"></param>
        public async Task<Response> DealFingerICCardMessageAsync(string physiccode)
        {           
            Log log = LogFactory.GetLogger("CWTaskTransfer.DealFingerICCardMessage");
            Response resp = new Response();
            try
            {
                if (moHall == null)
                {
                    resp.Message = "找不到车厅设备！";
                    return resp;
                }
                int warehouse = moHall.Warehouse;
                int code = moHall.DeviceCode;

                if (moHall.Mode != EnmModel.Automatic)
                {
                    motsk.AddNofication(warehouse, code, "5.wav");
                    resp.Message = "已转为人工作业";
                    return resp;
                }
                #region
                //ICCard iccd = new CWICCard().Find(ic => ic.PhysicCode == physiccode);
                //if (iccd == null)
                //{
                //    motsk.AddNofication(warehouse, code, "6.wav");
                //    resp.Message = "不是本系统用户";
                //    return resp;
                //}
                //if (iccd.Status == EnmICCardStatus.Lost ||
                //    iccd.Status == EnmICCardStatus.Disposed)
                //{
                //    motsk.AddNofication(warehouse, code, "7.wav");
                //    resp.Message = "卡已注销或挂失";
                //    return resp;
                //}
                #endregion
                CWSaveProof cwsaveproof = new CWSaveProof();
                #region 先判断存车指纹库内是否存在物理卡号
                SaveCertificate sproof = null;
                List<SaveCertificate> proofLst =await cwsaveproof.FindListAsync(p => p.IsFingerPrint == 2);
                foreach (SaveCertificate cert in proofLst)
                {
                    int nback = string.Compare(physiccode, cert.Proof);
                    //比对成功
                    if (nback == 0)
                    {
                        log.Debug("物理卡号查找成功，SN- " + cert.SNO);
                        sproof = cert;
                        break;
                    }
                }
                #endregion
                ICCard iccard = null;
                string SNO = "CCC";
                if (sproof != null)
                {
                    SNO = sproof.SNO.ToString();
                }
                else
                {
                    //如果没有的，则看看是否已注册过，如果注册过，则取当前注册卡号
                    //如果没有，则分配用户卡号
                    iccard =await new CWICCard().FindAsync(ic => ic.PhysicCode == physiccode);
                    if (iccard != null)
                    {
                        SNO = iccard.UserCode;
                    }
                    else
                    {
                        SNO = cwsaveproof.GetMaxSNO().ToString();
                    }
                }
                if (SNO.Length < 4)
                {
                    SNO = SNO.PadLeft(4, '0');
                }

                ImplementTask task =await motsk.FindAsync(tk => tk.ICCardCode == SNO && tk.IsComplete == 0);
                if (task != null)
                {
                    if (task.HallCode != moHall.DeviceCode)
                    {
                        motsk.AddNofication(warehouse, code, "8.wav");
                        resp.Message = "正在别的车厅作业";
                        return resp;
                    }

                    //取物动作
                    if (task.Type == EnmTaskType.TempGet)
                    {
                        if (task.Status == EnmTaskStatus.TempOCarOutWaitforDrive)
                        {
                            //取物处理刷卡,第三次刷卡
                            motsk.DealISWipeThreeCard(task, SNO);
                            resp.Code = 1;
                            resp.Message = "刷卡成功";
                            return resp;
                        }

                        if (task.Status == EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                        {
                            motsk.DealISwipedSecondCard(task, SNO);
                            resp.Code = 1;
                            resp.Message = "下发(1,9)";
                            return resp;
                        }
                    }

                    if (task.Status != EnmTaskStatus.ICarInWaitFirstSwipeCard &&
                        task.Status != EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                    {
                        motsk.AddNofication(warehouse, code, "9.wav");
                        resp.Message = "正在作业";
                        return resp;
                    }
                }
                WorkTask queue =await motsk.FindQueueAsync(qu => qu.ICCardCode == SNO);
                if (queue != null)
                {
                    motsk.AddNofication(warehouse, code, "9.wav");
                    resp.Message = "正在作业";
                    return resp;
                }
                int getcarCount = motsk.GetHallGetCarCount(warehouse, code);
                Location lct =await new CWLocation().FindLocationAsync(lt => lt.ICCode == SNO && lt.Type == EnmLocationType.Normal);

                #region 进车厅
                if (moHall.HallType == EnmHallType.Entrance)
                {
                    if (lct != null)
                    {
                        //请到出车厅刷卡取车
                        motsk.AddNofication(warehouse, code, "11.wav");
                        resp.Message = "请到出车厅刷卡取车";
                        return resp;
                    }
                    //没有取车位，但在存车指纹库内有相关记录，则删除
                    if (sproof != null)
                    {
                        cwsaveproof.Delete(sproof.ID);
                        resp.Message = "系统异常，请重新刷卡！";
                        log.Debug("进车时，找不到车位凭证，删除-" + sproof.SNO);
                        return resp;
                    }
                    if (moHall.TaskID == 0)
                    {
                        //车厅无车，不能存车
                        motsk.AddNofication(warehouse, code, "10.wav");
                        resp.Message = "车厅无车，不能存车";
                        return resp;
                    }
                    ImplementTask tsk =await motsk.FindAsync(moHall.TaskID);
                    if (tsk == null)
                    {
                        log.Error("依车厅TaskID找不到作业信息，TaskID-" + moHall.TaskID + "  hallCode-" + moHall.DeviceCode);
                        //系统故障
                        motsk.AddNofication(warehouse, code, "20.wav");
                        resp.Message = "系统故障";
                        return resp;
                    }
                    if (tsk.Status == EnmTaskStatus.ICarInWaitFirstSwipeCard)
                    {
                        #region
                        //处理刷第一次卡
                        //motsk.DealISwipedFirstCard(tsk, SNO);
                        //resp.Code = 1;
                        //resp.Message = "刷第一次卡成功，请再次刷卡";
#endregion

                        #region 允许存车时，才将当前指纹信息保存到指纹库中,先查询注册指纹库内是否有匹配模板
                        SaveCertificate scert = new SaveCertificate();
                        scert.IsFingerPrint = 2;
                        scert.Proof = physiccode;
                        if (iccard != null)
                        {
                            //是否有绑定用户
                            scert.CustID = iccard.CustID;
                        }
                        scert.SNO = Convert.ToInt32(SNO);
                        //添加凭证到存车指纹库中
                        Response respe = cwsaveproof.Add(scert);
                        if (respe.Code == 1)
                        {
                            log.Debug("存车按指纹，保存至存车卡号成功，SNO - " + SNO);
                        }
                        #endregion
                        #region
                        //    ZhiWenResult result = new ZhiWenResult {
                        //        IsTakeCar = 0,
                        //        PlateNum = "",
                        //        Sound = ""
                        //    };
                        //    resp.Data = result;

                        //    return resp;
                        //}
                        //else if (tsk.Status == EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                        //{
                        //    //处理刷第二次卡
                        //    if (tsk.ICCardCode != SNO)
                        //    {
                        //        motsk.AddNofication(warehouse, code, "64.wav");
                        //        resp.Message = "与第一次刷卡不一致";
                        //        return resp;
                        //    }
#endregion
                        motsk.DealISwipedSecondCard(tsk, SNO);
                        resp.Code = 1;
                        resp.Message = "刷卡成功，请稍后！";
                        string brand = "";
                        #region 获取车厅内识别车牌
                        PlateMappingDev device_plate =new CWPlate().FindPlate(moHall.Warehouse, moHall.DeviceCode);
                        if (device_plate != null)
                        {
                            if (!string.IsNullOrEmpty(device_plate.PlateNum))
                            {
                                brand = device_plate.PlateNum;
                            }
                        }
                        #endregion
                        ZhiWenResult result = new ZhiWenResult
                        {
                            IsTakeCar = 0,
                            PlateNum = brand,
                            Sound = ""
                        };
                        resp.Data = result;
                        return resp;
                    }
                    else if (tsk.ICCardCode == SNO)
                    {
                        motsk.AddNofication(warehouse, code, "9.wav");
                        resp.Message = "正在作业，请稍后";
                    }
                    else
                    {
                        //没有存车
                        motsk.AddNofication(warehouse, code, "14.wav");
                        resp.Message = "该卡没有存车";
                        return resp;
                    }
                }
                #endregion
                #region 出车厅
                else if (moHall.HallType == EnmHallType.Exit)
                {
                    bool isAltered = false;
                    if (lct == null || sproof == null)
                    {
                        #region 如果依物理卡号查找不到记录，表示之前存车的就不是使用卡号，可能使用指纹，这时也允许注册用户依车牌号出车
                        if (iccard != null)
                        {
                            Customer cc =await new CWICCard().FindCustAsync(iccard.CustID);
                            if (cc != null && !string.IsNullOrEmpty(cc.PlateNum))
                            {
                                lct =await new CWLocation().FindLocationAsync(l => l.PlateNum == cc.PlateNum);

                                sproof = new SaveCertificate();
                                sproof.CustID = cc.ID;
                                isAltered = true;
                            }
                        }
                        #endregion

                        if (lct == null)
                        {
                            //该卡没有存车
                            motsk.AddNofication(warehouse, code, "14.wav");
                            resp.Message = "该卡没有存车";
                            return resp;
                        }
                    }
                    if (lct.Status != EnmLocationStatus.Occupy)
                    {
                        //正在作业，
                        motsk.AddNofication(warehouse, code, "65.wav");
                        resp.Message = "车位状态不是占用";
                        return resp;
                    }
                    if (getcarCount >= mHallGetCount)
                    {
                        motsk.AddNofication(warehouse, code, "12.wav");
                        resp.Message = "取车队列已满，请稍后取车";
                        return resp;
                    }
                    #region                   
                    //判断是否已缴费
                    if (!this.JudgeIsChargeAndAllowOut(sproof.CustID, lct))
                    {
                        resp.Message = "请确认缴费后出车";
                        return resp;
                    }
                    #endregion
                    //生成取车作业，加入队列
                    motsk.DealOSwipedCard(moHall, lct);
                    resp.Code = 1;
                    resp.Message = "已加入取车队列，请稍后";
                    ZhiWenResult result = new ZhiWenResult
                    {
                        IsTakeCar = 1,
                        PlateNum = lct.PlateNum,
                        Sound = ""
                    };
                    resp.Data = result;
                    //是异常的，则将当前凭证加入存车指纹库
                    if (isAltered)
                    {
                        //将当前凭证加入
                        SaveCertificate scert = new SaveCertificate
                        {
                            IsFingerPrint = 2,
                            Proof = iccard.PhysicCode,
                            SNO = Convert.ToInt32(lct.ICCode),
                            CustID = sproof.CustID
                        };
                        cwsaveproof.Add(scert);
                    }

                    return resp;
                }
                #endregion
                #region 进出车厅
                else if (moHall.HallType == EnmHallType.EnterOrExit)
                {
                    bool isAltered = false;
                    #region 在存车指纹库内查找不到记录，表示之前存车的就不是使用卡号，可能使用指纹，这时也允许注册用户依车牌号出车
                    if (iccard != null)
                    {
                        Customer cc =await new CWICCard().FindCustAsync(iccard.CustID);
                        if (cc != null && !string.IsNullOrEmpty(cc.PlateNum))
                        {
                            sproof = new SaveCertificate();
                            sproof.CustID = cc.ID;
                            isAltered = true;

                            lct =await new CWLocation().FindLocationAsync(l => l.PlateNum == cc.PlateNum);
                        }
                    }
                    #endregion

                    #region 存车
                    if (lct == null) //是进车状态
                    {
                        //没有取车位，但在存车指纹库内有相关记录，则删除
                        if (sproof != null)
                        {
                            cwsaveproof.Delete(sproof.ID);
                            resp.Message = "系统异常，请重新刷卡！";
                            log.Debug("进车时，找不到车位凭证，删除-" + sproof.SNO);
                            return resp;
                        }

                        if (moHall.TaskID == 0)
                        {
                            //车厅无车，不能存车
                            motsk.AddNofication(warehouse, code, "10.wav");
                            resp.Message = "车厅无车，不能存车";
                            return resp;
                        }
                        ImplementTask tsk = motsk.Find(moHall.TaskID);
                        if (tsk == null)
                        {
                            log.Error("依车厅TaskID找不到作业信息，TaskID-" + moHall.TaskID + "  hallCode-" + moHall.DeviceCode);
                            //系统故障
                            motsk.AddNofication(warehouse, code, "20.wav");
                            resp.Message = "系统故障";
                            return resp;
                        }
                        if (tsk.Status == EnmTaskStatus.ICarInWaitFirstSwipeCard)
                        {
                            #region
                            //处理刷第一次卡
                            //motsk.DealISwipedFirstCard(tsk, SNO);
                            //resp.Code = 1;
                            //resp.Message = "刷一次卡成功，请再次刷卡";
#endregion

                            #region 允许存车时，才将当前指纹信息保存到指纹库中,先查询注册指纹库内是否有匹配模板
                            SaveCertificate scert = new SaveCertificate();
                            scert.IsFingerPrint = 2;
                            scert.Proof = physiccode;
                            if (iccard != null)
                            {
                                //是否有绑定用户
                                scert.CustID = iccard.CustID;
                            }
                            scert.SNO = Convert.ToInt32(SNO);
                            //添加凭证到存车指纹库中
                            Response respe = cwsaveproof.Add(scert);
                            if (respe.Code == 1)
                            {
                                log.Debug("存车刷卡，保存至刷卡信息成功，SNO - " + SNO);
                            }
                            else
                            {
                                log.Debug("存车刷卡，保存失败，SNO - " + SNO);
                            }
                            #endregion
                            #region
                            //    return resp;
                            //}
                            //else if (tsk.Status == EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                            //{
                            //    //处理刷第二次卡
                            //    if (tsk.ICCardCode != SNO)
                            //    {
                            //        motsk.AddNofication(warehouse, code, "64.wav");
                            //        resp.Message = "与第一次刷卡不一致";
                            //        return resp;
                            //    }
#endregion
                            motsk.DealISwipedSecondCard(tsk, SNO);
                            resp.Code = 1;
                            resp.Message = "刷卡成功,请稍后再离开！";
                            string brand = "";
                            #region 获取车厅内识别车牌
                            PlateMappingDev device_plate = new CWPlate().FindPlate(moHall.Warehouse, moHall.DeviceCode);
                            if (device_plate != null)
                            {
                                if (!string.IsNullOrEmpty(device_plate.PlateNum))
                                {
                                    brand = device_plate.PlateNum;
                                }
                            }
                            #endregion
                            ZhiWenResult result = new ZhiWenResult
                            {
                                IsTakeCar = 0,
                                PlateNum = brand,
                                Sound = ""
                            };
                            resp.Data = result;

                            return resp;
                        }
                        else if (tsk.ICCardCode == SNO)
                        {
                            motsk.AddNofication(warehouse, code, "9.wav");
                            resp.Message = "正在作业，请稍后";
                        }
                        else
                        {
                            //没有存车
                            motsk.AddNofication(warehouse, code, "14.wav");
                            resp.Message = "该卡没有存车";
                            return resp;
                        }
                    }
                    #endregion
                    #region 取车
                    else
                    {
                        if (lct.Status != EnmLocationStatus.Occupy)
                        {
                            //正在作业，
                            motsk.AddNofication(warehouse, code, "65.wav");
                            resp.Message = "车位状态不是占用";
                            return resp;
                        }
                        log.Debug("mHallGetCount - " + mHallGetCount + " ,当前车厅的队列 getcarCount - " + getcarCount);
                        if (getcarCount >= mHallGetCount)
                        {
                            motsk.AddNofication(warehouse, code, "12.wav");
                            resp.Message = "取车队列已满，请稍后取车";
                            return resp;
                        }

                        #region 如果车厅在存车，等存车刷卡了，取车才允许
                        if (moHall.TaskID != 0)
                        {
                            ImplementTask itask =await motsk.FindAsync(moHall.TaskID);
                            if (itask != null)
                            {
                                if (itask.Type == EnmTaskType.SaveCar)
                                {
                                    if (itask.Status == EnmTaskStatus.ICarInWaitFirstSwipeCard ||
                                        itask.Status == EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                                    {
                                        motsk.AddNofication(warehouse, code, "68.wav");
                                        resp.Message = "请等待存车刷卡时，再进行刷卡取车";
                                        return resp;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region                       
                        //判断是否已缴费
                        if (!this.JudgeIsChargeAndAllowOut(sproof.CustID, lct))
                        {
                            resp.Message = "请确认缴费后出车";
                            return resp;
                        }
                        #endregion

                        //生成取车作业，加入队列
                        motsk.DealOSwipedCard(moHall, lct);
                        resp.Code = 1;
                        resp.Message = "已加入取车队列，请稍后";
                        ZhiWenResult result = new ZhiWenResult
                        {
                            IsTakeCar = 1,
                            PlateNum = lct.PlateNum,
                            Sound = ""
                        };
                        resp.Data = result;

                        //是异常的，则将当前凭证加入存车指纹库
                        if (isAltered)
                        {
                            //将当前凭证加入
                            SaveCertificate scert = new SaveCertificate
                            {
                                IsFingerPrint = 2,
                                Proof = iccard.PhysicCode,
                                SNO = Convert.ToInt32(lct.ICCode),
                                CustID = sproof.CustID
                            };
                            cwsaveproof.Add(scert);
                            //删除原来的
                            string save_cert = lct.ICCode;
                            if (!string.IsNullOrEmpty(save_cert))
                            {
                                int csno = Convert.ToInt32(save_cert);
                                SaveCertificate savec =await cwsaveproof.FindAsync(d => d.SNO == csno);
                                if (savec != null)
                                {
                                    cwsaveproof.Delete(savec.ID);
                                }
                            }
                        }
                        return resp;
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Message = "系统异常";
            }
            return resp;
        }

        /// <summary>
        /// 界面临时用户缴费出车
        /// </summary>
        /// <returns></returns>
        public Response OCreateTempUserOfOutCar(Location loc)
        {
            Log log = LogFactory.GetLogger("CWTaskTransfer.OCreateTempUserOfOutCar");
            Response resp = new Response();
            try
            {
                #region
                //判断车厅
                if (moHall.HallType == EnmHallType.Entrance)
                {
                    resp.Message = "当前车厅是进车厅！";
                    return resp;
                }
                //判断车位
                if (loc.Type != EnmLocationType.Normal)
                {
                    resp.Message = "取车车位已被禁用";
                    return resp;
                }
                if (loc.Status != EnmLocationStatus.Occupy)
                {
                    resp.Message = "取车车位不是占用状态";
                    return resp;
                }
                resp = motsk.DealOSwipedCard(moHall, loc);
                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return resp;
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X") + " ";
                }
            }
            return returnStr;
        }

        /// <summary>
        /// 判断是否缴费，
        /// </summary>
        /// <returns></returns>
        private bool JudgeIsChargeAndAllowOut(int custID, Location loc)
        {
            Log log = LogFactory.GetLogger("JudgeIsChargeAndAllowOut");
            try
            {
                int warehouse = moHall.Warehouse;
                int code = moHall.DeviceCode;

                string isCharge = XMLHelper.GetRootNodeValueByXpath("root", "ChargeEnable");
                bool isChargeEnable = string.IsNullOrEmpty(isCharge) ? false : (isCharge == "1" ? true : false);
                if (isChargeEnable)
                {
                    #region 
                    Customer cust = new CWICCard().FindCust(custID);
                    if (cust == null)
                    {
                        if (!string.IsNullOrEmpty(loc.PlateNum))
                        {
                            cust = new CWICCard().FindCust(c => c.PlateNum == loc.PlateNum);
                            if (cust == null)
                            {
                                cust = new Customer();
                                cust.Type = EnmICCardType.Temp;
                                cust.UserName = "";
                            }
                        }
                    }

                    if (cust.Type == EnmICCardType.Temp)
                    {
                        int oouttimeofmin = 15;
                        string onlineChgOutTime = XMLHelper.GetRootNodeValueByXpath("root", "OnlineChgOutTime");
                        if (!string.IsNullOrEmpty(onlineChgOutTime))
                        {
                            oouttimeofmin = Convert.ToInt32(onlineChgOutTime);
                        }
                        CWRemoteServer cwcrd = new CWRemoteServer();
                        RemotePayFeeRcd rcd = cwcrd.Find(lc => lc.Warehouse == warehouse && lc.LocAddress == loc.Address);
                        if (rcd == null)
                        {
                            motsk.AddNofication(warehouse, code, "29.wav");
                            return false;
                        }

                        if (DateTime.Compare(DateTime.Now, rcd.RecordDtime.AddMinutes(oouttimeofmin)) > 0)
                        {
                            //网上缴费，允许出车时间已过期,请重新缴费
                            motsk.AddNofication(warehouse, code, "29.wav");

                            //再次修改车辆的存车时间
                            loc.InDate = rcd.RecordDtime.AddMinutes(oouttimeofmin);
                            new CWLocation().UpdateLocation(loc);

                            //同时删除其记录
                            cwcrd.Delete(rcd.ID);

                            return false;
                        }

                        //允许出车后，删除保留的记录
                        cwcrd.Delete(rcd.ID);

                        return true;
                    }
                    else if (cust.Type == EnmICCardType.Periodical || cust.Type == EnmICCardType.FixedLocation)
                    {
                        if (DateTime.Compare(cust.Deadline, DateTime.Now) < 0)
                        {
                            motsk.AddNofication(warehouse, code, "31.wav");
                            return false;
                        }
                        if (DateTime.Compare(cust.Deadline.AddDays(-2), DateTime.Now) < 0)
                        {
                            motsk.AddNofication(warehouse, code, "67.wav");
                        }
                        else if (DateTime.Compare(cust.Deadline.AddDays(-1), DateTime.Now) < 0)
                        {
                            motsk.AddNofication(warehouse, code, "66.wav");
                        }
                        return true;
                    }
                    #endregion
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return false;
            }
        }

    }
}
