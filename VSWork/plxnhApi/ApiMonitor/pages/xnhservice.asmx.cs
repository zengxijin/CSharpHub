using ApiMonitor.Buffer;
using ApiMonitor.DataConvertUtil;
using ApiMonitor.DB;
using ApiMonitor.log;
using Service.Util;
using Service.WebService.ServiceImpl.login;
using Service.WebService.ServiceImpl.MZBC;
using Service.WebService.ServiceImpl.RJZ;
using System;
using System.Collections.Generic;
using System.Data;
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
        /// <param name="name">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns>验证失败返回空</returns>
        [WebMethod]
        public string login(string name,string pwd)
        {
            string retVal = "";
            try
            {
                //todo:HIS系统先验证用户名和密码
                
                //调用接口认证
                LoginAuth service = new LoginAuth();

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
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
                retVal = "";
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
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
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
                    if(shisuan.getExecuteStatus() == true) //试算成功
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
        public string charge(string user_id,string DIAGNOSIS_CODE)
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
                string sql = ""; //查询HIS的SQL
                DataTable dt = new DataTable();
                dt = DBUtil.queryExecute(sql);

                //todo:返回的DataTable数据，可以通过调用DataTable2Json转为JSON格式，方便前台JavaScript处理和绑定
                retStr = DataConvert.DataTable2Json(dt);
                
            }
            catch (Exception ex)
            {
                XnhLogger.log(this.GetType().ToString() + " " + ex.StackTrace);
            }
            return retStr;
        }
    }
}
