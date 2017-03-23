using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Parking.Core;
using Parking.Auxiliary;

namespace Parking.Application
{
    /// <summary>
    /// 后台服务入口
    /// </summary>
    public class EqpService
    {
        private Dictionary<int,Thread> dic_taskThread;
        private Dictionary<int,List<Thread>> dic_iccdsThread;
        //用于断开连接时，关闭刷卡器
        private List<CardControl> CardControlList; 
        private bool isStart;

        private Dictionary<int, WorkFlow> dic_WorkFlows;

        #region 系统所需变量
        private int plcCount;
        private int plcRefresh;

        private int iccdRefresh;
        #endregion
        public EqpService()
        {           
            isStart = false;
            Init();
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

        private void Init()
        {            
            string warehouse = XMLHelper.GetRootNodeValueByXpath("root", "PlcCount");
            plcCount = string.IsNullOrEmpty(warehouse) ? 0 : Convert.ToInt32(warehouse);

            string prefresh = XMLHelper.GetRootNodeValueByXpath("root", "PLCRefresh");
            plcRefresh = string.IsNullOrEmpty(prefresh)?0:Convert.ToInt32(prefresh);

            string icfresh = XMLHelper.GetRootNodeValueByXpath("root", "ICcdRefresh");
            iccdRefresh = string.IsNullOrEmpty(icfresh) ? 0 : Convert.ToInt32(icfresh);

            dic_WorkFlows = new Dictionary<int, WorkFlow>();
            dic_taskThread = new Dictionary<int, Thread>();
            dic_iccdsThread = new Dictionary<int, List<Thread>>();
            //用于断开连接时，关闭刷卡器
            CardControlList = new List<CardControl>();
        }

        public bool OnStart()
        {
            Log log = LogFactory.GetLogger("EqpService.OnStart");
            if (plcCount < 1)
            {
                log.Error("PlcCount="+plcCount+" 配置出错！");
            } 
            isStart = true;
            for (int i = 1; i < plcCount + 1; i++)
            {
                XmlNode node = XMLHelper.GetPlcNodeByTagName("//root//setting", i.ToString(), "PlcIPAddress");
                if (node != null)
                {
                    string ipadrs = node.InnerText;  //plc ip地址

                    WorkFlow controller = new WorkFlow(ipadrs, i);
                    dic_WorkFlows.Add(i, controller);
                    //添加 S7 connection_1 连接项
                    XmlNode xnode = XMLHelper.GetPlcNodeByTagName("//root//setting", i.ToString(), "ConnectItem");
                    if (xnode != null)
                    {
                        string items = xnode.InnerText.Trim();
                        string[] array_items = items.Split(';');
                        if (array_items != null && array_items.Length > 4)
                        {
                            controller.S7_Connection_Items = new string[array_items.Length];
                            int te = 0;
                            foreach(string item in array_items)
                            {
                                controller.S7_Connection_Items[te++] = item.Trim();
                            }
                            log.Info("S7_Connection 连接项");
                            foreach(string item in controller.S7_Connection_Items)
                            {
                                log.Info(item);
                            }
                        }
                    }
                }

            }

            for (int i = 1; i < plcCount + 1; i++)
            {
                //启动作业线程
                Thread taskThread = new Thread(new ParameterizedThreadStart(DealMessage));
                taskThread.Start(i);
                dic_taskThread.Add(i, taskThread);

                //添加刷卡器线程，并启用
                XmlNode hallsNode = XMLHelper.GetPlcNodeByTagName("//root//setting", i.ToString(), "halls");
                string count = XMLHelper.GetXmlValueOfAttribute(hallsNode, "count");
                if (string.IsNullOrEmpty(count))
                {
                    log.Error("warehouse-"+i.ToString()+" 下 halls 属性count 为空，配置出错！");
                    continue;
                }
                List<Thread> iccdThreadLst = new List<Thread>();
                int hallCount = Convert.ToInt32(count);
                for(int j = 11; j < 11 + hallCount; j++)
                {
                    XmlNode node = XMLHelper.GetHallNodeByTageName("//root//setting", i.ToString(), j.ToString(), "ID");
                    if (node == null)
                    {
                        log.Error("找不到 库区-"+i.ToString()+" halls下指定车厅-"+j+ " 的（ID）节点，配置出错！");
                        continue;
                    }
                    int hallID = Convert.ToInt32(node.InnerText);
                    XmlNode iNode = XMLHelper.GetHallNodeByTageName("//root//setting", i.ToString(), j.ToString(), "ICCardIPAddrss");
                    if (iNode == null)
                    {
                        log.Error("找不到 库区-" + i.ToString() + " halls下指定车厅-" + j + " 的（ICCardIPAddrss）节点，配置出错！");
                        continue;
                    }
                    string ipadr = iNode.InnerText;

                    //添加刷卡器的线程
                    CardControl card = new CardControl(i, hallID, ipadr);
                    card.IsConnect = true;
                    card.IntervalTime = iccdRefresh;
                    Thread iccdThread = new Thread(new ThreadStart(card.ICCdReader));
                    iccdThread.Start();

                    iccdThreadLst.Add(iccdThread);

                    CardControlList.Add(card);
                }
                dic_iccdsThread.Add(i,iccdThreadLst);
            }
            
            return true;
        }

        public bool OnStop()
        {

            return true;
        }

        private void DealMessage(object wh)
        {
            Log log = LogFactory.GetLogger("EqpService.DealMessage");
            int warehouse = Convert.ToInt32(wh);
            if (!dic_WorkFlows.ContainsKey(warehouse))
            {
                log.Error("dic_WorkFlows没有包含key-"+warehouse+" 系统无法启动！");
                return;
            }
            WorkFlow controller = dic_WorkFlows[warehouse];
            try
            {
                controller.ConnectPLC();
            }
            catch (Exception ex)
            {
                log.Error("连接PLC异常，无法打开连接！系统无法启动！"+ex.ToString());
                isStart = false;
                return;
            }
            while (isStart)
            {
                try
                {
                    controller.DealAlarmInfo();
                    controller.TaskAssign();
                    controller.SendMessage();
                    controller.ReceiveMessage();

                    Thread.Sleep(plcRefresh);
                }
                catch(Exception ec)
                {
                    log.Error("处理业务异常-" + ec.ToString());
                    Thread.Sleep(5000);
                }               
            }
        }
    }
}
