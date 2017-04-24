using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Parking.Auxiliary;
using System.Text.RegularExpressions;

namespace Parking.Application
{
   
    public class SocketPlc:IPLC, IDisposable
    {
        #region 枚举型数据
        public enum CPUType
        {
            S7200 = 0,
            S7300 = 10,
            S7400 = 20,
            S71200 = 30,
        }

        /// <summary>
        /// 数据类型，默认为DB
        /// </summary>
        public enum DataType
        {
            Init = 0,
            Input = 129,
            Output = 130,
            Memory = 131,
            DataBlock = 132,
            Timer = 29,
            Counter = 28
        }

        /// <summary>
        /// 读取的数据单位 byte=1,Int=4
        /// 要返回的数据的类型
        /// </summary>
        public enum VarType
        {
            Bit = 0,
            Byte = 1,
            Word = 2,
            DWord = 3,
            Int = 4,
            DInt,
            Real,
            String,
            Timer,
            Counter
        }
        #endregion

        private string IP;
        private CPUType CPU;
        private short Rack;
        private short Slot;
        private bool isConnect;
        private Socket mSocket;

        public SocketPlc() 
        {
            isConnect = false;
        }

        public SocketPlc(string ip) : 
            this()
        {
            CPU = CPUType.S7300;
            IP = ip;
            Rack = 0;
            Slot = 2;
        }

        public SocketPlc(CPUType ctype, string ip, short rack, short slot)
            : this()
        {
            CPU = ctype;
            IP = ip;
            Rack = rack;  //0
            Slot = slot;  //0-2
        }

        /// <summary>
        /// 判断是否连接
        /// </summary>
        public bool IsConnected 
        {
            get 
            { 
                return isConnect;
            }
            set
            {
                ;
            }
        }

        /// <summary>
        /// 建立SOCKET,连接PLC
        /// </summary>
        /// <returns></returns>
        public bool ConnectPLC()
        {
            #region
            try 
            {
                if (!isIPAvailable) 
                {
                    throw new Exception("IP地址无效，或网络连接出现异常！");
                }
            }
            catch (Exception ex) 
            {               
                throw ex;
            }

            try 
            {
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
                mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);

                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(IP), 102);
                mSocket.Connect(endpoint);
            }
            catch (Exception ex) 
            {                
                throw ex;
            }

            try
            {
                byte[] bSend1 = new byte[22] { 3, 0, 0, 22, 17, 224, 0, 0, 0, 46, 0, 193, 2, 1, 0, 194, 2, 3, 0, 192, 1, 9 };
                //先做这两个的，1500后续测试再做
                if (CPU == CPUType.S7300 || CPU == CPUType.S71200)
                {
                    //S7300: Chr(193) & Chr(2) & Chr(1) & Chr(0)  'Eigener Tsap
                    bSend1[11] = 193;
                    bSend1[12] = 2;
                    bSend1[13] = 1;
                    bSend1[14] = 0;
                    //S7300: Chr(194) & Chr(2) & Chr(3) & Chr(2)  'Fremder Tsap
                    bSend1[15] = 194;
                    bSend1[16] = 2;
                    bSend1[17] = 3;
                    bSend1[18] = (byte)(Rack * 2 * 16 + Slot);
                }
                else 
                {                   
                    return false;
                }
                
                byte[] bReceive = new byte[256];
                mSocket.Send(bSend1, bSend1.Length, SocketFlags.None);
                int recvLenght = mSocket.Receive(bReceive, 22, SocketFlags.None);  //bSend1.Length=22
                if (recvLenght != 22)
                {                   
                    throw new Exception("建立PLC连接时，发送的22个字节数据后接收到的字节数据不匹配！");
                }

                byte[] bsend2 = new byte[25] { 3, 0, 0, 25, 2, 240, 128, 50, 1, 0, 0, 255, 255, 0, 8, 0, 0, 240, 0, 0, 3, 0, 3, 1, 0 };
                mSocket.Send(bsend2, 25, SocketFlags.None);
                if (mSocket.Receive(bReceive, 27, SocketFlags.None) != 27)
                {                   
                    throw new Exception("建立PLC连接时，发送的25个字节数据后接收到的字节数据不匹配！");
                }

                isConnect = true;
                return true;
            }
            catch(Exception ex)
            {
                isConnect = false;
                throw new Exception("与PLC建立握手失败，无法建立连接，异常信息："+ex.ToString());              
            }
            #endregion
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public bool DisConnectPLC() 
        {
            if (mSocket != null) 
            {
                if (mSocket.Connected) 
                {
                    isConnect = false;
                    mSocket.Close();
                }                
            }
            mSocket = null;

            return true;
        }

        /// <summary>
        /// 依订阅名，读取对应的数据
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns>DataType dataType, int DB, int startByteAdr, int count</returns>
        public object ReadData(string itemName, string varType)
        {
            DataType dataType = DataType.Init;
            int DB = 0;
            int startByteAddrs = 0;
            int count = 0;

            //itemName格式 S7:[S7 connection_1]DB1001,INT0,50
            Log log = LogFactory.GetLogger("CSocketPlc.ReadData");
            string[] lstItems = itemName.Split(',');
            if (lstItems.Length == 0)
            {                
                log.Info("Item-"+itemName+" 格式不正确！split(',')不出来");
                return null;
            }
            if (lstItems.Length < 3)
            {
                log.Info("Item-" + itemName + " 格式不正确！split(',')出来长度不小于3！");
                return null;
            }
            string head = lstItems[0];
            string[] hArray = head.Split(']');            
            if (hArray.Length == 0)
            {
                log.Info("Item-" + itemName + " 格式不正确！Split(']')不出来");
                return null;
            }
            string dtype = hArray.Last();           
            if (dtype.Contains("DB"))
            {
                dataType = DataType.DataBlock;
            }
            else
            {
                log.Info("Itme-"+itemName+"格式不正确，不包含DB！");
                return null;
            }
            DB = removeNotNumber(dtype);

            string startAddress = lstItems[1];
            if (!startAddress.ToLower().Contains("int"))
            {
                log.Info("Itme-" + itemName + "格式不正确，不包含INT！");
                return null;
            }
            startByteAddrs = removeNotNumber(startAddress);

            if (!isNumber(lstItems[2]))
            {
                log.Info("Itme-" + itemName + "格式不正确，最后一组不为数字！");
                return null;
            }
            count = Convert.ToInt32(lstItems[2]);

            VarType vtype = VarType.Byte;
            Enum.TryParse<VarType>(varType, out vtype);

            return Read(dataType, DB, startByteAddrs,vtype, count);
        }

        /// <summary>
        /// 依订阅名，读取对应的数据
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="value">数据，可以是字节(byte)，字节数组（byte[]），整形(int16)，整形数组（int16[]）</param>
        /// <returns></returns>
        public int WriteData(string itemName, object value)
        {
            DataType dataType = DataType.Init;
            int DB = 0;
            int startByteAddrs = 0;
           
            //itemName格式 S7:[S7 connection_1]DB1001,INT0,50
            Log log = LogFactory.GetLogger("CSocketPlc.WriteData");
            string[] lstItems = itemName.Split(',');
            if (lstItems.Length == 0)
            {
                log.Info("Item-" + itemName + " 格式不正确！split(',')不出来");
                return -1;
            }
            if (lstItems.Length < 3)
            {
                log.Info("Item-" + itemName + " 格式不正确！split(',')出来长度不小于3！");
                return -1;
            }
            string head = lstItems[0];
            string[] hArray = head.Split(']');
            if (hArray.Length == 0)
            {
                log.Info("Item-" + itemName + " 格式不正确！Split(']')不出来");
                return -1;
            }
            string dtype = hArray.Last();
            if (dtype.Contains("DB"))
            {
                dataType = DataType.DataBlock;
            }
            else
            {
                log.Info("Itme-" + itemName + "格式不正确，不包含DB！");
                return -1;
            }
            DB = removeNotNumber(dtype);

            string startAddress = lstItems[1];
            if (!startAddress.ToLower().Contains("int"))
            {
                log.Info("Itme-" + itemName + "格式不正确，不包含INT！");
                return -1;
            }
            startByteAddrs = removeNotNumber(startAddress);

            return Write(dataType, DB, startByteAddrs, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (mSocket != null)
            {
                if (mSocket.Connected)
                {
                    mSocket.Close();                   
                }                
            }
        }

        /// <summary>
        /// 读取PLC数据区字节数据
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="DB">要读取的数据地址</param>
        /// <param name="startByteAdr">字节数据的起始地址</param>
        /// <param name="count">读取的字节数据数</param>
        /// <returns></returns>
        private byte[] readBytesValue(DataType dataType, int DB, int startByteAdr, int count) 
        {
            try
            {
                #region               
                if (mSocket == null||isBlockConnected(mSocket))
                {
                    isConnect = false;
                    return null;
                }

                // first create the header
                int packageSize = 31;
                List<byte> package = new List<byte>();

                package.AddRange(new byte[] { 0x03, 0x00, 0x00 });
                package.Add((byte)packageSize);
                package.AddRange(new byte[] { 0x02, 0xf0, 0x80, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x04, 0x01, 0x12, 0x0a, 0x10 });
                // package.Add(0x02);  // datenart
                switch (dataType)
                {
                    case DataType.Timer:
                    case DataType.Counter:
                        package.Add((byte)dataType);
                        break;
                    default:
                        package.Add(0x02);
                        break;
                }
                package.AddRange(this.toByteArray((UInt16)count));
                package.AddRange(this.toByteArray((UInt16)DB));
                package.Add((byte)dataType);
                package.Add((byte)0);
                switch (dataType)
                {
                    case DataType.Timer:
                    case DataType.Counter:
                        package.AddRange(this.toByteArray((UInt16)(startByteAdr)));
                        break;
                    default:
                        package.AddRange(this.toByteArray((UInt16)((startByteAdr) * 8)));
                        break;
                }
                mSocket.Send(package.ToArray(), package.Count, SocketFlags.None);

                byte[] bReceive = new byte[512];
                int numReceivced = mSocket.Receive(bReceive, bReceive.Length, SocketFlags.None);
                if (bReceive[21] != 0xff)
                {
                    throw new Exception("接收到的21个数据字节不匹配！无法完成读取操作！");
                }
                byte[] rdbytes = new byte[count];
                for (int j = 0; j < count; j++)
                {
                    rdbytes[j] = bReceive[j + 25];
                }
                return rdbytes;
                #endregion
            }
            catch(SocketException ec)
            {
                isConnect = false;
                throw ec;
            }
            catch (Exception ex)
            {               
                throw ex;
            }
        }

        /// <summary>
        /// 读取数据区
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="DB"></param>
        /// <param name="startByteAddr"></param>
        /// <param name="varType">要返回的数据的类型,目前只有字节或短整型</param>
        /// <param name="varCount"></param>
        /// <returns>返回整型(int16,int16[])数据或字节型(byte,byte[])数据</returns>
        public object Read(DataType dataType, int DB, int startByteAddr, VarType varType, int varCount)
        {
            #region
            byte[] rdBytes = null;
            int cntBytes = 0;

            switch (varType) 
            {
                case VarType.Byte:
                    cntBytes = varCount;
                    if (cntBytes < 1) 
                    {
                        cntBytes = 1;
                    }
                    rdBytes = readBytesValue(dataType, DB, startByteAddr, cntBytes);
                    if (rdBytes == null) 
                    {
                        return null;
                    }
                    if (cntBytes == 1)
                    {
                        return rdBytes[0];
                    }
                    else 
                    {
                        return rdBytes;
                    }
                   
                case VarType.Int:
                    cntBytes = varCount * 2;
                    rdBytes = readBytesValue(dataType, DB, startByteAddr, cntBytes);
                    if (rdBytes == null)
                    {
                        return null;
                    }
                    if (varCount == 1) 
                    {
                        return this.fromByteArray(rdBytes);
                    }
                    return this.ToArray(rdBytes);                   
                default:
                    return null;                   
            }
            #endregion
        }

        /// <summary>
        /// 写入字节数组数据
        /// </summary>
        /// <param name="dataType">操作的数据对象</param>
        /// <param name="DB">数据值</param>
        /// <param name="startByteAdr">数据起始地址</param>
        /// <param name="value">要写入的数组</param>
        /// <returns></returns>
        private int writeBytes(DataType dataType, int DB, int startByteAdr, byte[] value)
        {
            #region
            try
            {
                if (mSocket == null || isBlockConnected(mSocket))
                {
                    isConnect = false;
                    return 101;
                }
                int varCount = value.Length;
                int packageSize = 35 + varCount;
                List<byte> package = new List<byte>();

                package.AddRange(new byte[] { 3, 0, 0 });
                package.Add((byte)packageSize);
                package.AddRange(new byte[] { 2, 0xf0, 0x80, 0x32, 1, 0, 0 });
                package.AddRange(this.toByteArray((ushort)(varCount - 1)));
                package.AddRange(new byte[] { 0, 0x0e });
                package.AddRange(this.toByteArray((ushort)(varCount + 4)));
                package.AddRange(new byte[] { 0x05, 0x01, 0x12, 0x0a, 0x10, 0x02 });
                package.AddRange(this.toByteArray((ushort)varCount));
                package.AddRange(this.toByteArray((ushort)(DB)));
                package.Add((byte)dataType);
                package.Add((byte)0);
                package.AddRange(this.toByteArray((ushort)(startByteAdr * 8)));
                package.AddRange(new byte[] { 0, 4 });
                package.AddRange(this.toByteArray((ushort)(varCount * 8)));

                // now join the header and the data
                package.AddRange(value);
                mSocket.Send(package.ToArray(),package.Count,SocketFlags.None);

                byte[] bReceive = new byte[513];
                mSocket.Receive(bReceive, 512, SocketFlags.None);
                if (bReceive[21] != 0xff) 
                {
                    throw new Exception("接收到的21个数据字节不匹配！无法完成读取操作！");
                }
                return 1;
            }
            catch (Exception ex) 
            {
                isConnect = false;
                throw ex;
            }
            #endregion
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="DB"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Write(DataType dataType, int DB, int startByteAdr, object value)
        {
            #region
            byte[] package = null;
            switch (value.GetType().Name) 
            {
                case "Byte":
                    package = new byte[] { (byte)value };
                    break;
                case "Int16":
                    package = this.ToByteArray((short)value);
                    break;
                case "Byte[]":
                    package = (byte[])value;
                    break;
                case "Int16[]":
                    package = this.ShortArrayToByteArray((short[])value);
                    break;
                default:
                    return 103;
            }
            return writeBytes(dataType,DB,startByteAdr,package);
            #endregion
        }

        private bool isIPAvailable 
        {
            get 
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(IP);
                if (reply != null) 
                {
                    if (reply.Status == IPStatus.Success) 
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 无符号短整型转化为字节数据
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private byte[] toByteArray(UInt16 value) 
        {
            byte[] bytes = new byte[2];
            int x = 2;
            long valLong = (long)((UInt16)value);
            for (int cnt = 0; cnt < x; cnt++)
            {
                Int64 x1 = (Int64)Math.Pow(256, (cnt));

                Int64 x3 = (Int64)(valLong / x1);
                bytes[x - cnt - 1] = (byte)(x3 & 255);
                valLong -= bytes[x - cnt - 1] * x1;
            }
            return bytes;
        }

        /// <summary>
        /// 将字节数据转化为整型，高低字节互换
        /// </summary>
        /// <param name="bytes">字节数据</param>
        /// <returns></returns>
        private Int16 fromByteArray(byte[] bytes)
        {
            return (Int16)(bytes[0] * 256 + bytes[1]);
        }

        /// <summary>
        /// 将字节数组转化为整型数组
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private  Int16[] ToArray(byte[] bytes)
        {
            Int16[] values = new Int16[bytes.Length / 2];
            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 2; cnt++)
                values[cnt] = fromByteArray(new byte[] { bytes[counter++], bytes[counter++] });

            return values;
        }

        /// <summary>
        /// 短整型转化为字节数据
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private byte[] ToByteArray(Int16 value)
        {
            byte[] bytes = new byte[2];
            int x = 2;
            long valLong = (long)((Int16)value);
            for (int cnt = 0; cnt < x; cnt++)
            {
                Int64 x1 = (Int64)Math.Pow(256, (cnt));

                Int64 x3 = (Int64)(valLong / x1);
                bytes[x - cnt - 1] = (byte)(x3 & 255);
                valLong -= bytes[x - cnt - 1] * x1;
            }
            return bytes;
        }

        /// <summary>
        /// 整型数组转化为字节型数组
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private byte[] ShortArrayToByteArray(Int16[] value)
        {
            List<byte> arr = new List<byte>();
            foreach (Int16 val in value)
            {                
                arr.AddRange(this.ToByteArray(val));
            }
            return arr.ToArray();
        }

        /// <summary>
        /// 检查Socket是否连接
        /// </summary>
        /// <param name="client"></param>
        /// <returns>false:连接，true:中断</returns>
        private bool isBlockConnected(Socket client)
        {
            bool blockingState = client.Blocking;
            try
            {
                byte[] tmp = new byte[1];
                client.Blocking = false;
                client.Send(tmp, 0, 0);
                return false;
            }
            catch (SocketException e)
            {
                // 产生 10035 == WSAEWOULDBLOCK 错误，说明被阻止了，但是还是连接的  
                if (e.NativeErrorCode.Equals(10035))
                    return false;
                else
                    return true;
            }
            finally
            {
                client.Blocking = blockingState;    // 恢复状态  
            }
        }

        /// <summary>
        /// 去掉字符串中非数字的
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private int removeNotNumber(string msg)
        {
            string key = Regex.Replace(msg,@"[^\d]*","");
            if (!string.IsNullOrWhiteSpace(key))
            {
                return Convert.ToInt32(key);
            }
            return -1;
        }
        /// <summary>
        /// 是否为数字
        /// </summary>
        /// <param name="linkNum"></param>
        /// <returns></returns>
        private bool isNumber(string linkNum)
        {
            string pattern = "^[0-9]*[1-9][0-9]*$";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(linkNum);
        }
    }
}
