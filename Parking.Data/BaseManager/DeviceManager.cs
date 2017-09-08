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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<Device> FindList(Expression<Func<Device, bool>> where,OrderParam param)
        {
            List<Device> allLst = _repository.FindList(where, param).ToList<Device>();
          
            return allLst;
        }

        public Page<Device> FindPageList(Page<Device> workPage, Expression<Func<Device, bool>> where, OrderParam oparam)
        {
            int totalNum = 0;
            List<Device> allLst = _repository.FindPageList(workPage.PageSize, workPage.PageIndex, out totalNum, where, new OrderParam[] { oparam }).ToList<Device>();
            
            workPage.ItemLists = allLst;
            workPage.TotalNumber = totalNum;
            return workPage;
        }
    }
}
