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
                //todo:HIS系统先验证用户名和密码
                /*
                string str_sql;
                str_sql = "select oper_name,pwd from code_operator where oper_code='" + name + "'";
                DataTable dt1 = DBUtil.queryExecute(str_sql);

                long ll_c;
                ll_c = dt1.Rows.Count;
                if (ll_c > 0)
                {
                    //p===pass
                    if (dt1.Rows[0]["pwd"].ToString() != pwd)
                    {
                        return "";//==
                    }
                    else
                    {
                        return "0;10011;admin;admin;刘德华;8881122;18012345678;天泰医院;DEP_AREA;USER_JG;DEP_LEVEL;AREA_CODE;T_IS_FLASH_AUTHORIZED;T_YEARS;T_IS_SK;T_IS_SK_HOSP;T_IS_XJ;T_RJZ_DATE;T_CH_START_DATE;T_CH_END_DATE;T_DY_MX_IS_HZ;T_IS_BLUSH_DAY;T_BLUSH_DAY;";
                    }
                }
                else
                {
                    return "";
                }
                */
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
                if (flag == "2")
                {
                    return retStr = DataConvert.getReturnJson("-1", "此病人在其他医院已经做过入院登记:" + zydjCheck.getExecuteResultPlainString());
                }
                if (flag == "3")
                {
                    return retStr = DataConvert.getReturnJson("-1", "程序异常:" + zydjCheck.getExecuteResultPlainString());
                }

                if (flag == "0")
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
                        {"D504_09",jsonDict["D504_09"]}, //住院号
                        };
                        HIS.bcZYBC(record);

                        retStr = DataConvert.getReturnJson("0", "保存住院登记成功");
                    }
                    else
                    {
                        retStr = DataConvert.getReturnJson("-1", newNotice.getExecuteResultPlainString());
                    }
                }

                if (flag == "1")
                {
                    //修改住院登记
                    ZYBC_PROC_UPDATE_NOTICE updateNotice = new ZYBC_PROC_UPDATE_NOTICE();
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("AREA_CODE", jsonDict["AREA_CODE"]); //病人地区编码(取前台选择的地区编码)
                    param.Add("D504_01", jsonDict["D504_01"]); //住院登记流水号
                    param.Add("D504_21", jsonDict["D504_21"]); //疾病代码
                    param.Add("D504_09", jsonDict["D504_09"]); //住院号
                    param.Add("D504_10", jsonDict["D504_10"]); //就诊类型
                    param.Add("D504_19", jsonDict["D504_19"]); //入院状态代码(对应S301-02.xls)
                    param.Add("D504_16", jsonDict["D504_16"]); //入院科室代码（对应S201-03.xls）
                    param.Add("D504_11", jsonDict["D504_11"]); //入院时间(格式为YYYY-MM-DD)
                    param.Add("D504_28", jsonDict["D504_28"]); //联系电话

                    updateNotice.executeSql(param);
                    if (updateNotice.getExecuteStatus() == true)
                    {
                        //保存修改到HIS
                        Dictionary<string, string> record = new Dictionary<string, string>() { 
                            { "D401_10", D401_10 }, 
                            { "D401_21", D401_21 },
                            { "D504_21", jsonDict["D504_21"] }
                        };
                        HIS.xgZYBC(record);
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

                        DataTable dt = HIS.getMZMX(map);
                        if (dt == null || dt.Rows.Count == 0)
                        {
                            XnhLogger.log("处方号：" + REC_NO + "查询门诊明细失败");
                            continue; //继续处理下一个
                        }
                        //拼接多个流水的数据
                        D502_04 += dt.Rows[0]["item_code"] as string + ";";
                        D502_09 += dt.Rows[0]["qty"] as string + ";";
                        D502_08 += dt.Rows[0]["price"] as string + ";";
                        D501_14 = dt.Rows[0]["oper_name"] as string;
                        REC_NO_ALL += REC_NO;
                    }

                    D502_04 = D502_04.Length > 1 ? D502_04.Substring(0, D502_04.Length - 1) : ""; //去掉最后一个分号
                    D502_09 = D502_09.Length > 1 ? D502_09.Substring(0, D502_09.Length - 1) : ""; //去掉最后一个分号
                    D502_08 = D502_08.Length > 1 ? D502_08.Substring(0, D502_08.Length - 1) : ""; //去掉最后一个分号

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
                    param.Add("D503_16", jsonDict["DEP_ID"]); //补偿机构代码(鉴于不做转外的，补偿机构代码和就医机构代码暂时一样)=DEP_ID
                    param.Add("D501_10", REC_TIME); //就诊日期(前台是用户自己选择的)格式为(YYYY-MM-DD)
                    param.Add("USER_ID", jsonDict["USER_ID"]); //取存储过的的用户ID
                    param.Add("FLAG", "1"); //1 试算 2 收费
                    param.Add("D502_04", D502_04); //药品编码字符串(用分号分隔)此处因药品可能是多个，药品编码和下一个药品编码用分号分隔(下面的数量、单价、比例也一样)
                    param.Add("D502_09", D502_09); //药品数量字符串(用分号分隔)
                    param.Add("D502_08", D502_08); //药品单价字符串(用分号分隔)
                    param.Add("D502_10", jsonDict["D502_10"]); //药品比例字符串(用分号分隔)
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
        public string charge(string USER_ID, string PARAM)
        {
            string retStr = "";
            try
            {
                //(1)收费
                MZBC_PROC_CALE_PRICE_LIST shoufei = new MZBC_PROC_CALE_PRICE_LIST();
                //根据前面一步缓存的试算信息
                string paramBase64 = DataConvert.Base64Decode(PARAM); //解码参数
                Dictionary<string, string> paramDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(paramBase64);
                
                //paramDict.Add("AREA_CODE", "");//病人地区编码(取前台选择的地区编码)
                //paramDict.Add("D504_01", "");//住院登记流水号
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

                shoufei.executeSql(paramDict);
                XnhLogger.log("收费成功，参数：" + shoufei.parames);
                XnhLogger.log("收费成功，结果：" + shoufei.getExecuteResultPlainString());
                //收费成功： S_Returns= 0;T_D502_01   （分号分隔）
                //T_D502_01：门诊登记流水号，此号要存储，以便后面打印补偿凭据的时候用。
                if (shoufei.getExecuteStatus() == true)
                {
                    retStr = DataConvert.getReturnJson("0", "收费成功，返回结果：" + shoufei.getExecuteResultPlainString());
                }
                else
                {
                    retStr = DataConvert.getReturnJson("-1", "收费失败，返回结果：" + shoufei.getExecuteResultPlainString());
                }

                //(2)收费成功后将相应的信息保存到数据库中，并修改HIS中补偿标志（供以后制作报表查询使用）
                //加上更新HIS的SQL操作
                Dictionary<string, string> retDict = shoufei.getResponseResultWrapperMap();
                //tDict["T_D502_01"]; 这个就是获取收费返回的结果
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " 收费失败： " + ex.StackTrace);
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
        public string chongzheng(string USER_ID, string AREA_NO,string MZLSH)
        {
            string retStr = "";
            try
            {
                string[] array = MZLSH.Split('$'); //前台选中多个流水号

                string info = "";
                foreach (string T_D502_01 in array)
                {
                    if (string.IsNullOrEmpty(T_D502_01) == true)
                    {
                        continue;
                    }

                    MZBC_MZCZ mzcz = new MZBC_MZCZ();
                    Dictionary<string, string> paramDict = new Dictionary<string, string>();
                    paramDict.Add("AREA_NO", AREA_NO);//病人地区编码(取前台选择的地区编码)
                    paramDict.Add("T_D502_01", T_D502_01);//取存储过的门诊登记流水号

                    mzcz.executeSql(paramDict);
                    if(mzcz.getExecuteStatus() == true) //冲正成功
                    {
                        info += "门诊流水:" + T_D502_01 + "冲正成功;";
                        //冲正成功要修改HIS标志
                        
                    //   Dictionary<string, string> map = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(selectedParam);
                   //    string REC_NO = map["REC_NO"];        //处方号
                   //     string REG_NO = map["REG_NO"];        //门诊流水号
                      //  Dictionary<string, string> record = new Dictionary<string, string>() { { "REC_NO", REC_NO }, { "REC_NO", REC_NO } };
                     //   HIS.modifyMZCZBJ(record);

                    }
                    else
                    {
                        info += "门诊流水:" + T_D502_01 + "冲正失败;失败信息:" + mzcz.getExecuteResultPlainString();
                    }

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
         + "( IP_REGISTER.CHRG_FLAG <> '3' ) )"
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
                string sql = "select "
           + "b.ip_no, "	//--住院号
           + "a.item_code, "	//--HIS项目编码
           + "price, "		//--HIS项目单价
           + "qty, "		//--HIS项目数量
           + "total, "		//--HIS项目总价格
           + "bill_time, "	//--记账时间
           + "a.basic_cls, "//多条同批次缺库存，其他批次收费标志
           + "pre_no, "		//--医嘱编号
           + "a.up_flag, "	//--上传标志
           + "standard, "	//--规格
           + "small_unit, "	//--单位
           + "(select item_cls from code_item where item_code=a.item_code ) as item_cls, "	//--项目类型(1,2,3:药品 4,5,6,7,8,9:其他)
           + "(select item_name from code_item where item_code=a.item_code ) as item_name " 	//--项目名称
           + "from "
           + "IP_BILL a left join IP_REGISTER b on a.reg_no=b.reg_no ,plus_item c "
           + "where "
           + "c.item_code=a.item_code and c.type=3 and a.reg_no='" + data + "'  "
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
                string sql = " SELECT cl_CHARGE_RECIPE.REC_NO as REC_NO,CODE_OPERATOR.oper_name,PATIENTINFO.NAME ,CL_RECIPE.REC_TIME, " +
                "CL_RECIPE.UP_FLAG,CL_RECIPE.REG_NO,CL_RECIPE.REG_NO as mzh, CL_CHARGE.CHRG_NO ,CL_CHARGE.CHRG_TIME, " +
                "CL_CHARGE.OPER_CODE ,CL_CHARGE.TYPE ,CL_CHARGE.REC_FLAG ,CL_CHARGE.FEE_CODE ,CL_CHARGE.STATUS, " +
                "WMSYS.WM_CONCAT(CL_CHARGE_INVOICE.INVO_NO) as INVO_NO, " +
                "(select sum(total_sum) from CL_CHRGENTRY where chrg_no = CL_CHARGE.CHRG_NO) as total, " +
      "max((select sum(total) from CL_RECENTRY where CL_RECENTRY.rec_no = CL_RECIPE.rec_no)) as total_rec,d.dept_name " +
      "FROM CL_CHARGE , CL_CHARGE_INVOICE ,CL_CHARGE_RECIPE,CL_RECIPE , PATIENTINFO,CODE_OPERATOR , CODE_DEPARTMENT d " +
      "WHERE ( CL_CHARGE.CHRG_NO = CL_CHARGE_INVOICE.CHRG_NO(+)) and (CL_CHARGE.CHRG_NO = CL_CHARGE_RECIPE.CHRG_NO) AND " +
      "(CL_CHARGE_RECIPE.REC_NO = CL_RECIPE.REC_NO) AND (CL_RECIPE.PID = PATIENTINFO.PID) AND (CL_CHARGE_RECIPE.FLAG = '1') AND " +
      "(CL_CHARGE.CLASS = '2' ) AND ((PATIENTINFO.NAME LIKE '%" + query + "%') or (INVO_NO like '%" + query + "%') ) " +
      //"(CL_CHARGE.CHRG_DATE >= '2016.01.01') AND (cl_CHARGE_RECIPE.TYPE = '2') and " +
      dateSql + " AND (cl_CHARGE_RECIPE.TYPE = '2') and " +
      "d.dept_code=CL_RECIPE.dept_code and CODE_OPERATOR.OPER_CODE=CL_RECIPE.dr_code " +
      "group by cl_CHARGE_RECIPE.REC_NO,CODE_OPERATOR.oper_name,PATIENTINFO.NAME ,CL_RECIPE.REC_TIME, " +
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

        [WebMethod]
        public string bclb(string AREA_NO)
        {
            string retStr = "";
            try
            {
                //RJZ_Get_S301_06_Zy zy = new RJZ_Get_S301_06_Zy();
                MZBC_Get_S301_06 zy = new MZBC_Get_S301_06();
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

    }
}