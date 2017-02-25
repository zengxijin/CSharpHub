using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Util
{
    /// <summary>
    /// 消息转换工具类
    /// </summary>
    public class MsgConvert
    {
        /// <summary>
        /// 将Dictionary<string,string>转为json格式输出，方便前台使用
        /// </summary>
        /// <param name="srcDict">输入Dictionary<string,string></param>
        /// <returns>json字符串</returns>
        public static string Dict2Json(Dictionary<string,string> srcDict) 
        {
            if (srcDict != null && srcDict.Count > 0)
            {
                return JsonConvert.SerializeObject(srcDict);
            }

            return "";
        }
    }
}
