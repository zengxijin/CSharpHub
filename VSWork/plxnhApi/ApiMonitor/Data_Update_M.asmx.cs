using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace ApiMonitor
{
    /// <summary>
    /// Data_Update_M 的摘要说明 模拟厂商接口
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class Data_Update_M : System.Web.Services.WebService
    {

        [WebMethod]
        public string Update_Data_String(string proc_name, string parames, string split)
        {
            //模拟登陆
            if (proc_name == "PROC_CHECK_USER")
            {
                if (parames == "admin&admin")
                {
                    return "0;10011;admin;admin;刘德华;8881122;18012345678;天泰医院;DEP_AREA;USER_JG;DEP_LEVEL;AREA_CODE;T_IS_FLASH_AUTHORIZED;T_YEARS;T_IS_SK;T_IS_SK_HOSP;T_IS_XJ;T_RJZ_DATE;T_CH_START_DATE;T_CH_END_DATE;T_DY_MX_IS_HZ;T_IS_BLUSH_DAY;T_BLUSH_DAY;";
                }
                return "4;登录失败;";//"0;12344;zengxijin;";
            }

            return proc_name + parames + split;
        }

        [WebMethod]
        public string Execute_Sql(string Sql_Str, string parames, string split)
        {
            return Sql_Str + parames + split;
        }
    }
}
