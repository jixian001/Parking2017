using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;

namespace Parking.Core
{
    public class AllocateLocation
    {
        /// <summary>
        /// 单个移动设备的车位分配
        /// </summary>
        /// <param name="checkCode"></param>
        /// <param name="iccd"></param>
        /// <param name="htask"></param>
        /// <param name="smg"></param>
        /// <returns></returns>
        public Location IAllocateLocation(string checkCode, ICCard iccd, Device hall, out int smg)
        {
            int warehouse = hall.Warehouse;
            int hallCode = hall.DeviceCode;
            int hallCol = Convert.ToInt32(hall.Address.Substring(1, 2));
            smg = 0;
            Location lct = null;

            if (iccd.Type == EnmICCardType.Temp || iccd.Type == EnmICCardType.Periodical)
            {
                lct = this.PPYAllocate(warehouse, checkCode, hallCol);
            }
            else if (iccd.Type == EnmICCardType.FixedLocation)
            {
                if (iccd.Warehouse != warehouse)
                {
                    //车辆停在其他库区
                    new CWTask().AddNofication(warehouse, hallCode, "16.wav");
                    return null;
                }
                lct = new CWLocation().SelectLocByAddress(warehouse, iccd.LocAddress);
                if (lct != null)
                {
                    if (compareSize(lct.LocSize, checkCode) < 0)
                    {
                        //车位尺寸与车辆不匹配
                        new CWTask().AddNofication(warehouse, hallCode, "61.wav");
                        return null;
                    }
                }
            }
            if (lct != null)
            {
                //选择TV

            }
            return lct;
        }

        /// <summary>
        /// 平面移动临时卡车位分配
        /// </summary>
        /// <returns></returns>
        private Location PPYAllocate(int warehouse, string checkCode, int hallCol)
        {
            CWICCard cwiccard = new CWICCard();
            CWDevice cwdevice = new CWDevice();
            #region 找出可用的TV
            List<Device> availableTvs = cwdevice.FindList(dev => dev.IsAble == 1);
            List<int> availlayers = new List<int>();
            foreach (Device dev in availableTvs)
            {
                availlayers.Add(dev.Layer);
            }
            #endregion
            List<Location> locations = new CWLocation().FindLocationList(loc => loc.Warehouse == warehouse &&
                                    loc.Type == EnmLocationType.Normal &&
                                    loc.Status == EnmLocationStatus.Space &&
                                    cwiccard.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null &&
                                    availlayers.Exists(layer => layer == loc.LocLayer));
            //优先分配靠近车厅的小车位
            List<Location> smallLocations = (locations.Where(loc => compareSize(loc.LocSize, checkCode) == 0).
                                                                      OrderBy(loc => Math.Abs(loc.LocColumn - hallCol))).ToList();
            //再分配靠近车厅的大车位
            List<Location> bigLocations = (locations.Where(loc => compareSize(loc.LocSize, checkCode) > 0).
                                                          OrderBy(loc => Math.Abs(loc.LocColumn - hallCol))).ToList();

            List<Location> orderByLocations = new List<Location>();
            orderByLocations.AddRange(smallLocations);
            orderByLocations.AddRange(bigLocations);

            #region 找出空闲的TV，优先分配
            OrderParam param = new OrderParam()
            {
                PropertyName = "Layer",
                Method = OrderMethod.Desc
            };
            //空闲可用的
            List<Device> freeDevices = cwdevice.FindList(dev => dev.IsAble == 1 && dev.TaskID == 0, param);
            foreach (Device tv in freeDevices)
            {
                foreach (Location loc in smallLocations)
                {
                    if (loc.LocLayer == tv.Layer)
                    {
                        return loc;
                    }
                }
            }
            #endregion
            #region 分配层中作业少的
            
            #endregion

            return null;
        }
        /// <summary>
        /// 巷道堆垛临时卡车位分配
        /// </summary>
        /// <returns></returns>
        private Location PXDAllocate()
        {

            return null;
        }

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
                return 0;
            }
            else if ((lCarLong >= CarLong) && (lCarWide >= CarWide) && (lCarHigh >= CarHigh))
            {
                return 1;
            }
            return -1;
        }

    }
}
