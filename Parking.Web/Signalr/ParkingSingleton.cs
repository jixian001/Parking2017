#region
using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Parking.Data;
using Parking.Core;
using Parking.Web.Models;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Newtonsoft.Json;
#endregion

namespace Parking.Web
{
    public class ParkingSingleton
    {
        // signleton instance
        private readonly static Lazy<ParkingSingleton> _instance = new Lazy<ParkingSingleton>(
            () => new ParkingSingleton(GlobalHost.ConnectionManager.GetHubContext<ParkingHub>().Clients));

        private Log log;

        private Dictionary<string, string> dicCurrClients = new Dictionary<string, string>();

        private ParkingSingleton(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

            log = LogFactory.GetLogger("ParkingSingleton");

            MainCallback<Location>.Instance().WatchEvent += FileWatch_LctnWatchEvent;
            MainCallback<Device>.Instance().WatchEvent += FileWatch_DeviceWatchEvent;
            MainCallback<ImplementTask>.Instance().WatchEvent += FileWatch_IMPTaskWatchEvent;
            MainCallback<WorkTask>.Instance().WatchEvent += ParkingSingleton_WatchEvent;

            SingleCallback.Instance().ICCardWatchEvent += ParkingSingleton_ICCardWatchEvent;
            SingleCallback.Instance().FaultsWatchEvent += ParkingSingleton_FaultsWatchEvent;
            SingleCallback.Instance().PlateWatchEvent += ParkingSingleton_PlateWatchEvent;

            CloudCallback.Instance().ParkingRcdWatchEvent += ParkingSingleton_ParkingRcdWatchEvent;

        }

        public IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public static ParkingSingleton Instance
        {
            get
            {
                return _instance.Value;
            }
            private set {; }
        }

        public void register(string client, string connID)
        {
            lock (dicCurrClients)
            {
                if (!dicCurrClients.ContainsKey(connID))
                {
                    dicCurrClients.Add(connID, client);
                }
            }
            Clients.All.nowUsers(dicCurrClients);
        }

        public void removeClient(string connID)
        {
            lock (dicCurrClients)
            {
                if (dicCurrClients.ContainsKey(connID))
                {
                    dicCurrClients.Remove(connID);
                }
            }
            Clients.All.nowUsers(dicCurrClients);
        }

        /// <summary>
        /// 推送车位信息
        /// </summary>
        /// <param name="loc"></param>
        private void FileWatch_LctnWatchEvent(int type, Location loca)
        {
            try
            {
                #region 更新车位状态
                if (loca != null)
                {
                    Clients.All.feedbackLocInfo(loca);
                }
                #endregion
                #region 更新统计信息
                Task.Factory.StartNew(() =>
                {
                    var info = new CWLocation().GetLocStatisInfo();
                    Clients.All.feedbackStatistInfo(info);

                    var data = new
                    {
                        small = info.SmallSpace,
                        big = info.BigSpace
                    };
                    string json = JsonConvert.SerializeObject(data);
                    Clients.All.LocStateToLed(json);
                });
                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 推送设备信息
        /// </summary>
        /// <param name="entity"></param>
        private void FileWatch_DeviceWatchEvent(int type, Device smg)
        {
            try
            {
                Clients.All.feedbackDevice(smg);

                #region 推送至LED显示
                if (smg.Type != EnmSMGType.Hall)
                {
                    return;
                }

                int ttype = 0;
                string iccode = "";
                if (smg.TaskID != 0)
                {
                    ImplementTask itask = new CWTask().Find(smg.TaskID);
                    if (itask != null)
                    {
                        ttype = (int)itask.Type;
                        #region
                        if (itask.Type == EnmTaskType.GetCar ||
                            itask.Type == EnmTaskType.TempGet)
                        {
                            Location loc = new CWLocation().FindLocation(l => l.ICCode == itask.ICCardCode);
                            if (loc != null)
                            {
                                if (!string.IsNullOrEmpty(loc.PlateNum))
                                {
                                    iccode = loc.PlateNum;
                                }
                                else
                                {
                                    if (loc.ICCode.Length == 4)
                                    {
                                        iccode = "卡号" + loc.ICCode;
                                    }
                                    else
                                    {
                                        iccode = "指纹" + loc.ICCode;
                                    }
                                }
                            }
                        }
                        else if (itask.Type == EnmTaskType.SaveCar)
                        {
                            PlateMappingDev map = new CWPlate().FindPlate(smg.Warehouse, smg.DeviceCode);
                            if (map != null)
                            {
                                if (!string.IsNullOrEmpty(map.PlateNum))
                                {
                                    iccode = map.PlateNum;
                                }
                                else
                                {
                                    string ccd = itask.ICCardCode;
                                    Location loc = new CWLocation().FindLocation(d => d.ICCode == ccd);
                                    if (loc != null)
                                    {
                                        if (!string.IsNullOrEmpty(loc.PlateNum))
                                        {
                                            iccode = loc.PlateNum;
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(ccd))
                                            {
                                                if (ccd.Length == 4)
                                                {
                                                    iccode = "卡号" + ccd;
                                                }
                                                else
                                                {
                                                    iccode = "指纹" + ccd;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        #endregion
                    }
                }
                else
                {
                    if (smg.InStep == 0 && smg.OutStep == 0)
                    {
                        iccode = "欢迎光临";
                    }
                }
                if (smg.Mode != EnmModel.Automatic)
                {
                    ttype = 10;
                    iccode = "欢迎光临";
                }

                var data = new
                {
                    Warehouse = smg.Warehouse,
                    HallID = smg.DeviceCode,
                    TaskType = ttype,
                    ICCode = ""
                };
                string json = JsonConvert.SerializeObject(data);

                Clients.All.DeviceStateToLed(json);

                //需要发送时再发送
                if (!string.IsNullOrEmpty(iccode))
                {
                    var iccd = new
                    {
                        Warehouse = smg.Warehouse,
                        HallID = smg.DeviceCode,
                        Message = iccode
                    };
                    string jsonIccd = JsonConvert.SerializeObject(iccd);
                    Clients.All.ICCodeInfoToLed(jsonIccd);
                }
                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 推送执行作业信息
        /// </summary>
        /// <param name="itask"></param>
        private void FileWatch_IMPTaskWatchEvent(int type, ImplementTask itask)
        {
            #region 给页面显示处理     
            string desp = itask.Warehouse.ToString() + itask.DeviceCode.ToString();
            string ctype = PlusCvt.ConvertTaskType(itask.Type);
            string status = PlusCvt.ConvertTaskStatus(itask.Status, itask.SendStatusDetail);
            DeviceTaskDetail detail = new DeviceTaskDetail
            {
                DevDescp = desp,
                TaskType = ctype,
                Status = status,
                Proof = itask.ICCardCode
            };
            //作业要删除时
            if (type == 3)
            {
                detail.TaskType = "";
                detail.Status = "";
                detail.Proof = "";
            }
            //给界面用
            Clients.All.feedbackImpTask(detail);
            #endregion

            #region 上传执行业务给云服务
            var iRet = new
            {
                Type = type,
                SubTask = itask
            };
            string jsonstr = JsonConvert.SerializeObject(iRet);
            Clients.All.feedbackImpTaskToCloud(jsonstr);
            #endregion
        }

        /// <summary>
        /// 推送队列信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entity"></param>
        private void ParkingSingleton_WatchEvent(int type, WorkTask mtsk)
        {
            try
            {
                if (mtsk.IsMaster == 1)
                {
                    return;
                }
                #region 上传队列信息给云服务
                var iRet = new
                {
                    Type = type,
                    MasterTask = mtsk
                };
                string jsonstr = JsonConvert.SerializeObject(iRet);
                Clients.All.feedbackWorkTaskToCloud(jsonstr);
                #endregion

                Task.Factory.StartNew(() =>
                {
                    #region 给LED的
                    int count = 0;
                    int wh = mtsk.Warehouse;
                    int hallID = mtsk.DeviceCode;
                    List<WorkTask> mtskLst = new CWTask().FindQueueList(d => d.Warehouse == wh && d.DeviceCode == hallID);
                    count = mtskLst.Count;

                    var data = new
                    {
                        Warehouse = wh,
                        HallID = hallID,
                        Count = count
                    };
                    string jsonStr = JsonConvert.SerializeObject(data);
                    //推送
                    Clients.All.QueueStateToLed(jsonStr);
                    #endregion
                });
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 客户端读卡时，回推卡号
        /// </summary>
        /// <param name="msg"></param>
        private void ParkingSingleton_ICCardWatchEvent(string msg)
        {
            Clients.All.feedbackReadICCard(msg);
        }

        /// <summary>
        /// 处理故障信息上报
        /// </summary>
        /// <param name="alarmLst"></param>
        private void ParkingSingleton_FaultsWatchEvent(int warehouse, int code, List<BackAlarm> alarmLst)
        {
            #region 更新报警页面
            if (code == 11)
            {
                Clients.All.feedbackhall1alarm(alarmLst);
            }
            else if (code == 12)
            {
                Clients.All.feedbackhall2alarm(alarmLst);
            }
            else if (code == 1)
            {
                Clients.All.feedbacktv1alarm(alarmLst);
            }
            else if (code == 2)
            {
                Clients.All.feedbacktv2alarm(alarmLst);
            }
            else if (code == 2)
            {
                Clients.All.feedbacktv3alarm(alarmLst);
            }
            #endregion

            #region 主页面的红色显示
            int isRed = 0;
            if (alarmLst.Exists(t => t.Type == 2))
            {
                isRed = 1;
            }
            var data = new
            {
                Warehouse = warehouse,
                DeviceCode = code,
                IsRed = isRed
            };
            Clients.All.feedbackDeviceFaultStat(data);
            #endregion
        }       

        /// <summary>
        /// 回调车牌识别给LED、车牌识别页面
        /// </summary>
        /// <param name="platenum"></param>
        /// <param name="headpath"></param>
        /// <param name="dtime"></param>
        private void ParkingSingleton_PlateWatchEvent(PlateDisplay zresult)
        {
            #region 显示给页面
            Clients.All.feedbackPlateInfo(zresult);
            #endregion

            #region LED
            var iccd = new
            {
                Warehouse = zresult.Warehouse,
                HallID = zresult.DeviceCode,
                Message = zresult.PlateNum
            };
            string jsonIccd = JsonConvert.SerializeObject(iccd);
            Clients.All.ICCodeInfoToLed(jsonIccd);
            #endregion

        }

        /// <summary>
        /// 停车记录推送到云平台
        /// </summary>
        /// <param name="record"></param>
        private void ParkingSingleton_ParkingRcdWatchEvent(ParkingRecord record)
        {
            string jsonstr = JsonConvert.SerializeObject(record);
            Clients.All.feedbackParkingRecordToCloud(jsonstr);
        }

    }
}