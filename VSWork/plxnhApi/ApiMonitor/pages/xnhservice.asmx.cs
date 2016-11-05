using ApiMonitor.log;
using Service.Util;
using Service.WebService.ServiceImpl.login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace ApiMonitor.pages
{
    /// <summary>
    /// xnhservice 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    [System.Web.Script.Services.ScriptService]
    public class xnhservice : System.Web.Services.WebService
    {
        /// <summary>
        /// 登录服务
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [WebMethod]
        public string login(string name,string pwd)
        {
            string retVal = "";
            try
            {
                //调用接口认证
                Service.WebService.IService service = new LoginAuth();

                //直接塞参数进入Dictionary，由框架自动组装顺序
                Dictionary<string, string> requestParam = new Dictionary<string, string>();
                requestParam.Add("USER_CODE", name);
                requestParam.Add("USER_PASS", pwd);

                //使用executeSql重载的Dictionary参数方法
                string response = service.executeSql("", requestParam, "&");
                Dictionary<string, string> responseDict = service.getResponseResultWrapperMap();
                //登录失败
                if (service.getExecuteStatus() == false)
                {
                    retVal = "";
                    //记日志
                    XnhLogger.log(service.getExecuteResultPlainString());
                }
                else
                {
                    retVal = MsgConvert.Dict2Json(responseDict);
                }
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.ToString());
                retVal = ex.ToString();
            }

            return retVal;
        }

        [WebMethod]
        public string aaa()
        {
            string retVal = "";

            return retVal;
        }
    }
}
