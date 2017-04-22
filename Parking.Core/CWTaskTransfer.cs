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

        public CWTaskTransfer(int code,int warehouse) :this()
        {
            moHall = new CWDevice().SelectSMG(code, warehouse);
        }

        /// <summary>
        /// 有车入库
        /// </summary>
        public void DealCarEntrance()
        {
            if (moHall.Mode != EnmModel.Automatic)
            {
                //模式不是全自动
                motsk.AddNofication(moHall.Warehouse,moHall.DeviceCode,"5.wav");
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

        /// <summary>
        /// 外形检测上报处理(1001,101)
        /// </summary>
        /// <param name="tsk"></param>
        /// <param name="distance"></param>
        /// <param name="carSize"></param>
        public void DealICheckCar(ImplementTask tsk,int distance,string carSize,int weight)
        {
            if (tsk.Type == EnmTaskType.TempGet)
            {
                //注意要刷卡后，将源地址车位与目的地址车位互换下
                Location tolct = new CWLocation().FindLocation(lc => lc.Warehouse == tsk.Warehouse && lc.Address == tsk.ToLctAddress);
                if (tolct.Status != EnmLocationStatus.TempGet)
                {
                    //取物车位不可用，则临时分配
                    motsk.IDealCheckedCar(tsk, moHall.DeviceCode, distance, carSize,weight);
                }
                else
                {
                    motsk.ITempDealCheckCar(tsk, tolct, distance, carSize,weight);
                }                
            }
            else
            {
                motsk.IDealCheckedCar(tsk, moHall.DeviceCode, distance, carSize,weight);
            }
        }

        /// <summary>
        /// 处理车辆离开
        /// </summary>
        /// <param name="task"></param>
        public void DealCarLeave(ImplementTask task)
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
                Location lct = new CWLocation().FindLocation(lt => lt.ICCode == iccd.UserCode && lt.Type == EnmLocationType.Normal && lt.Status == EnmLocationStatus.Occupy);

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

                    }
                    else if(tsk.Status == EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                    {
                        //处理刷第二次卡

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

                }
                #endregion
                #region 进出车厅
                else if (moHall.HallType == EnmHallType.EnterOrExit)
                {

                }
                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

    }
}
