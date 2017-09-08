using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Parking.Auxiliary
{
    /// <summary>
    /// 后台读卡，其动态库有问题，暂不使用这个方式，
    /// 读卡操作，在windows服务运行吧
    /// </summary>
    public class ICCardReader:ICCardRW
    {
        #region  读卡器网口操作动态库
        /// <summary>
        /// 初始化读写器
        /// </summary>
        /// <param name="ipAddrs"></param>
        /// <param name="len"></param>
        /// <returns>返回设备描述符</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern int rf_init(string ipAddrs, int len);

        /// <summary>
        /// 退出程序
        /// </summary>
        /// <param name="icdev"></param>
        /// <returns>0-表示成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_exit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int16 rf_exit(int icdev);

        /// <summary>
        ///37.蜂鸣几毫秒。
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="_Msec">蜂鸣时间，单位：毫秒</param>
        /// <returns>设备当前状态</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_beep", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int16 rf_beep(int icdev, int _Msec);

        /// <summary>
        /// 向读写器装载指定扇区的新密码（不与卡片进行通讯），读写器中有16个扇区的密码（0~15），每个扇区有两个密码(KEY A 和 KEY B)。
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="mode">密码类型(0 — KEY A、4 — KEY B）</param>
        /// <param name="secnr">须装载密码的扇区号(0～15)</param>
        /// <param name="keybuff">写入读写器的6字节新密码</param>
        /// <returns>0: 成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_load_key", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
       private static extern Int16 rf_load_key(int icdev, int mode, int secnr, [MarshalAs(UnmanagedType.LPArray)]byte[] keybuff);

        /// <summary>
        /// 将ASCII 字符转换为16 进制数。
        /// </summary>
        /// <param name="asc">ASCII 字符</param>
        /// <param name="hex">输出的16 进制数</param>
        /// <param name="len">ASCII 字符的长度</param>
        /// <returns>0: 成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "a_hex", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int16 a_hex([MarshalAs(UnmanagedType.LPArray)]byte[] asc, [MarshalAs(UnmanagedType.LPArray)]byte[] hex, int len);

        /// <summary>
        /// 将16 进制数转换为ASCII 字符。
        /// </summary>
        /// <param name="hex">16 进制数</param>
        /// <param name="asc">输出的ASCII 字符</param>
        /// <param name="len">16 进制数的长度</param>
        /// <returns>0: 成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "hex_a", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        //说明：     返回设备当前状态
        private static extern Int16 hex_a([MarshalAs(UnmanagedType.LPArray)]byte[] hex, [MarshalAs(UnmanagedType.LPArray)]byte[] asc, int len);

        /// <summary>
        /// 将RF（射频）模块的能量释放几毫秒
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="_Msec">复位时间 ( 0~ 500ms)</param>
        /// <returns>0: 成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_reset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int16 rf_reset(int icdev, int _Msec);

        /// <summary>
        /// 该函数向卡片发出寻卡命令，开始选择一张新卡片时需要执行该函数。
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="Mode">
        /// 寻卡模式:0 IDLE mode, 只有处在IDLE 状态的卡片才响应读写器的命令。1 ALL mode, 处在 IDLE 状态和HALT 状态的卡片都将响应读写器的命令。
        /// </param>
        /// <param name="tagtype">返回卡片类型</param>
        /// <returns>0: 成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_request", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int16 rf_request(int icdev, int Mode, out UInt16 tagtype);

        /// <summary>
        /// 激活读写器的防冲突队列。如果有几张MIFARE 卡片在感应区内，将会选择一张卡片，并返回卡片的序列号供将来调用rf_select 函数时使用。
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="bcnt">预选卡片使用的位, 标准调用时为bcnt=0</param>
        /// <param name="snr">返回的卡片序列号</param>
        /// <returns>0: 成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_anticoll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
       private static extern Int16 rf_anticoll(int icdev, int bcnt, out uint snr);

        /// <summary>
        /// 用指定的序列号选择卡片，将卡片的容量返回给PC 机。
        /// </summary>
        /// <param name="icdev">返回的设备描述符</param>
        /// <param name="snr">卡片的序列号</param>
        /// <param name="size">卡片容量的地址指针，目前该值不能使用</param>
        /// <returns>返回值， 0-表示成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_select", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int16 rf_select(int icdev, uint snr, out byte size);

        /// <summary>
        ///7.验证读写器中的密码与需要访问的卡片的同一扇区(0~15)的密码是否一致。如果读写器中选择的密码（可用rf_load_key 函数修改）与卡片的相匹配，密码验证通过，传输的数据将用以下的命令加密
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="mode">验证密码类型：0 — 用KEY A 验证 4 — 用 KEY B 验证</param>
        /// <param name="secnr">将要访问的卡片扇区号(0~15)</param>
        /// <returns>0-表示成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_authentication", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int16 rf_authentication(int icdev, int mode, int secnr);

        /// <summary>
        /// 从一张选定并通过密码验证的卡片读取一块共16个字节的数据。
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="blocknr">读取数据的块号(0~63)</param>
        /// <param name="databuff">Data:读取的数据，PC 机上RAM 的地址空间由调用该函数来分配。</param>
        /// <returns>0-表示成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_read", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int16 rf_read(int icdev, int blocknr, [MarshalAs(UnmanagedType.LPArray)]byte[] databuff);

        /// <summary>
        /// 11.将一块共16字节写入选定并验证通过的卡片中。
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="blocknr">写入数据的块地址 (1~63)</param>
        /// <param name="databuff">写入数据,长度为16字节</param>
        /// <returns>0: 成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_write", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
       private static extern Int16 rf_write(int icdev, int blocknr, [MarshalAs(UnmanagedType.LPArray)]byte[] databuff);

        /// <summary>
        ///8.将一张选中的卡片设为“Halt”模式，只有当该卡再次复位或用ALL 模式调用request 函数时，读写器才能够再次操作它
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <returns>0-表示成功</returns>
        [DllImport("RFBOOK.dll", EntryPoint = "rf_halt", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int16 rf_halt(int icdev);
        #endregion

        private string ipAddrs;
        private bool isConnect;
        private int mnDesc;
        private short mnAuthKeyMode = 0;
        private Log log;

        private ICCardReader()
        {
            log = LogFactory.GetLogger("ICCardReader");
        }

        public ICCardReader(string ip) :
            this()
        {
            ipAddrs = ip;
            isConnect = false;
        }

        public bool Connect()
        {
            isConnect = false;
            try
            {
                mnDesc = -1;
                if (string.IsNullOrEmpty(ipAddrs))
                {
                    return false;
                }
                //建立连接时，ping下
                Ping ping = new Ping();
                PingReply result = ping.Send(ipAddrs);
                if (result == null || result.Status != IPStatus.Success)
                {
                    return false;
                }
                mnDesc = rf_init(ipAddrs, ipAddrs.Length);
                if (mnDesc >= 0)
                {
                    isConnect = true;                   
                }
                //ICCardCommon.rf_beep(mnDesc, 50);
            }
            catch (Exception ex)
            {
                log.Error("Connect 异常："+ex.ToString());
            }
            return isConnect;
        }

        public bool Disconnect()
        {
            try
            {
                if (mnDesc >= 0)
                {
                    rf_halt(mnDesc);                    
                }
                rf_exit(mnDesc);
            }
            catch (Exception ex)
            {
                log.Error("Disconnect 异常：" + ex.ToString());
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
            try
            {
                physiccard = 0;               
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
                    //依这个来改变连接状态，以便可以进行重连
                    if (nback == 0xF1 || nback == 0xF2)
                    {
                        isConnect = false;
                        Disconnect();
                    }
                }
                else
                {
                    Connect();
                }
            }
            catch (Exception ex)
            {
                log.Error("GetPhyscard 异常："+ex.ToString());
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
            nback =rf_authentication(mnDesc, mnAuthKeyMode, nSector);
            if (nback != 0)
            {
                return -1;
            }
            nback = rf_read(mnDesc, blocknum, bdata);

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
            nback = rf_authentication(mnDesc, mnAuthKeyMode, nSector);
            if (nback != 0)
            {
                return -1;
            }
            nback = rf_write(mnDesc, (short)blocknum, bWrite);
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
                nback = rf_reset(mnDesc, 20);
                nback = rf_request(mnDesc, 0, out nCardType);
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
            nback =rf_anticoll(mnDesc, 0, out nCardSequenceNum);//查找卡片
            if (nback != 0)
            {
                return -1;
            }
            uint temp = nCardSequenceNum;
            nback =rf_select(mnDesc, temp, out strtemp);
            if (nback == 0)
            {
                rf_beep(mnDesc, 50);               
            }
            return nback;
        }

    }
}
