using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using log4net;

namespace Parking.Auxiliary
{
    public class LogFactory
    {
        //static LogFactory()
        //{
        //    FileInfo configFile = new FileInfo(HttpContext.Current.Server.MapPath("/Configs/log4net.config"));
        //    log4net.Config.XmlConfigurator.Configure(configFile);
        //}
        public static Log GetLogger(Type type)
        {
            return new Log(LogManager.GetLogger(type));
        }
        public static Log GetLogger(string str)
        {
            return new Log(LogManager.GetLogger(str));
        }

    }
}
