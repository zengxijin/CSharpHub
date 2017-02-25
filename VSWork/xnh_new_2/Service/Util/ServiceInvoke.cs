
using Service.ServiceReference1;
using Service.XHE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Util
{
    public class ServiceInvoke
    {
        private static Data_Update_IPortTypeClient clientPrd = null;
        private static Data_Update_MSoapClient clientTest = null;
        private static object lockObj = new object();
        public static string Update_Data_String(string procName,string parames,string split) 
        {
            if (ConfigUtil.isEnvPrd() == true) 
            {
                //生产环境接口
                return getXnhClient().Update_Data_String(procName, parames, split);
            }
            else 
            {
                //模拟环境接口
                return getWebSoapClient().Update_Data_String(procName, parames, split);
            }
        }

        public static string Execute_Sql(string sqlStr,string parames,string split)
        {
            if (ConfigUtil.isEnvPrd() == true)
            {
                //生产环境接口
                return getXnhClient().Execute_Sql(sqlStr, parames, split);
            }
            else
            {
                //模拟环境接口
                return getWebSoapClient().Execute_Sql(sqlStr, parames, split);
            }
        }

        private static Data_Update_MSoapClient getWebSoapClient()
        {
            if (clientTest == null)
            {
                lock (lockObj)
                {
                    if (clientTest == null)
                    {
                        clientTest = new Data_Update_MSoapClient();
                    }
                }
            }
            return clientTest;
        }

        private static Data_Update_IPortTypeClient getXnhClient()
        {
            if (clientPrd == null)
            {
                lock (lockObj)
                {
                    if (clientPrd == null)
                    {
                        clientPrd = new Data_Update_IPortTypeClient();
                    }
                }
            }
            return clientPrd;
        }
    }
}
