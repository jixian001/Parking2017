using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;

namespace Parking.Data
{
    /// <summary>
    /// 数据上下文工厂
    /// </summary>
    public class ContextFactory
    {
        /// <summary>
        /// 获取当前线程的数据上下文，唯一的
        /// </summary>
        /// <returns></returns>
        public static ParkingContext CurrentContext()
        {
            ParkingContext _nContext = CallContext.GetData("ParkingContext") as ParkingContext;
            if (_nContext == null)
            {
                _nContext = new ParkingContext();
                CallContext.SetData("ParkingContext", _nContext);
            }
            return _nContext;
        }
    }
}
