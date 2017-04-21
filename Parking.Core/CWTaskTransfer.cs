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

        public CWTaskTransfer()
        {
            motsk = new CWTask();
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



    }
}
