using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Parking.Auxiliary
{
    /// <summary>
    /// 指纹仪库操作类
    /// </summary>
    public class FPDll
    {
        public const int IMAGE_QUALITY_WET	= 0x02;
        public const int IMAGE_QUALITY_DRY = 0x04;

        public const int IMAGE_QUALITY_TOP = 0x02;
        public const int IMAGE_QUALITY_BOTTOM = 0x04;
        public const int IMAGE_QUALITY_LEFT = 0x08;
        public const int IMAGE_QUALITY_RIGHT = 0x10;
        public const int IMAGE_QUALITY_SMALL = 0x20;

        public const int IMAGE_QUALITY_TOP_LEFT = (0x02 | 0x08);

        public const int IMAGE_QUALITY_OK = 0;
        public const int IMAGE_QUALITY_UNKNOWN = 0xFF;
        public const int IMAGE_QUALITY_NO_FINGER = 0x01;

        /// <summary>
        /// 获取开发版本信息
        /// </summary>
        ///  <return>返回 0：表示成功；返回-1：表示失败；</return>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIGetSDKVersion(byte[] cpSDKInfo);

        /// <summary>
        /// 检测设备
        /// </summary>
        /// <param name="iDeviceType">设备类型，0—所有设备, 1—串口设备，1001—USB设备</param>
        /// <returns>返回 >0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIDevDetect(int iDeviceType);

        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="iDeviceType">设备类型，0—所有设备, 1—串口设备，1001—USB设备</param>
        /// <param name="iDeviceChannel">设备通道，即第几个设备</param>
        /// <returns>返回 设备句柄：表示成功；返回null：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern IntPtr FPIOpenDevice(int iDeviceType, int iDeviceChannel);

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPICloseDevice(ref IntPtr hDevice);

        /// <summary>
        /// 获取版本信息
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="psOutversion">设备信息</param>
        /// <param name="piLength">设备信息长度</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIGetVersion(IntPtr hDevice, byte[] psOutversion, ref int piLength);

        /// <summary>
        /// 获取设备序列号
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="psSerialNumber">设备序列号</param>
        /// <param name="piLength">设备序列号数据长度</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIGetSerialNumber(IntPtr hDevice, byte[] psSerialNumber, ref int piLength);

        /// <summary>
        /// 读用户数据
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="iOffset">数据偏移量</param>
        /// <param name="data">读取数据缓存</param>
        /// <param name="length">读取数据长度</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIReadData(IntPtr hDevice, int iOffset,byte[] data, int length);

        /// <summary>
        /// 写用户数据
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="iOffset">数据偏移量</param>
        /// <param name="data">写入数据缓存</param>
        /// <param name="length">写入数据长度</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIWriteData(IntPtr hDevice, int iOffset, string data, int length);

        /// <summary>
        /// 采集模板
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="iTimeout">超时时间</param>
        /// <param name="iImageDataType">传出的图像类型，1—raw数据，2—bmp格式数据</param>
        /// <param name="psMB">模板数据</param>
        /// <param name="piMBLength">模板数据长度</param>
        /// <param name="piImageWidth">图像宽度</param>
        /// <param name="piImageHeight">图像高度</param>
        /// <param name="psImageData1">图像1数据</param>
        /// <param name="piImageBufLen1">图像1数据长度</param>
        /// <param name="psImageData2">图像2数据</param>
        /// <param name="piImageBufLen2">图像2数据长度</param>
        /// <param name="psImageData3">图像3数据</param>
        /// <param name="piImageBufLen3">图像3数据长度</param>
        /// <param name="piMBQuality">模板质量</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPITemplate(IntPtr hDevice, int iTimeout, int iImageDataType, byte[] psMB, ref int piMBLength, ref int piMBQuality
        , ref int piImageWidth, ref int piImageHeight, byte[] psImageData1, ref int piImageBufLen1, byte[] psImageData2, ref int piImageBufLen2
        , byte[] psImageData3, ref int piImageBufLen3);

        /// <summary>
        /// 采集特征
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="iImageDataType">传出的图像类型，1—raw数据，2—bmp格式数据</param>
        /// <param name="psTZ">特征数据</param>
        /// <param name="piTZLength">特征数据长度</param>
        /// <param name="piImageWidth">图像宽度</param>
        /// <param name="piImageHeight">图像高度</param>
        /// <param name="psImageData">图像数据</param>
        /// <param name="piImageBufLen">图像数据长度</param>
        /// <param name="piTZQuality">特征质量</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIFeature(IntPtr hDevice, int iTimeout, int iImageDataType, byte[] psTZ, ref int piTZLength, ref int piTZQuality, ref int piImageWidth, ref int piImageHeight, byte[] psImageData, ref int piImageBufLen);

        /// <summary>
        /// 检测手指
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <returns>返回 0：表示按下；1：表示抬起；其他失败</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPICheckFinger(IntPtr hDevice);

        /// <summary>
        /// 采集指纹图像
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="iImageDataType">传出的图像类型，1—raw数据，2—bmp格式数据</param>
        /// <param name="piImageWidth">图像宽度</param>
        /// <param name="piImageHeight">图像高度</param>
        /// <param name="psImageData">图像数据</param>
        /// <param name="piImageQuality">图像质量</param>
        /// <param name="piFingerDryOrWet">图像干湿</param>
        /// <param name="piFingerOffset">图像偏移</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIGetImageData(IntPtr hDevice, int iTimeout, int iImageDataType, ref int piImageWidth, ref int piImageHeight, byte[] psImageData, ref int piImageBufLen, ref int piImageQuality, ref int piFingerDryOrWet, ref int piFingerOffset);

        /// <summary>
        /// 从指纹图像抽取指纹特征
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="iImageDataType">传出的图像类型，1—raw数据，2—bmp格式数据</param>
        /// <param name="iImageWidth">图像宽度</param>
        /// <param name="iImageHeight">图像高度</param>
        /// <param name="psImageData">图像数据</param>
        /// <param name="psTZ">特征数据</param>
        /// <param name="piTZLength">特征数据长度</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIExtractByImageData(IntPtr hDevice, int iImageDataType, byte[] psImageData, int iImageWidth, int iImageHeight, byte[] psTZ, ref int piTZLength); 

        /// <summary>
        /// 立即采集指纹图像
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="iImageDataType">传出的图像类型，1—raw数据，2—bmp格式数据</param>
        /// <param name="piImageWidth">图像宽度</param>
        /// <param name="piImageHeight">图像高度</param>
        /// <param name="psImageData">图像数据</param>
        /// <param name="piImageQuality">图像质量</param>
        /// <param name="piFingerDryOrWet">图像干湿</param>
        /// <param name="piFingerOffset">图像偏移</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIGetImageDataImmediately(IntPtr hDevice, int iImageDataType, ref int piImageWidth, ref int piImageHeight, byte[] psImageData, ref int piImageBufLen, ref int piImageQuality, ref int piFingerDryOrWet, ref int piFingerOffset);

        /// <summary>
        /// 指纹比对
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="psMB">模板数据</param>
        /// <param name="psTZ">特征数据</param>
        /// <param name="iLevel">安全等级</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIVerify(IntPtr hDevice, byte[] psMB, byte[] psTZ, int iLevel);

        /// <summary>
        /// 指纹图像点阵转灰色位图格式
        /// </summary>
        /// <param name="psImageData">点阵数据</param>
        /// <param name="iImageWidth">图像宽度</param>
        /// <param name="iImageHeight">图像高度</param>
        /// <param name="psBitmapData">bmp数据</param>
        /// <param name="lpBitmapLen">bmp数据长度</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIRawImageDataToGrayBitmapData(byte[] psImageData, int iImageWidth, int iImageHeight, byte[] psBitmapData, ref int lpBitmapLen);

        /// <summary>
        /// 位图格式转指纹图像点阵
        /// </summary>
        /// <param name="psBitmapData">bmp图像数据</param>
        /// <param name="iBitmapLen">bmp图像数据长度</param>
        /// <param name="psImageData">点阵数据</param>
        /// <param name="lpImageWidth">图像宽度</param>
        /// <param name="lpImageHeight">图像高度</param>
        /// <param name="lpImageLen">图像数据长度</param>
        /// <returns>返回 0：表示成功；返回-1：表示失败；</returns>
        [DllImport("JZT30Dev.dll")]
        public static extern int FPBitmapDataToRawImageData(byte[] psBitmapData, int iBitmapLen, byte[] psImageData, ref int lpImageWidth, ref int lpImageHeight, ref int lpImageLen);
        
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
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIHexFingerDataToBase64(byte[] psSrcBuf, int iSrcLen, byte[] psDesBuf, ref int piDestLen);

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
        [DllImport("JZT30Dev.dll")]
        public static extern int FPIBase64FingerDataToHex(byte[] psSrcBuf, int iSrcLen, byte[] psDesBuf, ref int piDestLen);
    }
}
