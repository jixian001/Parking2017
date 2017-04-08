using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Core;
using Parking.Auxiliary;
using Parking.Data;

namespace Parking.Application
{
    public class WorkFlow
    {
        private int warehouse;
        private string ipAddrs;
        private SocketPlc plcAccess;
        private string[] s7_Connection_Items = null;

        private CWTask cwtask = new CWTask();
        private CWDevice cwdevice = new CWDevice();

        private Int16 messageID = 0;

        private WorkFlow()
        {
        }
        public WorkFlow(string ipaddrs,int wh)
        {
            ipAddrs = ipaddrs;
            warehouse = wh;
            plcAccess = new SocketPlc(ipAddrs);

            messageID = (short)(new Random()).Next(1, 4000);        
        }

        public string[] S7_Connection_Items
        {
            get { return s7_Connection_Items; }
            set { s7_Connection_Items = value; }
        }

        public void ConnectPLC()
        {
            if (plcAccess != null)
            {
                plcAccess.ConnectPLC();
            }
        }

        public void DisConnect()
        {
            if (plcAccess != null)
            {
                plcAccess.DisConnectPLC();
            }
        }

        public void TaskAssign()
        {

        }

        public void SendMessage()
        {
            List<ImplementTask> tasks =cwtask.GetExecuteTasks();
            if (tasks == null)
            {
                return;
            }
            Log log = LogFactory.GetLogger("WorkFlow.SendMessage");
            try
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    ImplementTask task = tasks[i];
                    Device smg = cwdevice.SelectSMG(task.DeviceCode,warehouse);
                    if (smg == null)
                    {
                        log.Error("当前执行作业,绑定的设备号-"+task.DeviceCode+"  库区-"+task.Warehouse+" 不是系统");
                        continue;
                    }
                    if (smg.Type == EnmSMGType.Hall)
                    {
                        #region 车厅
                        if (smg.IsAble == 1 && smg.TaskID == task.ID &&
                            (task.SendStatusDetail == EnmTaskStatusDetail.NoSend ||
                            (task.SendStatusDetail == EnmTaskStatusDetail.SendWaitAsk && task.SendDtime.AddSeconds(10) < DateTime.Now)))
                        {
                            #region 存车
                            if (task.Status == EnmTaskStatus.ISecondSwipedWaitforCheckSize)
                            {
                                bool nback = this.sendData(this.packageMessage(1, 9, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.ISecondSwipedWaitforEVDown)
                            {
                                bool nback = this.sendData(this.packageMessage(1, 1, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.IEVDownFinishing)
                            {
                                bool nback = this.sendData(this.packageMessage(1, 54, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.IHallFinishing) //异常退出
                            {
                                bool nback = this.sendData(this.packageMessage(1, 55, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.ISecondSwipedWaitforCarLeave) //找不到合适车位
                            {
                                bool nback = this.sendData(this.packageMessage(1, 2, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.ICheckCarFail) //检测失败
                            {
                                bool nback = this.sendData(this.packageMessage(1001, 4, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            #endregion
                            #region 取车
                            else if (task.Status == EnmTaskStatus.OWaitforEVDown)
                            {
                                if (smg.IsAvailabe==1)
                                {
                                    bool nback = this.sendData(this.packageMessage(3, 1, smg.DeviceCode, task));
                                    if (nback)
                                    {
                                        cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                    }
                                }
                                else
                                {
                                    log.Info("取车时，车厅不可接收新指令。iccard-" + task.ICCardCode + "  hallID-" + smg.DeviceCode + " address-" + task.FromLctAddress);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.OEVDownFinishing)
                            {
                                bool nback = this.sendData(this.packageMessage(3, 54, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.OCarOutWaitforDriveaway)
                            {
                                bool nback = this.sendData(this.packageMessage(3, 2, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.OHallFinishing)
                            {
                                bool nback = this.sendData(this.packageMessage(3, 55, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            #endregion
                            #region 取物
                            else if (task.Status == EnmTaskStatus.TempWaitforEVDown)
                            {
                                if (smg.IsAvailabe == 1)
                                {
                                    bool nback = this.sendData(this.packageMessage(2, 1, smg.DeviceCode, task));
                                    if (nback)
                                    {
                                        cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                    }
                                }
                                else
                                {
                                    log.Info("取物时，车厅不可接收新指令。iccard-" + task.ICCardCode + "  hallID-" + smg.DeviceCode + " address-" + task.FromLctAddress);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.TempEVDownFinishing)
                            {
                                bool nback = this.sendData(this.packageMessage(2, 54, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.TempWaitforEVUp)
                            {
                                bool nback = this.sendData(this.packageMessage(2, 2, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.TempHallFinishing)
                            {
                                bool nback = this.sendData(this.packageMessage(2, 55, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                    else if (smg.Type == EnmSMGType.ETV)
                    {
                        #region ETV
                        if (smg.IsAble == 1 && smg.TaskID == task.ID &&
                            (task.SendStatusDetail == EnmTaskStatusDetail.NoSend ||
                            (task.SendStatusDetail == EnmTaskStatusDetail.SendWaitAsk && task.SendDtime.AddSeconds(10) < DateTime.Now)))
                        {
                            #region 装载
                            if (task.Status == EnmTaskStatus.TWaitforLoad)
                            {
                                if (smg.IsAvailabe == 1)
                                {
                                    bool nback = this.sendData(this.packageMessage(13, 1, smg.DeviceCode, task));
                                    if (nback)
                                    {
                                        cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                    }
                                }
                                else
                                {
                                    log.Info("装载时，TV不可接收新指令。作业类型："+task.Type.ToString()+"  iccard-" +
                                                                            task.ICCardCode + "  hallID-" + smg.DeviceCode + " FromAddress-" + 
                                                                            task.FromLctAddress+" ToAddress-"+task.ToLctAddress);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.LoadFinishing)
                            {
                                bool nback = this.sendData(this.packageMessage(13, 51, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            #endregion
                            #region 卸载
                            else if (task.Status == EnmTaskStatus.TWaitforUnload)
                            {
                                if (smg.IsAvailabe == 1)
                                {
                                    bool nback = this.sendData(this.packageMessage(14, 1, smg.DeviceCode, task));
                                    if (nback)
                                    {
                                        cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                    }
                                }
                                else
                                {
                                    log.Info("卸载时，TV不可接收新指令。作业类型：" + task.Type.ToString() + "  iccard-" +
                                                                            task.ICCardCode + "  hallID-" + smg.DeviceCode + " FromAddress-" +
                                                                            task.FromLctAddress + " ToAddress-" + task.ToLctAddress);
                                }
                            }
                            else if (task.Status == EnmTaskStatus.UnLoadFinishing)
                            {
                                bool nback = this.sendData(this.packageMessage(14, 51, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            #endregion
                            #region 移动
                            else if (task.Status == EnmTaskStatus.TWaitforMove)
                            {
                                if (smg.IsAvailabe == 1)
                                {
                                    bool nback = this.sendData(this.packageMessage(11, 1, smg.DeviceCode, task));
                                    if (nback)
                                    {
                                        cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                    }
                                }
                                else
                                {
                                    log.Info("移动时，TV不可接收新指令。");
                                }
                            }
                            else if (task.Status == EnmTaskStatus.MoveFinishing)
                            {
                                bool nback = this.sendData(this.packageMessage(11, 51, smg.DeviceCode, task));
                                if (nback)
                                {
                                    cwtask.UpdateSendStatusDetail(task, EnmTaskStatusDetail.SendWaitAsk);
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        public void ReceiveMessage()
        {
            Log log = LogFactory.GetLogger("WorkFlow.ReceiveMessage");
            try
            {
                bool hasdata = false;
                Int16[] data;
                unpackageMessage(out data, out hasdata);
                if (!hasdata)
                {
                    return;
                }
                Device smg = cwdevice.SelectSMG(data[6], data[1]);
                if (smg == null)
                {
                    log.Error("无效报文，找不到相关设备。deviceCode-"+data[6]+" warehouse-"+data[1]);
                    return;
                }
                ImplementTask task = cwtask.GetTaskBySmgID(smg.DeviceCode, smg.Warehouse);
                if (task != null)
                {
                    if (smg.Type == EnmSMGType.Hall)
                    {
                        #region
                        #region 存车
                        if (task.Status == EnmTaskStatus.IFirstSwipedWaitforCheckSize)
                        {
                            if (task.SendStatusDetail == EnmTaskStatusDetail.SendWaitAsk)
                            {
                                if (data[2] == 1 && data[3] == 9 && data[4] == 9999)
                                {
                                    cwtask.UpdateSendStatusDetail(task,EnmTaskStatusDetail.Asked);
                                }
                            }
                            if (data[2] == 1001 && data[4] == 101)
                            {

                            }
                        }
                        #endregion
                        #region 取车

                        #endregion
                        #region 取物

                        #endregion
                        #endregion
                    }
                    else if (smg.Type == EnmSMGType.ETV)
                    {
                        #region
                        #region 装载

                        #endregion
                        #region 卸载

                        #endregion
                        #region 移动

                        #endregion
                        #endregion
                    }
                }
                else
                {
                    #region 处理第一次入库
                    if (data[2] == 1001 && data[4] == 1)
                    {
                        new CWTaskTransfer(smg.DeviceCode, smg.Warehouse).DealCarEntrance();
                    }
                    #endregion
                    #region 处理开机故障报文
                    if (data[2] == 1074 && data[4] == 7)
                    {
                        cwdevice.Update(smg);
                        this.sendData(this.packageMessage(74, 1, smg.DeviceCode, null));
                    }
                    #endregion
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        public void DealAlarmInfo()
        {

        }

        /// <summary>
        /// 报文接收
        /// </summary>
        /// <param name="data">返回信息</param>
        /// <param name="hasData">有报文</param>
        private void unpackageMessage(out Int16[] data, out bool hasData)
        {
            #region
            hasData = false;
            data = null;
            if (JudgeSocketAvailabe() == false)
            {
                return;
            }
            string recvFlag = s7_Connection_Items[1];
            object recvBuffFlag = plcAccess.ReadData(recvFlag, SocketPlc.VarType.Int16);
            if (recvBuffFlag == null)
            {
                return;
            }
            //有数据要接收
            if (Convert.ToInt16(recvBuffFlag) == 9999)
            {
                string recvBuff = s7_Connection_Items[0];
                object recvData = plcAccess.ReadData(recvBuff, SocketPlc.VarType.Int16);
                if (recvBuff != null)
                {
                    //清空标志字                    
                    int flag = 0;
                    int nback = plcAccess.WriteData(recvFlag, flag);
                    if (nback == 1)
                    {
                        //读取数据成功，返回值
                        data = (Int16[])recvData;
                        hasData = true;
                        //记录
                        new CWTelegramLog().AddRecord(data, 2);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// 报文发送
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool sendData(Int16[] data)
        {
            Log log = LogFactory.GetLogger("WorkFlow.sendData");
            try
            {
                if (JudgeSocketAvailabe() == false)
                {
                    return false;
                }
                string sendbuff = s7_Connection_Items[3];
                //先读发送缓冲区标志字
                object sendFlag = plcAccess.ReadData(sendbuff, SocketPlc.VarType.Int16);
                if (sendFlag != null)
                {
                    //可以发送报文
                    if (Convert.ToInt16(sendFlag) == 0)
                    {
                        //写50个字
                        string sendItem = s7_Connection_Items[2];
                        int nback= plcAccess.WriteData(sendItem, data);
                        if (nback == 1)
                        {
                            //标志字置9999
                            int flag = 9999;
                            nback= plcAccess.WriteData(sendbuff, flag);
                            if (nback == 1) //完成写入工作
                            {
                                //记录报文
                                new CWTelegramLog().AddRecord(data, 1);
                                return true;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {               
                log.Error(ex.ToString());
            }
            return false;
        }

        private short[] packageMessage(int mtype,int stype,int smg,ImplementTask tsk)
        {
            short[] data = new short[50];
            if (tsk != null)
            {
                data[2] = Convert.ToInt16(mtype);
                data[3] = Convert.ToInt16(stype);
                data[6] = Convert.ToInt16(smg);
                data[11] = Convert.ToInt16(string.IsNullOrEmpty(tsk.ICCardCode)?"0":removeLetter(tsk.ICCardCode));
                data[23] = Convert.ToInt16(string.IsNullOrEmpty(tsk.CarSize)?"0":tsk.CarSize);
                data[25] = Convert.ToInt16(tsk.Distance);
                if (!string.IsNullOrEmpty(tsk.FromLctAddress))
                {
                    data[30] = Convert.ToInt16(tsk.FromLctAddress.Substring(0, 1));
                    data[31] = Convert.ToInt16(tsk.FromLctAddress.Substring(1, 2));
                    data[32] = Convert.ToInt16(tsk.FromLctAddress.Substring(3));
                }
                if (!string.IsNullOrEmpty(tsk.ToLctAddress))
                {
                    data[35] = Convert.ToInt16(tsk.ToLctAddress.Substring(0, 1));
                    data[36] = Convert.ToInt16(tsk.ToLctAddress.Substring(1, 2));
                    data[37] = Convert.ToInt16(tsk.ToLctAddress.Substring(3));
                }
                data[47] = (short)tsk.CarWeight;
            }
            else
            {
                data[2] = Convert.ToInt16(mtype);
                data[3] = Convert.ToInt16(stype);
                data[6] = Convert.ToInt16(smg);
            }
            data[0] = (short)warehouse;
            data[48] = getSerial();
            data[49] = (short)9999;
            return data;
        }

        /// <summary>
        /// 如果卡号中出现非数字的，以9来代替
        /// </summary>
        /// <param name="iccode"></param>
        /// <returns></returns>
        private string removeLetter(string iccode)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char aa in iccode)
            {
                if (char.IsDigit(aa))
                {
                    builder.Append(aa);
                }
                else
                {
                    builder.Append('9');
                }
            }
            return builder.ToString();
        }
        
        /// <summary>
        /// 获取报文ID
        /// </summary>
        /// <returns></returns>
        private short getSerial()
        {
            if (messageID < (short)4999)
            {
                messageID++;
            }
            else
            {
                messageID = 1;
            }
            return messageID;
        }      

        private bool JudgeSocketAvailabe()
        {
            Log log = LogFactory.GetLogger("WorkFlow.JudgeSocketAvailabe");
            if (s7_Connection_Items == null ||
                    s7_Connection_Items.Length < 4)
            {
                log.Error("s7_Connection_Items 无效,无法收发报文");
                return false;
            }
            if (plcAccess == null)
            {
                log.Error("没有建立有效的socket, plcAccess为空, 无法收发报文");
                return false;
            }
            if (!plcAccess.IsConnected)
            {
                ConnectPLC();
            }
            if (!plcAccess.IsConnected)
            {
                log.Error("plcAccess没有建立连接, 无法收发报文");
                return false;
            }
            return true;
        }
    }
}
