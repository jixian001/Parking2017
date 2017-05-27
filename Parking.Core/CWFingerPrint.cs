﻿using System;
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
        private FingerPrintManager manager = new FingerPrintManager();

        public CWFingerPrint()
        {
        }

        public Response Add(FingerPrint finger)
        {
            return manager.Add(finger);
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

        public Response Delete(int ID)
        {
            return manager.Delete(ID);
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
        /// <param name="custID"></param>
        /// <returns></returns>
        public async Task<Response> FindFingerPrintAsync(int custID)
        {
            return await Task.Run(()=> {
                Response resp = new Response();

                FPrint print = new FPrint();
                resp = print.OpenDevice();
                if (resp.Code == 0)
                {
                    return resp;
                }
                bool isLoop = true;
                while (isLoop)
                {
                    resp = print.CheckFinger();
                    if (resp.Code == 1)
                    {
                        isLoop = false;                       
                    }
                }
                resp = print.GetFingerTemplate();
                if (resp.Code == 1)
                {
                    //采集到指纹,匹配指纹库,是否有相同的指纹
                    byte[] current = resp.Data;
                    FingerPrint origPrint = null;
                    List<FingerPrint> printList = manager.FindList().ToList();
                    foreach(FingerPrint fp in printList)
                    {
                        byte[] orig = Encoding.Default.GetBytes(fp.FingerInfo);
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
                        short max =10000;
                        if (printList.Count > 0)
                        {
                            max = printList.Select(m => m.SN_Number).Max();
                        }
                        if (max > 32000)
                        {
                            max = 9000;
                        }
                        origPrint.SN_Number = ++max;
                        origPrint.FingerInfo = Encoding.Default.GetString(current);
                        origPrint.CustID = custID;
                        resp = manager.Add(origPrint);
                        resp.Data = null;
                        if (resp.Code == 1)
                        {
                            resp.Message = "绑定指纹成功";
                            resp.Data = origPrint.SN_Number;
                        }
                    }
                    else
                    {
                        resp.Code = 1;
                        resp.Data = origPrint.SN_Number;
                        if (origPrint.CustID == custID)
                        {
                            resp.Message = "指纹已经绑定到该用户";                            
                        }
                        else
                        {
                            Customer cust = new CWICCard().FindCust(custID);
                            if (cust != null)
                            {
                                resp.Message = "指纹已绑定到用户-" + cust.UserName + " ,车牌-" + cust.PlateNum;
                            }
                            else
                            {
                                resp.Message = "指纹库内有匹配指纹，CustID-" + origPrint.CustID;
                            }

                        }
                    }
                    
                }
                print.CloseDevice();
                return resp;
            });      
        }


    }
}
