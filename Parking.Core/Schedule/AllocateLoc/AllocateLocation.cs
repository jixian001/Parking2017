﻿using System;
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
            CWLocation cwlctn = new CWLocation();

            if (cust.Type == EnmICCardType.Temp ||
                cust.Type == EnmICCardType.Periodical)
            {
                #region 判断是否是预定了车位
                lct = cwlctn.FindLocation(cc => cc.Type == EnmLocationType.Normal && cc.Status == EnmLocationStatus.Book && cc.PlateNum == cust.PlateNum);
                if (lct != null)
                {
                    log.Info("当前车牌已预定车位，address-" + lct.Address + " ,wh- " + lct.Warehouse + " ,plate- " + cust.PlateNum + " ,cust- " + cust.UserName);
                    if (compareSize(lct.LocSize, checkCode) >= 0)
                    {
                        //分配ETV
                        Device dev = PXDAllocateEtvOfFixLoc(hall, lct);
                        if (dev != null)
                        {
                            smg = dev.DeviceCode;
                        }
                        return lct;
                    }
                    else
                    {
                        log.Info("当前预定车位，address-" + lct.Address + " ,wh- " + lct.Warehouse + " ,locsize- " + lct.LocSize + ",carsize- " + checkCode + " 尺寸不合适，按临时卡分配");
                        //释放预定的车位
                        lct.Status = EnmLocationStatus.Space;
                        lct.PlateNum = "";
                        lct.InDate= DateTime.Parse("2017-1-1");
                        cwlctn.UpdateLocation(lct);
                        //按临时车位处理
                        lct = this.PXDAllocate(hall, checkCode, out smg);
                    }
                }
                else  //没有预定车位，按临时车处理
                {
                    lct = this.PXDAllocate(hall, checkCode, out smg);
                }
                #endregion
            }
            else if (cust.Type == EnmICCardType.FixedLocation)
            {
                if (cust.Warehouse != warehouse)
                {
                    //车辆停在其他库区
                    cwtask.AddNofication(warehouse, hallCode, "16.wav");
                    return null;
                }
                lct = new CWLocation().FindLocation(l => l.Warehouse == cust.Warehouse && l.Address == cust.LocAddress);
                if (lct == null)
                {
                    cwtask.AddNofication(warehouse, hallCode, "20.wav");  
                    log.Error("固定车位时，依地址找不到对应车位，address - "+cust.LocAddress);
                    return null;
                }

                if (lct != null)
                {
                    if (compareSize(lct.LocSize, checkCode) < 0)
                    {
                        //车位尺寸与车辆不匹配
                        cwtask.AddNofication(warehouse, hallCode, "61.wav");
                        return null;
                    }
                }
                if (lct.Type != EnmLocationType.Normal)
                {
                    cwtask.AddNofication(warehouse, hallCode, "69.wav");
                    return null;
                }
                //如果是重列车位，判断前面车位是否是正常的
                if (lct.NeedBackup == 1)
                {
                    string fwdaddrs = (lct.LocSide - 2).ToString() + lct.Address.Substring(1);
                    Location forward = new CWLocation().FindLocation(l => l.Address == fwdaddrs);
                    if (forward != null)
                    {
                        if (forward.Type != EnmLocationType.Normal)
                        {
                            //前面车位已被禁用
                            cwtask.AddNofication(warehouse, hallCode, "15.wav");
                            return null;
                        }
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

        #region 巷道堆垛
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
            CWICCard cwiccd = new CWICCard();

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
            List<Customer> fixCustLst = cwiccd.FindCustList(cu => cu.Type == EnmICCardType.FixedLocation || cu.Type == EnmICCardType.VIP);
            //排除固定车位
            allLocLst = allLocLst.FindAll(lc => fixCustLst.Exists(cc => cc.LocAddress == lc.Address && cc.Warehouse == lc.Warehouse) == false);

            #region 车位尺寸一致
            List<Location> sameLctnLst = new List<Location>();

            #region
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
            sameLctnLst.AddRange(L42_locList_small);
            sameLctnLst.AddRange(L1_locList_small);

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
            sameLctnLst.AddRange(L42_locList_width);
            sameLctnLst.AddRange(L1_locList_width);

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
            sameLctnLst.AddRange(L42_locList_small_dif);
            sameLctnLst.AddRange(L1_locList_small_dif);

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
            sameLctnLst.AddRange(L42_locList_width_dif);
            sameLctnLst.AddRange(L1_locList_width_dif);
            #endregion

            #region 如果是重列车位，优先分配前面车位是空闲的，其次是占用的，最后才是执行中的
            List<Location> spaceLctnLst = new List<Location>();
            List<Location> occupyLctnLst = new List<Location>();
            List<Location> otherLctnLst = new List<Location>();
            foreach(Location loc in sameLctnLst)
            {
                if (loc.LocSide == 4)
                {
                    #region 判断前面车位状态
                    string fwdAddrs = "2" + loc.Address.Substring(1);
                    Location forward = cwlctn.FindLocation(l => l.Address == fwdAddrs);
                    if (forward != null)
                    {
                        if (forward.Type == EnmLocationType.Normal)
                        {
                            if (forward.Status == EnmLocationStatus.Space)
                            {
                                spaceLctnLst.Add(loc);
                            }
                            else if (forward.Status == EnmLocationStatus.Occupy)
                            {
                                occupyLctnLst.Add(loc);
                            }
                            else
                            {
                                otherLctnLst.Add(loc);
                            }
                        }
                    }
                    #endregion
                }
                else if (loc.LocSide == 2)
                {
                    #region 判断后面车位状态
                    string bckAddrs = "4" + loc.Address.Substring(1);
                    Location back = cwlctn.FindLocation(l => l.Address == bckAddrs);
                    if (back != null)
                    {
                        if (back.Type == EnmLocationType.Normal)
                        {
                            if (back.Status == EnmLocationStatus.Space)
                            {
                                spaceLctnLst.Add(loc);
                            }
                            else if (back.Status == EnmLocationStatus.Occupy)
                            {
                                occupyLctnLst.Add(loc);
                            }
                            else
                            {
                                otherLctnLst.Add(loc);
                            }
                        }
                        else //禁用的
                        {
                            spaceLctnLst.Add(loc);
                        }
                    }

                    #endregion
                }
                else
                {
                    spaceLctnLst.Add(loc);
                }
            }
            lctnLst.AddRange(spaceLctnLst);
            lctnLst.AddRange(occupyLctnLst);
            lctnLst.AddRange(otherLctnLst);
            #endregion

            #endregion

            #region 车位尺寸是大的
            List<Location> bigLctnLst = new List<Location>();

            #region
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
            bigLctnLst.AddRange(L42_locList_big);
            bigLctnLst.AddRange(L1_locList_big);

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
            bigLctnLst.AddRange(L42_locList_big_dif);
            bigLctnLst.AddRange(L1_locList_big_dif);

            #endregion

            #region 如果是重列车位，优先分配前面车位是空闲的，其次是占用的，最后才是执行中的
            List<Location> spaceLctnLst_big = new List<Location>();
            List<Location> occupyLctnLst_big = new List<Location>();
            List<Location> otherLctnLst_big = new List<Location>();
            foreach (Location loc in bigLctnLst)
            {
                if (loc.LocSide == 4)
                {
                    #region 判断前面车位状态
                    string fwdAddrs = "2" + loc.Address.Substring(1);
                    Location forward = cwlctn.FindLocation(l => l.Address == fwdAddrs);
                    if (forward != null)
                    {
                        if (forward.Type == EnmLocationType.Normal)
                        {
                            if (forward.Status == EnmLocationStatus.Space)
                            {
                                spaceLctnLst_big.Add(loc);
                            }
                            else if (forward.Status == EnmLocationStatus.Occupy)
                            {
                                occupyLctnLst_big.Add(loc);
                            }
                            else
                            {
                                otherLctnLst_big.Add(loc);
                            }
                        }
                    }
                    #endregion
                }
                else if (loc.LocSide == 2)
                {
                    #region 判断后面车位状态
                    string bckAddrs = "4" + loc.Address.Substring(1);
                    Location back = cwlctn.FindLocation(l => l.Address == bckAddrs);
                    if (back != null)
                    {
                        if (back.Type == EnmLocationType.Normal)
                        {
                            if (back.Status == EnmLocationStatus.Space)
                            {
                                spaceLctnLst_big.Add(loc);
                            }
                            else if (back.Status == EnmLocationStatus.Occupy)
                            {
                                occupyLctnLst_big.Add(loc);
                            }
                            else
                            {
                                otherLctnLst_big.Add(loc);
                            }
                        }
                        else //禁用的
                        {
                            spaceLctnLst_big.Add(loc);
                        }
                    }

                    #endregion
                }
                else
                {
                    spaceLctnLst_big.Add(loc);
                }
            }
            lctnLst.AddRange(spaceLctnLst_big);
            lctnLst.AddRange(occupyLctnLst_big);
            lctnLst.AddRange(otherLctnLst_big);
            #endregion

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
        #endregion

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
