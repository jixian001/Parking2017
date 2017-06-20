using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Parking.Auxiliary
{
    public class FPrintBase64
    {
        #region 动态库
        /// <summary>
        /// 十六进制格式的指纹模板/特征数据转Base64格式
        /// </summary>
        /// <param name="psSrcBuf">待转换数据数据</param>
        /// <param name="iSrcLen">待转换数据长度</param>
        /// <param name="psDesBuf">转换后数据</param>
        /// <param name="piDestLen">转换后数据长度</param>
        /// <param name="lpImageHeight">图像高度</param>
        /// <param name="lpImageLen">图像数据长度</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev_Base64.dll")]
        protected static extern int FPIHexFingerDataToBase64(byte[] psSrcBuf, int iSrcLen, byte[] psDesBuf, ref int piDestLen);

        /// <summary>
        /// Base64格式的指纹模板/特征数据转十六进制格式
        /// </summary>
        /// <param name="psSrcBuf">待转换数据数据</param>
        /// <param name="iSrcLen">待转换数据长度</param>
        /// <param name="psDesBuf">转换后数据</param>
        /// <param name="piDestLen">转换后数据长度</param>
        /// <param name="lpImageHeight">图像高度</param>
        /// <param name="lpImageLen">图像数据长度</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev_Base64.dll")]
        protected static extern int FPIBase64FingerDataToHex(byte[] psSrcBuf, int iSrcLen, byte[] psDesBuf, ref int piDestLen);
        #endregion

        /// <summary>
        /// base64格式的指纹转化为16进制格式
        /// </summary>
        /// <param name="cpMBStr"></param>
        /// <returns></returns>
        public static byte[] Base64FingerDataToHex(string cpMBStr)
        {
            byte[] psSrcBuf = Encoding.Default.GetBytes(cpMBStr);
            byte[] cpMBBuf = new byte[512];
            int piDestLen = 0;
            int iRet =FPIBase64FingerDataToHex(psSrcBuf, 512, cpMBBuf, ref piDestLen);
            if (iRet == 0)
            {
                byte[] psDesBuf = new byte[piDestLen];
                Array.ConstrainedCopy(cpMBBuf, 0, psDesBuf, 0, piDestLen);

                return psDesBuf;
            }
            return null;
        }

    }
}
