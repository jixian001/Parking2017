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
    /// 需要二次分配车位的，
    /// 1、复检时外形不合格
    /// 2、卸载时，检测到车位上有车
    /// </summary>
    public class AllocateLocBySecond
    {
        /// <summary>
        /// 依ETV作业范围，找出符合尺寸的车位
        /// 如果是重列车位，则前面的车位，一定是空闲的
        /// </summary>
        /// <param name="etv"></param>
        /// <returns></returns>
        public Location AllocateLocOfEtvScope(Device etv, string checkcode)
        {
            CWLocation cwlctn = new CWLocation();
            CWDevice cwdevice = new CWDevice();
            CWTask cwtask = new CWTask();
            CWICCard cwiccd = new CWICCard();

            List<Device> nEtvList = cwdevice.FindList(d => d.Type == EnmSMGType.ETV);
            WorkScope workscope = new WorkScope(nEtvList);
            CScope scope = workscope.GetEtvScope(etv);
            int etvColmn = Convert.ToInt32(etv.Address.Substring(1, 2));

            List<Location> allLocLst = cwlctn.FindLocList();

            var queryLstsmall = from loc in allLocLst
                                where loc.Type == EnmLocationType.Normal &&
                                      loc.Status == EnmLocationStatus.Space &&
                                      compareSize(loc.LocSize, checkcode) == 1 &&
                                      cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null
                                orderby Math.Abs(loc.LocColumn - etvColmn) ascending,
                                        loc.LocSide ascending
                                select loc;

            List<Location> priorLocsLst = new List<Location>();
            List<Location> nextLocsLst = new List<Location>();
            foreach (Location loc in queryLstsmall)
            {
                if (loc.LocColumn >= scope.LeftCol && loc.LocColumn <= scope.RightCol)
                {
                    #region
                    if (loc.LocSide == 2)
                    {
                        #region 如果后边的车位在作业，则最后分配
                        string bckAddrs = "4" + loc.Address.Substring(1);
                        Location back = cwlctn.FindLocation(l => l.Address == bckAddrs);
                        if (back != null)
                        {
                            if (back.Type == EnmLocationType.Normal)
                            {
                                if (back.Status == EnmLocationStatus.Space)
                                {
                                    priorLocsLst.Add(loc);
                                }
                                else
                                {
                                    nextLocsLst.Add(loc);
                                }
                            }
                            else if (back.Type == EnmLocationType.Disable)
                            {
                                priorLocsLst.Add(loc);
                            }
                        }
                        #endregion
                    }
                    else if (loc.LocSide == 4)
                    {
                        #region 如果是重列，则前面的车位一定是空闲的
                        string fwdAddrs = "2" + loc.Address.Substring(1);
                        Location forward = cwlctn.FindLocation(l => l.Address == fwdAddrs);
                        if (forward != null)
                        {
                            if (forward.Type == EnmLocationType.Normal &&
                                forward.Status == EnmLocationStatus.Space)
                            {
                                priorLocsLst.Add(loc);
                            }
                        }
                        #endregion
                    }
                    else if (loc.LocSide == 1)
                    {
                        priorLocsLst.Add(loc);
                    }
                    #endregion
                }
            }
            if (priorLocsLst.Count > 0)
            {
                return priorLocsLst[0];
            }
            if (nextLocsLst.Count > 0)
            {
                return nextLocsLst[0];
            }

            var queryLstbig = from loc in allLocLst
                              where loc.Type == EnmLocationType.Normal &&
                                    loc.Status == EnmLocationStatus.Space &&
                                    compareSize(loc.LocSize, checkcode) > 1 &&
                                    cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) == null
                              orderby Math.Abs(loc.LocColumn - etvColmn) ascending,
                                      loc.LocSide ascending
                              select loc;

            List<Location> priorLocsLstbig = new List<Location>();
            List<Location> nextLocsLstbig = new List<Location>();
            foreach (Location loc in queryLstbig)
            {
                if (loc.LocColumn >= scope.LeftCol && loc.LocColumn <= scope.RightCol)
                {
                    #region
                    if (loc.LocSide == 2)
                    {
                        #region 如果后边的车位在作业，则最后分配
                        string bckAddrs = "4" + loc.Address.Substring(1);
                        Location back = cwlctn.FindLocation(l => l.Address == bckAddrs);
                        if (back != null)
                        {
                            if (back.Type == EnmLocationType.Normal)
                            {
                                if (back.Status == EnmLocationStatus.Space)
                                {
                                    priorLocsLstbig.Add(loc);
                                }
                                else
                                {
                                    nextLocsLstbig.Add(loc);
                                }
                            }
                            else if (back.Type == EnmLocationType.Disable)
                            {
                                priorLocsLstbig.Add(loc);
                            }
                        }
                        #endregion
                    }
                    else if (loc.LocSide == 4)
                    {
                        #region 如果是重列，则前面的车位一定是空闲的
                        string fwdAddrs = "2" + loc.Address.Substring(1);
                        Location forward = cwlctn.FindLocation(l => l.Address == fwdAddrs);
                        if (forward != null)
                        {
                            if (forward.Type == EnmLocationType.Normal &&
                                forward.Status == EnmLocationStatus.Space)
                            {
                                priorLocsLstbig.Add(loc);
                            }
                        }
                        #endregion
                    }
                    else if (loc.LocSide == 1)
                    {
                        priorLocsLstbig.Add(loc);
                    }
                    #endregion
                }
            }

            if (priorLocsLstbig.Count > 0)
            {
                return priorLocsLstbig[0];
            }
            if (nextLocsLstbig.Count > 0)
            {
                return nextLocsLstbig[0];
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
