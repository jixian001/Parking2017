using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class DeviceManager:BaseManager<Device>
    {

        public bool IsExist(int code,int warehouse)
        {
            return base._repository.IsContains(dev => dev.DeviceCode == code&&dev.Warehouse==warehouse);
        }

        public Device Find(Expression<Func<Device, bool>> where)
        {
            return base._repository.Find(where);
        }

        public List<Device> FindList(Expression<Func<Device, bool>> where)
        {
            IQueryable<Device> iqueryLst = _repository.FindList(where);
            List<Device> allLst = new List<Device>();
            foreach (var tsk in iqueryLst)
            {
                allLst.Add(tsk);
            }
            return allLst;           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<Device> FindList(Expression<Func<Device, bool>> where,OrderParam param)
        {
            IQueryable<Device> iqueryLst= _repository.FindList(where, param);
            List<Device> allLst = new List<Device>();
            foreach (var tsk in iqueryLst)
            {
                allLst.Add(tsk);
            }
            return allLst;
        }

        public Page<Device> FindPageList(Page<Device> workPage, Expression<Func<Device, bool>> where, OrderParam oparam)
        {
            int totalNum = 0;
            IQueryable<Device> iqueryLst = _repository.FindPageList(workPage.PageSize, workPage.PageIndex, out totalNum, where, new OrderParam[] { oparam });
            List<Device> allLst = new List<Device>();
            foreach (var tsk in iqueryLst)
            {
                allLst.Add(tsk);
            }
            workPage.ItemLists = allLst;
            workPage.TotalNumber = totalNum;
            return workPage;
        }
    }
}
