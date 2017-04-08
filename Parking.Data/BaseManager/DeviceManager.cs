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
        /// 添加设备记录，暂不用，待后续用
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override Response Add(Device entity)
        {
            Response _resp = new Response();
            if (IsExist(entity.DeviceCode,entity.Warehouse))
            {
                _resp.Code = 2;
                _resp.Message = "当前设备Code已存在";
            }
            else
            {
                _resp = base.Add(entity);
            }
            return _resp;
        } 

        public bool IsExist(int code,int warehouse)
        {
            return base._repository.IsContains(dev => dev.DeviceCode == code&&dev.Warehouse==warehouse);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public Device Find(Expression<Func<Device, bool>> where)
        {
            return base._repository.Find(where);
        }
        
        public List<Device> FindList(Expression<Func<Device, bool>> where)
        {
            return _repository.FindList(where).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<Device> FindList(Expression<Func<Device, bool>> where,OrderParam param)
        {
            return _repository.FindList(where, param).ToList();
        }

        public Page<Device> FindPageList(Page<Device> workPage, Expression<Func<Device, bool>> where, OrderParam oparam)
        {
            int totalNum = 0;
            workPage.ItemLists = _repository.FindPageList(workPage.PageSize, workPage.PageIndex, out totalNum, where, new OrderParam[] { oparam }).ToList();
            workPage.TotalNumber = totalNum;
            return workPage;
        }
    }
}
