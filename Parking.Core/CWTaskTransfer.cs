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
        /// 外形检测上报处理
        /// </summary>
        /// <param name="tsk"></param>
        /// <param name="distance"></param>
        /// <param name="carSize"></param>
        public void DealICheckCar(ImplementTask tsk,int distance,string carSize)
        {
            if (tsk.Type == EnmTaskType.TempGet)
            {

            }
            else
            {

            }
        }
    }
}
