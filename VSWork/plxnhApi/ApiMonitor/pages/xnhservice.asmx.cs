using ApiMonitor.Buffer;
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
                string response = service.executeSql(requestParam);
                Dictionary<string, string> responseDict = service.getResponseResultWrapperMap();
                //登录失败
                if (service.getExecuteStatus() == false)
                { 
                    retVal = "";
                    //记日志
                    XnhLogger.log(this.GetType().ToString() + service.getExecuteResultPlainString());
                }
                else
                {
                    //登录成功，缓存用户信息
                    BufferUtil.setBuffer(responseDict["USER_ID"], "DEP_ID", responseDict["DEP_ID"]);

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
        public string readCard(string USER_ID,string AREA_NO, string M_MM)
        {
            string retVal = "";

            //todo:M_MM 读卡的加密串 此值的获取需要接口
            try
            {
                //(1)调用验证此卡号是否存在交易
                MZBC_Check_Ylzh_Bulsh check = new MZBC_Check_Ylzh_Bulsh();
                string retStr = check.executeSql(
                    new Dictionary<string, string>() { { "AREA_NO", AREA_NO }, { "M_MM", M_MM } }
                    );

                //卡号存在交易
                if (check.getExecuteStatus() == true)
                {
                    Dictionary<string, string> retDic = check.getResponseResultWrapperMap();
                    //返回的医疗证号
                    string D401_10 = retDic["D401_10"];
                    //缓存医疗号
                    BufferUtil.setBuffer(USER_ID, "D401_10", D401_10);

                    //(2)调用获取家庭成员交易
                    MZBC_Get_Member getMember = new MZBC_Get_Member();
                    string getMemberRetStr = getMember.executeSql(
                        new Dictionary<string, string>() { { "AREA_NO", AREA_NO }, { "D401_10", D401_10 } }
                        );
                    if (getMember.getExecuteStatus() == true)
                    {
                        //返回的家庭成员信息D401_21/D401_02;D401_21/D401_02
                        //成员序号：D401_21  CHAR(2)
                        //成员姓名：D401_02  VARCHAR2(24)
                        string retMember = (string)getMember.getResponseResultOtherWrapper();
                        //todo:家庭成员信息存储

                        //(3)根据医疗证号和序号调用查询基础人员信息交易，将信息显示到用户画面。
                        string[] memberArray = retMember.Split(new string[]{";"},StringSplitOptions.None);
                        foreach (string one in memberArray)
                        {
                            string D401_21 = one.Split(new string[] { "/" }, StringSplitOptions.None)[0]; //成员序号
                            //查询成员基本信息
                            MZBC_Get_Member_Information getMemberInfo = new MZBC_Get_Member_Information();
                            getMemberInfo.executeSql(
                                new Dictionary<string, string>() { { "AREA_NO", AREA_NO }, { "D401_10", D401_10 }, { "D401_21", D401_21 } }
                                );
                            Dictionary<string,string> memberBaseInfo = getMemberInfo.getResponseResultWrapperMap(); //成员基本信息
                            //(4)成员基本信息显示在用户界面 todo

                            //(5)验证本人是否已经住院（注：应该是住院状态不允许门诊报销）
                            //AREA_NO&D401_10&D401_21&DEP_ID
                            MZBC_PROC_ZYBZ_NOTICE_CHECK zy = new MZBC_PROC_ZYBZ_NOTICE_CHECK();
                             //从缓存获取信息
                            string DEP_ID = BufferUtil.getBufferByKey(USER_ID, "DEP_ID");
                            zy.executeSql(
                                new Dictionary<string, string>() { { "AREA_NO", AREA_NO }, { "D401_10", D401_10 }, { "D401_21", D401_21 }, { "DEP_ID", DEP_ID } }
                                );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.ToString());
            }

            return retVal;
        }
    
        /// <summary>
        /// 获取某个地区的补偿类别
        /// </summary>
        /// <param name="AREA_NO"></param>
        /// <returns></returns>
        [WebMethod]
        public string getBCLB(string AREA_NO)
        {
            try
            {
                MZBC_Get_S301_06 s301 = new MZBC_Get_S301_06();
                //补偿类别返回结果 0;ITEM_CODE/ITEM_NAME;ITEM_CODE/ITEM_NAME
                string retStr = s301.executeSql(new Dictionary<string, string>() { { "AREA_NO", AREA_NO } });
                //返回格式ITEM_CODE/ITEM_NAME;ITEM_CODE/ITEM_NAME
                return (string)s301.getResponseResultOtherWrapper();

            }catch(Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " getBCLB " + ex.ToString());
            }

            return "";
        }

        [WebMethod]
        public string tryCalculate(string user_id,string key)
        {
            try 
            {
                string val = BufferUtil.getBufferByKey(user_id, key);
                return val + " " + key; //从缓存获取信息
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " tryCalculate " + ex.ToString());
            }
            return "fail";
        }
    }
}
