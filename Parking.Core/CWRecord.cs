#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;
using System.Reflection;
using System.Text.RegularExpressions;
#endregion

namespace Parking.Core
{
    public class CWTelegramLog
    {
        private static TelegramLogManager manager = new TelegramLogManager();
        private static Log log;

        public CWTelegramLog()
        {
            log = LogFactory.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
        /// <summary>
        /// 报文收发的记录（1：发送，2：接收）
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">1：发送，2：接收</param>
        public void AddRecord(Int16[] data, int type)
        {
            TelegramLog tlog = new TelegramLog();
            try
            {
                tlog.RecordDtime = DateTime.Now;
                tlog.Type = type;
                if (type == 1)
                {
                    tlog.Warehouse = data[0];
                }
                if (type == 2)
                {
                    tlog.Warehouse = data[1];
                }
                if (data[4] != 0)
                {
                    tlog.Telegram = "(" + data[2] + "," + data[3] + "," + data[4] + ")";
                }
                else
                {
                    tlog.Telegram = "(" + data[2] + "," + data[3] + ")";
                }
                tlog.DeviceCode = data[6];
                tlog.ICCode = data[11].ToString();
                tlog.CarInfo = data[23] + "," + data[25] + "," + data[47];
                string fromAddrs = data[30] + "边" + data[31] + "列" + data[32] + "层";
                string toAddrs = data[35] + "边" + data[36] + "列" + data[37] + "层";
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
        public List<TelegramLog> FindPageList(int pageSize, int pageIndex, DateTime start, DateTime end, string queryName, string queryContent, out int totalCount)
        {
            totalCount = 0;
            List<TelegramLog> telegramLst = manager.FindList(tl => tl.RecordDtime >= start && tl.RecordDtime <= end);
            List<TelegramLog> queryTelegram = new List<TelegramLog>();
            if (queryName == "0")
            {
                queryTelegram.AddRange(telegramLst);
            }
            else
            {
                #region
                if (!string.IsNullOrEmpty(queryContent))
                {
                    if (queryName == "Warehouse")
                    {
                        queryTelegram.AddRange(telegramLst.Where(lst => queryContent.Contains(lst.Warehouse.ToString())));
                    }
                    else if (queryName == "DeviceCode")
                    {
                        //获取数字部分
                        string result = Regex.Replace(queryContent, @"[^0-9]+", "");
                        if (queryContent.Contains("厅"))
                        {
                            int hallcode = Convert.ToInt32(result) + 10;
                            queryTelegram.AddRange(telegramLst.Where(lst => lst.DeviceCode == hallcode));
                        }
                        else
                        {
                            int tv = Convert.ToInt32(result);
                            queryTelegram.AddRange(telegramLst.Where(lst => lst.DeviceCode == tv));
                        }
                    }
                    else if (queryName == "ICCode")
                    {
                        queryTelegram.AddRange(telegramLst.Where(lst => lst.ICCode == queryContent));
                    }
                    else if (queryName == "CarInfo")
                    {
                        queryTelegram.AddRange(telegramLst.Where(lst => lst.CarInfo.Contains(queryContent)));
                    }
                    else if (queryName == "Telegram")
                    {
                        queryTelegram.AddRange(telegramLst.Where(lst => lst.Telegram.Contains(queryContent)));
                    }
                    else if (queryName == "Address")
                    {
                        queryTelegram.AddRange(telegramLst.Where(lst => lst.FromAddress == queryContent || lst.ToAddress == queryContent));
                    }
                    else if (queryName == "SaveNum")
                    {
                        queryTelegram.AddRange(telegramLst.Where(lst => lst.Telegram.Contains("(1,9)")));
                    }
                    else if (queryName == "GetNum")
                    {
                        queryTelegram.AddRange(telegramLst.Where(lst => lst.Telegram.Contains("(2,1)") || lst.Telegram.Contains("(3,1)")));
                    }                  

                }
                #endregion
            }
            totalCount = queryTelegram.Count;
            if (pageIndex == 0 || pageSize == 0)
            {
                return queryTelegram;
            }

            return queryTelegram.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }


    }

    public class CWOperateRecordLog
    {
        private static OperateLogManager manager = new OperateLogManager();

        public List<OperateLog> FindPageList(int pageSize, int pageIndex, DateTime start, DateTime end, string queryName, string queryContent, out int totalCount)
        {
            totalCount = 0;
            List<OperateLog> infoLst = manager.FindList(tl => tl.CreateDate >= start && tl.CreateDate <= end);
            List<OperateLog> queryInfo = new List<OperateLog>();
            if (queryName == "0")
            {
                queryInfo.AddRange(infoLst);
            }
            else
            {
                if (queryName == "Description")
                {
                    if (!string.IsNullOrEmpty(queryContent))
                    {
                        queryInfo.AddRange(infoLst.Where(lst => lst.Description.Contains(queryContent)));
                    }
                }
                else
                {
                    queryInfo.AddRange(infoLst.Where(lst => lst.OptName.Contains(queryContent)));
                }
            }
            totalCount = queryInfo.Count;

            if (pageIndex == 0 || pageSize == 0)
            {
                return queryInfo;
            }
            return queryInfo.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }
    }

    public class CWFaultLog
    {
        private static FaultLogManager manager = new FaultLogManager();

        public List<FaultLog> FindPageList(int pageSize, int pageIndex, DateTime start, DateTime end, string queryName, string queryContent, out int totalCount)
        {
            totalCount = 0;
            List<FaultLog> infoLst = manager.FindList(tl => tl.CreateDate >= start && tl.CreateDate <= end);
            List<FaultLog> queryInfo = new List<FaultLog>();
            if (queryName == "0")
            {
                queryInfo.AddRange(infoLst);
            }
            else
            {
                if (!string.IsNullOrEmpty(queryContent))
                {
                    if (queryName == "Description")
                    {

                        queryInfo.AddRange(infoLst.Where(lst => lst.Description.Contains(queryContent)));

                    }
                    else if (queryName == "Warehouse")
                    {
                        queryInfo.AddRange(infoLst.Where(lst => queryContent.Contains(lst.Warehouse.ToString())));
                    }
                    else if (queryName == "DeviceCode")
                    {
                        //获取数字部分
                        string result = Regex.Replace(queryContent, @"[^0-9]+", "");
                        if (queryContent.Contains("厅"))
                        {
                            int hallcode = Convert.ToInt32(result) + 10;
                            queryInfo.AddRange(infoLst.Where(lst => lst.DeviceCode == hallcode));
                        }
                        else
                        {
                            int tv = Convert.ToInt32(result);
                            queryInfo.AddRange(infoLst.Where(lst => lst.DeviceCode == tv));
                        }
                    }
                }
            }
            totalCount = queryInfo.Count;
            if (pageIndex == 0 || pageSize == 0)
            {
                return queryInfo;
            }
            return queryInfo.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        }
    }
   
}
