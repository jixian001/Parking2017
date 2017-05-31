using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;
using Parking.Core.Schedule;

namespace Parking.Core
{
    public class AllocateLocation
    {
        /// <summary>
        /// 存车时，分配车位及ETV
        /// </summary>
        /// <param name="checkCode">外形尺寸</param>
        /// <param name="cust">顾客</param>
        /// <param name="hall">车厅</param>       
        /// <param name="smg"></param>
        /// <returns></returns>
        public Location IAllocateLocation(string checkCode, Customer cust, Device hall, out int smg)
        {
            Log log = LogFactory.GetLogger("AllocateLocation.IAllocateLocation");

            smg = 0;

            int warehouse = hall.Warehouse;
            int hallCode = hall.DeviceCode;
            int hallCol = Convert.ToInt32(hall.Address.Substring(1, 2));

            Location lct = null;
            CWTask cwtask = new CWTask();
            if (cust == null)
            {
                lct = this.PXDAllocate(hall, checkCode,out smg);
            }
            else if (cust.Type == EnmICCardType.Temp || 
                     cust.Type == EnmICCardType.Periodical)
            {
                lct = this.PXDAllocate(hall, checkCode, out smg);
            }
            else if (cust.Type == EnmICCardType.FixedLocation)
            {
                if (cust.Warehouse != warehouse)
                {
                    //车辆停在其他库区
                    cwtask.AddNofication(warehouse, hallCode, "16.wav");
                    return null;
                }
                lct = new CWLocation().SelectLocByAddress(cust.Warehouse, cust.LocAddress);
                if (lct != null)
                {
                    if (compareSize(lct.LocSize, checkCode) < 0)
                    {
                        //车位尺寸与车辆不匹配
                        cwtask.AddNofication(warehouse, hallCode, "61.wav");
                        return null;
                    }
                }
                //分配ETV


            }
              
            return lct;
        }

        /// <summary>
        /// 巷道堆垛临时卡车位分配       
        /// </summary>
        /// <returns></returns>
        private Location PXDAllocate(Device hall,string checkCode, out int smg)
        {
            smg = 0;

            CWLocation cwlctn = new CWLocation();
            CWDevice cwdevice = new CWDevice();

            List<Device> nEtvList = cwdevice.FindList(d => d.Type == EnmSMGType.ETV);
            WorkScope workscope = new WorkScope(nEtvList);
            int hallColmn = Convert.ToInt32(hall.Address.Substring(1,2));
            List<Device> reachHall = new List<Device>();
            #region 找出可达车厅的ETV集合
            foreach(Device dev in nEtvList)
            {
                CScope scope = workscope.GetEtvScope(dev);
                if (scope.LeftCol <= hallColmn && hallColmn <= scope.RightCol)
                {
                    if (dev.IsAble == 1)
                    {
                        reachHall.Add(dev);
                    }
                }
            }
            #endregion


            return null;
        }

        /// <summary>
        /// 比较尺寸
        /// </summary>
        /// <param name="size"></param>
        /// <param name="carSize"></param>
        /// <returns></returns>
        private int compareSize(string size, string carSize)
        {
            int CarLong = Convert.ToInt32(carSize.Substring(0, 1));
            int CarWide = Convert.ToInt32(carSize.Substring(1, 1));
            int CarHigh = Convert.ToInt32(carSize.Substring(2, 1));

            int lCarLong = Convert.ToInt32(size.Substring(0, 1));
            int lCarWide = Convert.ToInt32(size.Substring(1, 1));
            int lCarHigh = Convert.ToInt32(size.Substring(2, 1));

            if ((lCarLong == CarLong) && (lCarWide == CarWide) && (lCarHigh == CarHigh))
            {
                return 1;
            }
            else if ((lCarLong == CarLong) && (lCarWide > CarWide) && (lCarHigh == CarHigh))
            {
                return 2;
            }
            else if ((lCarLong >= CarLong) && (lCarWide >= CarWide) && (lCarHigh >= CarHigh))
            {
                return 3;
            }
            return -1;
        }

    }
}
