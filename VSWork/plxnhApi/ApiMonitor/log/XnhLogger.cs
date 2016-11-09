using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ApiMonitor.log
{
    /// <summary>
    /// 简单的日志记录器
    /// 可以重写覆写log方法，用于自有的日志框架
    /// </summary>
    public class XnhLogger
    {
        private static string logPath = ConfigurationManager.AppSettings["logPath"];
        public static void log(string msg)
        {
            using (StreamWriter sw = new StreamWriter(logPath + getLogFileName(), true, Encoding.Default))
            {
                sw.WriteLine(DateTime.Now.ToString() + " " + msg);
            }
        }


        public static string getLogFileName()
        {
            return DateTime.Now.ToString("yyyy-MM-dd") + "_log.txt";
        }
    }

}