using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    public class FPrint
    {
        private IntPtr ghDevicePtr = IntPtr.Zero;
        private const int iImageDataType = 2; //传出的图像类型，1—raw数据，2—bmp格式数据
        private const int iLevel = 3;  //安全等级

        public FPrint()
        {
        }

        /// <summary>
        /// 打开设备
        /// </summary>
        /// <returns></returns>
        public Response OpenDevice()
        {
            Response resp = new Response();
            ghDevicePtr = FPDll.FPIOpenDevice(0, 0);
            if (ghDevicePtr != IntPtr.Zero)
            {
                resp.Code = 1;
                resp.Message = "打开设备成功";
            }
            else
            {
                resp.Code = 0;
                resp.Message = "打开设备失败";
            }
            return resp;
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns></returns>
        public Response CloseDevice()
        {
            Response resp = new Response();
            if (ghDevicePtr == IntPtr.Zero)
            {
                resp.Code = 2;
                resp.Message = "请先打开设备";
                return resp;
            }
            int nback = FPDll.FPICloseDevice(ref ghDevicePtr);
            if (nback == 0)
            {
                resp.Code = 1;
                resp.Message = "关闭设备成功";
            }
            else
            {
                resp.Code = 0;
                resp.Message = "关闭设备失败";
            }
            return resp;
        }

        /// <summary>
        /// 检测手指
        /// </summary>
        /// <returns></returns>
        public Response CheckFinger()
        {
            Response resp = new Response();
            int nback = FPDll.FPICheckFinger(ghDevicePtr);
            if (nback == 0)
            {
                resp.Code = 1;
                resp.Message = "手指被按下";
            }
            return resp;
        }

        /// <summary>
        /// 采集指纹模板
        /// </summary>
        /// <returns></returns>
        public Response GetFingerTemplate()
        {
            Response resp = new Response();

            int iTimeOut = 0;
            int iImageBufLen1 = 0;
            int iImageBufLen2 = 0;
            int iImageBufLen3 = 0;
            int iImageWidth = 0;
            int iImageHeight = 0;
            int iMBQuality = 0;

            byte[] cpImageData1 = new byte[256 * 360 + 1];
            byte[] cpImageData2 = new byte[256 * 360 + 1];
            byte[] cpImageData3 = new byte[256 * 360 + 1];
            byte[] cpMBStr = new byte[1024];
            
            int iMBSize = 0;

            byte[] cpMBBuf = new byte[512];

            int nback = FPDll.FPITemplate(ghDevicePtr, iTimeOut, iImageDataType, cpMBBuf, ref iMBSize, ref iMBQuality, ref iImageWidth, ref iImageHeight, cpImageData1, ref iImageBufLen1, cpImageData2, ref iImageBufLen2, cpImageData3, ref iImageBufLen3);
            if (nback == 0)
            {
                byte[] cpMB = new byte[iMBSize];
                Array.ConstrainedCopy(cpMBBuf, 0, cpMB, 0, iMBSize);
                resp.Code = 1;
                resp.Message = "采集模板成功，模板质量：" + iMBQuality + " 长度：" + iMBSize;
                resp.Data = cpMB;
            }
            else
            {
                resp.Code = 0;
                resp.Message = "采集模板失败";                
            }

            return resp;
        }

        /// <summary>
        /// 十六进制格式的指纹模板/特征数据转 Base64 格式,使用default编码，生成字符串
        /// </summary>
        /// <param name="cpMBBuf"></param>
        /// <param name="iMBstr"></param>
        /// <returns></returns>
        public string HexFingerDataToBase64(byte[] cpMBBuf,int iMBstr)
        {
            byte[] cpMBStr = new byte[1024];
            int iMBStrLen = 0;
            int iRet = FPDll.FPIHexFingerDataToBase64(cpMBBuf, iMBstr, cpMBStr,ref iMBStrLen);
            if (iRet == 0)
            {
                byte[] psDesBuf = new byte[iMBStrLen];
                Array.ConstrainedCopy(cpMBStr, 0, psDesBuf, 0, iMBStrLen);
                return Encoding.Default.GetString(psDesBuf);
            }
            return null;
        }

        /// <summary>
        /// base64格式的指纹转化为16进制格式
        /// </summary>
        /// <param name="cpMBStr"></param>
        /// <returns></returns>
        public byte[] Base64FingerDataToHex(string cpMBStr)
        {
            byte[] psSrcBuf = Encoding.Default.GetBytes(cpMBStr);
            byte[] cpMBBuf = new byte[512];
            int piDestLen = 0;
            int iRet = FPDll.FPIBase64FingerDataToHex(psSrcBuf, 512, cpMBBuf, ref piDestLen);
            if (iRet == 0)
            {
                byte[] psDesBuf = new byte[piDestLen];
                Array.ConstrainedCopy(cpMBBuf, 0, psDesBuf, 0, piDestLen);

                return psDesBuf;
            }
            return null;
        }

        /// <summary>
        /// 指纹比对
        /// </summary>
        /// <param name="psMBB"></param>
        /// <param name="pzTZ"></param>
        /// <returns></returns>
        public Response VerifyFinger(byte[] psMB,byte[] pzTZ)
        {
            Response resp = new Response();
            if (ghDevicePtr == IntPtr.Zero)
            {
                resp.Code = 0;
                resp.Message = "请先打开设备";
                return resp;
            }
            int nback = FPDll.FPIVerify(ghDevicePtr, psMB, pzTZ,iLevel);
            if (nback == 0)
            {
                resp.Code = 1;
                resp.Message = "指纹比对成功";
            }
            else
            {
                resp.Message = "指纹比对失败";
            }
            return resp;
        }
       
        /// <summary>
        /// 采集特性值
        /// 判断手指干湿偏移
        /// </summary>
        /// <param name="nRetFDryWet"></param>
        /// <param name="nRetFOffset"></param>
        /// <returns></returns>
        private string checkFingerOffsetDryWet(int nRetFDryWet, int nRetFOffset)
        {
            string temp1, temp2;
            switch (nRetFDryWet)
            {
                case FPDll.IMAGE_QUALITY_DRY:
                    temp1 = "手指偏干";
                    break;
                case FPDll.IMAGE_QUALITY_WET:
                    temp1 = "手指偏湿";
                    break;
                case FPDll.IMAGE_QUALITY_OK:
                    temp1 = "手指干湿：正常";
                    break;
                case FPDll.IMAGE_QUALITY_UNKNOWN:
                    temp1 = "手指干湿：未知";
                    break;
                case FPDll.IMAGE_QUALITY_NO_FINGER:
                    temp1 = "图像无手指";
                    break;
                default:
                    temp1 = "手指干湿：未知";
                    break;
            }
            //if (temp1 == "手指干湿：正常")
            //{
            //    return "干湿偏移正常";
            //}
            switch (nRetFOffset)
            {
                case FPDll.IMAGE_QUALITY_TOP:
                    temp2 = "手指向上偏移";
                    break;
                case FPDll.IMAGE_QUALITY_BOTTOM:
                    temp2 = "手指向下偏移";
                    break;
                case FPDll.IMAGE_QUALITY_LEFT:
                    temp2 = "手指向左偏移";
                    break;
                case FPDll.IMAGE_QUALITY_RIGHT:
                    temp2 = "手指向右偏移";
                    break;
                case FPDll.IMAGE_QUALITY_SMALL:
                    temp2 = "指纹图像偏小";
                    break;
                case FPDll.IMAGE_QUALITY_TOP | FPDll.IMAGE_QUALITY_LEFT:
                    temp2 = "指纹图像偏左上方";
                    break;
                case FPDll.IMAGE_QUALITY_TOP | FPDll.IMAGE_QUALITY_RIGHT:
                    temp2 = "指纹图像偏右上方";
                    break;
                case FPDll.IMAGE_QUALITY_BOTTOM | FPDll.IMAGE_QUALITY_LEFT:
                    temp2 = "指纹图像偏左下方";
                    break;
                case FPDll.IMAGE_QUALITY_BOTTOM | FPDll.IMAGE_QUALITY_RIGHT:
                    temp2 = "指纹图像偏右下方";
                    break;
                case FPDll.IMAGE_QUALITY_OK:
                    temp2 = "手指偏移：正常";
                    break;
                case FPDll.IMAGE_QUALITY_UNKNOWN:
                    temp2 = "手指偏移：未知";
                    break;
                case FPDll.IMAGE_QUALITY_NO_FINGER:
                    temp2 = "图像无手指";
                    break;
                default:
                    temp2 = "手指偏移：未知";
                    break;
            }
            return (temp1 + "；" + temp2);
        }

    }
}
