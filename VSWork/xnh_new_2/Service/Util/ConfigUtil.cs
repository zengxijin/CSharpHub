using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Util
{
    public class ConfigUtil
    {
        private static JObject json   = null;
        private static object lockObj = new object();
        private static string configPath = ConfigurationManager.AppSettings["configPath"];
        private static string envCfg = ConfigurationManager.AppSettings["env"];

        /// <summary>
        /// 是否接口配置的是生产环境
        /// 由于农合接口没有测试环境，默认测试使用测试模拟环境接口
        /// </summary>
        /// <returns></returns>
        public static bool isEnvPrd()
        {
            if (envCfg.Equals("prd"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 初始化配置文件，加锁
        /// </summary>
        private static void loadConfig()
        {
            if (json == null)
            {
                lock (lockObj)
                {
                    StringBuilder sb = new StringBuilder();
                    using (StreamReader sr = new StreamReader(configPath, Encoding.Default))
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine().Trim();
                            sb.Append(line);
                        }
                    }
                    json = JObject.Parse(sb.ToString());
                }
            }
        }

        private static Dictionary<string, int> getJsonNode(string parentNode, string childNode)
        {
            loadConfig();

            Dictionary<String, int> retMap = new Dictionary<string, int>();

            JObject childNodeInfo = (JObject)json[parentNode][childNode];
            foreach (JProperty jp in childNodeInfo.Properties())
            {
                string key = jp.Name.ToString();
                int    val = int.Parse(jp.Value.ToString());
                //防重复key报错，先删后加
                retMap.Remove(key);
                retMap.Add(key, val);
            }

            return retMap;
        }

        public static Dictionary<string,int> getRequestParamConfig(string serviceName)
        {
            return getJsonNode(serviceName, "request");
        }

        public static Dictionary<string, int> getResponseParamConfig(string serviceName)
        {
            return getJsonNode(serviceName, "response");
        }

        public static string getConfigService(string serviceName)
        {
            loadConfig();

            return json[serviceName]["callService"]["service"].ToString();
        }
    }
}
