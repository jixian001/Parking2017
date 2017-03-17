using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace Parking.Auxiliary
{
    public class CRecord
    {       
        /// <summary>
        /// 异常记录
        /// </summary>
        /// <param name="message"></param>
        public static void RecordError(string message)
        {
            #region
            string basePath = AppDomain.CurrentDomain.BaseDirectory + Configs.GetValue("Error");
            try
            {               
                string logPath = basePath + @"\" + @"\" + DateTime.Now.Year.ToString() + "-" +
                    DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + ".txt";
                StreamWriter sw = null;
                if (File.Exists(logPath) == true)
                {
                    FileInfo fi = new FileInfo(logPath);
                    long len = fi.Length;

                    if (len < 4 * Math.Pow(1024, 2))
                    {
                        sw = File.AppendText(logPath);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    sw = File.CreateText(logPath);
                }

                if (sw != null)
                {
                    sw.WriteLine(DateTime.Now + "     " + message);
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (IOException)
            {
                try
                {
                    if (Directory.Exists(basePath) == false)//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(basePath);
                    }
                }
                catch
                {
                }
            }
            catch
            { }
            #endregion
        }

        /// <summary>
        /// 日常日志记录,0-作业记录
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void RecordLog(string message,int type)
        {
            string basepath= AppDomain.CurrentDomain.BaseDirectory + Configs.GetValue("Log");
            #region
            try
            {
                string logPath = basepath + @"\" + DateTime.Now.Year.ToString() + "-" +
                    DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + ".txt";
                StreamWriter sw = null;
                if (File.Exists(logPath) == true)
                {
                    FileInfo fi = new FileInfo(logPath);
                    long len = fi.Length;

                    if (len < 4 * Math.Pow(1024, 2))
                    {
                        sw = File.AppendText(logPath);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    sw = File.CreateText(logPath);
                }

                if (sw != null)
                {
                    switch (type)
                    {
                        case 0:
                            sw.WriteLine(DateTime.Now + "     " + "作业更新" + Environment.NewLine + message);
                            break;
                        case 1:
                            sw.WriteLine(DateTime.Now + "     " + "数据库更新" + Environment.NewLine + message);
                            break;
                        default:
                            sw.WriteLine(DateTime.Now + "     " + "其它" + Environment.NewLine + message);
                            break;
                    }

                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (IOException)
            {
                try
                {
                    if (Directory.Exists(basepath) == false)//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(basepath);
                    }
                }
                catch
                {
                }
            }
            catch
            {
            }
            #endregion
        }
    }
}
