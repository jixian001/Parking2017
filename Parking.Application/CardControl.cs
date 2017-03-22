using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Core;

namespace Parking.Application
{
    public class CardControl
    {
        private int nWarehouse;
        private int nHallID;
        private string nIPAddrs;
        private bool isStart;
        private ICCardReader icReader;
        private int interval;

        public CardControl(int wh,int hallID,string ipaddrs)
        {
            nWarehouse = wh;
            nHallID = hallID;
            nIPAddrs = ipaddrs;

            icReader = new ICCardReader(ipaddrs);
        }

        public bool IsConnect
        {
            get { return isStart; }
            set { isStart = value; }
        }

        public int IntervalTime
        {
            get
            {
                return interval;
            }
            set
            {
                interval = value;
            }
        }

        public  void ICCdReader()
        {
            Log log = LogFactory.GetLogger("CardControl.ICCdReader");
            if (icReader == null)
            {
                log.Error("warehouse-"+nWarehouse+" hallID-"+nHallID+" nIPaddress-"+nIPAddrs+" icReader没有初始化，为空的引用！");
                return;
            }
            icReader.Connect();
            while (isStart)
            {
                uint physiccard = 0;
                int nback = icReader.GetPhyscard(ref physiccard);
                if (nback == 0)
                {
                    //调用刷卡器处理方法

                }
                Thread.Sleep(interval);
            }
        }

        public void DisConnect()
        {
            icReader.Disconnect();
            isStart = false;
        }

    }
}
