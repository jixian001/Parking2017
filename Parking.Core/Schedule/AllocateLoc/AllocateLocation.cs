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
        /// 一个巷道内只有单个移动设备，车位分配
        /// </summary>
        /// <param name="checkCode"></param>
        /// <param name="iccd"></param>
        /// <param name="htask"></param>
        /// <param name="smg"></param>
        /// <returns></returns>
        public Location IAllocateLocation(string checkCode, ICCard iccd, Device hall, out int smg)
        {
            smg = 0;

            int warehouse = hall.Warehouse;
            int hallCode = hall.DeviceCode;
            int hallCol = Convert.ToInt32(hall.Address.Substring(1, 2));
           
            Location lct = null;
            CWTask cwtask = new CWTask();

            if (iccd.Type == EnmICCardType.Temp || iccd.Type == EnmICCardType.Periodical)
            {
                lct = this.PPYAllocate(warehouse, checkCode, hallCol);
            }
            else if (iccd.Type == EnmICCardType.FixedLocation)
            {
                if (iccd.Warehouse != warehouse)
                {
                    //车辆停在其他库区
                    cwtask.AddNofication(warehouse, hallCode, "16.wav");
                    return null;
                }
                lct = new CWLocation().SelectLocByAddress(warehouse, iccd.LocAddress);
                if (lct != null)
                {
                    if (compareSize(lct.LocSize, checkCode) < 0)
                    {
                        //车位尺寸与车辆不匹配
                        cwtask.AddNofication(warehouse, hallCode, "61.wav");
                        return null;
                    }
                }
            }
            if (lct != null)
            {
                //选择TV
                Device tv = new CWDevice().Find(d=>d.Warehouse==warehouse&&d.Layer==lct.LocLayer);
                if (tv != null)
                {
                    smg = tv.DeviceCode;
                }
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
            CWLocation cwlctn = new CWLocation();
            #region 找出可用的TV
            List<Device> availableTvs = cwdevice.FindList(dev => dev.IsAble == 1 &&
                                                                 dev.Warehouse == warehouse &&
                                                                 dev.Type == EnmSMGType.ETV);
            //TV层集合
            List<int> availlayers = new List<int>();
            foreach (Device dev in availableTvs)
            {
                availlayers.Add(dev.Layer);
            }
            #endregion
            //获取空闲车位
            List<Location> spacelocations = cwlctn.FindLocationList(loc => loc.Warehouse == warehouse &&
                                                                                loc.Type == EnmLocationType.Normal &&
                                                                                loc.Status == EnmLocationStatus.Space &&
                                                                                cwiccard.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null &&
                                                                                availlayers.Exists(layer => layer == loc.LocLayer));
            //获取占用或正在作业的车位
            List<Location> busylocations = cwlctn.FindLocationList(loc => loc.Warehouse == warehouse &&
                                                                                    loc.Type == EnmLocationType.Normal &&
                                                                                    loc.Status != EnmLocationStatus.Space &&
                                                                                    cwiccard.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null &&
                                                                                    availlayers.Exists(layer => layer == loc.LocLayer));
            //优先分配靠近车厅的小车位
            List<Location> smallLocations = (spacelocations.Where(loc => compareSize(loc.LocSize, checkCode) == 0).
                                                                      OrderBy(loc => Math.Abs(loc.LocColumn - hallCol))).ToList();
            //再分配靠近车厅的大车位
            List<Location> bigLocations = (spacelocations.Where(loc => compareSize(loc.LocSize, checkCode) > 0).
                                                          OrderBy(loc => Math.Abs(loc.LocColumn - hallCol))).ToList();

            #region 优先分配车位尺寸一致的
            #region 找出空闲的TV，优先分配 
            //空闲可用的
            List<Device> freeDevices = availableTvs.FindAll(av=>av.TaskID==0);
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
            #region 分配车位占用较少的层
            Dictionary<int, int> dicLayerAndOppucyCnt = new Dictionary<int, int>();
            foreach(int layer in availlayers)
            {
                int count = 0;
                foreach(Location loc in busylocations)
                {
                    if(compareSize(loc.LocSize, checkCode) == 0)
                    {
                        if (loc.LocLayer == layer)
                        {
                            count++;
                        }
                    }
                }
                dicLayerAndOppucyCnt.Add(layer,count);
            }
            Dictionary<int, int> dicOrderBy = dicLayerAndOppucyCnt.OrderBy(r => r.Value).ToDictionary(d => d.Key, c => c.Value);
            foreach (KeyValuePair<int,int> pair in dicOrderBy)
            {
                foreach (Location loc in smallLocations)
                {
                    if (loc.LocLayer == pair.Key)
                    {
                        return loc;
                    }
                }
            }
            #endregion
            #endregion
            #region 优先分配车位尺寸大于的
            #region 找出空闲的TV，优先分配 
            //空闲可用的          
            foreach (Device tv in freeDevices)
            {
                foreach (Location loc in bigLocations)
                {
                    if (loc.LocLayer == tv.Layer)
                    {
                        return loc;
                    }
                }
            }
            #endregion
            #region 分配车位占用较少的层
            Dictionary<int, int> dicLayerAndOppucyCnt_big = new Dictionary<int, int>();
            foreach (int layer in availlayers)
            {
                int count = 0;
                foreach (Location loc in busylocations)
                {
                    if (compareSize(loc.LocSize, checkCode) > 0)
                    {
                        if (loc.LocLayer == layer)
                        {
                            count++;
                        }
                    }
                }
                dicLayerAndOppucyCnt_big.Add(layer, count);
            }
            Dictionary<int, int> dicOrderBy_big = dicLayerAndOppucyCnt_big.OrderBy(r => r.Value).ToDictionary(d => d.Key, c => c.Value);
            foreach (KeyValuePair<int, int> pair in dicOrderBy_big)
            {
                foreach (Location loc in bigLocations)
                {
                    if (loc.LocLayer == pair.Key)
                    {
                        return loc;
                    }
                }
            }
            #endregion
            #endregion
            return null;
        }

        /// <summary>
        /// 巷道堆垛临时卡车位分配
        /// 单堆垛机
        /// </summary>
        /// <returns></returns>
        private Location PXDAllocate(int warehouse,int hallCol,string checkCode)
        {
            CWLocation cwlctn = new CWLocation();
            CWICCard cwiccard = new CWICCard();
            //获取空闲的车位集合
            List<Location> spacelocations = cwlctn.FindLocationList(loc => loc.Warehouse == warehouse &&
                                                                                loc.Type == EnmLocationType.Normal &&
                                                                                loc.Status == EnmLocationStatus.Space &&
                                                                                cwiccard.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null);
            //优先分配靠近车厅的小车位
            List<Location> smallLocations = (spacelocations.Where(loc => compareSize(loc.LocSize, checkCode) == 0).
                                                                      OrderBy(loc => Math.Abs(loc.LocColumn - hallCol))).ToList();
            if (smallLocations.Count > 0)
            {
                return smallLocations.First();
            }
            //再分配靠近车厅的大车位
            List<Location> bigLocations = (spacelocations.Where(loc => compareSize(loc.LocSize, checkCode) > 0).
                                                          OrderBy(loc => Math.Abs(loc.LocColumn - hallCol))).ToList();
            if (bigLocations.Count > 0)
            {
                return bigLocations.First();
            }

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
