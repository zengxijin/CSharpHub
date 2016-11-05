using ApiMonitor.log;
using Service.Util;
using Service.WebService.ServiceImpl.login;
using Service.WebService.ServiceImpl.MZBC;
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

        /// <summary>
        /// (2).读卡调用交易：
        ///1)	调用验证此卡号是否存在交易。
        ///2)	根据上一返回的医疗证号调用获取家庭成员交易，如果返回多个人员根据序号进行选择。
        ///3)	根据医疗证号和序号调用查询基础人员信息交易，将信息显示到用户画面。
        ///4)	并验证本人是否已经住院（注：应该是住院状态不允许门诊报销）。
        ///5)	调用取出补偿类别交易并在用户画面显示补偿类别，供门诊补偿时选择。
        /// </summary>
        /// <param name="AREA_NO">地区编码</param>
        /// <param name="M_MM"></param>
        /// <returns></returns>
        [WebMethod]
        public string readCard(string AREA_NO, string M_MM)
        {
            string retVal = "";

            MZBC_Check_Ylzh_Bulsh check = new MZBC_Check_Ylzh_Bulsh();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("AREA_NO", AREA_NO);
            param.Add("M_MM", M_MM);

            string retStr = check.executeSql("", param, "&");
            if (check.getExecuteStatus() == true)
            {
                Dictionary<string, string> retDic = check.getResponseResultWrapperMap();
            }

            return retVal;
        }
    }
}
