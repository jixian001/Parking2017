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
        /// 
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
                List<Device> orderbyLst = availableLst.OrderBy(d => Math.Abs(d.Region - loc.Region)).ToList();
                return orderbyLst[0];
            }
            return null;
        }

    }
}
