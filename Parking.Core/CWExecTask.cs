using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;

namespace Parking.Core
{
    /// <summary>
    /// 处理处于执行的作业
    /// </summary>
    public class CWExecTask
    {
        private CurrentTaskManager manager = new CurrentTaskManager();

        public CWExecTask()
        {
        }

        public List<ImplementTask> GetExecuteTasks()
        {
            return manager.GetCurrentTaskList();
        }



    }
}
