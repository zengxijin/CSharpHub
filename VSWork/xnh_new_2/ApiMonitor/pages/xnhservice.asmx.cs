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
                        retVal =  DataConvert.Dict2Json(new Dictionary<string, string>() { { "ylzh", D401_10 }, { "data", retMember } });
                        retVal = DataConvert.getReturnJson("0", retVal);

                        //家庭成员信息存储到HIS
                        string[] memberArray = retMember.Split(new string[]{";"},StringSplitOptions.None);
                        foreach (string one in memberArray)
                        {
                            string D401_21 = one.Split(new string[] { "/" }, StringSplitOptions.None)[0]; //成员序号
                            Dictionary<string, string> record = new Dictionary<string, string>() { { "D401_10", D401_10 }, { "D401_21", D401_21 } };
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
                //(1)验证输入疾病是否在疾病库中存在，存在可以继续登记
                MZBC_PROC_DIAGNOSIS_CHECK check = new MZBC_PROC_DIAGNOSIS_CHECK();
                string DIAGNOSIS_CODE = ""; //疾病代码
                check.executeSql(
                    new Dictionary<string, string>() { { "DIAGNOSIS_CODE", DIAGNOSIS_CODE } }
                 );
                
                //0	成功
                //1	此疾病在疾病库中不存在
                //2	程序异常
                if (check.getExecuteResultPlainString() != null && check.getExecuteResultPlainString().Length >= 1
                    && check.getExecuteResultPlainString().Substring(0,1) !="0")
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
                string AREA_NO = "";
                string D401_21 = "";
                string DEP_ID = "";
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

                string decodeParam = DataConvert.Base64Decode(PARAM); //解码
                Dictionary<string, string> jsonDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(decodeParam);

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
                    param.Add("D504_11", jsonDict["D504_11"]); //入院时间(格式为YYYY-MM-DD)
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


        public string qxrydj(string AREA_NO, string D504_01)
        {
            string retStr = "";
            try
            {
                ZYBC_PROC_DELETE_NOTICE deleteNotice = new ZYBC_PROC_DELETE_NOTICE();
                deleteNotice.executeSql(new Dictionary<string, string>() { { "AREA_NO", AREA_NO }, { "D504_01", D504_01 } });
                //0	成功
                //1	失败 
                //删除成功： S_Returns= 0
                //删除失败：S_Returns= 1;错误信息   （分号分隔）
                if (deleteNotice.getExecuteStatus() == true)
                {
                    //删除HIS的登记信息
                    HIS.scZYBC(new Dictionary<string, string>() { { "D504_01", D504_01 } });
                    retStr = DataConvert.getReturnJson("0", "删除入院登记成功");
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
        public string tryCalculate(string user_id, string DIAGNOSIS_CODE)
        {
            try
            {
                //(1)根据用户选择的疾病编码调用 验证输入疾病是否在疾病库中存在交易，判断是否存在，不存在提示报错，中断试算；存在进行试算业务交易。
                MZBC_PROC_DIAGNOSIS_CHECK dCheck = new MZBC_PROC_DIAGNOSIS_CHECK();
                string retStr = dCheck.executeSql(
                    new Dictionary<string, string>() { { "DIAGNOSIS_CODE", DIAGNOSIS_CODE } }
                    );
                if (retStr == "0")
                {
                    //(2)疾病存在，进行试算交易
                    RJZ_getShisuanResult shisuan = new RJZ_getShisuanResult();
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("D505_02", "");//登记流水号
                    param.Add("COME_AREA", "");//地区代码(支付单位)
                    param.Add("AREA_CODE", "");//病人地区编码(取前台选择的地区编码)
                    param.Add("D504_07", "");//家庭编号
                    param.Add("D504_02", "");//成员序号
                    param.Add("D504_14", "");//就医机构
                    param.Add("D504_21", "");//入院诊断(疾病代码)
                    param.Add("D504_11", "");//就诊日期(入院时间) (格式为YYYY-MM-DD)
                    param.Add("D506_15", "");//补偿类别代码
                    param.Add("D504_15", "");//就医机构级别(相关数据代码标准:S201-06)
                    param.Add("D504_06", "");//年龄
                    param.Add("D504_10", "");//就诊类型(相关数据代码标准:S301-05)
                    param.Add("D504_12", "");//出院时间(格式为YYYY-MM-DD)
                    param.Add("D504_29", "");//出院诊断（疾病代码）
                    param.Add("D504_16_D", "");//入院科室(相关数据代码标准:S201-03)
                    param.Add("D504_16_T", "");//出院科室(相关数据代码标准:S201-03)
                    param.Add("S701_01", "");//是否是中途结算(相关数据代码标准:S701-01)

                    shisuan.executeSql(param);
                    if (shisuan.getExecuteStatus() == true) //试算成功
                    {
                        //需要存储试算结果，进行收费交易
                        Dictionary<string, string> retDict = shisuan.getResponseResultWrapperMap();
                        //retDict["TOTAL_COSTS"];//总费用
                        //retDict["ZF_COSTS"];//自费费用
                        //retDict["TOTAL_CHAGE"];//合理费用
                        //retDict["D506_23"];//实际补偿金额
                        //retDict["D506_18"];//核算补偿金额[实际补偿合计额)
                        //retDict["BEGINPAY"];//本次起伏线
                        //retDict["SCALE"];//报销比例
                        //retDict["HEAV_REDEEM_SUM"];//大病支付额
                        //retDict["REDEEM_TOTAL"];//单次补偿合计
                    }
                }
                else if (retStr == "1")
                {
                    //疾病不存在
                }
                else
                {
                    //出错，记录日志
                    XnhLogger.log(this.GetType().ToString() + " tryCalculate " + DIAGNOSIS_CODE + "疾病代码不存在，接口返回结果：" + retStr);
                }
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " tryCalculate " + ex.StackTrace);
            }
            return "fail";
        }

        /// <summary>
        /// 收费
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="DIAGNOSIS_CODE"></param>
        /// <returns></returns>
        [WebMethod]
        public string charge(string user_id, string DIAGNOSIS_CODE)
        {
            try
            {
                //(1)收费
                RJZ_shoufei shoufei = new RJZ_shoufei();
                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                paramDict.Add("AREA_CODE", "");//病人地区编码(取前台选择的地区编码)
                paramDict.Add("D504_01", "");//住院登记流水号
                paramDict.Add("D504_12", "");//出院时间(格式为YYYY-MM-DD)
                paramDict.Add("D504_15", "");//就医机构级别(相关数据代码标准:S201-06)
                paramDict.Add("D504_17", "");//出院科室(相关数据代码标准:S201-03)
                paramDict.Add("D504_18", "");//经治医生
                paramDict.Add("D504_20", "");//出院状态(相关数据代码标准:S301-03)
                paramDict.Add("D504_22", "");//并发症(为空时传’NULL’)
                paramDict.Add("D506_03", "");//总费用（TOTAL_COSTS 总费用）试算得到
                paramDict.Add("D506_13", "");//可补偿住院医药费（TOTAL_CHAGE 合理费用）试算得到
                paramDict.Add("D506_18", "");//核算补偿金额（D506_18  核算补偿金额(实际补偿合计额)）试算得到
                paramDict.Add("D506_15", "");//补偿类别代码
                paramDict.Add("D506_14", "");//补偿账户类别(相关数据代码标准:S301-09)
                paramDict.Add("D506_16", "");//核算机构(代码)
                paramDict.Add("D506_17", "");//核算人
                paramDict.Add("D506_23", "");//实际补偿额（D506_23   实际补偿金额）试算得到
                paramDict.Add("D506_26", "");//付款人
                paramDict.Add("D506_27", "");//中途结算标志(相关数据代码标准:S701-01)
                paramDict.Add("SELF_PAY", "");//自费金额（ZF_COSTS  自费费用）试算得到
                paramDict.Add("HEAV_REDEEM_SUM", "");//大病支付金额（HEAV_REDEEM_SUM  大病支付额）试算得到
                paramDict.Add("BEGINPAY", "");//本次起付额（BEGINPAY   本次起伏线）试算得到
                paramDict.Add("D504_29", "");//出院诊断(疾病代码)

                shoufei.executeSql(paramDict);

                //(2)收费成功后将相应的信息保存到数据库中，并修改HIS中补偿标志（供以后制作报表查询使用）
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " diagnosisCheck " + ex.StackTrace);
            }

            return null;
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
                string sql = "select t.BZ_CODE from XCYB_ML_ZG_BZML t where t.bz_name like '%" + trim(query) + "%'"
                    + " or t.py_code like '%" + query + "%'";
                //如果要限制返回结果的条数，比如限制每次返回前10条
                //select t.BZ_CODE from XCYB_ML_ZG_BZML t where ( t.bz_name like '%1%' or t.py_code like '%1%') and rownum < 11
                string sqlLimit = "select t.BZ_CODE,t.BZ_NAME,t.PY_CODE from XCYB_ML_ZG_BZML t where (t.bz_name like '%" + query + "%'"
                   + " or t.py_code like '%" + query + "%') and rownum < 21";
                //日志查询后台SQL
                XnhLogger.log(sqlLimit);
                //返回查询结果，供前台绑定
                DataTable dt = DBUtil.queryExecute(sqlLimit);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string BZ_CODE = dr["BZ_CODE"] as string;
                        string BZ_NAME = dr["BZ_NAME"] as string;
                        string PY_CODE = dr["PY_CODE"] as string;
                        if (string.IsNullOrEmpty(BZ_CODE) == false && BZ_CODE.Trim() != "")
                        {
                            retStr += BZ_CODE.Trim() + "|" + BZ_NAME + "|" + PY_CODE + ",";
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
                //todo:由HIS提供视图或查询字段绑定
                retStr = DataConvert.getReturnJson("-1", "query=" + query + "　待由HIS提供视图或查询字段完成数据绑定");
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retStr = DataConvert.getReturnJson("-1", ex.ToString());
            }
            return retStr;
        }

        /// <summary>
        ///批量传明细
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string plcmx(string data)
        {
            string retStr = "";
            try
            {
                //todo:由HIS提供字段信息
                retStr = DataConvert.getReturnJson("-1", "data=" + data + "　待由HIS提供字段数据");
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
      "(CL_CHARGE.CLASS = '2' ) AND ((PATIENTINFO.NAME LIKE '%" + query + "%') or (INVO_NO like '%" + query + "%') ) AND " +
      "(CL_CHARGE.CHRG_DATE >= '2016.01.01') AND (cl_CHARGE_RECIPE.TYPE = '2') and " +
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
                    retStr = DataConvert.getReturnJson("0",zy.getExecuteResultPlainString().Substring(2));
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