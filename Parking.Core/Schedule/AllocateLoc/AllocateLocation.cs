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
        /// <param name="smg">ETV 设备号</param>
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
                Device dev = PXDAllocateEtvOfFixLoc(hall, lct);
                if (dev != null)
                {
                    smg = dev.DeviceCode;
                }
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
            #region
            CWLocation cwlctn = new CWLocation();
            CWDevice cwdevice = new CWDevice();
            CWTask cwtask = new CWTask();

            List<Device> nEtvList = cwdevice.FindList(d => d.Type == EnmSMGType.ETV);
            WorkScope workscope = new WorkScope(nEtvList);
            int hallColmn = Convert.ToInt32(hall.Address.Substring(1,2));
            List<Device> reachHall = new List<Device>();
            #region 找出可达车厅、可用的ETV集合
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

            List<Location> lctnLst = new List<Location>();
            #region 依距车厅的远近找出所有车位的集合          
            List<Location> allLocLst = cwlctn.FindLocList();
            #region 车位尺寸一致
            #region 首先 分配与车厅同一个区域的，车位尺寸一致的， 4-2-1依次排列，
            var L42_locList_small = from loc in allLocLst
                                    where loc.Type == EnmLocationType.Normal &&
                                          loc.Status == EnmLocationStatus.Space &&
                                          compareSize(loc.LocSize, checkCode) == 1 &&
                                          loc.LocSide != 1 &&
                                          loc.Region == hall.Region
                                    orderby Math.Abs(loc.LocColumn - hallColmn) ascending,
                                            loc.LocSide descending
                                    select loc;
            //1边
            var L1_locList_small = from loc in allLocLst
                                   where loc.Type == EnmLocationType.Normal &&
                                         loc.Status == EnmLocationStatus.Space &&
                                         compareSize(loc.LocSize, checkCode) == 1 &&
                                         loc.LocSide == 1 &&
                                         loc.Region == hall.Region
                                   orderby Math.Abs(loc.LocColumn - hallColmn) ascending
                                   select loc;
            #endregion
            lctnLst.AddRange(L42_locList_small);
            lctnLst.AddRange(L1_locList_small);

            #region 再次分配与车厅同一个区域，车位尺寸（仅宽度）偏大点的
            var L42_locList_width = from loc in allLocLst
                                    where loc.Type == EnmLocationType.Normal &&
                                          loc.Status == EnmLocationStatus.Space &&
                                          compareSize(loc.LocSize, checkCode) == 2 &&
                                          loc.LocSide != 1 &&
                                          loc.Region == hall.Region
                                    orderby Math.Abs(loc.LocColumn - hallColmn) ascending,
                                            loc.LocSide descending
                                    select loc;           
            //1边
            var L1_locList_width = from loc in allLocLst
                             where loc.Type == EnmLocationType.Normal &&
                                   loc.Status == EnmLocationStatus.Space &&
                                   compareSize(loc.LocSize, checkCode) == 2 &&
                                   loc.LocSide == 1 &&
                                   loc.Region == hall.Region
                             orderby Math.Abs(loc.LocColumn - hallColmn) ascending
                             select loc;
            #endregion
            lctnLst.AddRange(L42_locList_width);
            lctnLst.AddRange(L1_locList_width);

            #region 再分配与车厅不是同一个区域的
            //4边2边
            var L42_locList_small_dif = from loc in allLocLst
                                    where loc.Type == EnmLocationType.Normal &&
                                          loc.Status == EnmLocationStatus.Space &&
                                          compareSize(loc.LocSize, checkCode) == 1 &&
                                          loc.LocSide != 1 &&
                                          loc.Region != hall.Region
                                    orderby Math.Abs(loc.LocColumn - hallColmn) ascending,
                                            loc.LocSide descending
                                    select loc;
            //1边
            var L1_locList_small_dif = from loc in allLocLst
                                   where loc.Type == EnmLocationType.Normal &&
                                         loc.Status == EnmLocationStatus.Space &&
                                         compareSize(loc.LocSize, checkCode) == 1 &&
                                         loc.LocSide == 1 &&
                                         loc.Region != hall.Region
                                   orderby Math.Abs(loc.LocColumn - hallColmn) ascending
                                   select loc;
            #endregion
            lctnLst.AddRange(L42_locList_small_dif);
            lctnLst.AddRange(L1_locList_small_dif);

            #region 再次分配与车厅不同区域，车位尺寸（仅宽度）偏大点的
            var L42_locList_width_dif = from loc in allLocLst
                                    where loc.Type == EnmLocationType.Normal &&
                                          loc.Status == EnmLocationStatus.Space &&
                                          compareSize(loc.LocSize, checkCode) == 2 &&
                                          loc.LocSide != 1 &&
                                          loc.Region != hall.Region
                                    orderby Math.Abs(loc.LocColumn - hallColmn) ascending,
                                            loc.LocSide descending
                                    select loc;
            //1边
            var L1_locList_width_dif = from loc in allLocLst
                                   where loc.Type == EnmLocationType.Normal &&
                                         loc.Status == EnmLocationStatus.Space &&
                                         compareSize(loc.LocSize, checkCode) == 2 &&
                                         loc.LocSide == 1 &&
                                         loc.Region != hall.Region
                                   orderby Math.Abs(loc.LocColumn - hallColmn) ascending
                                   select loc;
            #endregion
            lctnLst.AddRange(L42_locList_width_dif);
            lctnLst.AddRange(L1_locList_width_dif);
            #endregion

            #region 车位尺寸偏大的
            #region 首先 分配与车厅同一个区域的，车位尺寸一致的， 4-2-1依次排列，
            var L42_locList_big = from loc in allLocLst
                                    where loc.Type == EnmLocationType.Normal &&
                                          loc.Status == EnmLocationStatus.Space &&
                                          compareSize(loc.LocSize, checkCode) == 3 &&
                                          loc.LocSide != 1 &&
                                          loc.Region == hall.Region
                                    orderby Math.Abs(loc.LocColumn - hallColmn) ascending,
                                            loc.LocSide descending
                                    select loc;
            //1边
            var L1_locList_big = from loc in allLocLst
                                   where loc.Type == EnmLocationType.Normal &&
                                         loc.Status == EnmLocationStatus.Space &&
                                         compareSize(loc.LocSize, checkCode) == 3 &&
                                         loc.LocSide == 1 &&
                                         loc.Region == hall.Region
                                   orderby Math.Abs(loc.LocColumn - hallColmn) ascending
                                   select loc;
            #endregion
            lctnLst.AddRange(L42_locList_big);
            lctnLst.AddRange(L1_locList_big);

            #region 再分配与车厅不是同一个区域的
            var L42_locList_big_dif = from loc in allLocLst
                                  where loc.Type == EnmLocationType.Normal &&
                                        loc.Status == EnmLocationStatus.Space &&
                                        compareSize(loc.LocSize, checkCode) == 3 &&
                                        loc.LocSide != 1 &&
                                        loc.Region != hall.Region
                                  orderby Math.Abs(loc.LocColumn - hallColmn) ascending,
                                          loc.LocSide descending
                                  select loc;
            //1边
            var L1_locList_big_dif = from loc in allLocLst
                                 where loc.Type == EnmLocationType.Normal &&
                                       loc.Status == EnmLocationStatus.Space &&
                                       compareSize(loc.LocSize, checkCode) == 3 &&
                                       loc.LocSide == 1 &&
                                       loc.Region != hall.Region
                                 orderby Math.Abs(loc.LocColumn - hallColmn) ascending
                                 select loc;
            #endregion
            lctnLst.AddRange(L42_locList_big_dif);
            lctnLst.AddRange(L1_locList_big_dif);

            #endregion
            #endregion
            if (reachHall.Count == 1)
            {
                CScope scope = workscope.GetEtvScope(reachHall[0]);
                //只要一个ETV可达车厅，
                //依作业范围，找出在TV范围内的车位
                foreach (Location loc in lctnLst)
                {
                    if (scope.LeftCol <= loc.LocColumn && loc.LocColumn <= scope.RightCol)
                    {
                        smg = reachHall[0].DeviceCode;
                        return loc;
                    }
                }
            }
            else if (reachHall.Count == 2)
            {
                //如果有两个空闲的
                List<Device> freeEtvs = reachHall.FindAll(ch => ch.TaskID == 0);
                if (freeEtvs.Count == 2)
                {
                    #region                    
                    freeEtvs = reachHall.OrderBy(h=>Math.Abs(h.Region-hall.Region)).ToList();
                    foreach(Device dev in freeEtvs)
                    {
                        CScope scope = workscope.GetEtvScope(dev);
                        foreach (Location loc in lctnLst)
                        {
                            if (scope.LeftCol <= loc.LocColumn && loc.LocColumn <= scope.RightCol)
                            {
                                smg = dev.DeviceCode;
                                return loc;
                            }
                        }
                    }
                    #endregion
                }
                else if (freeEtvs.Count == 1)
                {
                    #region
                    Device dev = freeEtvs[0];
                    if (dev.Region == hall.Region)
                    {
                        #region
                        smg = dev.DeviceCode;
                        CScope scope = workscope.GetEtvScope(dev);
                        foreach (Location loc in lctnLst)
                        {
                            if (scope.LeftCol <= loc.LocColumn && loc.LocColumn <= scope.RightCol)
                            {
                                return loc;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region
                        //如果不是同一区域的
                        //判断另一个ETV在执行什么样的动作，
                        //另一台ETV要不要跨区作业
                        Device other = reachHall.Find(h=>h.DeviceCode!=dev.DeviceCode);
                        if (other.TaskID != 0)
                        {
                            ImplementTask task = cwtask.Find(other.TaskID);
                            if (task != null)
                            {
                                string toAddrs = "";
                                if(task.Status==EnmTaskStatus.TWaitforLoad||
                                    task.Status == EnmTaskStatus.TWaitforMove)
                                {
                                    toAddrs = task.FromLctAddress;
                                }
                                else
                                {
                                    toAddrs = task.ToLctAddress;
                                }
                                int toCol = Convert.ToInt32(toAddrs.Substring(1,2));
                                //正在作业TV
                                int currCol = Convert.ToInt32(other.Address.Substring(1,2));
                                //空闲TV
                                int freeCol= Convert.ToInt32(dev.Address.Substring(1, 2));

                                //空闲TV在右侧，即2号ETV空闲
                                if (currCol < freeCol)
                                {
                                    #region TV2空闲
                                    if (hallColmn >= toCol + 3)
                                    {
                                        //1#车厅不在范围内，则允许2#ETV动作
                                        smg = dev.DeviceCode;
                                        CScope scope = workscope.GetEtvScope(dev);
                                        foreach (Location loc in lctnLst)
                                        {
                                            if (scope.LeftCol <= loc.LocColumn && loc.LocColumn <= scope.RightCol)
                                            {
                                                return loc;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region 1#TV空闲
                                    if (hallColmn <= toCol - 3)
                                    {
                                        //2#车厅不在范围内，则允许1#ETV动作
                                        smg = dev.DeviceCode;
                                        CScope scope = workscope.GetEtvScope(dev);
                                        foreach (Location loc in lctnLst)
                                        {
                                            if (scope.LeftCol <= loc.LocColumn && loc.LocColumn <= scope.RightCol)
                                            {
                                                return loc;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                List<Device> orderbyEtvs = reachHall.OrderBy(dr=>Math.Abs(dr.Region-hall.Region)).ToList();
                #region 两个都在忙或上述找不到时，则依车厅区域内的ETV优先，分配ETV
                foreach(Device devc in orderbyEtvs)
                {                    
                    CScope scope = workscope.GetEtvScope(devc);
                    foreach (Location loc in lctnLst)
                    {
                        if (scope.LeftCol <= loc.LocColumn && loc.LocColumn <= scope.RightCol)
                        {
                            smg = devc.DeviceCode;
                            return loc;
                        }
                    }
                }

                #endregion
            }
            #endregion
            return null;
        }

        /// <summary>
        /// 固定车位时，ETV分配
        /// </summary>
        /// <param name="hall">存车车厅</param>
        /// <param name="loc">目的车位</param>
        /// <returns></returns>
        public Device PXDAllocateEtvOfFixLoc(Device hall,Location loc)
        {
            CWDevice cwdevice = new CWDevice();
            CWTask cwtask = new CWTask();

            List<Device> nEtvList = cwdevice.FindList(d => d.Type == EnmSMGType.ETV);
            WorkScope workscope = new WorkScope(nEtvList);
            int hallColmn = Convert.ToInt32(hall.Address.Substring(1, 2));
            int locColmn = loc.LocColumn;

            List<Device> availableLst = new List<Device>();
            #region 可达车厅、可达车位、可用的ETV集合
            foreach (Device dev in nEtvList)
            {
                if (dev.IsAble == 1)
                {
                    CScope scope = workscope.GetEtvScope(dev);
                    if (scope.LeftCol <= hallColmn && hallColmn <= scope.RightCol)
                    {
                        if(scope.LeftCol <= locColmn && locColmn <= scope.RightCol)
                        {
                            availableLst.Add(dev);
                        }
                    }
                }
            }
            #endregion
            if (availableLst.Count == 1)
            {
                return availableLst[0];
            }
            else if (availableLst.Count == 2)
            {
                List<Device> freeLst = availableLst.FindAll(d => d.TaskID == 0);
                if (freeLst.Count == 1)
                {
                    Device dev = freeLst[0];
                    if (dev.Region == hall.Region)
                    {
                        return dev;
                    }
                    Device other = availableLst.FirstOrDefault(d => d.TaskID != 0);
                    if (other != null)
                    {
                        #region 另一ETV在忙,判断当前闲的ETV，是否可达车厅来装载，如果可以，则允许下发
                        ImplementTask task = cwtask.Find(other.TaskID);
                        if (task != null)
                        {
                            string toAddrs = "";
                            if (task.Status == EnmTaskStatus.TWaitforLoad ||
                                task.Status == EnmTaskStatus.TWaitforMove)
                            {
                                toAddrs = task.FromLctAddress;
                            }
                            else
                            {
                                toAddrs = task.ToLctAddress;
                            }
                            int toCol = Convert.ToInt32(toAddrs.Substring(1, 2));
                            //正在作业TV
                            int currCol = Convert.ToInt32(other.Address.Substring(1, 2));
                            //空闲TV
                            int freeCol = Convert.ToInt32(dev.Address.Substring(1, 2));

                            //空闲TV在右侧，即2号ETV空闲
                            if (currCol < freeCol)
                            {
                                #region TV2 空闲
                                if (hallColmn >= toCol + 3)
                                {
                                    //1#车厅不在范围内，则允许2#ETV动作
                                    return dev;
                                }
                                #endregion
                            }
                            else
                            {
                                #region 1#TV 空闲
                                if (hallColmn <= toCol - 3)
                                {
                                    //2#车厅不在范围内，则允许1#ETV动作
                                    return dev;
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                }
                //有两个空闲或，两个忙时，都优先分配车厅所在区域的ETV
                List<Device> orderbyLst = availableLst.OrderBy(d => Math.Abs(d.Region - hall.Region)).ToList();
                return orderbyLst[0];
            }
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
