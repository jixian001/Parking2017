using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Parking.Data;

namespace Parking.Web.Models
{
    public class DeviceTaskDetail
    {
        public string DevDescp { get; set; }
        /// <summary>
        /// 作业类型
        /// </summary>
        public string TaskType { get; set; }
        /// <summary>
        /// 作业凭证（卡号或指纹编号）
        /// </summary>
        public string Proof { get; set; }
        /// <summary>
        /// 作业状态
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// 供界面显示用
    /// </summary>
    public class PlusCvt
    {
        /// <summary>
        /// 将作业类型转化为中文表示方式
        /// </summary>
        /// <param name="ttype"></param>
        /// <returns></returns>
        public static string ConvertTaskType(EnmTaskType ttype)
        {
            string msg = "";
            switch (ttype)
            {
                case EnmTaskType.SaveCar:
                    msg = "存车";
                    break;
                case EnmTaskType.GetCar:
                    msg = "取车";
                    break;
                case EnmTaskType.Move:
                    msg = "移动";
                    break;
                case EnmTaskType.Avoid:
                    msg = "避让";
                    break;
                case EnmTaskType.TempGet:
                    msg = "取物";
                    break;
                case EnmTaskType.Transpose:
                    msg = "挪移";
                    break;
                default:
                    msg = ttype.ToString();
                    break;
            }
            return msg;
        }

        /// <summary>
        /// 将作业状态转化为中文表示方式
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string ConvertTaskStatus(EnmTaskStatus status, EnmTaskStatusDetail detail)
        {
            string msg = "";
            #region
            switch (status)
            {
                case EnmTaskStatus.ICarInWaitFirstSwipeCard:
                    msg = "有车入库";
                    break;
                case EnmTaskStatus.IFirstSwipedWaitforCheckSize:
                    msg = "刷第一次卡";
                    break;
                case EnmTaskStatus.ISecondSwipedWaitforCheckSize:
                    msg = "下发(1,9)";
                    break;
                case EnmTaskStatus.ISecondSwipedWaitforEVDown:
                    msg = "下发(1,1)";
                    break;
                case EnmTaskStatus.ISecondSwipedWaitforCarLeave:
                    msg = "等待车辆离开";
                    break;
                case EnmTaskStatus.IEVDownFinished:
                    msg = "确认入库";
                    break;
                case EnmTaskStatus.IEVDownFinishing:
                    msg = "下发(1,54)";
                    break;
                case EnmTaskStatus.ICheckCarFail:
                    msg = "检测失败";
                    break;
                case EnmTaskStatus.IHallFinishing:
                    msg = "下发(1,55)";
                    break;
                case EnmTaskStatus.OWaitforEVDown:
                    msg = "取车(3,1)";
                    break;
                case EnmTaskStatus.OEVDownFinishing:
                    msg = "下发(3,54)";
                    break;
                case EnmTaskStatus.OEVDownWaitforTVLoad:
                    msg = "等待TV卸载";
                    break;
                case EnmTaskStatus.OWaitforEVUp:
                    msg = "出车卸载完成";
                    break;
                case EnmTaskStatus.OCarOutWaitforDriveaway:
                    msg = "等待车辆离开";
                    break;
                case EnmTaskStatus.OHallFinishing:
                    msg = "车辆已驶出";
                    break;
                case EnmTaskStatus.TempWaitforEVDown:
                    msg = "取物(2,1)";
                    break;
                case EnmTaskStatus.TempEVDownFinishing:
                    msg = "下发(2,54)";
                    break;
                case EnmTaskStatus.TempEVDownWaitforTVLoad:
                    msg = "等待TV卸载";
                    break;
                case EnmTaskStatus.TempWaitforEVUp:
                    msg = "出车卸载完成";
                    break;
                case EnmTaskStatus.TempOCarOutWaitforDrive:
                    msg = "等待车辆离开";
                    break;
                case EnmTaskStatus.TempHallFinishing:
                    msg = "车辆已驶出";
                    break;
                case EnmTaskStatus.Finished:
                    msg = "作业完成";
                    break;
                case EnmTaskStatus.TMURO:
                    msg = "故障中";
                    break;
                case EnmTaskStatus.TWaitforLoad:
                    msg = "装载中";
                    break;
                case EnmTaskStatus.TWaitforUnload:
                    msg = "卸载中";
                    break;
                case EnmTaskStatus.TWaitforMove:
                    msg = "移动中";
                    break;
                case EnmTaskStatus.LoadFinishing:
                    msg = "装载完成";
                    break;
                case EnmTaskStatus.UnLoadFinishing:
                    msg = "卸载完成";
                    break;
                case EnmTaskStatus.MoveFinishing:
                    msg = "移动完成";
                    break;
                case EnmTaskStatus.WillWaitForUnload:
                    msg = "等待下发卸载";
                    break;
                default:
                    msg = status.ToString();
                    break;
            }
            if (detail == EnmTaskStatusDetail.SendWaitAsk)
            {
                msg += ",等待ACK";
            }
            #endregion
            return msg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public static string ConvertSendStateDetail(EnmTaskStatusDetail detail)
        {
            string msg = "";
            switch (detail)
            {
                case EnmTaskStatusDetail.NoSend:
                    msg = "等待发送";
                    break;
                case EnmTaskStatusDetail.SendWaitAsk:
                    msg = "等待ACK";
                    break;
                case EnmTaskStatusDetail.Asked:
                    msg = "交互成功";
                    break;
                default:
                    msg = detail.ToString();
                    break;
            }
            return msg;
        }
    }
}