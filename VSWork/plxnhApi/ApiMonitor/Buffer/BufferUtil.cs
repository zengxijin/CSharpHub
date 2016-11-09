using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiMonitor.Buffer
{
    /// <summary>
    /// 缓存信息框架
    /// 可以根据需要进行修改为缓存在数据库等操作
    /// </summary>
    public class BufferUtil
    {
        private static object lockObj = new object();
        
        public static void setBuffer(string user_id,string key, string val)
        {
            lock (lockObj)
            {
                HttpCookie cookies = HttpContext.Current.Response.Cookies.Get(user_id);
                if (cookies == null) 
                {
                    cookies = new HttpCookie(user_id);
                    cookies[key] = val;
                    cookies.Expires = DateTime.Now.AddHours(24);
                    HttpContext.Current.Response.Cookies.Add(cookies);
                }
                else
                {
                    cookies[key] = val;
                }
            }
        }

        public static string getBufferByKey(string user_id,string key)
        {
            HttpCookie cookies = HttpContext.Current.Request.Cookies[user_id];
            if (cookies!=null)
            {
                return cookies[key];
            }
            return "";
        }
    }
}