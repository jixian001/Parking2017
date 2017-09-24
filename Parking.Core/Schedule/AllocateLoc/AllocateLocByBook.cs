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
    /// <summary>
    /// 依车位尺寸查找车位
    /// </summary>
    public class AllocateLocByBook
    {
        /// <summary>
        /// 分配车位:
        /// 1、优先分配中间区域的车位
        /// 2、判断ETV可达，车厅可达
        /// </summary>
        /// <param name="checkcode"></param>
        /// <returns></returns>
        public Location AllocateLoc(string checkcode)
        {
            //优先分配中间区域的车位
            int middlecolmn = 9;

            CWLocation cwlctn = new CWLocation();
            CWDevice cwdevice = new CWDevice();
            CWTask cwtask = new CWTask();
            CWICCard cwiccd = new CWICCard();

            #region 依距车厅的远近找出所有车位的集合          
            List<Location> allLocLst = cwlctn.FindLocList();
            List<Customer> fixCustLst = cwiccd.FindCustList(cu => cu.Type == EnmICCardType.FixedLocation || cu.Type == EnmICCardType.VIP);
            //排除固定车位
            List<Location> suitLocsLst = allLocLst.FindAll(lc => fixCustLst.Exists(cc => cc.LocAddress == lc.Address && cc.Warehouse == lc.Warehouse) == false);

           
            List<Location> sameLctnLst = new List<Location>();
            
            var locList_small = from loc in suitLocsLst
                                where loc.Type == EnmLocationType.Normal &&
                                      loc.Status == EnmLocationStatus.Space &&
                                      string.Compare(loc.LocSize, checkcode) == 0
                                orderby loc.LocSide ascending, Math.Abs(loc.LocColumn - middlecolmn) ascending                                        
                                select loc;

            var locList_big = from loc in suitLocsLst
                              where loc.Type == EnmLocationType.Normal &&
                                    loc.Status == EnmLocationStatus.Space &&
                                    string.Compare(loc.LocSize, checkcode) > 0
                              orderby loc.LocSide ascending, Math.Abs(loc.LocColumn - middlecolmn) ascending
                              select loc;

            sameLctnLst.AddRange(locList_small);
            sameLctnLst.AddRange(locList_big);

            List<Location> lctnLst = new List<Location>();
            #region 如果是重列车位，优先分配前面车位是空闲的，其次是占用的，最后才是执行中的
            List<Location> spaceLctnLst = new List<Location>();
            List<Location> occupyLctnLst = new List<Location>();
            List<Location> otherLctnLst = new List<Location>();
            foreach (Location loc in sameLctnLst)
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

            if (lctnLst.Count == 0)
            {
                return null;
            }

            List<Device> nEtvList = cwdevice.FindList(d => d.Type == EnmSMGType.ETV);
            WorkScope workscope = new WorkScope(nEtvList);
            List<Device> availHallLst = cwdevice.FindList(d => d.Type == EnmSMGType.Hall && d.Mode == EnmModel.Automatic);
            List<Device> availEtvLst = nEtvList.FindAll(d => d.Mode == EnmModel.Automatic);

            if (availEtvLst.Count == 0 || 
                availHallLst.Count == 0)
            {
                return null;
            }

            if (availEtvLst.Count == 2 && availHallLst.Count == 2)
            {
                return lctnLst.FirstOrDefault();
            }
            if (availEtvLst.Count == 1)
            {
                CScope scope = workscope.GetEtvScope(availEtvLst[0]);
                if (availHallLst.Count == 2)
                {
                    foreach(Location loc in lctnLst)
                    {
                        if (loc.LocColumn >= scope.LeftCol && loc.LocColumn <= scope.RightCol)
                        {
                            return loc;
                        }
                    }
                }
                else
                {
                    Device hall = availHallLst[0];
                    int hallcolmn = Convert.ToInt32(hall.Address.Substring(1,2));
                    if (hallcolmn >= scope.LeftCol && hallcolmn <= scope.RightCol)
                    {
                        foreach (Location loc in lctnLst)
                        {
                            if (loc.LocColumn >= scope.LeftCol && loc.LocColumn <= scope.RightCol)
                            {
                                return loc;
                            }
                        }
                    }
                }
            }

            #endregion

            return null;
        }

    }
}
