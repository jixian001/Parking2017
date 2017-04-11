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
    /// 处理处于执行的作业
    /// </summary>
    public class CWTask
    {
        private CurrentTaskManager manager = new CurrentTaskManager();

        private static List<string> soundsList = new List<string>();

        public CWTask()
        {
        }

        /// <summary>
        /// 添加声音文件
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        /// <param name="soundFile"></param>
        public void AddNofication(int warehouse,int hallID,string soundFile)
        {
            string sfile = warehouse.ToString() + ";" + hallID.ToString() + ";" + soundFile;
            if (!soundsList.Contains(sfile))
            {
                soundsList.Add(sfile);
            }
        }

        /// <summary>
        /// 移除声音
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        public void ClearNotification(int warehouse,int hallID)
        {
            for (int i = 0; i < soundsList.Count; i++)
            {
                string[] infos = soundsList[i].Split(';');
                if (infos.Length > 1)
                {
                    int wh = Convert.ToInt32(infos[0]);
                    int hall = Convert.ToInt32(infos[1]);
                    if (wh == warehouse && hall == hallID)
                    {
                        soundsList.RemoveAt(i);
                    }
                }
            }
        }
        /// <summary>
        /// 获取声音
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        /// <returns></returns>
        public string GetNotification(int warehouse, int hallID)
        {
            for (int i = 0; i < soundsList.Count; i++)
            {
                string[] infos = soundsList[i].Split(';');
                if (infos.Length > 2)
                {
                    int wh = Convert.ToInt32(infos[0]);
                    int hall = Convert.ToInt32(infos[1]);
                    if (wh == warehouse && hall == hallID)
                    {
                        string sound = infos[2];
                        soundsList.RemoveAt(i);
                        return sound;
                    }
                }
            }
            return null;
        }

        public List<ImplementTask> GetExecuteTasks()
        {
            return manager.GetCurrentTaskList();
        }       

        /// <summary>
        /// 更新作业报文的发送状态
        /// </summary>
        /// <param name="task"></param>
        /// <param name="detail"></param>
        public void UpdateSendStatusDetail(ImplementTask task,EnmTaskStatusDetail detail)
        {
            task.SendStatusDetail = detail;
        }

        /// <summary>
        /// 依ID获取任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ImplementTask Find(Expression<Func<ImplementTask,bool>> where)
        {
            return manager.Find(where);
        }

        /// <summary>
        /// 依设备查找任务
        /// </summary>
        /// <param name="smg"></param>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public ImplementTask GetTaskBySmgID(int smg,int warehouse)
        {
            return manager.Find(tsk=>tsk.DeviceCode==smg&&tsk.Warehouse==warehouse);
        }

        /// <summary>
        /// 有车入库
        /// </summary>
        /// <param name="hall"></param>
        public void DealFirstCarEntrance(Device hall)
        {
            ImplementTask task = new ImplementTask();
            task.Warehouse = hall.Warehouse;
            task.DeviceCode = hall.DeviceCode;
            task.Type = EnmTaskType.SaveCar;
            task.Status = EnmTaskStatus.ICarInWaitFirstSwipeCard;
            task.SendStatusDetail = EnmTaskStatusDetail.NoSend;
            task.CreateDate = DateTime.Now;
            #region 发送时间没有暂为空，看看有没有异常出现
            task.SendDtime = DateTime.Parse("2017-1-1");
            #endregion
            task.HallCode = hall.DeviceCode;
            task.FromLctAddress = hall.Address;
            task.ToLctAddress = "";
            task.ICCardCode = "";
            task.Distance = 0;
            task.CarSize = "";
            task.IsComplete = 0;
            Response _resp = manager.Add(task);
            if (_resp.Code == 1)
            {
                //这里是否可以获取到ID？或者再查询一次
                hall.TaskID = task.ID;
                new CWDevice().Update(hall);
            }
        }

        /// <summary>
        /// 外形检测上报处理
        /// </summary>
        /// <param name="hallID"></param>
        /// <param name="distance"></param>
        /// <param name="carSize"></param>
        public void IDealCheckedCar(ImplementTask htsk, int hallID,int distance,string checkCode)
        {
            htsk.CarSize = checkCode;
            htsk.Distance = distance;
            htsk.SendDtime = DateTime.Now;
            htsk.SendStatusDetail = EnmTaskStatusDetail.NoSend;

            #region 上报外形数据不正确
            if (checkCode.Length != 3)
            {
                htsk.Status =EnmTaskStatus.ISecondSwipedWaitforCarLeave;
                //更新任务信息
                manager.Update(htsk);
                //
                this.AddNofication(htsk.Warehouse,htsk.DeviceCode, "60.wav");
                return;
            }
            #endregion
            
            //暂以卡号为准
            ICCard iccd = new CWICCard().Find(ic=>ic.UserCode==htsk.ICCardCode);
            if (iccd == null)
            {
                //上位控制系统故障
                this.AddNofication(htsk.Warehouse,hallID,"20.wav");
                this.AddNofication(htsk.Warehouse, hallID, "6.wav");
                return;
            }
            Device hall = new CWDevice().SelectSMG(hallID, htsk.Warehouse);
            int smgID;
            Location lct = new AllocateLocation().IAllocateLocation(checkCode, iccd,hall, out smgID);




        }

        public Page<ImplementTask> FindPageList(Page<ImplementTask> pageTask,OrderParam param)
        {
            if (param == null)
            {
                param = new OrderParam()
                {
                    PropertyName = "ID",
                    Method = OrderMethod.Asc
                };
            }
            Page<ImplementTask> page = manager.FindPageList(pageTask,param);
            return page;
        }

        public Response CompleteTask(int tid)
        {
            manager.Delete(tid);
            Response _resp = new Response()
            {
                Code = 1,
                Message = "手动完成作业成功,ID-" + tid
            };
            return _resp;
        }

        public Response ResetTask(int tid)
        {
            manager.Delete(tid);
            Response _resp = new Response()
            {
                Code = 1,
                Message = "手动复位作业成功,ID-"+tid
            };
            return _resp;
        }

        #region 队列管理
        private WorkTaskManager manager_queue = new WorkTaskManager();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public WorkTask FindQueue(Expression<Func<WorkTask, bool>> where)
        {
            return manager_queue.Find(where);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public List<WorkTask> FindQueueList(Expression<Func<WorkTask, bool>> where)
        {
            return manager_queue.FindList(where);
        }

        /// <summary>
        /// 依分页条件查询所有
        /// </summary>
        /// <param name="pageWork"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Page<WorkTask> FindPageList(Page<WorkTask> pageWork, OrderParam param)
        {
            if (param == null)
            {
                param = new OrderParam()
                {
                    PropertyName = "ID",
                    Method = OrderMethod.Asc
                };
            }
            Page<WorkTask> page = manager_queue.FindPageList(pageWork, param);
            return page;
        }

        /// <summary>
        /// 依查询条件，查找相应分页信息
        /// </summary>
        /// <param name="pageWork"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Page<WorkTask> FindPagelist(Page<WorkTask> pageWork,Expression<Func<WorkTask,bool>> where, OrderParam param)
        {
            if (param == null)
            {
                param = new OrderParam()
                {
                    PropertyName = "ID",
                    Method = OrderMethod.Asc
                };
            }
            Page<WorkTask> page = manager_queue.FindPageList(pageWork,where,param);
            return page;
        }
        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Response DeleteQueue(int ID)
        {
            return manager_queue.Delete(ID);
        }

        #endregion
    }
}
