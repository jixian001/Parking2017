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
    /// ic卡的业务逻辑
    /// </summary>
    public class CWICCard
    {
        private ICCardManager manager = new ICCardManager();

        public CWICCard()
        {
        }       

        public ICCard Find(int id)
        {
            return manager.Find(id);
        }

        public ICCard Find(Expression<Func<ICCard, bool>> where)
        {
            return manager.Find(where);
        }

        public Response Update(ICCard iccd,bool isSave)
        {
            return manager.Update(iccd,isSave);
        }

        public Response Update(ICCard iccd)
        {
            return manager.Update(iccd);
        }

        public List<ICCard> FindIccdList(Expression<Func<ICCard, bool>> where)
        {
            return manager.FindList(where);
        }

        /// <summary>
        /// 制卡
        /// </summary>
        /// <param name="physic"></param>
        /// <param name="usecode"></param>
        /// <returns></returns>
        public Response MakeICCard(string physic,string usercode)
        {
            Response resp = new Response();
            if (string.IsNullOrEmpty(physic) || string.IsNullOrEmpty(usercode))
            {
                resp.Message = "传输异常，物理卡号或用户卡号为空！";
                return resp;
            }
            ICCard physICCard = Find(ic=>ic.PhysicCode==physic);
            ICCard userICCard = Find(ic=>ic.UserCode==usercode);

            if (physICCard == null)  //物理卡号不存在
            {
                if (userICCard == null)
                {
                    #region 添加
                    ICCard iccd = new ICCard()
                    {
                        PhysicCode = physic,
                        UserCode = usercode,                                               
                        Status = EnmICCardStatus.Normal,
                        CreateDate = DateTime.Now,
                        LossDate = DateTime.Parse("2017-1-1"),
                        LogoutDate= DateTime.Parse("2017-1-1"),                      
                        CustID=0
                    };
                    resp = manager.Add(iccd);
                    if (resp.Code == 1)
                    {
                        resp.Message = "制卡成功！";
                    }
                    #endregion
                }
                else
                {
                    resp.Message = "该用户卡号-"+usercode+" 已被占用，请输入有效的卡号！";
                }
            }
            else //已制过卡
            {
                #region
                if (userICCard == null)
                {
                    //修改卡号
                    physICCard.UserCode = usercode;
                    physICCard.Status = EnmICCardStatus.Normal;
                    resp= manager.Update(physICCard);
                    if (resp.Code == 1)
                    {
                        resp.Message = "修改用户卡号成功！";
                    }
                }
                else
                {
                    if (physICCard.UserCode != userICCard.UserCode)
                    {
                        resp.Message = "该用户卡号-" + usercode + " 已被占用！";
                    }
                    else
                    {
                        resp.Code = 1;
                        resp.Message = "制卡卡号与原卡号一致！";
                    }
                }
                #endregion
            }
            return resp;
        }


        #region 顾客

        private CustomerManager manager_cust = new CustomerManager();

        public Response AddCust(Customer cust,bool isSave)
        {
            return manager_cust.Add(cust, isSave);
        }

        public Response AddCust(Customer cust)
        {
            return manager_cust.Add(cust);
        }

        public Response UpdateCust(Customer cust, bool isSave)
        {
            return manager_cust.Update(cust,isSave);
        }

        public Response UpdateCust(Customer cust)
        {
            return manager_cust.Update(cust);
        }

        public Response Delete(int ID, bool isSave)
        {
            return manager_cust.Delete(ID,isSave);
        }

        public Response Delete(int ID)
        {

            return manager_cust.Delete(ID);
        }

        public Response SaveChange()
        {
            return manager_cust.SaveChanges();
        }

        public Customer FindCust(int ID)
        {
            return manager_cust.Find(ID);
        }

        public Customer FindCust(Expression<Func<Customer, bool>> where)
        {
            return manager_cust.Find(where);
        }

        public List<Customer> FindCustList(Expression<Func<Customer, bool>> where)
        {
            return manager_cust.FindList(where);
        }

        /// <summary>
        /// 依车位地址查找是否被绑定
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public Customer FindFixLocationByAddress(int warehouse, string address)
        {
            return manager_cust.Find(cust=>(cust.Type==EnmICCardType.FixedLocation||
                                            cust.Type==EnmICCardType.VIP)&&
                                            cust.Warehouse==warehouse&&
                                            cust.LocAddress==address);           
        }

        #endregion
    }
}
