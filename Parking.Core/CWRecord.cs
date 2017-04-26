using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;
using System.Reflection;

namespace Parking.Core
{
    public class CWTelegramLog
    {
        private TelegramLogManager manager = new TelegramLogManager();
        private Log log;

        public CWTelegramLog()
        {
            log = LogFactory.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
        /// <summary>
        /// 报文收发的记录（1：发送，2：接收）
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">1：发送，2：接收</param>
        public void AddRecord(Int16[] data,int type)
        {
            TelegramLog tlog = new TelegramLog();
            try
            {
                tlog.RecordDtime = DateTime.Now;
                tlog.Type = type;
                if (type == 1)
                {
                    tlog.Warehouse = data[1];
                }
                if (type == 2)
                {
                    tlog.Warehouse = data[0];
                }
                if (data[4] != 0)
                {
                    tlog.Telegram = "(" + data[2] + "," + data[3] + "," + data[4]+")";
                }
                else
                {
                    tlog.Telegram = "(" + data[2] + "," + data[3] + ")";
                }
                tlog.DeviceCode = data[6];
                tlog.ICCode = data[11].ToString();
                tlog.CarInfo = data[23] + "," + data[25] + "," + data[47];
                string fromAddrs = data[30]+"边"+data[31]+"列"+data[32]+"层";
                string toAddrs= data[35] + "边" + data[36] + "列" + data[37] + "层";
                tlog.FromAddress = fromAddrs;
                tlog.ToAddress = toAddrs;                
                tlog.TelegramID = data[48];

                manager.Add(tlog);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 查询报文信息
        /// </summary>      
        public List<TelegramLog> FindPageList(int pageSize,int pageIndex,DateTime start,DateTime end,string queryName,string queryContent)
        {
            return manager.FindList().ToList();
        }


    }



}
