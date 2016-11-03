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

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string login(string name,string pwd)
        {
            string retVal = "";

            //调用接口认证
            Service.WebService.IService service = new LoginAuth();

            //直接塞参数进入Dictionary，由框架自动组装顺序
            Dictionary<string, string> requestParam = new Dictionary<string, string>();
            requestParam.Add("USER_CODE", name);
            requestParam.Add("USER_PASS", pwd);

            //使用executeSql重载的Dictionary参数方法
            string response = service.executeSql("", requestParam, "&");
            Dictionary<string, string> responseDict = service.getResponseResultWrapperMap();

            retVal = MsgConvert.Dict2Json(responseDict);

            //通过Dictionary直接获取值
            //string USER_ID   = responseDict["USER_ID"];
            //string USER_CODE = responseDict["USER_CODE"];
            //...


            return retVal;
        }
    }
}
