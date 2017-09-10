using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Core.Schedule;

namespace Parking.Core
{
    /// <summary>
    /// 取车、取物分配TV
    /// </summary>
    public class AllocateTV
    {      
        /// <summary>
        /// 取车、取物分配
        /// </summary>
        /// <param name="hall"></param>
        /// <param name="lctn"></param>
        /// <returns></returns>
        public Device Allocate(Device hall,Location loc)
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
                        if (scope.LeftCol <= locColmn && locColmn <= scope.RightCol)
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
                    if (dev.Region == loc.Region)
                    {
                        return dev;
                    }
                    Device other = availableLst.FirstOrDefault(d => d.TaskID != 0);
                    if (other != null)
                    {
                        #region 另一ETV在忙,判断当前闲的ETV，是否可达车位来装载，如果可以，则允许下发
                        ImplementTask task = cwtask.Find(other.TaskID);
                        if (task != null)
                        {
                            string toAddrs = "";
                            if (task.Status == EnmTaskStatus.TWaitforLoad)
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
                                if (locColmn >= toCol + 3)
                                {
                                    //1#车厅不在范围内，则允许2#ETV动作
                                    return dev;
                                }
                                #endregion
                            }
                            else
                            {
                                #region 1#TV 空闲
                                if (locColmn <= toCol - 3)
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
        /// 没有任何作业时，如果缓存位被占用，则强制回挪，
        /// TV分配，挪移车位分配
        /// </summary>
        /// <param name="frlct"></param>
        /// <param name="tolct"></param>
        /// <returns></returns>
        public Device PPYAllocateTvOfTransfer(Location frlct,out Location tolct)
        {
            tolct = null;
            CWLocation cwlctn = new CWLocation();
            CWICCard cwiccd = new CWICCard();
            List<Location> allLocLst = cwlctn.FindLocList();           
            var query = from loc in allLocLst
                        where loc.Type == EnmLocationType.Normal &&
                              loc.Status == EnmLocationStatus.Space &&
                              loc.Region==frlct.Region&&
                              cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null &&
                              loc.NeedBackup==0 &&
                              compareSize(loc.LocSize, frlct.LocSize) >= 0
                        orderby Math.Abs(frlct.LocColumn - loc.LocColumn)
                        select loc;
            List<Location> lctnLst = query.ToList();
            if (lctnLst.Count > 0)
            {
                tolct = lctnLst.FirstOrDefault();

                Device tv = new CWDevice().Find(d=>d.Region==frlct.Region);
                return tv;
            }
            return null;
        }

        /// <summary>
        /// 没有任何作业时，如果缓存位被占用，则强制回挪，
        /// TV分配，挪移车位分配
        /// </summary>
        /// <param name="frlct"></param>
        /// <param name="tolct"></param>
        /// <returns></returns>
        public Device AllocateTvOfTransport(Location frlct,out Location toLct)
        {
            toLct = null;
            CWDevice cwdevice = new CWDevice();
            CWLocation cwlctn = new CWLocation();
            CWICCard cwiccd = new CWICCard();
           
            List<Device> nEtvList = cwdevice.FindList(d => d.Type == EnmSMGType.ETV);
            WorkScope workscope = new WorkScope(nEtvList);           
            int locColmn = frlct.LocColumn;

            List<Device> availableLst = new List<Device>();
            #region 可达车位、可用的ETV集合
            foreach (Device dev in nEtvList)
            {
                if (dev.IsAble == 1)
                {
                    CScope scope = workscope.GetEtvScope(dev);
                    if (scope.LeftCol <= locColmn && locColmn <= scope.RightCol)
                    {
                        availableLst.Add(dev);
                    }
                }
            }
            #endregion
            
            List<Location> allLocLst = cwlctn.FindLocList();
            var query = from loc in allLocLst
                        where loc.Type == EnmLocationType.Normal &&
                              loc.Status == EnmLocationStatus.Space &&
                              cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null &&
                              (loc.LocSide == 1 || loc.LocSide == 2) &&
                              compareSize(loc.LocSize, frlct.LocSize) > 0
                        orderby Math.Abs(frlct.LocColumn - loc.LocColumn), 
                                loc.LocSide ascending
                        select loc;
            List<Location> lctnLst = query.ToList();

            if (availableLst.Count == 0)
            {
                return null;
            }
            if (availableLst.Count == 1)
            {
                CScope scope= workscope.GetEtvScope(availableLst[0]);
                foreach(Location loc in lctnLst)
                {
                    if (scope.LeftCol <= loc.LocColumn && loc.LocColumn <= scope.RightCol)
                    {
                        toLct = loc;
                        return availableLst[0];
                    }
                }
            }
            else
            {
                List<Device> orderbyLst = availableLst.OrderBy(d => Math.Abs(d.Region - frlct.Region)).ToList();
                foreach(Device dev in orderbyLst)
                {
                    CScope scope = workscope.GetEtvScope(dev);
                    foreach (Location loc in lctnLst)
                    {
                        if (scope.LeftCol <= loc.LocColumn && loc.LocColumn <= scope.RightCol)
                        {
                            toLct = loc;
                            return availableLst[0];
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 巷道堆垛式---要生成挪移作业时，挪移车位的查找
        /// </summary>
        /// <param name="nEtv"></param>
        /// <param name="frLct"></param>
        /// <returns></returns>
        public Location AllocateTvNeedTransfer(Device nEtv,Location frLct)
        {
            CWDevice cwdevice = new CWDevice();
            CWLocation cwlctn = new CWLocation();
            CWICCard cwiccd = new CWICCard();

            int warehouse = nEtv.Warehouse;
            List<Device> nEtvList = cwdevice.FindList(d => d.Type == EnmSMGType.ETV);
            WorkScope workscope = new WorkScope(nEtvList);
            CScope scope = workscope.GetEtvScope(nEtv);
            
            List<Location> availLst = new List<Location>();
            List<Location> backOccupyLst = new List<Location>();
            List<Location> lctnLst = cwlctn.FindLocList();            
            //先找出相同区域的，尺寸一致的，如果是2边车位，则后面的车位不能是作业的车位
            var query_same = from loc in lctnLst
                             where loc.Type == EnmLocationType.Normal &&
                                 loc.Status == EnmLocationStatus.Space &&
                                 loc.Region == frLct.Region &&
                                 loc.LocSide != 4 &&
                                (compareSize(loc.LocSize, frLct.LocSize) == 1 ||
                                 compareSize(loc.LocSize, frLct.LocSize) == 2)&&
                                 cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null
                             orderby  Math.Abs(frLct.LocColumn-loc.LocColumn), loc.LocSide ascending
                             select loc;

            foreach(Location loc in query_same)
            {
                if (loc.LocSide == 2)
                {
                    string backAddrs = string.Format((loc.LocSide+2).ToString() + loc.Address.Substring(1));
                    Location back = cwlctn.FindLocation(lc => lc.Address == backAddrs);
                    if (back != null)
                    {
                        if (back.Status == EnmLocationStatus.Space)
                        {
                            availLst.Add(loc);
                        }
                        else if (back.Status == EnmLocationStatus.Occupy)
                        {
                            backOccupyLst.Add(loc);
                        }

                    }
                }
                else
                {
                    availLst.Add(loc);
                }
            }
            availLst.AddRange(backOccupyLst);
            //再找缓存车位
            var query_temp = from loc in lctnLst
                             where loc.Type == EnmLocationType.Temporary &&
                                    loc.Status == EnmLocationStatus.Space &&
                                    compareSize(loc.LocSize, frLct.LocSize) > 0
                             orderby Math.Abs(loc.Region - frLct.Region)
                             select loc;
            availLst.AddRange(query_temp);
            //最后找不同区域的，尺寸一致的，如果是2边车位，则后面的车位不能是作业的车位
            var query_diff = from loc in lctnLst
                             where loc.Type == EnmLocationType.Normal &&
                                 loc.Status == EnmLocationStatus.Space &&
                                 loc.Region != frLct.Region &&
                                 loc.LocSide != 4 &&
                                (compareSize(loc.LocSize, frLct.LocSize) == 1 ||
                                 compareSize(loc.LocSize, frLct.LocSize) == 2) &&
                                 cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null
                             orderby Math.Abs(frLct.LocColumn - loc.LocColumn), loc.LocSide ascending
                             select loc;
            List<Location> backOccupyLst_Big = new List<Location>();
            foreach (Location loc in query_diff)
            {
                if (loc.LocSide == 2)
                {
                    string backAddrs = string.Format((loc.LocSide + 2).ToString() + loc.Address.Substring(1));
                    Location back = cwlctn.FindLocation(lc => lc.Address == backAddrs);
                    if (back != null)
                    {
                        if (back.Status == EnmLocationStatus.Space)
                        {
                            availLst.Add(loc);
                        }else if (back.Status == EnmLocationStatus.Occupy)
                        {
                            backOccupyLst_Big.Add(loc);
                        }
                    }
                }
                else
                {
                    availLst.Add(loc);
                }
            }
            availLst.AddRange(backOccupyLst_Big);           

            foreach (Location loc in availLst)
            {
                if (scope.LeftCol <= loc.LocColumn && loc.LocColumn <= scope.RightCol)
                {
                    return loc;
                }
            }
            return null;
        }

        /// <summary>
        /// 6174平面移动库挪移车位查找，库区只有一个车位尺寸
        /// </summary>
        /// <param name="frLct"></param>
        /// <returns></returns>
        public Location PPYAllocateLctnNeedTransfer(Location frLct)
        {
            CWLocation cwlctn = new CWLocation();
            CWICCard cwiccd = new CWICCard();

            List<Location> lctnLst = cwlctn.FindLocList();
            //找出1、2边空闲的车位
            var query_same = from loc in lctnLst
                             where loc.Type == EnmLocationType.Normal &&
                                 loc.Status == EnmLocationStatus.Space &&
                                 loc.Region == frLct.Region &&
                                 loc.LocSide != 3 &&
                                 compareSize(loc.LocSize, frLct.LocSize) >= 0 &&
                                 cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null
                             orderby Math.Abs(frLct.LocColumn - loc.LocColumn)
                             select loc;

            List<Location> availLst = new List<Location>();
            foreach (Location loc in query_same)
            {
                //如果是1边车位，后面的车位要是空闲或占用的
                if (loc.LocSide == 1)
                {
                    string backAddrs = string.Format((loc.LocSide + 2).ToString() + loc.Address.Substring(1));
                    Location back = cwlctn.FindLocation(lc => lc.Address ==backAddrs);
                    if (back != null)
                    {
                        if (back.Status == EnmLocationStatus.Space ||
                            back.Status == EnmLocationStatus.Occupy)
                        {
                            availLst.Add(loc);
                        }
                    }
                }
                else
                {
                    availLst.Add(loc);
                }
            }

            //再找缓存车位
            var query_temp = from loc in lctnLst
                             where loc.Type == EnmLocationType.Temporary &&
                                   loc.Status == EnmLocationStatus.Space &&
                                   loc.Region == frLct.Region &&
                                   compareSize(loc.LocSize, frLct.LocSize) >= 0
                             orderby Math.Abs(frLct.LocColumn - loc.LocColumn)
                             select loc;
            availLst.AddRange(query_temp);
            if (availLst.Count > 0)
            {
                return availLst[0];
            }
            return null;
        }

        /// <summary>
        /// 挪移时车位选择
        /// </summary>
        /// <param name="frlct"></param>
        /// <param name="tolct"></param>
        /// <returns></returns>
        public Device TransportToAllocateTV(Location frlct,Location tolct)
        {
            CWDevice cwdevice = new CWDevice();
            List<Device> nEtvList = cwdevice.FindList(d => d.Type == EnmSMGType.ETV);
            WorkScope workscope = new WorkScope(nEtvList);
            if (nEtvList.Count > 0)
            {
                List<Device> orderbyLst = nEtvList.OrderBy(d => Math.Abs(d.Region - frlct.Region)).ToList();
                foreach(Device dev in orderbyLst)
                {
                    CScope scope = workscope.GetEtvScope(dev);
                    if (scope.LeftCol <= frlct.LocColumn && frlct.LocColumn <= scope.RightCol)
                    {
                        if (scope.LeftCol <= tolct.LocColumn && tolct.LocColumn <= scope.RightCol)
                        {
                            return dev;
                        }
                    }
                }
            }
            return null;
        }



        /// <summary>
        /// 
        /// </summary>       
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
