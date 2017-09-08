using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;

namespace Parking.Core
{
    /// <summary>
    /// 车库禁用操作
    /// </summary>
    public class ContractLimit
    {
        private static string dealine = "";
        
        /// <summary>
        /// 更新当前时间
        /// </summary>
        /// <returns></returns>
        public Response UpdateLocalTime()
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("UpdateLocalTime");
            try
            {
                string xpath = @"/root/Limit";
                string lastdtime = XMLHelper.GetXmlNodeValue(xpath, "current");
                if (string.IsNullOrEmpty(lastdtime))
                {
                    resp.Message = "获取上一次时间失败！";
                    return resp;
                }
                if (string.IsNullOrEmpty(dealine))
                {
                    dealine = XMLHelper.GetXmlNodeValue(xpath, "value");
                }
                if (string.IsNullOrEmpty(dealine))
                {
                    resp.Message = "获取使用期限失败！";
                    return resp;
                }
                string dealine_str = FromBase64Str(dealine);
                DateTime expires = DateTime.Parse(dealine_str);

                string lastStr = FromBase64Str(lastdtime);
                DateTime last = DateTime.Parse(lastStr);    
                //按天滚动            
                if (DateTime.Compare(last,DateTime.Parse(DateTime.Now.ToShortDateString())) < 0)
                {
                    // 如果记录时间小于系统时间，则表示正常，实行更新
                    string current = ToBase64String(DateTime.Now.ToShortDateString());
                    XMLHelper.SetXmlNodeValue(xpath, "current", current);
                }
                #region
                // code=1413时，表示不启用停库功能.
                // ..  =1394时，允许修改截止时间.
                // ..为其他时，表示启用停库功能.
                #endregion
                string code = XMLHelper.GetXmlNodeValue(xpath, "code");
                if (string.IsNullOrEmpty(code))
                {
                    resp.Message = "控制代码为空";
                    return resp;
                }
                bool isAble = false;
                bool isDisp = false;
                if(code != "1413" && code != "1394")
                {
                    isAble = true;
                }
                if (isAble)
                {                    
                    //如果当前时间与设定时间出现30天的误差，如果启用停库时，则主界面弹提示框
                    if (DateTime.Compare(DateTime.Now, last.AddDays(-30)) < 0)
                    {
                        resp.Code = 2;
                        resp.Message = "系统时间(" + DateTime.Now.ToShortDateString() + ")与记录时间(" + lastStr + ")不一致,请修改系统时间";
                        return resp;
                    }
                    
                    //小于截止时间10天的，不允许存车
                    if (DateTime.Compare(last, expires.AddDays(-10)) < 0)
                    {
                        CWTask.SaveLimit = true;
                        isDisp = true;
                    }
                    //停库
                    if (DateTime.Compare(last, expires) < 0)
                    {
                        CWTask.GarageLimit = true;                        
                    }
                }
                resp.Code = 1;
                resp.Message = "Code-" + code + ",Last-" + lastStr + ",Dealine-" + dealine_str;
                //提示停库倒计时
                if (isDisp)
                {
                    TimeSpan ts = expires - last;
                    int days = ts.Days;
                    resp.Code = 3;
                    resp.Message = "已停用存车功能，距停库还有 " + days + " 天";
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                resp.Message = "操作XML异常";
            }
            return resp;
        }

        /// <summary>
        /// 字符串转化为base64编码输出
        /// </summary>
        /// <param name="dtime"></param>
        /// <returns></returns>
        public string ToBase64String(string dtime)
        {
            byte[] bytes = Encoding.Default.GetBytes(dtime);
            string baseStr = Convert.ToBase64String(bytes);
            return baseStr;
        }

        /// <summary>
        /// 将base64字符串解码
        /// </summary>
        /// <param name="base64_str"></param>
        /// <returns></returns>
        public string FromBase64Str(string base64_str)
        {
            byte[] buffer = Convert.FromBase64String(base64_str);
            string orig = Encoding.Default.GetString(buffer);
            return orig;
        }

    }
}
