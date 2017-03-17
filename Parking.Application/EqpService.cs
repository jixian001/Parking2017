using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parking.Core;

namespace Parking.Application
{
    /// <summary>
    /// 后台服务入口
    /// </summary>
    public class EqpService
    {      
        private bool isStart;
        public EqpService()
        {           
            isStart = false;
        }

        /// <summary>
        /// 运行状态
        /// </summary>
        public bool RunState
        {
            get
            {
                return isStart;
            }
        }

        public bool OnStart()
        {


            return true;
        }

        public bool OnStop()
        {

            return true;
        }

    }
}
