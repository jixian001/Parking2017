using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Core
{
    /// <summary>
    /// 存取时，先从指纹库中找出对应的指纹编号，
    /// 再以这个编号作为凭证，进行存取操作，
    /// 每个指纹都要绑定于相关车主
    /// </summary>
    public class CWFingerPrint
    {
        private static FingerPrintManager manager = new FingerPrintManager();
        
        public CWFingerPrint()
        {
        }

        public Response Add(FingerPrint finger,bool isSave)
        {
            return manager.Add(finger,isSave);
        }

        public Response Add(FingerPrint finger)
        {
            return manager.Add(finger);
        }

        public Response Update(FingerPrint finger,bool isSave)
        {
            return manager.Update(finger,isSave);
        }

        public Response Update(FingerPrint finger)
        {
            return manager.Update(finger);
        }

        public FingerPrint Find(Expression<Func<FingerPrint, bool>> where)
        {
            return manager.Find(where);
        }

        public List<FingerPrint> FindFingersList()
        {
            return manager.FindList().ToList();
        }

        public List<FingerPrint> FindList(Expression<Func<FingerPrint, bool>> where)
        {
            return manager.FindList(where);
        }

        public Response Delete(int ID,bool isSave)
        {
            return manager.Delete(ID,isSave);
        }

        public Response Delete(int ID)
        {
            return manager.Delete(ID);
        }

        public Response SaveChange()
        {
            return manager.SaveChanges();
        }        

        //定义一个异步方法
        /// <summary>
        /// 1、打开指纹仪
        /// 2、检测手指
        /// 3、读取指纹特性
        /// 4、查询指纹库是否有注册的指纹
        /// 5、添加指纹特性到指纹库内
        /// 6、关闭指纹仪
        /// 7、返回 response
        /// </summary>      
        /// <returns></returns>
        public async Task<Response> AddFingerPrintAsync(int custID)
        {
            
            return await Task.Run(()=> {
                Response resp = new Response();
                Log log = LogFactory.GetLogger("AddFingerPrintAsync");

                try
                {
                    FPrint print = new FPrint();
                    resp = print.OpenDevice();
                    if (resp.Code == 0)
                    {                       
                        print.CloseDevice();
                        return resp;
                    }
                    bool isLoop = true;
                    DateTime now = DateTime.Now;
                    while (isLoop)
                    {
                        resp = print.CheckFinger();
                        if (resp.Code == 1)
                        {
                            log.Debug(resp.Message);
                            isLoop = false;
                        }
                        TimeSpan ts = DateTime.Now - now;
                        if (ts.TotalSeconds > 30)
                        {
                            print.CloseDevice();
                            resp.Message = "超时，请重新操作";
                            return resp;
                        }
                    }
                    resp = print.GetFingerTemplate();
                    log.Debug(resp.Message);
                    if (resp.Code == 1)
                    {
                        //采集到指纹,匹配指纹库,是否有相同的指纹
                        byte[] current = resp.Data;
                        FingerPrint origPrint = null;
                        List<FingerPrint> printList = manager.FindList().ToList();
                        foreach (FingerPrint fp in printList)
                        {
                            byte[] orig = print.Base64FingerDataToHex(fp.FingerInfo);
                            if (orig == null)
                            {
                                log.Debug("指纹-" + fp.FingerInfo + " ,转化为Byte失败！");
                            }
                           
                            resp = print.VerifyFinger(current, orig);
                            if (resp.Code == 1)
                            {
                                origPrint = fp;
                                break;
                            }
                        }
                        //没有指纹库内没有匹配指纹，允许添加
                        if (origPrint == null)
                        {
                            origPrint = new FingerPrint();
                            Int32 max = 10000;
                            if (printList.Count > 0)
                            {
                                max = printList.Select(m => m.SN_Number).Max();
                            }
                            if (max > 32000)
                            {
                                max = 9000;
                            }
                            origPrint.SN_Number = ++max;
                            string finfo = print.HexFingerDataToBase64(current, current.Length);
                            if (string.IsNullOrEmpty(finfo))
                            {
                                resp.Message = "指纹Length-" + current.Length + " ,转化为BASE64失败！";
                            }
                            else
                            {
                                //this.PrintFingerData(current);
                                //this.PrintFingerStrData(finfo);

                                origPrint.FingerInfo = finfo;
                                origPrint.CustID = custID;
                                resp = manager.Add(origPrint);
                                resp.Data = null;
                                if (resp.Code == 1)
                                {
                                    resp.Message = "绑定指纹成功";
                                    resp.Data = origPrint.SN_Number;
                                }
                            }
                        }
                        else //有匹配指纹
                        {
                            resp.Code = 0;
                            resp.Data = null;
                            Customer cust = new CWICCard().FindCust(origPrint.CustID);
                            if (cust != null)
                            {
                                resp.Message = "指纹已绑定到用户-" + cust.UserName + " ,车牌-" + cust.PlateNum;
                            }
                            else
                            {
                                resp.Message = "指纹库内有匹配指纹，CustID-" + origPrint.CustID;
                            }
                            if (custID != 0)
                            {
                                if (origPrint.CustID == custID)
                                {
                                    resp.Message = "库内已有匹配指纹已绑定到车主";
                                }
                            }
                        }

                    }
                    print.CloseDevice();
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                    resp.Message = "系统异常";                   
                }
                return resp;
            });      
        }

        /// <summary>
        /// 前端读取指纹模板后，后台处理，是否保存
        /// </summary>
        /// <param name="custID"></param>
        /// <param name="strMBBuf"></param>
        /// <returns></returns>
        public Response SubmitFingerTemplate(int custID,string strMBBuf)
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("SubmitFingerTemplate");
            try
            {              
                FingerPrint origPrint = null;
                byte[] current = FPrintBase64.Base64FingerDataToHex(strMBBuf);               
                List<FingerPrint> printList = manager.FindList().ToList();
                foreach (FingerPrint fp in printList)
                {
                    byte[] orig = FPrintBase64.Base64FingerDataToHex(fp.FingerInfo);
                    if (orig == null)
                    {
                        log.Debug("指纹-" + fp.FingerInfo + " ,转化为Byte失败！");
                    }

                    int iRet = FiPrintMatch.FPIMatch(current, orig, 3);
                    if (iRet == 0)
                    {
                        origPrint = fp;
                        break;
                    }
                }
                //没有指纹库内没有匹配指纹，允许添加
                if (origPrint == null)
                {
                    origPrint = new FingerPrint();
                    Int32 max = 10000;
                    if (printList.Count > 0)
                    {
                        max = printList.Select(m => m.SN_Number).Max();
                    }
                    if (max > 32000)
                    {
                        max = 9000;
                    }
                    origPrint.SN_Number = ++max;

                    origPrint.FingerInfo = strMBBuf;
                    origPrint.CustID = custID;
                    resp = manager.Add(origPrint);
                    resp.Data = null;
                    if (resp.Code == 1)
                    {
                        resp.Message = "绑定指纹成功";
                        resp.Data = origPrint.SN_Number;
                    }
                }
                else //有匹配指纹
                {
                    resp.Code = 0;
                    resp.Data = null;
                    Customer cust = new CWICCard().FindCust(origPrint.CustID);
                    if (cust != null)
                    {
                        resp.Message = "指纹已绑定到用户-" + cust.UserName + " ,车牌-" + cust.PlateNum;
                    }
                    else
                    {
                        resp.Message = "指纹库内有匹配指纹，CustID-" + origPrint.CustID;
                    }
                    if (custID != 0)
                    {
                        if (origPrint.CustID == custID)
                        {
                            resp.Message = "库内已有匹配指纹已绑定到当前车主";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Message = "系统异常";
            }
            return resp;
        }


        private void PrintFingerData(byte[] psMBBuf)
        {
            Task.Factory.StartNew(()=> {

                Log log = LogFactory.GetLogger("打印字节指纹");
                log.Debug("指纹模板数量- " + psMBBuf.Length);
                StringBuilder strBuild = new StringBuilder();
                int i = 0;
                foreach(byte by in psMBBuf)
                {
                    if (i % 20 == 0 && i != 0)
                    {
                        strBuild.Append(Environment.NewLine+" ["+ by.ToString("X")+"] ");
                    }
                    else
                    {
                        strBuild.Append(" [" + by.ToString("X") + "] ");
                    }
                }
                log.Debug(strBuild.ToString());

            });           
        }

        private void PrintFingerStrData(string strData)
        {
            Task.Factory.StartNew(() => {

                Log log = LogFactory.GetLogger("打印字节指纹");
                log.Debug("Base64编码后的指纹- " + strData);              

            });
        }

        /// <summary>
        /// 依指纹特性值，查找对应的顾客及存车车位
        /// </summary>
        /// <param name="strTZ"></param>
        /// <returns></returns>
        public Response FindCustByFPrintFeacture(string strTZ)
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("FindCustByFPrintFeacture");
            try
            {
                FingerPrint origPrint = null;
                byte[] current = FPrintBase64.Base64FingerDataToHex(strTZ);
                List<FingerPrint> printList = manager.FindList().ToList();
                foreach (FingerPrint fp in printList)
                {
                    byte[] orig = FPrintBase64.Base64FingerDataToHex(fp.FingerInfo);
                    if (orig == null)
                    {
                        log.Debug("指纹-" + fp.FingerInfo + " ,转化为Byte失败！");
                    }

                    int iRet = FiPrintMatch.FPIMatch(current, orig, 3);
                    if (iRet == 0)
                    {
                        origPrint = fp;
                        break;
                    }
                }
                if (origPrint != null)
                {
                    resp.Code = 1;
                    RetFPring iRet = new RetFPring();
                    Customer cust = new CWICCard().FindCust(origPrint.CustID);
                    if (cust != null)
                    {
                        iRet.UserName = cust.UserName;
                        iRet.Plate = cust.PlateNum;
                    }
                    Location loc = new CWLocation().FindLocation(lc => lc.ICCode == origPrint.SN_Number.ToString());
                    if (loc != null)
                    {
                        iRet.Warehouse = loc.Warehouse.ToString();
                        iRet.LocAddrs = loc.Address;
                    }
                    resp.Message = "找到匹配指纹";
                    resp.Data = iRet;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return resp;
        }


    }
}
