using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;
using System.Linq.Expressions;

namespace Parking.Core
{
    /// <summary>
    /// 车位信息的业务逻辑
    /// </summary>
    public class CWLocation
    {
        private LocationManager manager = new LocationManager();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public Response UpdateLocation(Location loc)
        {
            Response resp = manager.Update(loc);
            if (resp.Code == 1)
            {
                MainCallback<Location>.Instance().OnChange(2, loc);
            }
            return resp;
        }

        public async Task<Response> UpdateLocationAsync(Location loc)
        {
            Response resp = await manager.UpdateAsync(loc);
            if (resp.Code == 1)
            {
                MainCallback<Location>.Instance().OnChange(2, loc);
            }
            return resp;
        }

        /// <summary>
        /// 获取所有车位
        /// </summary>
        /// <returns></returns>
        public List<Location> FindLocList()
        {
            return manager.FindList().ToList();
        }

        public async Task<List<Location>> FindLocationListAsync()
        {
            return await manager.FindListAsync();
        }

        /// <summary>
        /// 存车时获取效车位
        /// </summary>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public List<Location> FindLocationList(Expression<Func<Location, bool>> where)
        {
            return manager.FindLocationList(where, null);
        }

        public async Task<List<Location>> FindLocationListAsync(Expression<Func<Location, bool>> where)
        {
            return await manager.FindListAsync(where);
        }

        public Location FindLocation(Expression<Func<Location, bool>> where)
        {
            return manager.FindLocation(where);
        }

        public async Task<Location> FindLocationAsync(Expression<Func<Location, bool>> where)
        {
            return await manager.FindAsync(where);
        }

        public int TransportLoc(Location fromLoc, Location toLoc)
        {
            toLoc.Status = EnmLocationStatus.Occupy;
            toLoc.CarSize = fromLoc.CarSize;
            toLoc.WheelBase = fromLoc.WheelBase;
            toLoc.ICCode = fromLoc.ICCode;
            toLoc.InDate = fromLoc.InDate;
            toLoc.PlateNum = fromLoc.PlateNum;
            toLoc.ImagePath = fromLoc.ImagePath;

            UpdateLocation(toLoc);

            fromLoc.Status = EnmLocationStatus.Space;
            fromLoc.CarSize = "";
            fromLoc.WheelBase = 0;
            fromLoc.ICCode = "";
            fromLoc.InDate = DateTime.Parse("2017-1-1");
            fromLoc.PlateNum = "";
            fromLoc.ImagePath = "";

            UpdateLocation(fromLoc);

            string rcdmsg = "数据挪移，卡号 - " + toLoc.ICCode + " 源车位 - " + fromLoc.Address + " 目的车位 - " + toLoc.Address;
            OperateLog olog = new OperateLog
            {
                Description = rcdmsg,
                CreateDate = DateTime.Now,
                OptName = ""
            };
            new CWOperateRecordLog().AddOperateLog(olog);

            return 1;
        }

        public async Task<Response> DisableLocationAsync(int warehouse, string addrs, bool isDis)
        {
            Response _resp = new Response();
            try
            {
                Location loc =await new CWLocation().FindLocationAsync(lc => lc.Warehouse == warehouse && lc.Address == addrs);
                if (loc == null)
                {
                    _resp.Code = 0;
                    _resp.Message = "找不到车位-" + addrs;
                    return _resp;
                }
                if (loc.Type == EnmLocationType.Invalid ||
                    loc.Type == EnmLocationType.Hall ||
                    loc.Type == EnmLocationType.ETV)
                {
                    _resp.Code = 0;
                    _resp.Message = "当前车位-" + addrs + "无效，不允许操作！";
                    return _resp;
                }
                if (isDis)
                {
                    if (loc.Type == EnmLocationType.Normal)
                    {
                        loc.Type = EnmLocationType.Disable;
                    }
                    else if (loc.Type == EnmLocationType.Temporary)
                    {
                        loc.Type = EnmLocationType.TempDisable;
                    }
                }
                else
                {
                    if (loc.Type == EnmLocationType.Disable||
                        loc.Type==EnmLocationType.HasCarLocker)
                    {
                        loc.Type = EnmLocationType.Normal;
                    }
                    else if (loc.Type == EnmLocationType.TempDisable)
                    {
                        loc.Type = EnmLocationType.Temporary;
                    }
                }
                _resp = UpdateLocation(loc);
            }
            catch (Exception ex)
            {
                Log log = LogFactory.GetLogger("DisableLocation");
                log.Error(ex.ToString());
            }
            return _resp;
        }

        /// <summary>
        /// 数据出库
        /// </summary>
        /// <param name="wh"></param>
        /// <param name="addrs"></param>
        /// <returns></returns>
        public Response LocationOut(int warehouse, string Addrs)
        {
            Response resp = new Response();
            Location loc = FindLocation(lc => lc.Warehouse == warehouse && lc.Address == Addrs);
            if (loc == null)
            {
                resp.Message = "找不到车位-" + Addrs;
                return resp;
            }
            #region 推送停车记录给云平台
            ParkingRecord pkRecord = new ParkingRecord
            {
                TaskType = 1,
                LocAddrs = loc.Address,
                Proof = loc.ICCode,
                PlateNum = loc.PlateNum,
                carpicture = loc.ImagePath,
                CarSize = string.IsNullOrEmpty(loc.CarSize) ? 0 : Convert.ToInt32(loc.CarSize),
                LocSize = string.IsNullOrEmpty(loc.LocSize) ? 0 : Convert.ToInt32(loc.LocSize),
                InDate = loc.InDate.ToString()
            };
            CloudCallback.Instance().WatchParkingRcd(pkRecord);
            #endregion

            string iccd = loc.ICCode;

            loc.Status = EnmLocationStatus.Space;
            loc.ICCode = "";
            loc.WheelBase = 0;
            loc.CarSize = "";
            loc.InDate = DateTime.Parse("2017-1-1");
            loc.PlateNum = "";
            loc.ImagePath = "";

            resp = new CWLocation().UpdateLocation(loc);

            string rcdmsg = "数据出库，卡号 - " + iccd + " 车位 - " + loc.Address;
            OperateLog olog = new OperateLog
            {
                Description = rcdmsg,
                CreateDate = DateTime.Now,
                OptName = ""
            };
            new CWOperateRecordLog().AddOperateLog(olog);

            #region 删除存车指纹库记录
            if (!string.IsNullOrEmpty(iccd))
            {
                CWSaveProof cwsaveproof = new CWSaveProof();
                int sno = Convert.ToInt32(iccd);
                SaveCertificate scert = cwsaveproof.Find(d => d.SNO == sno);
                if (scert != null)
                {
                    cwsaveproof.Delete(scert.ID);
                }
            }
            #endregion

            return resp;
        }

        /// <summary>
        /// 数据入库
        /// </summary>
        /// <param name="orig"></param>
        /// <returns></returns>
        public Response LocationIn(Location orig)
        {
            Response resp = new Response();
            int warehouse = orig.Warehouse;
            string Addrs = orig.Address;
            Location loc = new CWLocation().FindLocation(lc => lc.Warehouse == warehouse && lc.Address == Addrs);
            if (loc == null)
            {
                resp.Message = "找不到车位-" + Addrs;
                return resp;
            }
            if (loc.Type != EnmLocationType.Normal)
            {
                resp.Message = "车位不可用，请使用另一个";
                return resp;
            }
            if (loc.Status != EnmLocationStatus.Space)
            {
                resp.Message = "当前车位不是空闲的，无法使用该车位！";
                return resp;
            }
            if (string.Compare(loc.LocSize, orig.CarSize) < 0)
            {
                resp.Message = "车位尺寸不匹配，无法完成操作！";
                return resp;
            }
            SaveCertificate scert = new SaveCertificate();
            scert.IsFingerPrint = 2;
            int proof = Convert.ToInt32(orig.ICCode);
            if (proof >= 10000)
            {
                //是指纹时，找出是否注册了
                FingerPrint fprint = new CWFingerPrint().Find(fp => fp.SN_Number == proof);
                if (fprint == null)
                {
                    resp.Message = "当前凭证是指纹编号，但库里找不到注册的指纹";
                    return resp;
                }
                scert.Proof = fprint.FingerInfo;
                scert.SNO = fprint.SN_Number;
                scert.CustID = fprint.CustID;
            }
            else
            {
                ICCard iccode = new CWICCard().Find(ic => ic.UserCode == orig.ICCode);
                if (iccode == null)
                {
                    resp.Message = "请先注册当前卡号，再使用！";
                    return resp;
                }
                if (iccode.Status != EnmICCardStatus.Normal)
                {
                    resp.Message = "该卡已注销或挂失！";
                    return resp;
                }
                scert.Proof = iccode.PhysicCode;
                scert.SNO = Convert.ToInt32(iccode.UserCode);
                scert.CustID = iccode.CustID;
            }
            #region 判断存车指纹库中，是否有该记录，如果有，则需更换信息
            CWSaveProof cwsaveprooft = new CWSaveProof();
            SaveCertificate svcert = cwsaveprooft.Find(s => s.Proof == scert.Proof);
            if (svcert != null)
            {
                resp.Message = "存车指纹库中存在该记录，当前卡号不可用！";
                return resp;
            }
            #endregion
            Location lctn = new CWLocation().FindLocation(l => l.ICCode == orig.ICCode);
            if (lctn != null)
            {
                resp.Message = "该卡已被使用，车位 - " + lctn.Address;
                return resp;
            }
            ImplementTask itask = new CWTask().Find(tsk => tsk.ICCardCode == orig.ICCode);
            if (itask != null)
            {
                resp.Message = "该卡正在作业，无法使用";
                return resp;
            }
            WorkTask wtask = new CWTask().FindQueue(wk => wk.ICCardCode == orig.ICCode);
            if (wtask != null)
            {
                resp.Message = "该卡已加入队列，无法使用";
                return resp;
            }

            loc.Status = EnmLocationStatus.Occupy;
            loc.ICCode = orig.ICCode;
            loc.WheelBase = orig.WheelBase;
            loc.CarSize = orig.CarSize;
            loc.PlateNum = orig.PlateNum;
            loc.InDate = orig.InDate;
            loc.ImagePath = "";

            resp = new CWLocation().UpdateLocation(loc);           

            string rcdmsg = "数据入库，卡号 - " + loc.ICCode + " 车位 - " + loc.Address + " 轴距 - " + loc.WheelBase + " 外形 - " + loc.CarSize;
            OperateLog olog = new OperateLog
            {
                Description = rcdmsg,
                CreateDate = DateTime.Now,
                OptName = ""
            };
            new CWOperateRecordLog().AddOperateLog(olog);

            //添加到指纹库中
            cwsaveprooft.Add(scert);

            #region 推送停车记录给云平台
            ParkingRecord pkRecord = new ParkingRecord
            {
                TaskType = 0,
                LocAddrs = loc.Address,
                Proof = loc.ICCode,
                PlateNum = loc.PlateNum,
                carpicture = loc.ImagePath,
                CarSize = Convert.ToInt32(loc.CarSize),
                LocSize = Convert.ToInt32(loc.LocSize),
                InDate = loc.InDate.ToString()
            };
            CloudCallback.Instance().WatchParkingRcd(pkRecord);
            #endregion

            return resp;
        }

        public async Task<LocStatInfo> GetLocStatisInfoAsync()
        {
            int total = 0;
            int occupy = 0;
            int space = 0;
            int fix = 0;
            int bspace = 0;
            int sspace = 0;
            try
            {
                List<Location> locLst = await new CWLocation().FindLocationListAsync();
                CWICCard cwiccd = new CWICCard();
                for (int i = 0; i < locLst.Count; i++)
                {
                    Location loc = locLst[i];
                    #region
                    if (loc.Type != EnmLocationType.Hall &&
                   loc.Type != EnmLocationType.Invalid)
                    {
                        total++;
                    }
                    bool isFixLoc = false;
                    if (cwiccd.FindFixLocationByAddress(loc.Warehouse, loc.Address) != null)
                    {
                        fix++;
                        isFixLoc = true;
                    }
                    if (loc.Type == EnmLocationType.Normal)
                    {
                        if (loc.Status == EnmLocationStatus.Space)
                        {
                            if (!isFixLoc)
                            {
                                space++;
                                if (loc.LocSize.Length == 3)
                                {
                                    string last = loc.LocSize.Substring(2);
                                    if (last == "1")
                                    {
                                        sspace++;
                                    }
                                    else if (last == "2")
                                    {
                                        bspace++;
                                    }
                                }
                            }
                        }
                        else if (loc.Status == EnmLocationStatus.Occupy)
                        {
                            occupy++;
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Log log = LogFactory.GetLogger("GetLocStatisInfoAsync");
                log.Error(ex.ToString());
            }
            LocStatInfo info = new LocStatInfo();
            info.Total = total;
            info.Occupy = occupy;
            info.Space = space;
            info.SmallSpace = sspace;
            info.BigSpace = bspace;
            info.FixLoc = fix;

            return info;
        }

        public LocStatInfo GetLocStatisInfo()
        {
            int total = 0;
            int occupy = 0;
            int space = 0;
            int fix = 0;
            int bspace = 0;
            int sspace = 0;
            try
            {
                CWICCard cwiccd = new CWICCard();
                List<Location> locLst = new CWLocation().FindLocList();
                List<Customer> custsLst = cwiccd.FindCustList(cu=>cu.Type==EnmICCardType.FixedLocation||cu.Type==EnmICCardType.VIP);
               
                for (int i = 0; i < locLst.Count; i++)
                {
                    Location loc = locLst[i];
                    #region
                    if (loc.Type != EnmLocationType.Hall &&
                   loc.Type != EnmLocationType.Invalid)
                    {
                        total++;
                    }
                    bool isFixLoc = false;
                    if (custsLst.Exists(cc=>cc.LocAddress==loc.Address&&cc.Warehouse==loc.Warehouse))
                    {
                        fix++;
                        isFixLoc = true;
                    }
                    if (loc.Type == EnmLocationType.Normal)
                    {
                        if (loc.Status == EnmLocationStatus.Space)
                        {
                            if (!isFixLoc)
                            {
                                space++;
                                if (loc.LocSize.Length == 3)
                                {
                                    string last = loc.LocSize.Substring(2);
                                    if (last == "1")
                                    {
                                        sspace++;
                                    }
                                    else if (last == "2")
                                    {
                                        bspace++;
                                    }
                                }
                            }
                        }
                        else if (loc.Status == EnmLocationStatus.Occupy)
                        {
                            occupy++;
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Log log = LogFactory.GetLogger("GetLocStatisInfoAsync");
                log.Error(ex.ToString());
            }
            LocStatInfo info = new LocStatInfo();
            info.Total = total;
            info.Occupy = occupy;
            info.Space = space;
            info.SmallSpace = sspace;
            info.BigSpace = bspace;
            info.FixLoc = fix;

            return info;
        }
    }
}
