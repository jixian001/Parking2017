using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace Parking.Application
{
    public class ICCardReader:ICCardRW
    {
        private string ipAddrs;
        private bool isConnect;
        private int mnDesc;
        private short mnAuthKeyMode = 0;

        public ICCardReader(string ip)
        {
            ipAddrs = ip;
            isConnect = false;
        }

        public bool Connect()
        {
            isConnect = false;
            if (string.IsNullOrEmpty(ipAddrs))
            {
                return false;
            }
            mnDesc = ICCardCommon.rf_init(ipAddrs, ipAddrs.Length);
            if (mnDesc >= 0)
            {
                isConnect = true;
                return true;
            }
            return false;
        }

        public bool Disconnect()
        {            
            if (mnDesc >= 0)
            {
                ICCardCommon.rf_halt(mnDesc);
                ICCardCommon.rf_exit(mnDesc);
            }
            isConnect = false;
            return true;
        }
        /// <summary>
        /// 读物理卡号
        /// </summary>
        /// <param name="physicode"></param>
        /// <returns></returns>
        public int GetPhyscard(ref uint physiccard)
        {
            physiccard = 0;
            if (!string.IsNullOrEmpty(ipAddrs))
            {
                Ping ping = new Ping();
                PingReply result = ping.Send(ipAddrs);
                if (result == null || result.Status != IPStatus.Success)
                {
                    return -1;
                }
            }
            if (isConnect)
            {
                UInt16 nICType = 0;
                int nback = RequestICCard(ref nICType);
                if (nback == 0)
                {
                    uint physic = 0;
                    nback = SelectCard(ref physic);
                    if (nback == 0)
                    {
                        //处理卡号
                        physiccard = physic;
                        return nback;
                    }
                }

                if(nback == 0xF1 || nback == 0xF2)
                {
                    isConnect = false;
                    Disconnect();
                }
            }
            else
            {
                Connect();
            }
            return -1;
        }
        /// <summary>
        /// 读物理内存区数据
        /// </summary>
        /// <param name="nSector"></param>
        /// <param name="nDBnum"></param>
        /// <param name="breturn"></param>
        /// <returns></returns>
        public int ReadSectorMemory(Int16 nSector, Int16 nDBNum, ref byte[] breturn)
        {
            int nback = -1;
            byte[] bdata = new byte[16];
            short blocknum = (short)(nSector * 4 + nDBNum);
            nback =ICCardCommon.rf_authentication(mnDesc, mnAuthKeyMode, nSector);
            if (nback != 0)
            {
                return -1;
            }
            nback = ICCardCommon.rf_read(mnDesc, blocknum, bdata);

            breturn = bdata;
            return nback;
        }

        /// <summary>
        /// 向扇区中写入数据
        /// </summary>
        /// <param name="nSector"></param>
        /// <param name="nDBNum"></param>
        /// <param name="bWrite"></param>
        /// <param name="nback"></param>
        /// <returns>0:success; 其他：fail</returns>
        public int WriteSectorMemory(Int16 nSector, Int16 nDBNum, byte[] bWrite, ref int nback)
        {
            nback = -1;
            int blocknum = nSector * 4 + nDBNum;
            nback = ICCardCommon.rf_authentication(mnDesc, mnAuthKeyMode, nSector);
            if (nback != 0)
            {
                return -1;
            }
            nback = ICCardCommon.rf_write(mnDesc, (short)blocknum, bWrite);
            return nback;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nCardType"></param>
        /// <returns></returns>
        private int RequestICCard(ref UInt16 nCardType)
        {
            int nback = -1;
            if (mnDesc > 0) //设备初始化了
            {
                nback = ICCardCommon.rf_reset(mnDesc, 20);
                nback = ICCardCommon.rf_request(mnDesc, 0, out nCardType);
            }
            return nback;
        }

        /// <summary>
        /// 防冲突的情况下选卡，并选择其中停车卡
        /// </summary>
        /// <returns>返回值为0表示成功，-1表示防冲突函数失败，否则表示选卡不成功。</returns>
        private int SelectCard(ref uint nCardSequenceNum)
        {            
            byte strtemp;
            int nback = -1;
            nback =ICCardCommon.rf_anticoll(mnDesc, 0, out nCardSequenceNum);//查找卡片
            if (nback != 0)
            {
                return -1;
            }
            uint temp = nCardSequenceNum;
            nback =ICCardCommon.rf_select(mnDesc, temp, out strtemp);
            if (nback == 0)
            {
                ICCardCommon.rf_beep(mnDesc, 50);               
            }
            return nback;
        }

    }
}
