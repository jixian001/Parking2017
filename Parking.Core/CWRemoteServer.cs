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
    /// 用于处理云服务上传上来的信息
    /// </summary>
    public class CWRemoteServer
    {
        private static RemoteFeeRecordManager manager = new RemoteFeeRecordManager();

        /// <summary>
        /// 添加，修改车位的入库时间,
        /// </summary>
        /// <param name="rcd"></param>
        /// <returns></returns>
        public Response Add(RemotePayFeeRcd rcd)
        {
            Log log = LogFactory.GetLogger("CWRemoteServer add");
            Response resp = new Response();
            try
            {
                CWLocation cwlctn = new CWLocation();
                Location loc = cwlctn.FindLocation(lc => lc.ICCode == rcd.strICCardID);
                if (loc == null)
                {
                    loc = cwlctn.FindLocation(lc => lc.PlateNum == rcd.strPlateNum);
                }
                if (loc != null)
                {                    
                    //修改车位的入库时间
                    loc.InDate = DateTime.Now;
                    cwlctn.UpdateLocation(loc);

                    rcd.Warehouse = loc.Warehouse;
                    rcd.LocAddress = loc.Address;

                    log.Info("云服务下发付款成功通知,strICCardID- " + rcd.strICCardID + " ,strPlateNum- " + rcd.strPlateNum + " ,wh- " + loc.Warehouse + " ,address- " + loc.Address);

                    return manager.Add(rcd);
                }
                else
                {
                    resp.Message = "找不到存车卡号";
                    log.Info("云服务下发缴费通知时，在库内找不到对应车位，strICCardID- " + rcd.strICCardID + " ,strPlateNum- " + rcd.strPlateNum);
                }
            }
            catch (Exception ex)
            {
                resp.Message = "出现异常- "+ex.ToString();
                log.Error(ex.ToString());
            }
            return resp;
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public RemotePayFeeRcd Find(Expression<Func<RemotePayFeeRcd, bool>> where)
        {
            return manager.Find(where);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Response Delete(int id)
        {
            return manager.Delete(id);
        }


    }
}
