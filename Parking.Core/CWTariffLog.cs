using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;

namespace Parking.Core
{
    public class CWTariffLog
    {
        #region 临时收费记录
        private TempChargeLogManager templogManager = new TempChargeLogManager();

        /// <summary>
        /// 添加临时收费记录
        /// </summary>
        public async Task<Response> AddTempLogAsync(TempUserChargeLog templog)
        {
            return await templogManager.AddAsync(templog);
        }

        /// <summary>
        /// 临时用户缴费记录
        /// </summary>
        /// <returns></returns>
        public List<TempUserChargeLog> FindPageListForTempChgLog(int pageSize, int pageIndex, DateTime start, DateTime end, string queryName, string queryContent, out int totalCount)
        {
            totalCount = 0;
            List<TempUserChargeLog> tempchglogLst = new List<TempUserChargeLog>();
            Log log = LogFactory.GetLogger("FindPageListForTempChgLog");
            try
            {

                tempchglogLst = templogManager.FindList(tl => tl.RecordDTime >= start && tl.RecordDTime <= end);
                List<TempUserChargeLog> queryFixLog = new List<TempUserChargeLog>();
                if (queryName == "0")
                {
                    queryFixLog.AddRange(tempchglogLst);
                }
                else
                {
                    #region
                    if (!string.IsNullOrEmpty(queryContent))
                    {
                        if (queryName == "Address")
                        {
                            queryFixLog.AddRange(tempchglogLst.Where(lst => lst.Address == queryContent));
                        }
                        else if (queryName == "Plate")
                        {
                            queryFixLog.AddRange(tempchglogLst.Where(lst => lst.Plate.Contains(queryContent) || lst.Plate == queryContent));
                        }
                        else if (queryName == "Proof")
                        {
                            queryFixLog.AddRange(tempchglogLst.Where(lst => lst.Proof == queryContent));
                        }
                        else if (queryName == "OprtCode")
                        {
                            queryFixLog.AddRange(tempchglogLst.Where(lst => lst.OprtCode == queryContent));
                        }

                    }
                    #endregion
                }
                totalCount = queryFixLog.Count;
                if (pageIndex == 0 || pageSize == 0)
                {
                    return queryFixLog;
                }

                return queryFixLog.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return tempchglogLst;
        }
        #endregion

        #region 固定收费记录
        private FixChargeLogManager fixChgLogManager = new FixChargeLogManager();

        /// <summary>
        /// 添加固定收费记录
        /// </summary>
        /// <param name="flog"></param>
        /// <returns></returns>
        public async Task<Response> AddFixLogAsync(FixUserChargeLog flog)
        {
            return await fixChgLogManager.AddAsync(flog);
        }

        /// <summary>
        /// 固定用户缴费记录
        /// </summary>
        /// <returns></returns>
        public List<FixUserChargeLog> FindPageListForFixChgLog(int pageSize, int pageIndex, DateTime start, DateTime end, string queryName, string queryContent, out int totalCount)
        {
            totalCount = 0;
            List<FixUserChargeLog> fixchglogLst = new List<FixUserChargeLog>();
            Log log = LogFactory.GetLogger("FindPageListForFixChgLog");
            try
            {

                fixchglogLst = fixChgLogManager.FindList(tl => tl.RecordDTime >= start && tl.RecordDTime <= end);
                List<FixUserChargeLog> queryFixLog = new List<FixUserChargeLog>();
                if (queryName == "0")
                {
                    queryFixLog.AddRange(fixchglogLst);
                }
                else
                {
                    #region
                    if (!string.IsNullOrEmpty(queryContent))
                    {
                        if (queryName == "UserName")
                        {
                            queryFixLog.AddRange(fixchglogLst.Where(lst => queryContent.Contains(lst.UserName)));
                        }
                        else if (queryName == "PlateNum")
                        {
                            queryFixLog.AddRange(fixchglogLst.Where(lst => lst.PlateNum.Contains(queryContent) || lst.PlateNum == queryContent));
                        }
                        else if (queryName == "Proof")
                        {
                            queryFixLog.AddRange(fixchglogLst.Where(lst => lst.Proof.Contains(queryContent)));
                        }
                        else if (queryName == "OprtCode")
                        {
                            queryFixLog.AddRange(fixchglogLst.Where(lst => lst.OprtCode == queryContent));
                        }
                        else if (queryName == "CurrDeadline")
                        {
                            //小于期限
                            DateTime compdt = DateTime.Parse(queryContent);
                            foreach (FixUserChargeLog fix in fixchglogLst)
                            {
                                DateTime deal = DateTime.Parse(fix.CurrDeadline);
                                if (DateTime.Compare(deal, compdt) <= 0)
                                {
                                    queryFixLog.Add(fix);
                                }
                            }
                        }

                    }
                    #endregion
                }
                totalCount = queryFixLog.Count;
                if (pageIndex == 0 || pageSize == 0)
                {
                    return queryFixLog;
                }

                return queryFixLog.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return fixchglogLst;
        }

        #endregion       

    }
}
