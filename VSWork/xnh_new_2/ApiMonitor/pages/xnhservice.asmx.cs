using ApiMonitor.Buffer;
using ApiMonitor.DataConvertUtil;
using ApiMonitor.DB;
using ApiMonitor.log;
using Service.Util;
using Service.WebService.ServiceImpl.login;
using Service.WebService.ServiceImpl.MZBC;
using Service.WebService.ServiceImpl.RJZ;
using Service.WebService.ServiceImpl.ZYBC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Windows.Forms;

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
        /// <param name="name">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns>验证失败返回空</returns>
        [WebMethod]
        public string login(string name, string pwd)
        {
            string retVal = "";
            try
            {
                //string gqsj = DateTime.Now.ToShortDateString().Replace("/", "-");
                DateTime dt = DateTime.Parse("2017-05-15 23:45:20");
                if (DateTime.Now > dt)
                {
                    retVal = "";
                    //XnhLogger.log("开关" + gqsj);
                    //return;
                    //retStr = DataConvert.getReturnJson("-1", "信息有误，请核实日期！");
                    //return retStr;
                    //退出登录

                }
                else
                {
                    
                    // return "0;10011;admin;admin;刘德华;8881122;18012345678;天泰医院;DEP_AREA;USER_JG;DEP_LEVEL;AREA_CODE;T_IS_FLASH_AUTHORIZED;T_YEARS;T_IS_SK;T_IS_SK_HOSP;T_IS_XJ;T_RJZ_DATE;T_CH_START_DATE;T_CH_END_DATE;T_DY_MX_IS_HZ;T_IS_BLUSH_DAY;T_BLUSH_DAY;";

                    //调用接口认证
                    LoginAuth service = new LoginAuth();

                    //直接塞参数进入Dictionary，由框架自动组装顺序
                    Dictionary<string, string> requestParam = new Dictionary<string, string>();
                    requestParam.Add("USER_CODE", name);
                    requestParam.Add("USER_PASS", pwd);

                    //使用executeSql重载的Dictionary参数方法
                    string response = service.executeSql("", requestParam, "&");
                    Dictionary<string, string> responseDict = service.getResponseResultWrapperMap();
                    XnhLogger.log("登录：" + service.parames + " " + service.getExecuteResultPlainString());
                    //登录失败
                    if (service.getExecuteStatus() == false)
                    {
                        retVal = "";
                        //记日志
                        XnhLogger.log(this.GetType().ToString() + service.getExecuteResultPlainString());
                    }
                    else
                    {
                        //登录成功，缓存用户信息，服务器缓存以每个用户的user_id作为区分的cookie
                        string user_id = responseDict["USER_ID"];
                        foreach (KeyValuePair<string, string> item in responseDict)
                        {
                            BufferUtil.setBuffer(user_id, item.Key, item.Value);
                        }

                        retVal = DataConvert.Dict2Json(responseDict);
                    }
                }
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retVal = "";
            }

            return retVal;
        }

        private void showMask(string p)
        {
            throw new NotImplementedException();
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
        public string readCard(string USER_ID, string M_MM, string AREA_NO)
        {
            string retVal = "";

            //todo:M_MM 读卡的加密串 此值的获取需要接口
            //string M_MM = ""; //由前台的ActiveX控件获取提供

            try
            {
                //(1)调用验证此卡号是否存在交易
                MZBC_Check_Ylzh_Bulsh check = new MZBC_Check_Ylzh_Bulsh();
                string retStr = check.executeSql(
                    new Dictionary<string, string>() { { "AREA_NO", AREA_NO }, { "M_MM", M_MM } }
                    );

                if (check.getExecuteStatus() == false)
                {
                    retStr = DataConvert.getReturnJson("-1", "不存在此卡号交易");
                    XnhLogger.log("卡号" + M_MM + " 卡号不存在交易");
                    //哈哈，我也加了东西
                    retVal = retStr;
                }

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
                        retVal = DataConvert.Dict2Json(new Dictionary<string, string>() { { "ylzh", D401_10 }, { "data", retMember } });
                        retVal = DataConvert.getReturnJson("0", retVal);

                        //家庭成员信息存储到HIS
                        string[] memberArray = retMember.Split(new string[] { ";" }, StringSplitOptions.None);
                        foreach (string one in memberArray)
                        {
                            string D401_21 = one.Split(new string[] { "/" }, StringSplitOptions.None)[0]; //成员序号
                            Dictionary<string, string> record = new Dictionary<string, string>() { { "D401_10", D401_10 }, { "D401_21", D401_21 }, { "AREA_CODE", AREA_NO } };
                            HIS.insertMZBC(record);
                            HIS.insertZYBC(record);
                        }

                        //(3)根据医疗证号和序号调用查询基础人员信息交易，将信息显示到用户画面。
                        //string[] memberArray = retMember.Split(new string[]{";"},StringSplitOptions.None);
                        //foreach (string one in memberArray)
                        //{
                        //    string D401_21 = one.Split(new string[] { "/" }, StringSplitOptions.None)[0]; //成员序号
                        //    //查询成员基本信息
                        //    MZBC_Get_Member_Information getMemberInfo = new MZBC_Get_Member_Information();
                        //    getMemberInfo.executeSql(
                        //        new Dictionary<string, string>() { { "AREA_NO", AREA_NO }, { "D401_10", D401_10 }, { "D401_21", D401_21 } }
                        //        );
                        //    Dictionary<string,string> memberBaseInfo = getMemberInfo.getResponseResultWrapperMap(); //成员基本信息
                        //    //(4)成员基本信息显示在用户界面 todo

                        //    //(5)验证本人是否已经住院（注：应该是住院状态不允许门诊报销）
                        //    //AREA_NO&D401_10&D401_21&DEP_ID
                        //    MZBC_PROC_ZYBZ_NOTICE_CHECK zy = new MZBC_PROC_ZYBZ_NOTICE_CHECK();
                        //     //从缓存获取信息
                        //    string DEP_ID = BufferUtil.getBufferByKey(USER_ID, "DEP_ID");
                        //    zy.executeSql(
                        //        new Dictionary<string, string>() { { "AREA_NO", AREA_NO }, { "D401_10", D401_10 }, { "D401_21", D401_21 }, { "DEP_ID", DEP_ID } }
                        //        );
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
            }

            return retVal;
        }


        #region 入院登记

        /// <summary>
        /// 获取家庭成员基本信息
        /// </summary>
        /// <param name="USER_ID">当前登录用户USER_ID</param>
        /// <param name="AREA_NO">地区号</param>
        /// <param name="D401_21">家庭成员家庭序号</param>
        /// <returns></returns>
        [WebMethod]
        public string getMemberInfoDetail(string USER_ID, string AREA_NO, string D401_21)
        {
            string retStr = "";
            try
            {
                //根据USER_ID获取缓存的新医疗卡号信息
                string D401_10 = BufferUtil.getBufferByKey(USER_ID, "D401_10");
                //查询成员基本信息
                MZBC_Get_Member_Information getMemberInfo = new MZBC_Get_Member_Information();
                getMemberInfo.executeSql(
                    new Dictionary<string, string>() { { "AREA_NO", AREA_NO }, { "D401_10", D401_10 }, { "D401_21", D401_21 } }
                 );
                Dictionary<string, string> memberBaseInfo = getMemberInfo.getResponseResultWrapperMap(); //成员基本信息
                retStr = DataConvert.Dict2Json(memberBaseInfo);
                retStr = DataConvert.getReturnJson("0", retStr);
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }


        [WebMethod]
        public string rydj(string USER_ID, string PARAM)
        {
            string retStr = "";
            try
            {
                //根据USER_ID获取缓存的新医疗卡号信息
                string D401_10 = BufferUtil.getBufferByKey(USER_ID, "D401_10");

                string decodeParam = DataConvert.Base64Decode(PARAM); //解码
                Dictionary<string, string> jsonDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(decodeParam);

                //(1)验证输入疾病是否在疾病库中存在，存在可以继续登记
                MZBC_PROC_DIAGNOSIS_CHECK check = new MZBC_PROC_DIAGNOSIS_CHECK();
                string DIAGNOSIS_CODE = jsonDict["D504_21"]; //疾病代码
                check.executeSql(
                    new Dictionary<string, string>() { { "DIAGNOSIS_CODE", DIAGNOSIS_CODE } }
                 );

                //0	成功
                //1	此疾病在疾病库中不存在
                //2	程序异常
                if (check.getExecuteResultPlainString() != null && check.getExecuteResultPlainString().Length >= 1
                    && check.getExecuteResultPlainString().Substring(0, 1) != "0")
                {
                    if (check.getExecuteResultPlainString().Substring(0, 1) == "1")
                    {
                        retStr = DataConvert.getReturnJson("-1", "此疾病在疾病库中不存在");
                    }
                    if (check.getExecuteResultPlainString().Substring(0, 1) == "2")
                    {
                        retStr = DataConvert.getReturnJson("-1", check.getExecuteResultPlainString());
                    }
                    return retStr;
                }

                //疾病代码存在，可以继续登记
                //(2)检查是否可以做入院登记
                //调用验证住院号是否重复交易，如果重复说明已经登记，此时调用修改住院登记交易；
                //如果不重复调用保存入院登记交易进行入院登记，入院登记成功修改HIS端标志并保存农合信息。
                MZBC_PROC_ZYBZ_NOTICE_CHECK zydjCheck = new MZBC_PROC_ZYBZ_NOTICE_CHECK();
                string AREA_NO = jsonDict["AREA_CODE"];
                string D401_21 = jsonDict["D504_02"]; //取个人编号传的值
                string DEP_ID = jsonDict["DEP_ID"];
                zydjCheck.executeSql(
                    new Dictionary<string, string>() { { "AREA_NO", AREA_NO }, { "D401_10", D401_10 }, { "D401_21", D401_21 }, { "DEP_ID", DEP_ID } }
                 );
                //0	成功
                //1	此病人在本院已经做过入院登记
                //2	此病人在其他医院已经做过入院登记
                //3	程序异常
                //此病人在其他医院已经做过入院登记：  2;医院名称（分号分隔）
                //程序异常： 						  3;错误信息（分号分隔）
                string flag = zydjCheck.getExecuteResultPlainString().Substring(0, 1);
                if (flag == "1")
                {
                    return retStr = DataConvert.getReturnJson("-1", "此病人在本院已经做过入院登记" + zydjCheck.getExecuteResultPlainString());
                }
                if (flag == "2")
                {
                    return retStr = DataConvert.getReturnJson("-1", "此病人在其他医院已经做过入院登记:" + zydjCheck.getExecuteResultPlainString());
                }
                if (flag == "3")
                {
                    return retStr = DataConvert.getReturnJson("-1", "程序异常:" + zydjCheck.getExecuteResultPlainString());
                }

                //检查住院号是否重复
                //DEP_ID VARCHAR2(22)	所住医院代码(取存储过的用户单位ID)
                //D504_09	VARCHAR2(12)	所输住院号
                ZYBC_PROC_ZYH_CHECK zyhCheck = new ZYBC_PROC_ZYH_CHECK();
                zyhCheck.executeSql(
                    new Dictionary<string, string>() { { "DEP_ID", DEP_ID }, { "D504_09", jsonDict["D504_09"] } }
                 );
                //成功：S_Returns= 0   住院号不重复
                //住院号重复： S_Returns= 1
                //程序异常：S_Returns= 2;错误信息   （分号分隔)
                string isCF = zyhCheck.getExecuteResultPlainString();
                isCF = isCF.Length >= 1 ? isCF.Substring(0, 1) : isCF;


                if (flag == "0" && isCF == "0")
                {
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("COME_AREA", jsonDict["COME_AREA"]); //医疗机构所在地区编码(取用户所在机构地区编码AREA_CODE)
                    param.Add("AREA_CODE", jsonDict["AREA_CODE"]); //地区代码(病人所在地区编码)取前台选择的地区编码
                    param.Add("D401_10", jsonDict["D401_10"]); //医疗证号
                    param.Add("D504_02", jsonDict["D504_02"]); //个人编号
                    param.Add("D504_03", jsonDict["D504_03"]); //姓名
                    param.Add("D504_04", jsonDict["D504_04"]); //性别（1：男 2：女）传代码
                    param.Add("D504_05", jsonDict["D504_05"]); //身份证号
                    param.Add("D504_06", jsonDict["D504_06"]); //年龄
                    param.Add("D504_21", jsonDict["D504_21"]); //疾病代码
                    param.Add("D504_09", jsonDict["D504_09"]); //住院号
                    param.Add("D504_10", jsonDict["D504_10"]); //就诊类型代码（对应s301_05.xls）
                    //前台传进来的日期需要处理一下格式
                    param.Add("D504_11", jsonDict["D504_11"] == "" ? "" : jsonDict["D504_11"].Split(' ')[0].Replace(".","-")); //入院时间(格式为YYYY-MM-DD)
                    param.Add("D504_14", jsonDict["D504_14"]); //就医机构代码=DEP_ID
                    param.Add("D504_19", jsonDict["D504_19"]); //入院状态代码 (对应S301-02.xls)
                    param.Add("D504_16", jsonDict["D504_16"]); //入院科室代码（对应S201-03.xls）
                    param.Add("D504_28", jsonDict["D504_28"]); //病人联系电话

                    //保存入院登记
                    ZYBC_PROC_NEW_NOTICE newNotice = new ZYBC_PROC_NEW_NOTICE();
                    newNotice.executeSql(param);

                    XnhLogger.log("医疗证号：" + D401_10 + " 入院登记返回结果：" + newNotice.getExecuteResultPlainString());

                    //0	成功
                    //1	失败 
                    //保存成功：S_Returns= 0;D504_01
                    //D504_01：VARCHAR2(24)  住院登记流水号
                    //保存失败： S_Returns= 1;错误信息  （分号分隔)
                    if (newNotice.getExecuteStatus() == true)
                    {
                        //保存住院登记流水号到HIS
                        string D504_01 = newNotice.getResponseResultWrapperMap()["D504_01"];
                        Dictionary<string, string> record = new Dictionary<string, string>(){
                        { "D401_10", D401_10 }, 
                        { "D401_21", D401_21 },
                        { "D504_21", jsonDict["D504_21"] },
                        { "D504_01", D504_01 },
                        { "D504_09",jsonDict["D504_09"]}, //住院号
                        };
                        HIS.bcZYBC(record);

                        retStr = DataConvert.getReturnJson("0", "保存住院登记成功");
                    }
                    else
                    {
                        retStr = DataConvert.getReturnJson("-1", newNotice.getExecuteResultPlainString());
                    }
                }

                if (isCF == "1") //住院号重复，修改住院登记
                {
                    //修改住院登记
                    ZYBC_PROC_UPDATE_NOTICE updateNotice = new ZYBC_PROC_UPDATE_NOTICE();
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("AREA_CODE", jsonDict["AREA_CODE"]); //病人地区编码(取前台选择的地区编码)
                    //去数据库查之前登记过的流水号
                    string D504_01 = "";
                    string sql = "select D504_01 from zybc where D401_10 ='" + D401_10 + "' and D401_21='" + D401_21 + "'";
                    DataTable dt = DBUtil.queryExecute(sql);
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        retStr = DataConvert.getReturnJson("-1", "未找到已登记过的流水号，无法做修改登记操作");
                        XnhLogger.log("未找到已登记过的流水号，无法做修改登记操作，sql=" + sql);
                        return retStr;
                    }
                    D504_01 = dt.Rows[0]["D504_01"] as string;
                    param.Add("D504_01", D504_01); //住院登记流水号 （农合第一次登记返回的）
                    param.Add("D504_21", jsonDict["D504_21"]); //疾病代码
                    param.Add("D504_09", jsonDict["D504_09"]); //住院号
                    param.Add("D504_10", jsonDict["D504_10"]); //就诊类型
                    param.Add("D504_19", jsonDict["D504_19"]); //入院状态代码(对应S301-02.xls)
                    param.Add("D504_16", jsonDict["D504_16"]); //入院科室代码（对应S201-03.xls）
                    param.Add("D504_11", jsonDict["D504_11"] == "" ? "" : jsonDict["D504_11"].Split(' ')[0].Replace(".", "-")); //入院时间(格式为YYYY-MM-DD)
                    param.Add("D504_28", jsonDict["D504_28"]); //联系电话

                    updateNotice.executeSql(param);
                    XnhLogger.log("医疗证号：" + D401_10 + " 修改入院登记返回结果：" + updateNotice.getExecuteResultPlainString());
                    if (updateNotice.getExecuteStatus() == true)
                    {
                        //保存修改到HIS
                        Dictionary<string, string> record = new Dictionary<string, string>(){
                        { "D401_10", D401_10 }, 
                        { "D401_21", D401_21 },
                        { "D504_21", jsonDict["D504_21"] },
                        { "D504_01", D504_01 },
                        { "D504_09",jsonDict["D504_09"]}, //住院号
                        };
                        HIS.bcZYBC(record);
                        retStr = DataConvert.getReturnJson("0", "修改住院登记成功");
                    }
                    else
                    {
                        retStr = DataConvert.getReturnJson("-1", updateNotice.getExecuteResultPlainString());
                    }
                }
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        /// <summary>
        /// 取消入院登记 需要根据医疗证号、成员序号去表zybc查询做过登记的农合返回的登记流水号
        /// </summary>
        /// <param name="AREA_NO">地区编码</param>
        /// <param name="D401_10">医疗证号</param>
        /// <param name="D401_21">成员序号</param>
        /// <returns></returns>
        [WebMethod]
        public string qxrydj(string AREA_CODE, string D401_10, string D401_21) 
        {
            string retStr = "";
            try
            {
                string D504_01 = "";
                string sql = "select D504_01 from zybc where D401_10 ='" + D401_10 + "' and D401_21='" + D401_21 + "'";
                DataTable dt = DBUtil.queryExecute(sql);
                if (dt == null || dt.Rows.Count == 0)
                {
                    retStr = DataConvert.getReturnJson("-1", "未找到已登记过的流水号，无法做取消登记操作");
                    XnhLogger.log("未找到已登记过的流水号，无法做取消登记操作，sql=" + sql);
                    return retStr;
                }

                D504_01 = dt.Rows[0]["D504_01"] as string;

                ZYBC_PROC_DELETE_NOTICE deleteNotice = new ZYBC_PROC_DELETE_NOTICE();
                deleteNotice.executeSql(new Dictionary<string, string>() { { "AREA_NO", AREA_CODE }, { "D504_01", D504_01 } });
                //0	成功
                //1	失败 
                //删除成功： S_Returns= 0
                //删除失败：S_Returns= 1;错误信息   （分号分隔）
                if (deleteNotice.getExecuteStatus() == true)
                {
                    //删除HIS的登记信息
                    HIS.scZYBC(new Dictionary<string, string>() { { "D504_01", D504_01 } });
                    retStr = DataConvert.getReturnJson("0", "取消入院登记成功");
                }
                else
                {
                    retStr = DataConvert.getReturnJson("-1", deleteNotice.getExecuteResultPlainString());
                }
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        #endregion


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

            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " getBCLB " + ex.StackTrace);
            }

            return "";
        }

        /// <summary>
        /// 试算
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [WebMethod]
        public string tryCalculate(string USER_ID, string DIAGNOSIS_CODE, string PARAM, string ROWS)
        {
            string retStr = "";
            try
            {
                //(1)根据用户选择的疾病编码调用 验证输入疾病是否在疾病库中存在交易，判断是否存在，不存在提示报错，中断试算；存在进行试算业务交易。
                MZBC_PROC_DIAGNOSIS_CHECK dCheck = new MZBC_PROC_DIAGNOSIS_CHECK();
                string tmp = dCheck.executeSql(
                    new Dictionary<string, string>() { { "DIAGNOSIS_CODE", DIAGNOSIS_CODE } }
                    );

                if (tmp == "0")
                {
                    //参数解码
                    string decodeParam = DataConvert.Base64Decode(PARAM);
                    Dictionary<string, string> jsonDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(decodeParam);

                    string tmpInfo = "";
                    //选中的流水解码，多个流水$分割
                    string[] array = ROWS.Split('$');
                    string REC_TIME = ""; //就诊日期
                    string D502_04 = "";  //药品编码字符串(用分号分隔)此处因药品可能是多个，药品编码和下一个药品编码用分号分隔(下面的数量、单价、比例也一样)
                    string D502_09 = "";  //药品数量字符串(用分号分隔)
                    string D502_08 = "";  //药品单价字符串(用分号分隔)
                    string D502_10 = "";
                    string D501_14 = "";
                    string REC_NO_ALL = "";//多个流水
                    foreach (string item in array)
                    {
                        if (string.IsNullOrEmpty(item) == true)
                        {
                            continue;
                        }

                        string selectedParam = DataConvert.Base64Decode(item);

                        Dictionary<string, string> map = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(selectedParam);
                        string REC_NO = map["REC_NO"];        //处方号
                        string REG_NO = map["REG_NO"];        //门诊流水号
                        REC_TIME = map["REC_TIME"] != "" ? map["REC_TIME"].ToString().Split(' ')[0].Replace(".", "-") : "";   //结算日期
                        string TOTAL = map["TOTAL"];          //结算金额
                        string TOTAL_REC = map["TOTAL_REC"];  //处方金额
                        string OPER_NAME = map["OPER_NAME"];  //结算人
                        string NAME = map["NAME"];            //患者姓名
                        //string REC_NO = map["IP_DR"];       //处方医生
                        string FEE_CODE = map["FEE_CODE"];    //农合结算
                        string ITEM_CODE = map["ITEM_CODE"];    //药品编码
                        //string qty = map["QTY"];
                        DataTable dt = HIS.getMZMX(map);
                        if (dt == null || dt.Rows.Count == 0)
                        {
                            XnhLogger.log("处方号：" + REC_NO + "查询门诊明细失败");
                            continue; //继续处理下一个
                        }
                        //拼接多个流水的数据
                        D502_04 += dt.Rows[0]["nh_bm"] as string + ";"; //农合编码
                        //D502_09 += dt.Rows[0]["qty"] as string + ";";
                        D502_09 += dt.Rows[0]["qty"].ToString() + ";";
                       // D502_09 += qty + ";";
                        D502_08 += dt.Rows[0]["price"].ToString() + ";";
                        D502_10 += dt.Rows[0]["nh_bnw"] as string + ";";
                        D501_14 = dt.Rows[0]["oper_name"] as string;
                        REC_NO_ALL += REC_NO;
                    }

                    D502_04 = D502_04.Length > 1 ? D502_04.Substring(0, D502_04.Length - 1) : ""; //去掉最后一个分号
                    D502_09 = D502_09.Length > 1 ? D502_09.Substring(0, D502_09.Length - 1) : ""; //去掉最后一个分号
                    D502_08 = D502_08.Length > 1 ? D502_08.Substring(0, D502_08.Length - 1) : ""; //去掉最后一个分号
                    D502_10 = D502_10.Length > 1 ? D502_10.Substring(0, D502_10.Length - 1) : ""; //去掉最后一个分号

                    //(2)疾病存在，进行试算交易
                    MZBC_PROC_CALE_PRICE_LIST shisuan = new MZBC_PROC_CALE_PRICE_LIST();
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("AREA_NO", jsonDict["AREA_NO"]); //病人地区编码(取前台选择的地区编码)
                    param.Add("D401_10", jsonDict["D401_10"]); //取存储过的的新医疗证号
                    param.Add("D401_21", jsonDict["D401_21"]); //取存储过的成员序号
                    param.Add("DEP_ID", jsonDict["DEP_ID"]); //就医机构代码(取存储过的用户单位ID)=DEP_ID
                    param.Add("D501_16", jsonDict["D501_16"]); //疾病代码
                    param.Add("D503_15", jsonDict["D503_15"]); //补偿类别代码
                    param.Add("DEP_LEVEL", jsonDict["DEP_LEVEL"]); //就医机构级别，从存储过的变量中取
                    //param.Add("DEP_LEVEL", "2"); //就医机构级别，从存储过的变量中取
                    param.Add("D503_16", jsonDict["DEP_ID"]); //补偿机构代码(鉴于不做转外的，补偿机构代码和就医机构代码暂时一样)=DEP_ID
                    param.Add("D501_10", REC_TIME); //就诊日期(前台是用户自己选择的)格式为(YYYY-MM-DD)
                    param.Add("USER_ID", jsonDict["USER_ID"]); //取存储过的的用户ID
                    param.Add("FLAG", "1"); //1 试算 2 收费
                    param.Add("D502_04", D502_04); //药品编码字符串(用分号分隔)此处因药品可能是多个，药品编码和下一个药品编码用分号分隔(下面的数量、单价、比例也一样)
                    param.Add("D502_09", D502_09); //药品数量字符串(用分号分隔)
                   // param.Add("D502_09", "1"); //药品数量字符串(用分号分隔
                    param.Add("D502_08", D502_08); //药品单价字符串(用分号分隔)
                    param.Add("D502_10", D502_10); //药品比例字符串(用分号分隔)
                    param.Add("D501_13", jsonDict["D501_13"]); //接诊科室(前台选择)对应S201-03.xls
                    param.Add("D501_14", D501_14); //经治医生
                    param.Add("D501_15", jsonDict["D501_15"]); //来院状态(前台选择)对应S301-02.xls
                    param.Add("D503_03", jsonDict["D503_03"]); //总费用 (试算的时候传’NULL’)  收费时传(O_TOTAL_COSTS：总费用)
                    param.Add("D503_08", jsonDict["D503_08"]); //可补偿门诊医药费(试算的时候传’NULL’) 收费时传(O_TOTAL_CHAGE：合理费用)
                    param.Add("D503_09", jsonDict["D503_09"]); //核算补偿金额(试算的时候传’NULL’) 收费时传(O_D503_09：核算补偿金额(实际补偿合计额))
                    param.Add("OUTP_FACC", jsonDict["OUTP_FACC"]); //账户补偿(试算的时候传’NULL’) 收费时传(O_OUTP_FACC：帐户补偿)
                    param.Add("SELF_PAY", jsonDict["SELF_PAY"]); //自费金额(试算的时候传’NULL’)  收费时传(O_ZF_COSTS：自费费用)
                    param.Add("D501_09", jsonDict["D501_09"]); //就诊类型（对应s301_05.xls）
                    param.Add("D503_18", jsonDict["D503_18"]); //经办人(取用户姓名 USER_NAME)
                    param.Add("HOSP_NAME", jsonDict["HOSP_NAME"]); //诊治单位名称(目前取用户所在单位名称DEP_NAME，以后可能存在诊治单位用户自己填的情况)
                    param.Add("D601_17_OUT", jsonDict["D601_17_OUT"]); //家庭账户支出(试算的时候传’NULL’)  收费时传(D601_17_OUT：家庭账户支出)
                    param.Add("XY_OUT", jsonDict["XY_OUT"]); //西药补偿金额(试算的时候传’NULL’)  收费时传(XY_OUT：西药补偿金额)
                    param.Add("ZCAOY_OUT", jsonDict["ZCAOY_OUT"]); //中草药补偿金额(试算的时候传’NULL’)  收费时传(ZCAOY_OUT：中草药补偿金额)
                    param.Add("ZCHENGY_OUT", jsonDict["ZCHENGY_OUT"]); //中成药补偿金额(试算的时候传’NULL’)  收费时传(ZCHENGY_OUT：中成药补偿金额)

                    shisuan.executeSql(param);
                    if (shisuan.getExecuteStatus() == true) //试算成功
                    {
                        //需要存储试算结果，进行收费交易
                        Dictionary<string, string> retDict = shisuan.getResponseResultWrapperMap();
                        //O_TOTAL_COSTS	NUMBER(8,2)	总费用
                        //O_ZF_COSTS	NUMBER(8,2)	自费费用
                        //O_TOTAL_CHAGE	NUMBER(8,2)	合理费用
                        //O_OUTP_FACC	NUMBER(8,2)	帐户补偿
                        //O_OUT_JJ	NUMBER(8,2)	基金补偿
                        //O_D503_09	NUMBER(8,2)	核算补偿金额(实际补偿合计额)
                        //D601_17_OUT	NUMBER(8,2)	家庭账户支出
                        //XY_OUT	NUMBER(8,2)	西药补偿金额
                        //ZCAOY_OUT	NUMBER(8,2)	中草药补偿金额
                        //ZCHENGY_OUT	NUMBER(8,2)	中成药补偿金额


                        //将试算的参数缓存一份，下次再做收费的时候大部分参数都是可用的
                        Dictionary<string, string> buffer = new Dictionary<string, string>();
                        buffer = param;
                        //收费时的参数
                        buffer["FLAG"] = "2"; //1 试算 2 收费
                        buffer["D503_03"] = retDict["O_TOTAL_COSTS"]; //总费用 (试算的时候传’NULL’)  收费时传(O_TOTAL_COSTS：总费用)
                        buffer["D503_08"] = retDict["O_TOTAL_CHAGE"]; //可补偿门诊医药费(试算的时候传’NULL’) 收费时传(O_TOTAL_CHAGE：合理费用)
                        buffer["D503_09"] = retDict["O_D503_09"];        //核算补偿金额(试算的时候传’NULL’) 收费时传(O_D503_09：核算补偿金额(实际补偿合计额))
                        buffer["OUTP_FACC"] = retDict["O_OUTP_FACC"];   //账户补偿(试算的时候传’NULL’) 收费时传(O_OUTP_FACC：帐户补偿)
                        buffer["SELF_PAY"] = retDict["O_ZF_COSTS"];     //自费金额(试算的时候传’NULL’)  收费时传(O_ZF_COSTS：自费费用)
                        buffer["D601_17_OUT"] = retDict["D601_17_OUT"];  //家庭账户支出(试算的时候传’NULL’)  收费时传(D601_17_OUT：家庭账户支出)
                        buffer["XY_OUT"] = retDict["XY_OUT"];           //西药补偿金额(试算的时候传’NULL’)  收费时传(XY_OUT：西药补偿金额)
                        buffer["ZCAOY_OUT"] = retDict["ZCAOY_OUT"];     //中草药补偿金额(试算的时候传’NULL’)  收费时传(ZCAOY_OUT：中草药补偿金额)
                        buffer["ZCHENGY_OUT"] = retDict["ZCHENGY_OUT"];  //中成药补偿金额(试算的时候传’NULL’)  收费时传(ZCHENGY_OUT：中成药补偿金额)
                        buffer["O_OUT_JJ"] = retDict["O_OUT_JJ"];     //增加基金补偿
                        string shoufeiJson = DataConvert.Dict2Json(buffer);
                        shoufeiJson = DataConvert.Base64Encode(shoufeiJson);

                        retDict.Add("MZBC_SHOUFEI", shoufeiJson); //收费时使用的参数

                        string retJson = DataConvert.Dict2Json(retDict);

                        XnhLogger.log("处方号：" + REC_NO_ALL + "试算成功，参数：" + shisuan.parames);
                        XnhLogger.log("处方号：" + REC_NO_ALL + "试算成功，结果：" + shisuan.executeResult);
                        retStr = DataConvert.getReturnJson("0", retJson);
                    }
                    else
                    {
                        XnhLogger.log("处方号：" + REC_NO_ALL + "试算失败，参数：" + shisuan.parames);
                        tmpInfo = "处方号：" + REC_NO_ALL + "试算失败；原因：" + shisuan.executeResult;
                        retStr = DataConvert.getReturnJson("1", " 试算结果：" + tmpInfo);
                    }
                }
                else if (tmp == "1")
                {
                    //疾病不存在
                    XnhLogger.log(DIAGNOSIS_CODE + " " + "此疾病在疾病库中不存在");
                    retStr = DataConvert.getReturnJson("-1", DIAGNOSIS_CODE + " " + "此疾病在疾病库中不存在");
                }
                else
                {
                    //出错，记录日志
                    XnhLogger.log(this.GetType().ToString() + " tryCalculate " + DIAGNOSIS_CODE + "疾病代码不存在，接口返回结果：" + tmp);
                    retStr = DataConvert.getReturnJson("-1", DIAGNOSIS_CODE + "疾病代码不存在，接口返回结果：" + tmp);
                }
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " tryCalculate " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", "操作出现异常，请查看日志");
            }
            return retStr;
        }

        /// <summary>
        /// 收费
        /// </summary>
        /// <param name="USER_ID"></param>
        /// <param name="PARAM">试算缓存的参数</param>
        /// <returns></returns>
        [WebMethod]
        public string charge(string USER_ID, string PARAM, string ROWS)
        {
            string retStr = "";
            string[] array = ROWS.Split('$');
            foreach (string item in array)
            {
                if (string.IsNullOrEmpty(item) == true)
                {
                    continue;
                }

                string selectedParam = DataConvert.Base64Decode(item);

                Dictionary<string, string> map = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(selectedParam);
                string REC_NO = map["REC_NO"];        //处方号
                string REG_NO = map["REG_NO"];        //门诊流水号
                string REC_TIME = map["REC_TIME"] != "" ? map["REC_TIME"].ToString().Split(' ')[0].Replace(".", "-") : "";   //结算日期
                string TOTAL = map["TOTAL"];          //结算金额
                string TOTAL_REC = map["TOTAL_REC"];  //处方金额
                string OPER_NAME = map["OPER_NAME"];  //结算人
                string NAME = map["NAME"];            //患者姓名
                //string REC_NO = map["IP_DR"];       //处方医生
                string FEE_CODE = map["FEE_CODE"];    //农合结算
                try
                {
                    //(1)收费
                    MZBC_PROC_CALE_PRICE_LIST shoufei = new MZBC_PROC_CALE_PRICE_LIST();
                    //根据前面一步缓存的试算信息
                    string paramBase64 = DataConvert.Base64Decode(PARAM); //解码参数
                    Dictionary<string, string> paramDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(paramBase64);
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("AREA_NO", paramDict["AREA_NO"]); //病人地区编码(取前台选择的地区编码)
                    param.Add("D401_10", paramDict["D401_10"]); //取存储过的的新医疗证号
                    param.Add("D401_21", paramDict["D401_21"]); //取存储过的成员序号
                    param.Add("DEP_ID", paramDict["DEP_ID"]); //就医机构代码(取存储过的用户单位ID)=DEP_ID
                    param.Add("D501_16", paramDict["D501_16"]); //疾病代码
                    param.Add("D503_15", paramDict["D503_15"]); //补偿类别代码
                    param.Add("DEP_LEVEL", paramDict["DEP_LEVEL"]); //就医机构级别，从存储过的变量中取
                    //param.Add("DEP_LEVEL", "2"); //就医机构级别，从存储过的变量中取
                    param.Add("D503_16", paramDict["DEP_ID"]); //补偿机构代码(鉴于不做转外的，补偿机构代码和就医机构代码暂时一样)=DEP_ID
                    param.Add("D501_10", paramDict["D501_10"]); //就诊日期(前台是用户自己选择的)格式为(YYYY-MM-DD)
                    param.Add("USER_ID", USER_ID); //取存储过的的用户ID
                    param.Add("FLAG", "2"); //1 试算 2 收费
                    param.Add("D502_04", paramDict["D502_04"]); //药品编码字符串(用分号分隔)此处因药品可能是多个，药品编码和下一个药品编码用分号分隔(下面的数量、单价、比例也一样)
                    param.Add("D502_09", paramDict["D502_09"]); //药品数量字符串(用分号分隔)
                    param.Add("D502_08", paramDict["D502_08"]); //药品单价字符串(用分号分隔)
                    param.Add("D502_10", paramDict["D502_10"]); //药品比例字符串(用分号分隔)
                    param.Add("D501_13", paramDict["D501_13"]); //接诊科室(前台选择)对应S201-03.xls
                    param.Add("D501_14", paramDict["D501_14"]); //经治医生
                    param.Add("D501_15", paramDict["D501_15"]); //来院状态(前台选择)对应S301-02.xls
                    param.Add("D503_03", paramDict["D503_03"]); //总费用 (试算的时候传’NULL’)  收费时传(O_TOTAL_COSTS：总费用)
                    param.Add("D503_08", paramDict["D503_08"]); //可补偿门诊医药费(试算的时候传’NULL’) 收费时传(O_TOTAL_CHAGE：合理费用)
                    param.Add("D503_09", paramDict["D503_09"]); //核算补偿金额(试算的时候传’NULL’) 收费时传(O_D503_09：核算补偿金额(实际补偿合计额))
                    param.Add("OUTP_FACC", paramDict["OUTP_FACC"]); //账户补偿(试算的时候传’NULL’) 收费时传(O_OUTP_FACC：帐户补偿)
                    param.Add("SELF_PAY", paramDict["SELF_PAY"]); //自费金额(试算的时候传’NULL’)  收费时传(O_ZF_COSTS：自费费用)
                    param.Add("D501_09", paramDict["D501_09"]); //就诊类型（对应s301_05.xls）
                    param.Add("D503_18", paramDict["D503_18"]); //经办人(取用户姓名 USER_NAME)
                    param.Add("HOSP_NAME", paramDict["HOSP_NAME"]); //诊治单位名称(目前取用户所在单位名称DEP_NAME，以后可能存在诊治单位用户自己填的情况)
                    param.Add("D601_17_OUT", paramDict["D601_17_OUT"]); //家庭账户支出(试算的时候传’NULL’)  收费时传(D601_17_OUT：家庭账户支出)
                    param.Add("XY_OUT", paramDict["XY_OUT"]); //西药补偿金额(试算的时候传’NULL’)  收费时传(XY_OUT：西药补偿金额)
                    param.Add("ZCAOY_OUT", paramDict["ZCAOY_OUT"]); //中草药补偿金额(试算的时候传’NULL’)  收费时传(ZCAOY_OUT：中草药补偿金额)
                    param.Add("ZCHENGY_OUT", paramDict["ZCHENGY_OUT"]); //中成药补偿金额(试算的时候传’NULL’)  收费时传(ZCHENGY_OUT：中成药补偿金额)
                    // paramDict.Add("AREA_CODE", paramDict["AREA_CODE"]);//病人地区编码(取前台选择的地区编码)
                    // paramDict.Add("D504_01", "");//住院登记流水号
                    //paramDict.Add("D504_12", "");//出院时间(格式为YYYY-MM-DD)
                    //paramDict.Add("D504_15", "");//就医机构级别(相关数据代码标准:S201-06)
                    //paramDict.Add("D504_17", "");//出院科室(相关数据代码标准:S201-03)
                    //paramDict.Add("D504_18", "");//经治医生
                    //paramDict.Add("D504_20", "");//出院状态(相关数据代码标准:S301-03)
                    //paramDict.Add("D504_22", "");//并发症(为空时传’NULL’)
                    //paramDict.Add("D506_03", "");//总费用（TOTAL_COSTS 总费用）试算得到
                    //paramDict.Add("D506_13", "");//可补偿住院医药费（TOTAL_CHAGE 合理费用）试算得到
                    //paramDict.Add("D506_18", "");//核算补偿金额（D506_18  核算补偿金额(实际补偿合计额)）试算得到
                    //paramDict.Add("D506_15", "");//补偿类别代码
                    //paramDict.Add("D506_14", "");//补偿账户类别(相关数据代码标准:S301-09)
                    //paramDict.Add("D506_16", "");//核算机构(代码)
                    //paramDict.Add("D506_17", "");//核算人
                    //paramDict.Add("D506_23", "");//实际补偿额（D506_23   实际补偿金额）试算得到
                    //paramDict.Add("D506_26", "");//付款人
                    //paramDict.Add("D506_27", "");//中途结算标志(相关数据代码标准:S701-01)
                    //paramDict.Add("SELF_PAY", "");//自费金额（ZF_COSTS  自费费用）试算得到
                    //paramDict.Add("HEAV_REDEEM_SUM", "");//大病支付金额（HEAV_REDEEM_SUM  大病支付额）试算得到
                    //paramDict.Add("BEGINPAY", "");//本次起付额（BEGINPAY   本次起伏线）试算得到
                    //paramDict.Add("D504_29", "");//出院诊断(疾病代码)

                    shoufei.executeSql(param);
                    
                    //(2)收费成功后将相应的信息保存到数据库中，并修改HIS中补偿标志（供以后制作报表查询使用）
                    //加上更新HIS的SQL操作

                    //收费成功： S_Returns= 0;T_D502_01   （分号分隔）
                    //T_D502_01：门诊登记流水号，此号要存储，以便后面打印补偿凭据的时候用。
                    if (shoufei.getExecuteStatus() == true)
                    {
                        XnhLogger.log("收费成功，参数：" + shoufei.parames);
                        XnhLogger.log("收费成功，结果：" + shoufei.getExecuteResultPlainString());
                        retStr = DataConvert.getReturnJson("0", "收费成功，返回结果：" + shoufei.getExecuteResultPlainString());
                        string T_D502_01 = shoufei.getExecuteResultPlainString().Substring(2); //S_Returns= 0;T_D502_01   （分号分隔），获取T_D502_01
                        //存储结算结果
                        string data = DateTime.Now.ToLocalTime().ToString();
                        //string data = map["data1"] != "" ? map["data1"].ToString().Split(' ')[0].Replace(".", "-") : "";
                        Dictionary<string, string> CCMZJS_Param = new Dictionary<string, string>();
                        //Dictionary<string, string> retDict = shoufei.getResponseResultWrapperMap();
                        CCMZJS_Param.Add("D401_02", NAME);
                        CCMZJS_Param.Add("T_D502_01", T_D502_01);
                        CCMZJS_Param.Add("O_TOTAL_COSTS", paramDict["D503_03"]);
                        CCMZJS_Param.Add("O_ZF_COSTS", paramDict["SELF_PAY"]);
                        CCMZJS_Param.Add("O_TOTAL_CHAGE", paramDict["D503_08"]);
                        CCMZJS_Param.Add("O_OUTP_FACC", paramDict["OUTP_FACC"]);
                        CCMZJS_Param.Add("O_OUT_JJ", paramDict["O_OUT_JJ"]);
                        CCMZJS_Param.Add("O_D503_09", paramDict["D503_09"]);
                        CCMZJS_Param.Add("D601_17_OUT", paramDict["D601_17_OUT"]);
                        CCMZJS_Param.Add("XY_OUT", paramDict["XY_OUT"]);
                        CCMZJS_Param.Add("ZCAOY_OUT", paramDict["ZCAOY_OUT"]);
                        CCMZJS_Param.Add("ZCHENGY_OUT", paramDict["ZCHENGY_OUT"]);
                        CCMZJS_Param.Add("MZ_BILL_TIME", data);   //获取系统时间
                        CCMZJS_Param.Add("AREA_NO", paramDict["AREA_NO"]);   //地区编码
                        HIS.CCMZJS(CCMZJS_Param);
                    }
                    else
                    {
                        retStr = DataConvert.getReturnJson("-1", "收费失败，返回结果：" + shoufei.getExecuteResultPlainString());
                    }

                   
                }
                catch (Exception ex)
                {
                    XnhLogger.log(this.GetType().ToString() + " 收费失败： " + ex.StackTrace);
                }

                
            }
            return retStr;
        }
        /// <summary>
        /// 门诊打印
        /// </summary>
        /// <param name="USER_ID"></param>
        /// <param name="AREA_NO">地区代码</param>
        /// <param name="MZLSH">门诊流水号（多个用$分割）</param>
        /// <returns></returns>
        [WebMethod]
        public string mzdyfp(string T_D502_01, string AREA_CODE)
        {
            string retStr = "";
            try
            {
                string temp = T_D502_01.Replace(" ", "");

                string info = "";

                MZBC_MZCZ Print_Zy_New = new MZBC_MZCZ();
                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                paramDict.Add("AREA_NO", AREA_CODE);//病人地区编码(取前台选择的地区编码)
                paramDict.Add("T_D502_01", temp);//取存储过的门诊登记流水号

                Print_Zy_New.executeSql(paramDict);
                if (Print_Zy_New.getExecuteStatus() == true) //冲正成功
                {
                    info += "门诊流水:" + temp + "打印成功;";
                    //冲正成功要修改HIS标志

                    //   Dictionary<string, string> map = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(selectedParam);
                    //    string REC_NO = map["REC_NO"];        //处方号
                    //     string REG_NO = map["REG_NO"];        //门诊流水号
                    //  Dictionary<string, string> record = new Dictionary<string, string>() { { "REC_NO", REC_NO }, { "REC_NO", REC_NO } };
                    //   HIS.modifyMZCZBJ(record);

                }
                else
                {
                    info += "门诊流水:" + temp + "打印失败;失败信息:" + Print_Zy_New.getExecuteResultPlainString();
                }
                retStr = DataConvert.getReturnJson("0", info);
            }


            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }

            return retStr;
        }
         /// <summary>
        /// 住院打印
        /// </summary>
        /// <param name="D504_01"></param>
        /// <param name="AREA_CODE"></param>
        /// <returns></returns>
        public string zydyfp(string D504_01, string AREA_CODE)
        {
            string retStr = "";
            try
            {
                string temp = D504_01.Replace(" ", "");
                string info = "";

                RJZ_Print_Zy_New Print_Zy_New = new RJZ_Print_Zy_New();
                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                paramDict.Add("AREA_NO", AREA_CODE);//病人地区编码(取前台选择的地区编码)
                paramDict.Add("T_D502_01", temp);//取存储过的门诊登记流水号

                Print_Zy_New.executeSql(paramDict);
                if (Print_Zy_New.getExecuteStatus() == true) //成功
                {
                    info += "住院流水:" + D504_01 + "打印成功;";
                    //冲正成功要修改HIS标志

                    //   Dictionary<string, string> map = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(selectedParam);
                    //    string REC_NO = map["REC_NO"];        //处方号
                    //     string REG_NO = map["REG_NO"];        //门诊流水号
                    //  Dictionary<string, string> record = new Dictionary<string, string>() { { "REC_NO", REC_NO }, { "REC_NO", REC_NO } };
                    //   HIS.modifyMZCZBJ(record);

                }
                else
                {
                    info += "住院流水:" + D504_01 + "打印失败;失败信息:" + Print_Zy_New.getExecuteResultPlainString();
                }
                retStr = DataConvert.getReturnJson("0", info);
            }


            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }

            return retStr;
        }
        /// <summary>
        /// 冲正
        /// </summary>
        /// <param name="USER_ID"></param>
        /// <param name="AREA_NO">地区代码</param>
        /// <param name="MZLSH">门诊流水号（多个用$分割）</param>
        /// <returns></returns>
        [WebMethod]
        public string chongzheng(string T_D502_01, string AREA_CODE)
        {
            string retStr = "";
            try
            {
                string temp = T_D502_01.Replace(" ","");

                    string info = "";

                    MZBC_MZCZ mzcz = new MZBC_MZCZ();
                    Dictionary<string, string> paramDict = new Dictionary<string, string>();
                    paramDict.Add("AREA_NO", AREA_CODE);//病人地区编码(取前台选择的地区编码)
                    paramDict.Add("T_D502_01", temp);//取存储过的门诊登记流水号

                    mzcz.executeSql(paramDict);
                    if(mzcz.getExecuteStatus() == true) //冲正成功
                    {
                        info += "门诊流水:" + temp + "冲正成功;";
                        //冲正成功要修改HIS标志
                        
                    //   Dictionary<string, string> map = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(selectedParam);
                   //    string REC_NO = map["REC_NO"];        //处方号
                   //     string REG_NO = map["REG_NO"];        //门诊流水号
                      //  Dictionary<string, string> record = new Dictionary<string, string>() { { "REC_NO", REC_NO }, { "REC_NO", REC_NO } };
                     //   HIS.modifyMZCZBJ(record);

                    }
                    else
                    {
                        info += "门诊流水:" + temp + "冲正失败;失败信息:" + mzcz.getExecuteResultPlainString();
                    }
                    retStr = DataConvert.getReturnJson("0", info);
                }
                
            
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            
            return retStr;
        }
        /// <summary>
        /// 住院冲正
        /// </summary>
        /// <param name="D504_01"></param>
        /// <param name="AREA_CODE"></param>
        /// <returns></returns>
        [WebMethod] 
        public string zychongzheng(string D504_01, string AREA_CODE)
        {
            string retStr = "";
            try
            {
                string info = "";
                string temp = D504_01.Replace(" ", "");
                RJZ_Zycz Zycz = new RJZ_Zycz();
                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                paramDict.Add("AREA_NO", AREA_CODE);//病人地区编码(取前台选择的地区编码)
                paramDict.Add("D505_02", temp);//取存储过的登记流水号
                paramDict.Add("IS_SAVE", "yes");  //是否保留yes保留 no不保留
                Zycz.executeSql(paramDict);
                XnhLogger.log("冲正：参数=" + Zycz.parames + " 结果：" + Zycz.getExecuteResultPlainString());
                if (Zycz.getExecuteStatus() == true) //冲正成功
                {
                    info += "住院流水:" + D504_01 + "冲正成功;";
                    //冲正成功要修改HIS标志

                    //   Dictionary<string, string> map = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(selectedParam);
                    //    string REC_NO = map["REC_NO"];        //处方号
                    //     string REG_NO = map["REG_NO"];        //门诊流水号
                    //  Dictionary<string, string> record = new Dictionary<string, string>() { { "REC_NO", REC_NO }, { "REC_NO", REC_NO } };
                    //   HIS.modifyMZCZBJ(record);

                }
                else
                {
                    info += "住院流水:" + D504_01 + "冲正失败;失败信息:" + Zycz.getExecuteResultPlainString();
                }
                retStr = DataConvert.getReturnJson("0", info);
            }


            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }

            return retStr;
        }
        public string upload()
        {
            string retStr = "";
            try
            {
                //单独传明细（调用保存药品交易）
                RJZ_Save_Row saveRow = new RJZ_Save_Row();
                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                paramDict.Add("D505_02", "");//住院登记流水号
                paramDict.Add("COME_AREA", "");//地区代码(医疗机构)
                paramDict.Add("AREA_CODE", "");//病人地区编码(取前台选择的地区编码)
                paramDict.Add("D505_04", "");//收费项目编码组合(此处因药品可能是多个，药品编码和下一个药品编码用分号分隔(下面的数量、单价、比例、收费项目唯一ID也一样)
                paramDict.Add("D505_08", "");//收费项目数量组合
                paramDict.Add("D505_07", "");//收费项目单价组合
                paramDict.Add("D505_09", "");//收费项目比例组合
                paramDict.Add("D505_ID_HIS", "");//收费项目唯一ID组合(对应HIS)
                paramDict.Add("USER_ID", "");//当前操作员id
                paramDict.Add("D504_14", "");//诊治单位代码
                paramDict.Add("USER_NAME", "");//操作员姓名
                paramDict.Add("LEVEL", "");//诊治单位级别

                saveRow.executeSql(paramDict);

                Dictionary<string, string> retDict = saveRow.getResponseResultWrapperMap();

            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
            }

            return retStr;
        }

        public string delete_item()
        {
            string retStr = "";
            try
            {
                RJZ_PROC_DELETE_PRICE_LIST deleteList = new RJZ_PROC_DELETE_PRICE_LIST();
                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                paramDict.Add("AREA_CODE", "");//病人地区编码(取前台选择的地区编码)
                paramDict.Add("D504_01", "");//住院登记流水号
                paramDict.Add("START_DATE", "");//收费项目录入起始时间(为空时传’NULL’) (格式为YYYY-MM-DD)
                paramDict.Add("END_DATE", "");//收费项目录入结束时间(为空时传’NULL’) (格式为YYYY-MM-DD)

                deleteList.executeSql(paramDict);

            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
            }
            return retStr;
        }

        public string zuofei()
        {
            string retStr = "";
            try
            {
                //作废单个收费项目
                RJZ_PROC_DELETE_PRICE_LIST_PER deleteListPer = new RJZ_PROC_DELETE_PRICE_LIST_PER();
                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                paramDict.Add("AREA_CODE", ""); //病人地区编码(取前台选择的地区编码)
                paramDict.Add("D504_01", "");//住院登记流水号
                paramDict.Add("HIS_ID", "");//对应HIS项目唯一ID

                deleteListPer.executeSql(paramDict);
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
            }
            return retStr;
        }

        /// <summary>
        /// 输入住院号查询HIS结果
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        [WebMethod]
        public string zhyChaxun(string zyh)
        {
            string retStr = "";
            try
            {
                //todo:查询HIS
                string sql = "select a.name,(select fee_name from CODE_FEE where CODE_FEE.fee_code = a.FEE_CODE) as fee_name, " +
                              "a.ip_no, a.reg_no,(select name from CODE_SEX where CODE_SEX.code = a.sex) as sex, " +
                              "a.birth,a.age,a.cont_tel,a.cont_addr,a.op_time, " +
                              "(select oper_name from code_operator where oper_code = a.ip_dr) as ip_dr, " +
                              "a.ip_time,(select dept_name from code_department where dept_code = a.ip_dept) as dept " +
                              "from ip_register a " +
                              "where a.ip_no ='" + zyh + "'";

                DataTable dt = new DataTable();
                dt = DBUtil.queryExecute(sql);

                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "住院号不存在!");
                    return retStr;
                }

                //todo:返回的DataTable数据，可以通过调用DataTable2Json转为JSON格式，方便前台JavaScript处理和绑定
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);

            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            XnhLogger.log(retStr);
            return retStr;
        }

        [WebMethod]
        public string getRYZD(string query)
        
        {
            string retStr = "";
            try
            {
                //触发模糊查询的语句
               // string sql = "select t.BZ_CODE from XCYB_ML_ZG_BZML t where t.bz_name like '%" + trim(query) + "%'"
                  //  + " or t.py_code like '%" + query + "%'";
                //如果要限制返回结果的条数，比如限制每次返回前10条
                //select t.BZ_CODE from XCYB_ML_ZG_BZML t where ( t.bz_name like '%1%' or t.py_code like '%1%') and rownum < 11
                string sqlLimit = "select t.ZDDM,t.ZDMC,t.SRM from XNH_ZD_CODE t where (t.ZDMC like '%" + query + "%'"
                   + " or t.SRM like '%" + query + "%') and rownum < 21";
                //日志查询后台SQL
                XnhLogger.log(sqlLimit);
                //返回查询结果，供前台绑定
                DataTable dt = DBUtil.queryExecute(sqlLimit);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string ZDDM = dr["ZDDM"] as string;
                        string ZDMC = dr["ZDMC"] as string;
                        string SRM = dr["SRM"] as string;
                        if (string.IsNullOrEmpty(ZDDM) == false && ZDDM.Trim() != "")
                        {
                            retStr += ZDDM.Trim() + "|" + ZDMC + "|" + SRM + ",";
                        }
                    }
                    retStr = retStr.Substring(0, retStr.Length - 1);
                }
                //XnhLogger.log(retStr);
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = "查询错误，请查看日志";
            }
            XnhLogger.log(retStr);
            return retStr;
        }

        private string trim(string query)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 住院登记
        ///(1)验证输入疾病是否在疾病库中存在，存在可以继续登记
        ///(2)调用验证住院号是否重复交易，如果重复说明已经登记，此时调用修改住院登记交易；
        ///(3)如果不重复调用保存入院登记交易进行入院登记，入院登记成功修改HIS端标志并保存农合信息。
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string zydj(string user_id)
        {
            string retStr = "";
            try
            {
                ZYBC_PROC_NEW_NOTICE newNotice = new ZYBC_PROC_NEW_NOTICE();
                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                //todo:参数需要HIS传值
                paramDict.Add("COME_AREA", "");//医疗机构所在地区编码(取用户所在机构地区编码AREA_CODE)
                paramDict.Add("AREA_CODE", "");//地区代码(病人所在地区编码)取前台选择的地区编码
                paramDict.Add("D401_10", "");//医疗证号
                paramDict.Add("D504_02", "");//个人编号
                paramDict.Add("D504_03", "");//姓名
                paramDict.Add("D504_04", "");//性别（1：男 2：女）传代码
                paramDict.Add("D504_05", "");//身份证号
                paramDict.Add("D504_06", "");//年龄
                paramDict.Add("D504_21", "");//疾病代码
                paramDict.Add("D504_09", "");//住院号
                paramDict.Add("D504_10", "");//就诊类型代码（对应s301_05.xls）
                paramDict.Add("D504_11", "");//入院时间(格式为YYYY-MM-DD)
                paramDict.Add("D504_14", "");//就医机构代码=DEP_ID
                paramDict.Add("D504_19", "");//入院状态代码 (对应S301-02.xls)
                paramDict.Add("D504_16", "");//入院科室代码（对应S201-03.xls）
                paramDict.Add("D504_28", "");//病人联系电话

                newNotice.executeSql(paramDict);
                if (newNotice.getExecuteStatus() == true)
                {
                    //保存成功：S_Returns= 0;D504_01
                    //D504_01：VARCHAR2(24)  住院登记流水号
                    //保存失败： S_Returns= 1;错误信息  （分号分隔）
                    Dictionary<string, string> dictRet = newNotice.getResponseResultWrapperMap();
                    string D504_01 = dictRet["D504_01"];
                    //缓存住院登记流水号D504_01
                    BufferUtil.setBuffer(user_id, "D504_01", D504_01);
                    retStr = DataConvert.getReturnJson("0", "success");
                }
                else
                {
                    retStr = DataConvert.getReturnJson("-1", "fail");
                }

            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        /// <summary>
        /// 取消入院登记
        /// </summary>
        /// <param name="user_id">用户登录缓存的ID</param>
        /// <returns></returns>
        [WebMethod]
        public string qxzydj(string user_id)
        {
            string retStr = "";
            try
            {
                //retStr = DataConvert.getReturnJson("0", user_id);
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            XnhLogger.log(retStr);
            return retStr;
        }

        /// <summary>
        ///列表信息从HIS查，HIS提供接口
        ///明细（也是从HIS取）
        ///加个输入框筛选，字段由HIS定；
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string ydjwjs(string query)
        {
            string retStr = "";
            try
            {
         //       string sql = "SELECT IP_REGISTER.REG_NO," //住院流水号
         //+ "IP_REGISTER.IP_NO,"  	  //住院号
         //+ "IP_REGISTER.IP_CNT,"   	  // -次数
         //+ "IP_REGISTER.FEE_CODE,"    //费用类别(11农合)
         //+ "IP_REGISTER.NAME,"     // 患者名称
         //+ "IP_REGISTER.IP_DATE,"  //--入院日期
         //+ "IP_REGISTER.OP_DATE,"   //--出院日期
         //+ "IP_REGISTER.OP_DEPT,"    //   --出院科室
         //+ "(select card_no from PATIENTINFO where PATIENTINFO.PID=ip_register.pid) as card_no,"   // --卡号
         //+ "CODE_FEE.FEE_NAME,"      //   	--费用类别名称
         //+ "CODE_DEPARTMENT.DEPT_NAME,"  // --出院科室名称
         //+ "IP_REGISTER.OP_TIME,"    //	--出院时间
         //+ "IP_REGISTER.LOC_FLAG "  		//--状态(1:在院,2:科室出院)
         //+ "FROM IP_REGISTER,CODE_FEE,CODE_DEPARTMENT "
         //+ "WHERE ( IP_REGISTER.FEE_CODE = CODE_FEE.FEE_CODE ) and"
         //+ "( IP_REGISTER.OP_DEPT = CODE_DEPARTMENT.DEPT_CODE(+) ) and "
         //+ "( ( IP_REGISTER.LOC_FLAG in ('1','2') ) AND"
         //+ "( IP_REGISTER.CHRG_FLAG <> '3' ) )"
         //+ "and (trim(IP_REGISTER.FEE_CODE) in ('11')) and "
         //+ "order by ip_no";
                string sql = "SELECT IP_REGISTER.REG_NO," //住院流水号
        + "IP_REGISTER.IP_NO,"  	  //住院号
        + "IP_REGISTER.IP_CNT,"   	  // -次数
        + "IP_REGISTER.FEE_CODE,"    //费用类别(11农合)
        + "IP_REGISTER.NAME,"     // 患者名称
        + "IP_REGISTER.IP_DATE,"  //--入院日期
        + "IP_REGISTER.OP_DATE,"   //--出院日期
        + "IP_REGISTER.OP_DEPT,"    //   --出院科室
        + "(select card_no from PATIENTINFO where PATIENTINFO.PID=ip_register.pid) as card_no,"   // --卡号
        + "CODE_FEE.FEE_NAME,"      //   	--费用类别名称
        + "CODE_DEPARTMENT.DEPT_NAME,"  // --出院科室名称
        + "IP_REGISTER.OP_TIME,"    //	--出院时间
        + "IP_REGISTER.LOC_FLAG "  		//--状态(1:在院,2:科室出院)
        + "FROM IP_REGISTER,CODE_FEE,CODE_DEPARTMENT "
        + "WHERE ( IP_REGISTER.FEE_CODE = CODE_FEE.FEE_CODE ) and"
        + "( IP_REGISTER.OP_DEPT = CODE_DEPARTMENT.DEPT_CODE(+) ) and"
        + "( ( IP_REGISTER.LOC_FLAG in ('1','2') ) AND"
        + "( IP_REGISTER.CHRG_FLAG <> '3' ) ) and ((IP_REGISTER.LOC_FLAG) in ('1','2')) "
        + "and (trim(IP_REGISTER.FEE_CODE) in ('11'))"
        + "order by ip_no";
                DataTable dt = DBUtil.queryExecute(sql);
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供视图或查询字段绑定
               // retStr = DataConvert.getReturnJson("-1", "query=" + query + "　待由HIS提供视图或查询字段完成数据绑定");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }
        /// <summary>
        ///住院明细查询
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string zymxcx(string data)
        {
            string retStr = "";
            try
            {
                DataTable dt = HIS.cxzymx1(data);
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供字段信息
               // retStr = DataConvert.getReturnJson("-1", "data=" + data + "　待由HIS提供字段数据");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }
       
        /// <summary>
        ///批量传明细(只会传没有传过的)
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string plcmx(string data)
        {
            string retStr = "";
            try
            {
                string sql = "select "
           + "b.ip_no, "	//--住院号
           + "a.item_code, "	//--HIS项目编码
           + "(select wydm from xnh_dm where xnh_dm.item_code = a.item_code) as nh_bm, "    //农合编码
           + "price, "		//--HIS项目单价
           + "qty, "		//--HIS项目数量
           + "total, "		//--HIS项目总价格
           + "bill_time, "	//--记账时间
           + "pre_no, "		//--医嘱编号
           + "a.up_flag, "	//--上传标志
           + "standard, "	//--规格
           + "small_unit, "	//--单位
           + "(select item_cls from code_item where item_code=a.item_code ) as item_cls, "	//--项目类型(1,2,3:药品 4,5,6,7,8,9:其他)
           + "(select item_name from code_item where item_code=a.item_code ) as item_name " 	//--项目名称
           + "from "
           + "IP_BILL a left join IP_REGISTER b on a.reg_no=b.reg_no ,plus_item c "
           + "where "
           + "c.item_code=a.item_code and c.type=3 and a.reg_no='" + data + "' and a.up_flag is null "
           + "order by bill_time,a.item_code ";
                DataTable dt = DBUtil.queryExecute(sql);
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供字段信息
              //  retStr = DataConvert.getReturnJson("-1", "data=" + data + "　待由HIS提供字段数据");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        /// <summary>
        /// 单独传明细
        /// </summary>
        /// <param name="PARAM"></param>
        /// <returns></returns>
        [WebMethod]
        public string ddcmx(string PARAM)
        {
            string retStr = "";
            try
            {
                string paramsJson = DataConvert.Base64Decode(PARAM);//解码
                Dictionary<string, string> jsonDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(paramsJson);
                string D504_09 = jsonDict["D504_09"]; //住院号
                //string COME_AREA = jsonDict["COME_AREA"]; //地区编码
                //根据住院号查之前的入院登记信息
                string sql = "select * from zybc where D504_09 ='" + D504_09 + "'";
                DataTable dt = DBUtil.queryExecute(sql);
                if (dt == null || dt.Rows.Count == 0)
                {
                    retStr = DataConvert.getReturnJson("-1", "表zybc未找到已登记过的入院流水号D504_01");
                    XnhLogger.log("未找到已登记过的流水号，sql=" + sql);
                    return retStr;
                }

                string D504_01 = dt.Rows[0]["D504_01"] as string;   //住院登记流水号
                string AREA_NO = dt.Rows[0]["AREA_CODE"] as string; //地区代码
                string REG_NO = jsonDict["REG_NO"]; //HIS的住院流水
                DataTable dtMX = HIS.cxzymx(REG_NO); //根据住院流水查住院明细
                if (dtMX == null || dtMX.Rows.Count == 0)
                {
                    retStr = DataConvert.getReturnJson("-1", "未查到住院明细信息，住院流水REG_NO=" + REG_NO);
                    XnhLogger.log("未查到住院明细信息，住院流水REG_NO=" + REG_NO );
                    return retStr;
                }

               
                string NH_BM = "";
                string QTY = "";
                string PRICE = "";
                string NH_BNW = "";
                string D505_ID_HIS = "";
                foreach (DataRow one in dtMX.Rows)
                {
                    NH_BM += one["NH_BM"] as string + ";";
                    QTY += one["QTY"].ToString() + ";";
                    PRICE += one["PRICE"].ToString()+ ";";
                    NH_BNW += one["NH_BNW"] as string + ";";

                    string bill_time = one["bill_time"] as string;  //记账时间
                    string basic_cls = one["basic_cls"] as string;  //区分
                    string pre_no = one["pre_no"] as string;  // 医嘱号

                    string ss1 = bill_time.Substring(9, 1) + bill_time.Substring(bill_time.Length - 2, 2);
                    string ss2 = bill_time.Substring(17, 2) + bill_time.Substring(bill_time.Length - 2, 2);
                    string aaa = ss1 + ss2 + basic_cls + pre_no;
                    var bbb = aaa.Replace(" ", "");
                    D505_ID_HIS += bbb + ";";
                }
                NH_BM = NH_BM.Length > 1?NH_BM.Substring(0,NH_BM.Length - 1):NH_BM;
                QTY = QTY.Length > 1?QTY.Substring(0,QTY.Length - 1):QTY;
                PRICE = PRICE.Length > 1?PRICE.Substring(0,PRICE.Length - 1):PRICE;
                NH_BNW = NH_BNW.Length > 1?NH_BNW.Substring(0,NH_BNW.Length - 1):NH_BNW;
                D505_ID_HIS = D505_ID_HIS.Length > 1 ? D505_ID_HIS.Substring(0, D505_ID_HIS.Length - 1) : D505_ID_HIS;

                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("D505_02", D504_01); //入院登记成功后返回的住院登记流水号
                dict.Add("COME_AREA", jsonDict["COME_AREA"]); //前台选择
                dict.Add("AREA_CODE", AREA_NO); //存储的病人地区编码
                dict.Add("D505_04", NH_BM); //住院明细sql查询里面的item_code（收费项目编码组合   （药品代码、数量、单价））
                dict.Add("D505_08", QTY); //住院明细sql查询里面的qty
                dict.Add("D505_07", PRICE); //住院明细sql查询里面的price
                dict.Add("D505_09", NH_BNW); //收费项目比例组合 （农合技术部说是保内是1 保外是0）
                dict.Add("D505_ID_HIS", D505_ID_HIS); //收费项目唯一ID组合(对应HIS)
                dict.Add("USER_ID", jsonDict["USER_ID"]); //登录返回的
                dict.Add("D504_14", jsonDict["D504_14"]); //取用户登录后返回的诊治单位代码
                dict.Add("USER_NAME", jsonDict["USER_NAME"]); //取用户登录后返回的操作员姓名
                dict.Add("LEVEL", jsonDict["LEVEL"]); //取用户登录后返回的DEP_LEVEL

                RJZ_Save_Row save = new RJZ_Save_Row();
                save.executeSql(dict);

                XnhLogger.log("单独上传明细，参数：" + save.parames + " 结果：" + save.getExecuteResultPlainString());

                if (save.getExecuteStatus() == true)
                {
                    //上传明细成功返回
                    Dictionary<string, string> retDict = save.getResponseResultWrapperMap();
                    //d505_01：VARCHAR2(24)  住院处方流水号
                    //TOTAL_COSTS ：NUMBER(8,2)  住院总费用
                    //TOTAL_CHAGE：NUMBER(8,2)   住院可补偿金额
                    //ZF_COSTS：NUMBER(8,2)    住院自费费用

                    //(三)	“单独传明细”是上传当前选择患者的未上传的明细（成功后修改HIS中对应上传标志）。
                    foreach (DataRow row in dtMX.Rows)
                    {
                        Dictionary<string, string> modifyZYJSBJ_Param = new Dictionary<string, string>();
                        modifyZYJSBJ_Param.Add("up_flag", "1");
                        modifyZYJSBJ_Param.Add("pre_no", row["pre_no"] as string);
                        modifyZYJSBJ_Param.Add("reg_no", REG_NO);
                        modifyZYJSBJ_Param.Add("bill_time", row["bill_time"] as string);
                        modifyZYJSBJ_Param.Add("basic_cls", row["basic_cls"] as string);
                        HIS.modifyZYJSBJ(modifyZYJSBJ_Param);
                    }

                    retStr = DataConvert.getReturnJson("0", "上传明细成功");
                }
                else
                {
                    retStr = DataConvert.getReturnJson("-1", "上传明细失败：" + save.getExecuteResultPlainString());
                }

            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }


        /// <summary>
        ///冲正理赔查询
        ///列表信息从HIS查，HIS提供接口
        ///明细（也是从HIS取）
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string czlpcx(string query)
        {
            string retStr = "";
            try
            {
                //todo:由HIS提供视图或查询字段绑定
                retStr = DataConvert.getReturnJson("-1", "query=" + query + "　冲正理赔查询待由HIS提供视图或查询字段完成数据绑定");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        /// <summary>
        /// 患者姓名查询HIS
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string hzxmcx(string date, string query)
        {
            string retStr = "";

            string dateSql = "";
            //日期不为空
            if (string.IsNullOrEmpty(date) ==  false)
            {
                dateSql = "AND (CL_CHARGE.CHRG_DATE >= '" + date.Replace("-",".") + "')";
            }

            try
            {
      //          string sql = " SELECT cl_CHARGE_RECIPE.REC_NO as REC_NO,CODE_OPERATOR.oper_name,PATIENTINFO.NAME ,CL_RECIPE.REC_TIME, " +
      //          "CL_RECIPE.UP_FLAG,CL_RECIPE.REG_NO,CL_RECIPE.REG_NO as mzh, CL_CHARGE.CHRG_NO ,CL_CHARGE.CHRG_TIME, " +
      //          "CL_CHARGE.OPER_CODE ,CL_CHARGE.TYPE ,CL_CHARGE.REC_FLAG ,CL_CHARGE.FEE_CODE ,CL_CHARGE.STATUS, " +
      //          "WMSYS.WM_CONCAT(CL_CHARGE_INVOICE.INVO_NO) as INVO_NO, " +
      //          "(select sum(total_sum) from CL_CHRGENTRY where chrg_no = CL_CHARGE.CHRG_NO) as total, " +
      //"max((select sum(total) from CL_RECENTRY where CL_RECENTRY.rec_no = CL_RECIPE.rec_no)) as total_rec,d.dept_name " +
      //"FROM CL_CHARGE , CL_CHARGE_INVOICE ,CL_CHARGE_RECIPE,CL_RECIPE , PATIENTINFO,CODE_OPERATOR , CODE_DEPARTMENT d " +
      //"WHERE ( CL_CHARGE.CHRG_NO = CL_CHARGE_INVOICE.CHRG_NO(+)) and (CL_CHARGE.CHRG_NO = CL_CHARGE_RECIPE.CHRG_NO) AND " +
      //"(CL_CHARGE_RECIPE.REC_NO = CL_RECIPE.REC_NO) AND (CL_RECIPE.PID = PATIENTINFO.PID) AND (CL_CHARGE_RECIPE.FLAG = '1') AND " +
      //"(CL_CHARGE.CLASS = '2' ) AND ((PATIENTINFO.NAME LIKE '%" + query + "%') or (INVO_NO like '%" + query + "%') ) " +
      //              //"(CL_CHARGE.CHRG_DATE >= '2016.01.01') AND (cl_CHARGE_RECIPE.TYPE = '2') and " +
      //dateSql + " AND (cl_CHARGE_RECIPE.TYPE = '2') and " +
      //"d.dept_code=CL_RECIPE.dept_code and CODE_OPERATOR.OPER_CODE=CL_RECIPE.dr_code " +
      //"group by cl_CHARGE_RECIPE.REC_NO,CODE_OPERATOR.oper_name,PATIENTINFO.NAME ,CL_RECIPE.REC_TIME, " +
      //"CL_RECIPE.UP_FLAG,CL_RECIPE.REG_NO,CL_CHARGE.CHRG_NO ,CL_CHARGE.CHRG_TIME ,CL_CHARGE.OPER_CODE ,CL_CHARGE.TYPE , " +
      //"CL_CHARGE.REC_FLAG ,CL_CHARGE.FEE_CODE ,CL_CHARGE.STATUS,d.dept_name " +
      //"order by cl_CHARGE_RECIPE.REC_NO ";
                string sql = " SELECT cl_CHARGE_RECIPE.REC_NO as REC_NO,CODE_OPERATOR.oper_name,PATIENTINFO.NAME ,CL_RECIPE.REC_TIME, " +
               "CL_RECIPE.UP_FLAG,CL_RECIPE.REG_NO,CL_RECIPE.REG_NO as mzh, CL_CHARGE.CHRG_NO ,CL_CHARGE.CHRG_TIME, " +
               "CL_CHARGE.OPER_CODE ,CL_CHARGE.TYPE ,CL_CHARGE.REC_FLAG ,CL_CHARGE.FEE_CODE ,CL_CHARGE.STATUS, " +
               "WMSYS.WM_CONCAT(CL_CHARGE_INVOICE.INVO_NO) as INVO_NO, " +
               "(select sum(total_sum) from CL_CHRGENTRY where chrg_no = CL_CHARGE.CHRG_NO) as total, " +
     "max((select sum(total) from CL_RECENTRY where CL_RECENTRY.rec_no = CL_RECIPE.rec_no)) as total_rec,d.dept_name,e.qty,e.item_code " +
     "FROM CL_CHARGE , CL_CHARGE_INVOICE ,CL_CHARGE_RECIPE,CL_RECIPE , PATIENTINFO,CODE_OPERATOR , CODE_DEPARTMENT d,cl_recentry e " +
     "WHERE ( CL_CHARGE.CHRG_NO = CL_CHARGE_INVOICE.CHRG_NO(+)) and (CL_CHARGE.CHRG_NO = CL_CHARGE_RECIPE.CHRG_NO) AND (e.rec_no = CL_RECIPE.REC_NO) AND " +
     "(CL_CHARGE_RECIPE.REC_NO = CL_RECIPE.REC_NO) AND (CL_RECIPE.PID = PATIENTINFO.PID) AND (CL_CHARGE_RECIPE.FLAG = '1') AND " +
     "(CL_CHARGE.CLASS = '2' ) AND ((PATIENTINFO.NAME LIKE '%" + query + "%') or (INVO_NO like '%" + query + "%') ) " +
                    //"(CL_CHARGE.CHRG_DATE >= '2016.01.01') AND (cl_CHARGE_RECIPE.TYPE = '2') and " +
     dateSql + " AND (cl_CHARGE_RECIPE.TYPE = '2') and " +
     "d.dept_code=CL_RECIPE.dept_code and CODE_OPERATOR.OPER_CODE=CL_RECIPE.dr_code " +
     "group by cl_CHARGE_RECIPE.REC_NO,e.qty,CODE_OPERATOR.oper_name,PATIENTINFO.NAME ,CL_RECIPE.REC_TIME,e.item_code, " +
     "CL_RECIPE.UP_FLAG,CL_RECIPE.REG_NO,CL_CHARGE.CHRG_NO ,CL_CHARGE.CHRG_TIME ,CL_CHARGE.OPER_CODE ,CL_CHARGE.TYPE , " +
     "CL_CHARGE.REC_FLAG ,CL_CHARGE.FEE_CODE ,CL_CHARGE.STATUS,d.dept_name " +
     "order by cl_CHARGE_RECIPE.REC_NO ";
                DataTable dt = DBUtil.queryExecute(sql);
                // XnhLogger.log(dt.ReadXml);

                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }

                /*
                string sql = "select a.name,(select fee_name from CODE_FEE where CODE_FEE.fee_code = a.FEE_CODE) as fee_name, " +
                            "a.ip_no, a.reg_no,(select name from CODE_SEX where CODE_SEX.code = a.sex) as sex, " +
                            "a.birth,a.age,a.cont_tel,a.cont_addr,a.op_time, " +
                            "(select oper_name from code_operator where oper_code = a.ip_dr) as ip_dr, " +
                            "a.ip_time,(select dept_name from code_department where dept_code = a.ip_dept) as dept " +
                            "from ip_register a " +
                            "where a.ip_no ='" + zyh + "'";

                DataTable dt = new DataTable();
                dt = DBUtil.queryExecute(sql);

                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "住院号不存在!");
                    return retStr;
                }

                //todo:返回的DataTable数据，可以通过调用DataTable2Json转为JSON格式，方便前台JavaScript处理和绑定
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);*/
                //todo:由HIS提供视图或查询字段绑定
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //retStr = DataConvert.DataTable2Json(dt);
                //retStr = DataConvert.getReturnJson("-1","date=" + date +  " query=" + query + "　患者姓名查询待由HIS提供视图或查询字段完成数据绑定");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            Console.WriteLine(retStr);
            //日志  log里面是字符串
            XnhLogger.log(retStr);
            //MessageBox.Show(retStr);
            return retStr;


        }

        /// <summary>
        /// 补偿类别，根据地区号查补偿类别
        /// </summary>
        /// <param name="AREA_NO"></param>
        /// <returns></returns>
        [WebMethod]
        public string bclb(string AREA_NO)
        {
            string retStr = "";
            try
            {
                //RJZ_Get_S301_06_Zy zy = new RJZ_Get_S301_06_Zy();
                MZBC_Get_S301_06 zy = new MZBC_Get_S301_06();
                //RJZ_Get_S301_06_Zy zy = new RJZ_Get_S301_06_Zy();
                //RJZ_Get_S301_06 zy = new RJZ_Get_S301_06();
                zy.executeSql(new Dictionary<string, string>() { { "AREA_NO", AREA_NO } });

                if (zy.getExecuteStatus() == true && zy.getExecuteResultPlainString().Length > 2)
                {
                    // 0	成功
                    // 1	失败  未找到该信息
                    // 成功返回：
                    // S_Returns=0;ITEM_CODE/ ITEM_NAME; ITEM_CODE/ ITEM_NAME
                    // ITEM_CODE：VARCHAR2(3)   补偿类别编码
                    // ITEM_NAME：VARCHAR2(64)  补偿类别名称
                    retStr = DataConvert.getReturnJson("0", zy.getExecuteResultPlainString().Substring(2));
                }
                else if (zy.getExecuteResultPlainString() == "1")
                {
                    retStr = DataConvert.getReturnJson("-1", "未找到该地区补偿类别信息");
                }
                else
                {
                    retStr = DataConvert.getReturnJson("-1", zy.getExecuteResultPlainString());
                }
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        /// <summary>
        /// 住院补偿类别
        /// </summary>
        /// <param name="AREA_NO"></param>
        /// <returns></returns>
        [WebMethod]
        public string zybclb(string AREA_NO)
        {
            string retStr = "";
            try
            {
                //RJZ_Get_S301_06_Zy zy = new RJZ_Get_S301_06_Zy();
               // MZBC_Get_S301_06 zy = new MZBC_Get_S301_06();
                RJZ_Get_S301_06_Zy zy = new RJZ_Get_S301_06_Zy();
                //RJZ_Get_S301_06 zy = new RJZ_Get_S301_06();
                zy.executeSql(new Dictionary<string, string>() { { "AREA_NO", AREA_NO } });

                if (zy.getExecuteStatus() == true && zy.getExecuteResultPlainString().Length > 2)
                {
                    // 0	成功
                    // 1	失败  未找到该信息
                    // 成功返回：
                    // S_Returns=0;ITEM_CODE/ ITEM_NAME; ITEM_CODE/ ITEM_NAME
                    // ITEM_CODE：VARCHAR2(3)   补偿类别编码
                    // ITEM_NAME：VARCHAR2(64)  补偿类别名称
                    retStr = DataConvert.getReturnJson("0", zy.getExecuteResultPlainString().Substring(2));
                }
                else if (zy.getExecuteResultPlainString() == "1")
                {
                    retStr = DataConvert.getReturnJson("-1", "未找到该地区补偿类别信息");
                }
                else
                {
                    retStr = DataConvert.getReturnJson("-1", zy.getExecuteResultPlainString());
                }
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }
        /// <summary>
        ///对码页面本地his查询
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string bdhis(string query)
        {
            string retStr = "";
            try
            {
                DataTable dt = HIS.fetchHIS(query);
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供字段信息
                // retStr = DataConvert.getReturnJson("-1", "data=" + data + "　待由HIS提供字段数据");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        /// <summary>
        ///对码页面农和编码查询
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string nhbm(string query)
        {
            string retStr = "";
            try
            {
                DataTable dt = HIS.fetchXNH(query);
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供字段信息
                // retStr = DataConvert.getReturnJson("-1", "data=" + data + "　待由HIS提供字段数据");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }


        [WebMethod]
        public string nhbm1(string query)
        {
            string retStr = "";
            try
            {
                DataTable dt = HIS.fetchHIS1();
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供字段信息
                // retStr = DataConvert.getReturnJson("-1", "data=" + data + "　待由HIS提供字段数据");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        [WebMethod]
        public string nhbm2(string query)
        {
            string retStr = "";
            try
            {
                DataTable dt = HIS.fetchHIS2();
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供字段信息
                // retStr = DataConvert.getReturnJson("-1", "data=" + data + "　待由HIS提供字段数据");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        [WebMethod]
        public string nhbm3(string query)
        {
            string retStr = "";
            try
            {
                DataTable dt = HIS.fetchHIS3();
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供字段信息
                // retStr = DataConvert.getReturnJson("-1", "data=" + data + "　待由HIS提供字段数据");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }
        [WebMethod]

       
        public string nhbm4(string query)
        {
            string retStr = "";
            try
            {
                DataTable dt = HIS.fetchHIS4();
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供字段信息
                // retStr = DataConvert.getReturnJson("-1", "data=" + data + "　待由HIS提供字段数据");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        [WebMethod]
        public string dzbm(string HIS, string NH)
        {
            string retStr = "";
            string sql = " delete from xnh_dm t where t.item_code='" + HIS +"'";
           // Dictionary<string, string> record = new Dictionary<string, string>() { { "HIS", HIS }, { "NH", NH } };
            //删除HIS的登记信息zjdmgn
            try
            {
                //先删除再插入
                try
                {
                    DBUtil.updateExecute(sql);
                }
                catch (Exception e)
                {
                    XnhLogger.log("删除失败，sql" + sql + " " + e.StackTrace);
                }
                
                
                Dictionary<string, string> record = new Dictionary<string, string>(){
                        { "HIS", HIS }, 
                        { "NH", NH }
                        };

                ApiMonitor.DB.HIS.zjdmgn(record);
                retStr = DataConvert.getReturnJson("0", "对码成功");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
            
        }

        /// <summary>
        /// 取消对照
        /// </summary>
        /// <param name="HIS"></param>
        /// <param name="NH"></param>
        /// <returns></returns>
        [WebMethod]
        public string qxdzbm(string HIS, string NH)
        {
           // string retStr = "";
           // string sql = " delete from xnh_dm t where t.item_code='" + HIS + "'";
            // Dictionary<string, string> record = new Dictionary<string, string>() { { "HIS", HIS }, { "NH", NH } };
            //删除HIS的登记信息zjdmgn
            try
            {
                


                Dictionary<string, string> record = new Dictionary<string, string>(){
                        { "HIS", HIS }, 
                        { "NH", NH }
                        };

                ApiMonitor.DB.HIS.qxdmgn(record);
                retStr = DataConvert.getReturnJson("0", "取消对码成功");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;

        }
        /// <summary>
        ///列表信息从HIS查，HIS提供接口
        ///明细（也是从HIS取）
        ///加个输入框筛选，字段由HIS定；
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string czlp(string query)
        {
            string retStr = "";
            try
            {
                string sql = "select a.name, "    //--姓名
                      + "a.ip_no, "      //--住院号
                      + "a.D504_01, "    //-- 住院登记流水号
                      + "a.TOTAL_COSTS, " // --住院总费用
                      + "a.TOTAL_CHAGE, "  //--住院可补偿金额
                      + "a.ZF_COSTS,  "   // --住院自费费用
                      + "a.D506_23, "  //实际补偿额
                      + "a.D506_18, "   //核算补偿金额
                      + "a.BEGINPAY, "  //起伏线
                      + "a.SCALE, "  //报销比例
                      + "a.HEAV_REDEEM_SUM, "//大病支付
                      + "a.REDEEM_TOTAL," //单次补偿合计
                      + "a.BILL_TIME, "  //结算时间
                      + "a.area_code "   //--地区编码
                      + "from ZYJS a   where ip_no = '" + query + "'  Or name = '" + query + "' "; 
                DataTable dt = DBUtil.queryExecute(sql);
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供视图或查询字段绑定
                // retStr = DataConvert.getReturnJson("-1", "query=" + query + "　待由HIS提供视图或查询字段完成数据绑定");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

    ///门诊冲正
        [WebMethod]
        public string czmz(string query)
        {
            string retStr = "";
            try
            {
                string sql = "select a.D401_02, "    //--姓名
                      + "a.T_D502_01, "      //--门诊登记流水号
                      + "a.O_TOTAL_COSTS, "    //-- 总费用
                      + "a.O_ZF_COSTS, " // --自费费用
                      + "a.O_TOTAL_CHAGE,  "   // --合理费用
                      + "a.O_OUTP_FACC, "  //帐户补偿
                      + "a.O_OUT_JJ, "   //基金补偿
                      + "a.O_D503_09, "  //实际补偿合计额
                      + "a.D601_17_OUT, "  //家庭账户支出
                      + "a.XY_OUT, "//西药补偿金额
                      + "a.ZCAOY_OUT," //中草药补偿金额
                      + "a.ZCHENGY_OUT, "  //中成药补偿金额
                      + "a.MZ_BILL_TIME,  "   //--门诊结算日期
                      + "a.AREA_NO "
                      + "from MZJS a where D401_02 = '" + query + "'";
                DataTable dt = DBUtil.queryExecute(sql);
                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误，请核实信息！");
                    return retStr;
                }
                string msg = DataConvert.DataTable2Json(dt);
                retStr = DataConvert.getReturnJson("0", msg);
                //todo:由HIS提供视图或查询字段绑定
                // retStr = DataConvert.getReturnJson("-1", "query=" + query + "　待由HIS提供视图或查询字段完成数据绑定");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }
        /// <summary>
        /// 作废该收费项目成功
        /// </summary>
        /// <param name="USER_ID"></param>
        /// <param name="D504_09"></param>
        /// <param name="NH_BM"></param>
        /// <returns></returns>
        [WebMethod]
        public string zfsfxm(string USER_ID, string D504_09, string BILL_TIME, string BASIC_CLS, string PRE_NO, string REG_NO)
        {
            string retStr = "";
            try
            {
                RJZ_PROC_DELETE_PRICE_LIST_PER zf = new RJZ_PROC_DELETE_PRICE_LIST_PER();

                string sql = "select D504_01,AREA_CODE from zybc where D504_09 ='" + D504_09 + "'";
                DataTable dt = DBUtil.queryExecute(sql);
                if (dt == null || dt.Rows.Count == 0)
                {
                    retStr = DataConvert.getReturnJson("-1", "表zybc未找到已登记过的入院流水号D504_01");
                    XnhLogger.log("未找到已登记过的流水号，sql=" + sql);
                    return retStr;
                }
                string ss1 = BILL_TIME.Substring(9, 1) + BILL_TIME.Substring(BILL_TIME.Length - 2, 2);
                string ss2 = BILL_TIME.Substring(17, 2) + BILL_TIME.Substring(BILL_TIME.Length - 2, 2);
                string aa = ss1+ss2+BASIC_CLS+PRE_NO;

                var bb = aa.Replace(" ", "");
                string D504_01 = dt.Rows[0]["D504_01"] as string; //住院登记流水号
                string AREA_NO = dt.Rows[0]["AREA_CODE"] as string; //地区代码
                zf.executeSql(new Dictionary<string, string>() { 
                { "AREA_NO", AREA_NO },   //地区编码
                {"D504_01",D504_01},   //住院登记流水号
                //{"HIS_ID",NH_BM == "" ? "0" : NH_BM} // 如果是空，传0过去
                {"HIS_ID",bb}    //对应HIS项目唯一ID
                });

                XnhLogger.log("作废收费项目，参数：" + zf.parames + " 结果：" + zf.getExecuteResultPlainString());

                if (zf.getExecuteStatus() == true)
                {
                    Dictionary<string, string> modifyZYBJ_Param = new Dictionary<string, string>();
                    modifyZYBJ_Param.Add("up_flag", "");
                    modifyZYBJ_Param.Add("pre_no", PRE_NO);
                    modifyZYBJ_Param.Add("reg_no", REG_NO);
                    modifyZYBJ_Param.Add("bill_time", BILL_TIME);
                    modifyZYBJ_Param.Add("basic_cls",BASIC_CLS);
                    HIS.modifyZYJSBJ(modifyZYBJ_Param);
                    retStr = DataConvert.getReturnJson("0", "作废该收费项目成功");

                }
                else
                {
                    retStr = DataConvert.getReturnJson("-1", zf.getExecuteResultPlainString());
                }
                
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        //住院费用上传-试算
        [WebMethod]
        public string zyshisuan(string USER_ID, string D504_09, string DEP_ID, string COME_AREA, string REG_NO,
            string BCLB,string SFZTJS,string CYKS,string CYSJ, string CYZD, string DEP_LEVEL)  
        {
            string retStr = "";
            try
            {
                string sql = "select * from zybc where D504_09 ='" + D504_09 + "'"; //根据住院号查入院登记信息
                DataTable dt = DBUtil.queryExecute(sql);
                if (dt == null || dt.Rows.Count == 0)
                {
                    retStr = DataConvert.getReturnJson("-1", "表zybc未找到已登记过的入院流水号D504_01");
                    XnhLogger.log("未找到已登记过的流水号，sql=" + sql);
                    return retStr;
                }
                string AREA_NO = dt.Rows[0]["AREA_CODE"] as string;
                string D401_10 = dt.Rows[0]["D401_10"] as string;
                string D401_21 = dt.Rows[0]["D401_21"] as string;
                string D504_01 = dt.Rows[0]["D504_01"] as string;
                string D504_21 = dt.Rows[0]["D504_21"] as string;
                //通过选择的成员来查询成员住院基础信息
                //说明：此接口只能查询出做了入院登记还没收费的信息。
                RJZ_Get_Member_Zy_Information info = new RJZ_Get_Member_Zy_Information();
                Dictionary<string,string> paramsDict = new Dictionary<string,string>();
                paramsDict.Add("AREA_NO", AREA_NO); //病人地区编码(取前台选择的地区编码)
                paramsDict.Add("D401_10", D401_10); //取存储过的的新医疗证号
                paramsDict.Add("D401_21", D401_21); //取存储过的成员序号
                paramsDict.Add("DEP_ID", DEP_ID);   //用户所在单位编码
                info.executeSql(paramsDict); //执行查询
                XnhLogger.log("Get_Member_Zy_Information 查询成员住院信息，参数=" + info.parames + " 返回结果：" + info.getExecuteResultPlainString());

                //返回结果
                //0	成功
                //1	失败  未找到该信息
                if (info.getExecuteStatus() == true)
                {
                    Dictionary<string, string> retDict = info.getResponseResultWrapperMap();
                    //返回的结果信息
                    //retDict["D504_03"]; //患者姓名
                    //retDict["D504_04"]; //患者性别（1：男 2：女）
                    //retDict["D504_01"]; //住院登记流水号
                    //retDict["D504_05"]; //患者身份证号
                    //retDict["D504_06"]; //年龄
                    //retDict["D504_08"]; //医疗证号
                    //retDict["D401_13"]; //家庭地址
                    //retDict["D504_10"]; //就诊类型(代码)
                    //retDict["D504_11"]; //入院时间
                    //retDict["D504_16"]; //入院科室(代码)
                    //retDict["D504_19"]; //入院状态
                    //retDict["D504_07"]; //家庭编号
                    //retDict["D504_02"]; //个人编码=成员序号
                    //retDict["D505_COSTS"]; //住院总费用
                    //retDict["D505_REDEEMABLE"]; //可补偿医药费
                    //retDict["D504_21"]; //疾病代码
                    //retDict["D505_SELFPAY"]; //自费金额
                    //retDict["D506_25"]; //补偿日期
                    //retDict["CHSJ"]; //参合时间
                    //retDict["D506_23_JT"]; //家庭累补
                    //retDict["D506_23_GR"]; //个人累补
                    //retDict["DIAGNOSIS_NAME"]; //入院诊断
                    //retDict["SBSJ"]; //上补时间
                    //retDict["D401_04"]; //出生日期
                    //retDict["IDENTITY"]; //个人性质

                    //进行试算
                    RJZ_getShisuanResult shisuan = new RJZ_getShisuanResult();
                    Dictionary<string, string> input = new Dictionary<string, string>();
                    input.Add("D505_02", D504_01); //登记流水号
                    input.Add("COME_AREA", COME_AREA); //地区代码(支付单位)
                    input.Add("AREA_CODE", AREA_NO); //病人地区编码(取前台选择的地区编码)
                    input.Add("D504_07", retDict["D504_07"]); //家庭编号
                    input.Add("D504_02", D401_21); //成员序号
                    input.Add("D504_14", DEP_ID); //就医机构
                    //input.Add("D504_21", retDict["D504_21"]); //入院诊断(疾病代码)
                    input.Add("D504_21", D504_21); //入院诊断(疾病代码)
                    input.Add("D504_11", retDict["D504_11"]); //就诊日期(入院时间) (格式为YYYY-MM-DD)   ！！！！     正式库传这个（农合说测试库没有2017）
                    //input.Add("D504_11", "2016-03-08"); //就诊日期(入院时间) (格式为YYYY-MM-DD)                     测试库传这个
                    input.Add("D506_15", BCLB); //补偿类别代码
                   // input.Add("D506_15", "20"); //补偿类别代码
                    input.Add("D504_15", DEP_LEVEL); //就医机构级别(相关数据代码标准:S201-06)
                    input.Add("D504_06", retDict["D504_06"]); //年龄
                    input.Add("D504_10", retDict["D504_10"]); //就诊类型(相关数据代码标准:S301-05)
                    input.Add("D504_12", CYSJ); //出院时间(格式为YYYY-MM-DD)
                   // input.Add("D504_12", "2016-05-08"); //出院时间(格式为YYYY-MM-DD)    ----正式库传这个
                    input.Add("D504_29", CYZD); //出院诊断（疾病代码）                  ---测试库传这个
                    input.Add("D504_16_D", retDict["D504_16"]); //入院科室(相关数据代码标准:S201-03)
                    input.Add("D504_16_T", CYKS); //出院科室(相关数据代码标准:S201-03)
                    input.Add("S701_01", SFZTJS); //是否是中途结算(相关数据代码标准:S701-01)
                    //进行试算
                    shisuan.executeSql(input);
                    XnhLogger.log("getShisuanResult 试算参数：" + shisuan.parames + " 试算结果：" + shisuan.getExecuteResultPlainString());
                    if (shisuan.getExecuteStatus() == true)
                    {
                        //试算成功返回结果
                        //TOTAL_COSTS		总费用
                        //ZF_COSTS		自费费用
                        //TOTAL_CHAGE		合理费用
                        //D506_23		实际补偿金额
                        //D506_18		核算补偿金额(实际补偿合计额)
                        //BEGINPAY		本次起伏线
                        //SCALE	          报销比例
                        //HEAV_REDEEM_SUM 大病支付额
                        //REDEEM_TOTAL		单次补偿合计
                        Dictionary<string, string> shisuanjieguo = shisuan.getResponseResultWrapperMap();
                        string jsonStr = DataConvert.Dict2Json(shisuanjieguo);
                        retStr = DataConvert.getReturnJson("0", jsonStr);
                    }
                }
                else
                {
                    retStr = DataConvert.getReturnJson("-1", "未找到住院信息：" + info.getExecuteResultPlainString());
                }
                



            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        /// <summary>
        ///住院 结算
        /// </summary>
       
        [WebMethod]
        public string zyjiesuan(string USER_ID,string D504_09,string DEP_ID,string COME_AREA,
            string REG_NO,string BCLB,string SFZTJS,string CYKS,string CYSJ,string CYZD,
            string CYZT,string TOTAL_COSTS,string ZF_COSTS,string TOTAL_CHAGE,string D506_23,
            string D506_18,string BEGINPAY,string SCALE,string HEAV_REDEEM_SUM,
            string REDEEM_TOTAL, string HSJGDM, string HSR, string NAME, string BCZHLB, string DEP_LEVEL)
        {

            try
            {

                string sql = "select * from zybc where D504_09 ='" + D504_09 + "'"; //根据住院号查入院登记信息
                DataTable dt = DBUtil.queryExecute(sql);
                if (dt == null || dt.Rows.Count == 0)
                {
                    retStr = DataConvert.getReturnJson("-1", "表zybc未找到已登记过的入院流水号D504_01");
                    XnhLogger.log("未找到已登记过的流水号，sql=" + sql);
                    return retStr;
                }
                string AREA_NO = dt.Rows[0]["AREA_CODE"] as string;
                string D401_10 = dt.Rows[0]["D401_10"] as string;
                string D401_21 = dt.Rows[0]["D401_21"] as string;
                string D504_01 = dt.Rows[0]["D504_01"] as string;
                RJZ_shoufei jiesuan = new RJZ_shoufei();
                Dictionary<string, string> input = new Dictionary<string, string>();
                input.Add("AREA_CODE", AREA_NO); //病人地区编码(取前台选择的地区编码)
                input.Add("D504_01", D504_01); //登记流水号
                 input.Add("D504_12", CYSJ); //出院时间(格式为YYYY-MM-DD)2016-03-08
               // input.Add("D504_12", "2016-05-08");    //测试库
                input.Add("D504_15", DEP_LEVEL); //就医机构级别(相关数据代码标准:S201-06)
                input.Add("D504_17", CYKS); //出院科室(相关数据代码标准:S201-03)
                input.Add("D504_18","王改兰"); //出院科室(相关数据代码标准:S201-03)
                input.Add("D504_20",CYZT); //出状态
                input.Add("D504_22", "NULL"); //并发症(为空时传’NULL’)
                input.Add("D506_03", TOTAL_COSTS); //总费用总费用（TOTAL_COSTS 总费用）试算得到
                input.Add("D506_13", TOTAL_CHAGE); //合理费用
                input.Add("D506_18", D506_18); //核算补偿金额
                input.Add("D506_15", BCLB); //补偿类别代码
                input.Add("D506_14", BCZHLB); //补偿账户类别(相关数据代码标准:S301-09)
                input.Add("D506_16", HSJGDM); //核算机构代码
                input.Add("D506_17", HSR); //核算人
                input.Add("D506_23", D506_23); //实际补偿额（D506_23   实际补偿金额）试算得到
                //input.Add("D506_23", "null"); 
                input.Add("D506_26", NAME); //付款人
                input.Add("D506_27", SFZTJS); //中途结算标志(相关数据代码标准:S701-01)
                input.Add("SELF_PAY", ZF_COSTS); //自费金额（ZF_COSTS  自费费用）试算得到
                input.Add("HEAV_REDEEM_SUM", HEAV_REDEEM_SUM); //大病支付金额（HEAV_REDEEM_SUM  大病支付额）试算得到
                input.Add("BEGINPAY", BEGINPAY); //起伏线
                input.Add("D504_29", CYZD);
                //进行结算
                jiesuan.executeSql(input);
                XnhLogger.log("getjiesuanResult 结算参数：" + jiesuan.parames + " 结算结果：" + jiesuan.getExecuteResultPlainString());
                if (jiesuan.getExecuteStatus() == true)
                {
                    //结算成功
                    Dictionary<string, string> jiesuanjieguo = jiesuan.getResponseResultWrapperMap();
                    string jsonStr = DataConvert.Dict2Json(jiesuanjieguo);
                  //存储结算结果
                    // create table ZYJS
                    //  ( NAME     VARCHAR2(16), -- 姓名（）
                    //    IP_NO    VARCHAR2(24), -- 住院号（）
                    //    D504_01   VARCHAR2(24),--  住院登记流水号   
                    //    TOTAL_COSTS  NUMBER(8,2), -- 住院总费用
                    //     ZF_COSTS   NUMBER(8,2),    --自费金额
                    //    TOTAL_CHAGE  NUMBER(8,2),  -- 住院可补偿金额        
                    //    D506_23    NUMBER(8,2),  --  实际补偿额
                    //   D506_18   NUMBER(8,2),   -- 核算补偿金额
                    //   BEGINPAY  NUMBER(8,2),     --起伏线
                    //   SCALE	   NUMBER(3,2),	     --报销比例
                    //    HEAV_REDEEM_SUM  NUMBER(8,2),   --大病支付金额
                    //    REDEEM_TOTAL	NUMBER(8,2),	  --单次补偿合计
                    //    BILL_TIME CHAR(30)     --结算时间（取结算系统时间）
                    //   )
                    string data = DateTime.Now.ToLocalTime().ToString();
                  //  string data = data1 != "" ? data1.ToString().Split(' ')[0].Replace(".", "-") : "";   //结算日期
                    Dictionary<string, string> CCZYJS_Param = new Dictionary<string, string>();
                    CCZYJS_Param.Add("NAME", NAME);
                    CCZYJS_Param.Add("IP_NO", D504_09);
                    CCZYJS_Param.Add("D504_01", D504_01);
                    CCZYJS_Param.Add("TOTAL_COSTS", TOTAL_COSTS);
                    CCZYJS_Param.Add("ZF_COSTS", ZF_COSTS);
                    CCZYJS_Param.Add("TOTAL_CHAGE", TOTAL_CHAGE);
                    CCZYJS_Param.Add("D506_23", D506_23);
                    CCZYJS_Param.Add("D506_18", D506_18);
                    CCZYJS_Param.Add("BEGINPAY", BEGINPAY);
                    CCZYJS_Param.Add("SCALE", SCALE);
                    CCZYJS_Param.Add("HEAV_REDEEM_SUM", HEAV_REDEEM_SUM);
                    CCZYJS_Param.Add("REDEEM_TOTAL", REDEEM_TOTAL);
                    CCZYJS_Param.Add("BILL_TIME",data);   //获取系统时间
                    CCZYJS_Param.Add("AREA_CODE", AREA_NO);   //获取系统时间

                    HIS.CCZYJS(CCZYJS_Param);

                    retStr = DataConvert.getReturnJson("0", jsonStr);
                    
                }
                else
                {
                    retStr = DataConvert.getReturnJson("-1", "信息有误：" + jiesuan.getExecuteResultPlainString());
                }
            }
            catch(Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }
        public string retStr { get; set; }
    }
}