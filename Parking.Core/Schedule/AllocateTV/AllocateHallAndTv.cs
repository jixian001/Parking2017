using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Core;

namespace Parking.Core.Schedule
{
    public class AllocateHallAndTv
    {
        /// <summary>
        /// 自由分配车厅,以取车位的区域、ETV作用范围来决定
        /// 车厅分配原则：
        ///  一、两车厅都可用的；
        ///     如果两个车厅都空闲；
        ///     1、如果两ETV都空闲可用，则从哪里刷卡，哪里出车;
        ///     2、如果有一台ETV空闲可用，如果可达车位，则选择与ETV区域一致的车厅出车;
        ///     3、如果两台ETV都忙，则分配与车位区域一致的车厅出车 ;      
        ///  二、只有一个车厅可用的，则只能选一个;
        /// </summary>
        /// <param name="mohall"></param>
        /// <param name="loc"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public Device Allocate(Device mohall, Location loc)
        {
            CWDevice cwdevice = new CWDevice();
            List<Device> nEtvList = cwdevice.FindList(d => d.Type == EnmSMGType.ETV);
            WorkScope workscope = new WorkScope(nEtvList);
            int hallColmn = Convert.ToInt32(mohall.Address.Substring(1, 2));
            int locColmn = loc.LocColumn;
            //找出模式是全自动的出车厅的集合
            List<Device> hallsLst = cwdevice.FindList(h => h.Type == EnmSMGType.Hall &&
                                                           h.HallType != EnmHallType.Entrance &&
                                                           h.Mode == EnmModel.Automatic);
            if (hallsLst.Count == 2)
            {
                List<Device> availEtvsLst = nEtvList.FindAll(e => e.IsAble == 1);
                #region
                List<Device> freeHallsLst = hallsLst.FindAll(h => h.TaskID == 0).OrderBy(d => Math.Abs(d.Region - loc.Region)).ToList();
                //如果两个车厅都空闲的
                if (freeHallsLst.Count == 2)
                {
                    #region
                    if (availEtvsLst.Count == 2)
                    {
                        return mohall;
                    }
                    else if (availEtvsLst.Count == 1)
                    {
                        #region
                        Device cetv = availEtvsLst[0];
                        CScope scope = workscope.GetEtvScope(cetv);
                        if (locColmn >= scope.LeftCol && locColmn <= scope.RightCol)
                        {
                            //优先刷卡车厅
                            if ((hallColmn >= scope.LeftCol && hallColmn <= scope.RightCol))
                            {
                                return mohall;
                            }
                            //如果没有的，则再查找
                            foreach (Device ha in freeHallsLst)
                            {
                                int hallCol = Convert.ToInt32(ha.Address.Substring(1, 2));
                                if (hallCol >= scope.LeftCol && hallCol <= scope.RightCol)
                                {
                                    return ha;
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                else if (freeHallsLst.Count == 1)
                {
                    Device hall = freeHallsLst[0];
                    #region
                    //在空闲的车厅刷卡
                    if (hall.DeviceCode == mohall.DeviceCode &&
                        hall.Warehouse == mohall.Warehouse)
                    {
                        #region
                        if (availEtvsLst.Count == 2)
                        {
                            return mohall;
                        }
                        else if (availEtvsLst.Count == 1)
                        {
                            Device cetv = availEtvsLst[0];
                            CScope scope = workscope.GetEtvScope(cetv);
                            if (locColmn >= scope.LeftCol && locColmn <= scope.RightCol)
                            {
                                if ((hallColmn >= scope.LeftCol && hallColmn <= scope.RightCol))
                                {
                                    return mohall;
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region
                        //不在空闲的车厅刷卡
                        if (availEtvsLst.Count == 2)
                        {
                            //取车位与空闲车厅是一个区的
                            if (hall.Region == loc.Region)
                            {
                                return hall;
                            }
                            else
                            {
                                //或在公共区域内，也允许空闲的车厅去接车
                                int min = 5;
                                int max = 14;
                                if (locColmn > min && locColmn < max)
                                {
                                    return hall;
                                }
                                else
                                {
                                    return mohall;
                                }
                            }
                        }
                        else if (availEtvsLst.Count == 1)
                        {
                            Device cetv = availEtvsLst[0];
                            CScope scope = workscope.GetEtvScope(cetv);

                            int hcolmn = Convert.ToInt32(hall.Address.Substring(1, 2));

                            if (locColmn >= scope.LeftCol && locColmn <= scope.RightCol)
                            {
                                if ((hcolmn >= scope.LeftCol && hcolmn <= scope.RightCol))
                                {
                                    return hall;
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }

                List<Device> busyHallsLst = hallsLst.FindAll(h => h.TaskID != 0).OrderBy(d => Math.Abs(d.Region - loc.Region)).ToList();
                //如果两个都忙
                if (busyHallsLst.Count == 2)
                {
                    if (availEtvsLst.Count == 2)
                    {
                        return busyHallsLst[0];
                    }
                    else if (availEtvsLst.Count == 1)
                    {
                        #region
                        Device cetv = availEtvsLst[0];
                        CScope scope = workscope.GetEtvScope(cetv);
                        if (locColmn >= scope.LeftCol && locColmn <= scope.RightCol)
                        {
                            //如果没有的，则再查找
                            foreach (Device ha in busyHallsLst)
                            {
                                int hallCol = Convert.ToInt32(ha.Address.Substring(1, 2));
                                if (hallCol >= scope.LeftCol && hallCol <= scope.RightCol)
                                {
                                    return ha;
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }

            #region 找不到最佳出车厅，则选择当前车厅
            Device moEtv = null;
            foreach (Device etv in nEtvList)
            {
                if (etv.IsAble == 1)
                {
                    CScope scope = workscope.GetEtvScope(etv);
                    if ((hallColmn >= scope.LeftCol && hallColmn <= scope.RightCol) &&
                        (locColmn >= scope.LeftCol && locColmn <= scope.RightCol))
                    {
                        moEtv = etv;
                        break;
                    }
                }
            }
            if (moEtv == null)
            {
                return null;
            }
            return mohall;
            #endregion
        }

    }
}
