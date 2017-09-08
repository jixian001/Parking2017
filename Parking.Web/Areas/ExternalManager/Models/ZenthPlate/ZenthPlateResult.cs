using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parking.Web.Areas.ExternalManager.Models
{
    /// <summary>
    /// 车牌识别POST信息格式
    /// 内容详细等级: 较详细
    /// 发送大图片
    /// </summary>
    public class ZenthPlateResult
    {
        public ZAlarmInfoPlate AlarmInfoPlate { get; set; }
    }

    public class ZAlarmInfoPlate
    {
        public int channel { get; set; }
        public string deviceName { get; set; }
        public string ipaddr { get; set; }
        public ZResult result { get; set; }
        public string serialno { get; set; }
    }

    public class ZResult
    {
        public ZPlateResult PlateResult { get; set; }
    }

    public class ZPlateResult
    {
        public int beforeBase64imageFileLen { get; set; }
        public int bright { get; set; }
        public int carBright { get; set; }
        public int carColor { get; set; }
        public int colorType { get; set; }
        public int colorValue { get; set; }
        public int confidence { get; set; }
        public int direction { get; set; }
        public string imageFile { get; set; }
        public int imageFileLen { get; set; }
        public string license { get; set; }
        public ZTimeStamp timeStamp { get; set; }
        public int triggerType { get; set; }
        public int type { get; set; }
    }

    public class ZTimeStamp
    {
        public ZTimeval Timeval { get; set; }
    }

    public class ZTimeval
    {
        public int decday { get; set; }
        public int dechour { get; set; }
        public int decmin { get; set; }
        public int decmon { get; set; }
        public int decsec { get; set; }
        public int decyear { get; set; }
        public Int64 sec { get; set; }
        public Int64 usec { get; set; }

    }
}