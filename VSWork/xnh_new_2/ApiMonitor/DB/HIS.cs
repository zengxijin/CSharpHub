using ApiMonitor.log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ApiMonitor.DB
{
    public class HIS
    {
        #region mzbc
        //create table mzbc
        //(D401_10      VARCHAR2(18),   医疗证号
        // D401_21      CHAR(2),        成员序号
        // T_D502_01    VARCHAR2(24)    门诊登记流水号（后面打印补偿凭据的时候用）  
        //)
        /// <summary>
        /// 插入新纪录
        /// </summary>
        /// <param name="sqlParam"></param>
        public static void insertMZBC(Dictionary<string,string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("insertMZBC sqlParam参数为空");
                return;
            }

            string sql = "insert into mzbc " +
                    "select '$D401_10$','$D401_21$','$T_D502_01$' from dual "
                    + "where not exists (select * from mzbc t where t.D401_10='$D401_10$' and t.D401_21='$D401_21$' )";
            try
            {
                sql = sql.Replace("$D401_10$", (sqlParam.ContainsKey("D401_10") == true ? sqlParam["D401_10"] : ""));
                sql = sql.Replace("$D401_21$", (sqlParam.ContainsKey("D401_21") == true ? sqlParam["D401_21"] : ""));
                sql = sql.Replace("$T_D502_01$", (sqlParam.ContainsKey("T_D502_01") == true ? sqlParam["T_D502_01"] : ""));

                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }
         /// <summary>
        
        ///门诊冲正修改his标记
        /// </summary>
        public static void modifyMZCZBJ(Dictionary<string,string> sqlParam)
        {
         if (sqlParam == null || sqlParam.Count == 0)
         {
             XnhLogger.log("modifyMZCZBJ sqlParam参数为空");
             return ;
         }
         string sql = "UPDATE cl_recipe SET yb_up = '0' where rec_no = '$REC_NO$' and reg_no = '$REG_NO$' ";
            try
            {
            sql = sql.Replace("0", (sqlParam.ContainsKey("yb_up") == true ? sqlParam["yb_up"] : ""));
            sql = sql.Replace("$REC_NO$", (sqlParam.ContainsKey("rec_no") == true ? sqlParam["rec_no"] : ""));
            sql = sql.Replace("$REG_NO$", (sqlParam.ContainsKey("reg_no") == true ? sqlParam["reg_no"] : ""));
            DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }
        ///门诊结算修改his标记
        /// </summary>
        public static void modifyMZJSBJ(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("modifyMZCZBJ sqlParam参数为空");
                return;
            }
            string sql = "UPDATE cl_recipe SET yb_up = '1',"
              + "yb_caseno = '+此处结算返回的门诊登记流水号(不知道参数在哪里取)+' "
            + "where rec_no in ('$REC_NO$') and reg_no in ('$REG_NO$') ";
            try
            {
                sql = sql.Replace("0", (sqlParam.ContainsKey("yb_up") == true ? sqlParam["yb_up"] : ""));
                sql = sql.Replace("$REC_NO$", (sqlParam.ContainsKey("rec_no") == true ? sqlParam["rec_no"] : ""));
                sql = sql.Replace("$REG_NO$", (sqlParam.ContainsKey("reg_no") == true ? sqlParam["reg_no"] : ""));
                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }
        /// <summary>
        /// 存储门诊结算返回结果，后期做报表
        /// create table MZJS
        ///  (D401_02        VARCHAR2(18),     -- 成员姓名
        ///   T_D502_01      CHAR(30),       -- 门诊登记流水号
        ///   TOTAL_COSTS    VARCHAR2(24),      --总费用
        ///   ZF_COSTS       VARCHAR2(24),      --自费费用
        ///   TOTAL_CHAGE       VARCHAR2(24),      --合理费用
        ///   D506_23          VARCHAR2(24),      --实际补偿金额
        ///   D506_18           VARCHAR2(24),    --核算补偿金额[实际补偿合计额)
        ///   BEGINPAY           VARCHAR2(24),   --本次起伏线
        ///   SCALE             VARCHAR2(24),    --报销比例
        ///   HEAV_REDEEM_SUM   VARCHAR2(24),    --大病支付额
        ///   REDEEM_TOTAL       VARCHAR2(24)    --单次补偿合计
        ///   )
        /// </summary> 
        /// <param name="sqlParam"></param>
        public static void CCMZJS(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("MZJS sqlParam参数为空");
                return;
            }
            string sql = "insert into MZJS(D401_02,T_D502_01,TOTAL_COSTS,ZF_COSTS,TOTAL_CHAGE,D506_23,D506_18,BEGINPAY,SCALE,HEAV_REDEEM_SUM,REDEEM_TOTAL) "
                          + "values (D401_02,T_D502_01,TOTAL_COSTS,ZF_COSTS,TOTAL_CHAGE,D506_23,D506_18,BEGINPAY,SCALE,HEAV_REDEEM_SUM,REDEEM_TOTAL)";
 ///依次是：成员姓名，门诊登记流水号，总费用，自费费用，合理费用，实际补偿金额，核算补偿金额[实际补偿合计额)，本次起伏线，报销比例，大病支付额，单次补偿合计
            try
            {
                sql = sql.Replace("$D401_02$", (sqlParam.ContainsKey("D401_02") == true ? sqlParam["D401_02"] : ""));
                sql = sql.Replace("$T_D502_01$", (sqlParam.ContainsKey("T_D502_01") == true ? sqlParam["T_D502_01"] : ""));
                sql = sql.Replace("$TOTAL_COSTS$", (sqlParam.ContainsKey("TOTAL_COSTS") == true ? sqlParam["TOTAL_COSTS"] : ""));
                sql = sql.Replace("$ZF_COSTS$", (sqlParam.ContainsKey("ZF_COSTS") == true ? sqlParam["ZF_COSTS"] : ""));
                sql = sql.Replace("$TOTAL_CHAGE$", (sqlParam.ContainsKey("TOTAL_CHAGE") == true ? sqlParam["TOTAL_CHAGE"] : ""));
                sql = sql.Replace("$D506_23$", (sqlParam.ContainsKey("D506_23") == true ? sqlParam["D506_23"] : ""));
                sql = sql.Replace("$D506_18$", (sqlParam.ContainsKey("D506_18") == true ? sqlParam["D506_18"] : ""));
                sql = sql.Replace("$BEGINPAY$", (sqlParam.ContainsKey("BEGINPAY") == true ? sqlParam["BEGINPAY"] : ""));
                sql = sql.Replace("$SCALE$", (sqlParam.ContainsKey("SCALE") == true ? sqlParam["SCALE"] : ""));
                sql = sql.Replace("$HEAV_REDEEM_SUM$", (sqlParam.ContainsKey("HEAV_REDEEM_SUM") == true ? sqlParam["HEAV_REDEEM_SUM"] : ""));
                sql = sql.Replace("$REDEEM_TOTAL$", (sqlParam.ContainsKey("REDEEM_TOTAL") == true ? sqlParam["REDEEM_TOTAL"] : ""));
                //sql = sql.Replace("$TOTAL_COSTS$", (sqlParam.ContainsKey("TOTAL_COSTS") == true ? sqlParam["TOTAL_COSTS"] : ""));
                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }
        ///门诊冲正删除存储的费用信息
        /// </summary>
        public static void deleteCCMZJS(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("deleteCCMZJS sqlParam参数为空");
                return;
            }
            string sql = "delete from MZJS where T_D502_01 = T_D502_01 and D401_02 = D401_02";
            try
            {
                sql = sql.Replace("$T_D502_01$", (sqlParam.ContainsKey("T_D502_01") == true ? sqlParam["T_D502_01"] : ""));
                sql = sql.Replace("$D401_02$", (sqlParam.ContainsKey("D401_02") == true ? sqlParam["D401_02"] : ""));
                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }
        public static void updateMZBC(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("updateMZBC sqlParam参数为空");
                return;
            }

            string sql = "UPDATE mzbc t SET t.T_D502_01='$T_D502_01$' WHERE t.D401_10 = '$D401_10$' AND t.D401_21 = '$D401_21$'";
            try
            {
                sql = sql.Replace("$D401_10$", (sqlParam.ContainsKey("D401_10") == true ? sqlParam["D401_10"] : ""));
                sql = sql.Replace("$D401_21$", (sqlParam.ContainsKey("D401_21") == true ? sqlParam["D401_21"] : ""));
                sql = sql.Replace("$T_D502_01$", (sqlParam.ContainsKey("T_D502_01") == true ? sqlParam["T_D502_01"] : ""));

                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }

        public static DataTable getMZMX(Dictionary<string, string> sqlParam)
        {
            DataTable dt = null;
            string sql = "";
            try
            {
                //门诊明细信息
                sql = 
                "select"
                + " a.rec_no,"	 //处方号
                + " a.item_code,"	 //HIS项目编码
                + " a.price,"	 //HIS项目单价
                + "(select wydm from xnh_dm where xnh_dm.item_code = b.item_code ) as nh_bm, "  // --农合编码
                + "(select bxbl from xnh_ypzl ,xnh_dm where xnh_ypzl.wydm = xnh_dm.wydm and xnh_dm.item_code = b.item_code) as nh_bnw,"//报销比例
                + " a.qty,"		 //项目数量
                + " a.total,"	 //项目总金额
                + " b.item_name,"	 //HIS项目名称
                + " b.item_cls,"	 //项目类型(1,2,3:药品 4,5,6,7,8,9:其他)
                + " c.standard,"	 //规格
                + " c.small_unit," 	 //单位
                + " d.DR_CODE,"	 //开单医生
                + " (select oper_name from code_operator where oper_code=d.dr_code) as oper_name,"  //医生名称
                + " d.dept_code,"	 //开单科室
                + " (select dept_name from CODE_DEPARTMENT where dept_code=d.dept_code) as dept_name"  //科室名称
                + " from CL_RECENTRY a,code_item b,plus_item c ,cl_recipe d"
                + " where a.item_code<>'9999'"
                + " and a.row_status='0' and a.rec_no='$REC_NO$' "
                + " and a.rec_no = d.REC_NO"
                + " and a.item_code=b.item_code and b.item_code=c.item_code and c.type=2";

                sql = sql.Replace("$REC_NO$", (sqlParam.ContainsKey("REC_NO") == true ? sqlParam["REC_NO"] : ""));

                dt = DBUtil.queryExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log("sql=" + sql + " 异常：" + ex.ToString());
            }

            return dt;
        }
        
        
        #endregion

        #region zybc

        //create table zybc
        //(D401_10   VARCHAR2(18),  医疗证号
        // D401_21   CHAR(2),       成员序号
        // D504_01   VARCHAR2(24),  住院登记流水号（打印住院补偿凭据用到）
        // D504_21   VARCHAR2(40)   入院诊断  (费用上传会用)
        //)

        /// <summary>
        /// 根据选择的人员来来显示基础人员信息存储成员序号、医疗证号
        /// </summary>
        /// <param name="sqlParam"></param>
        public static void insertZYBC(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("insertZYBC sqlParam参数为空");
                return;
            }

            string sql = "insert into zybc(D401_10,D401_21,D504_21,D504_01,AREA_CODE) " +
                    "select '$D401_10$','$D401_21$','$D504_01$','$D504_21$','$AREA_CODE$' from dual "
                    + "where not exists (select * from zybc t where t.D401_10='$D401_10$' and t.D401_21='$D401_21$' )";
            try
            {
                sql = sql.Replace("$D401_10$", (sqlParam.ContainsKey("D401_10") == true ? sqlParam["D401_10"] : ""));
                sql = sql.Replace("$D401_21$", (sqlParam.ContainsKey("D401_21") == true ? sqlParam["D401_21"] : ""));
                sql = sql.Replace("$D504_21$", (sqlParam.ContainsKey("D504_21") == true ? sqlParam["D504_21"] : ""));
                sql = sql.Replace("$D504_01$", (sqlParam.ContainsKey("D504_01") == true ? sqlParam["D504_01"] : ""));
                sql = sql.Replace("$AREA_CODE$", (sqlParam.ContainsKey("AREA_CODE") == true ? sqlParam["AREA_CODE"] : ""));
                

                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }

        /// <summary>
        /// 入院登记保存时存储入院诊断、住院登记流水号 
        /// </summary>
        /// <param name="sqlParam"></param>
        public static void bcZYBC(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("bcZYBC sqlParam参数为空");
                return;
            }
            //UPDATE zybc SET D504_21=D504_21,D504_01=D504_01 WHERE D401_10 = D401_10 AND D401_21 = D401_21;
            string sql = "UPDATE zybc t SET t.D504_21='$D504_21$',t.D504_01='$D504_01$',t.D504_09='$D504_09$' WHERE t.D401_10 = '$D401_10$' AND t.D401_21 = '$D401_21$'";
            try
            {
                sql = sql.Replace("$D401_10$", (sqlParam.ContainsKey("D401_10") == true ? sqlParam["D401_10"] : "")); //医疗证号
                sql = sql.Replace("$D401_21$", (sqlParam.ContainsKey("D401_21") == true ? sqlParam["D401_21"] : "")); //成员序号

                sql = sql.Replace("$D504_21$", (sqlParam.ContainsKey("D504_21") == true ? sqlParam["D504_21"] : "")); //疾病代码
                sql = sql.Replace("$D504_01$", (sqlParam.ContainsKey("D504_01") == true ? sqlParam["D504_01"] : "")); //住院登记流水号（农合返回的）
                sql = sql.Replace("$D504_09$", (sqlParam.ContainsKey("D504_09") == true ? sqlParam["D504_09"] : "")); //住院号

                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }

        /// <summary>
        /// 修改入院登记更新入院诊断
        /// </summary>
        /// <param name="sqlParam"></param>
        public static void xgZYBC(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("xgZYBC sqlParam参数为空");
                return;
            }
            //update zybc set D504_21 = D504_21 WHERE D401_10 = D401_10 AND D401_21 = D401_21;
            string sql = "UPDATE zybc t SET t.D504_21='$D504_21$' WHERE t.D401_10 = '$D401_10$' AND t.D401_21 = '$D401_21$'";
            try
            {
                sql = sql.Replace("$D401_10$", (sqlParam.ContainsKey("D401_10") == true ? sqlParam["D401_10"] : ""));
                sql = sql.Replace("$D401_21$", (sqlParam.ContainsKey("D401_21") == true ? sqlParam["D401_21"] : ""));
                sql = sql.Replace("$D504_21$", (sqlParam.ContainsKey("D504_21") == true ? sqlParam["D504_21"] : ""));

                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }

        /// <summary>
        /// 删除入院登记
        /// </summary>
        /// <param name="sqlParam"></param>
        public static void scZYBC(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("scZYBC sqlParam参数为空");
                return;
            }
            //delete from zybc  where D504_01 = D504_01;
            string sql = "delete from zybc  where D504_01 = '$D504_01$'";
            try
            {
                sql = sql.Replace("$D504_01$", (sqlParam.ContainsKey("D504_01") == true ? sqlParam["D504_01"] : ""));

                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }
        /// <summary>
        /// 存储住院结算返回费用，后期做报表
        /// create table ZYJS
        ///  ( NAME     VARCHAR2(16), -- 姓名（）
        ///    IP_NO    VARCHAR2(24), -- 住院号（）
        ///    D504_01   VARCHAR2(24),--  住院登记流水号
        ///    
        ///    TOTAL_COSTS  NUMBER(8,2), -- 住院总费用
        ///    TOTAL_CHAGE  NUMBER(8,2),  -- 住院可补偿金额
        ///    D506_18   NUMBER(8,2),   -- 核算补偿金额
        ///    D506_23    NUMBER(8,2),  --  实际补偿额
        ///    ZF_COSTS   NUMBER(8,2),    --自费金额
        ///    HEAV_REDEEM_SUM  NUMBER(8,2), --大病支付金额
        ///    BEGINPAY  NUMBER(8,2)     --起伏线
        ///    
        ///   )
        /// </summary> 
        /// <param name="sqlParam"></param>
        public static void CCZYJS(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("MZJS sqlParam参数为空");
                return;
            }
            string sql = "insert into ZYJS(NAME,IP_NO,D504_01,TOTAL_COSTS,TOTAL_CHAGE,D506_18,D506_23,ZF_COSTS,HEAV_REDEEM_SUM,BEGINPAY) "
                          + "values (NAME,IP_NO,D504_01,TOTAL_COSTS,TOTAL_CHAGE,D506_18,D506_23,ZF_COSTS,HEAV_REDEEM_SUM,BEGINPAY)";
          
            try
            {
                sql = sql.Replace("$NAME$", (sqlParam.ContainsKey("NAME") == true ? sqlParam["NAME"] : ""));
                sql = sql.Replace("$IP_NO$", (sqlParam.ContainsKey("IP_NO") == true ? sqlParam["IP_NO"] : ""));
                sql = sql.Replace("$d505_02$", (sqlParam.ContainsKey("d505_02") == true ? sqlParam["d505_02"] : ""));
                sql = sql.Replace("$d505_01$", (sqlParam.ContainsKey("d505_01") == true ? sqlParam["d505_01"] : ""));
                sql = sql.Replace("$TOTAL_COSTS$", (sqlParam.ContainsKey("TOTAL_COSTS") == true ? sqlParam["TOTAL_COSTS"] : ""));
                sql = sql.Replace("$TOTAL_CHAGE$", (sqlParam.ContainsKey("TOTAL_CHAGE") == true ? sqlParam["TOTAL_CHAGE"] : ""));
               
                sql = sql.Replace("$D506_18$", (sqlParam.ContainsKey("D506_18") == true ? sqlParam["D506_18"] : ""));
                sql = sql.Replace("$D506_23$", (sqlParam.ContainsKey("D506_23") == true ? sqlParam["D506_23"] : ""));
                sql = sql.Replace("$ZF_COSTS$", (sqlParam.ContainsKey("ZF_COSTS") == true ? sqlParam["ZF_COSTS"] : ""));
                sql = sql.Replace("$HEAV_REDEEM_SUM$", (sqlParam.ContainsKey("HEAV_REDEEM_SUM") == true ? sqlParam["HEAV_REDEEM_SUM"] : ""));
                sql = sql.Replace("$BEGINPAY$", (sqlParam.ContainsKey("BEGINPAY") == true ? sqlParam["BEGINPAY"] : ""));
              
                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }
        ///住院明细上传修改对应his标记
        /// </summary>
        public static void modifyZYJSBJ(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("modifyZYCZBJ sqlParam参数为空");
                return;
            }
            string sql = "UPDATE ip_bill SET up_flag = '$up_flag$',"
            + "where REG_NO ='$reg_no$' and pre_no = '$pre_no$' and bill_time = '$bill_time$' and basic_cls = '$basic_cls$'";
            try
            {
                sql = sql.Replace("$up_flag$", (sqlParam.ContainsKey("up_flag") == true ? sqlParam["up_flag"] : ""));
                sql = sql.Replace("$pre_no$", (sqlParam.ContainsKey("pre_no") == true ? sqlParam["pre_no"] : ""));
                sql = sql.Replace("$reg_no$", (sqlParam.ContainsKey("reg_no") == true ? sqlParam["reg_no"] : ""));
                sql = sql.Replace("$bill_time$", (sqlParam.ContainsKey("bill_time") == true ? sqlParam["bill_time"] : ""));
                sql = sql.Replace("$basic_cls$", (sqlParam.ContainsKey("basic_cls") == true ? sqlParam["basic_cls"] : ""));
                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }

        ///入院登记修改对应his标记
        /// </summary>
        public static void modifyRYDJ(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("modifyRYDJ sqlParam参数为空");
                return;
            }
            string sql = "UPDATE ip_register SET up_flag = '$up_flag$' "  //入院登记成功改成‘1’
            + "where REG_NO ='$reg_no$' ";
            try
            {
                sql = sql.Replace("$up_flag$", (sqlParam.ContainsKey("up_flag") == true ? sqlParam["up_flag"] : ""));
               
                sql = sql.Replace("$reg_no$", (sqlParam.ContainsKey("reg_no") == true ? sqlParam["reg_no"] : ""));
               
                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }
        public static DataTable cxzymx(string reg_no)
        {
            string sql = "";
            try
            {
                sql = "select "
                  + "b.ip_no, "	//--住院号
                  + "a.item_code, "	//--HIS项目编码
                  + "price, "		//--HIS项目单价
                  + "(select wydm from xnh_dm where xnh_dm.item_code = a.item_code) as nh_bm, "    //农合编码
                  + "(select bxbl from xnh_ypzl  where xnh_ypzl.wydm = d.wydm and d.item_code = a.item_code) as nh_bnw, "//--报销比例
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
                  + "IP_BILL a left join IP_REGISTER b on a.reg_no=b.reg_no ,plus_item c left join xnh_dm d on c.item_code = d.item_code "
                  + "where "
                  + "c.item_code=a.item_code and c.type=3 and a.reg_no='" + reg_no + "'  "
                  + "order by bill_time,a.item_code ";

                DataTable dt = DBUtil.queryExecute(sql);
                return dt;
            }
            catch (Exception ex)
            {
                XnhLogger.log("查询住院明细：sql=" + sql + " 异常：" + ex.StackTrace);
            }
            return null;
        }
        ///提取本地his药品编码1
        /// </summary>
        public static DataTable fetchHIS(string ypdm)
        {

            string sql = "";
           
             try
            {
                sql = "select a.item_code,  " //--his编码
                          + "a.item_name,  " // --his项目名称
       + "(select wydm from xnh_dm where xnh_dm.item_code = a.item_code) as nh_code, "  //  --农合中心编码
       + " (select ypmc from xnh_ypzl,xnh_dm where xnh_ypzl.wydm = xnh_dm.wydm and xnh_dm.item_code = a.item_code) as nh_name, "      //--农合名称
       + " a.item_cls, "    //--项目类别123药品，其他诊疗
       + " a.py_code, "   // --拼音码
       + " b.standard, "   //--规格
       + "b.small_unit, "  //  --单位
       + " b.ret_price  "  //  --价格
      +" from code_item a,plus_item b "
      + " where (b.type = '3' and a.item_code = b.item_code) and (a.item_code like '%" + ypdm + "%' or a.item_name like '%" + ypdm + "%' or a.py_code like '%" + ypdm + "%')  order by a.item_code ";
                DataTable dt = DBUtil.queryExecute(sql);
                return dt;
            }
            catch (Exception ex)
            {
                XnhLogger.log("his药品数据提取：sql=" + sql + " 异常：" + ex.StackTrace);
            }
            return null;
        }
        ///未对吗
        public static DataTable fetchHIS1()
        {

            string sql = "";

            try
            {
                sql = "select * from (select a.item_code,  " //--his编码
                          + "a.item_name,  " // --his项目名称
       + "(select wydm from xnh_dm where xnh_dm.item_code = a.item_code) as nh_code, "  //  --农合中心编码
       + " (select ypmc from xnh_ypzl,xnh_dm where xnh_ypzl.wydm = xnh_dm.wydm and xnh_dm.item_code = a.item_code) as nh_name, "      //--农合名称
       + " a.item_cls, "    //--项目类别123药品，其他诊疗
       + " a.py_code, "   // --拼音码
       + " b.standard, "   //--规格
       + "b.small_unit, "  //  --单位
       + " b.ret_price  "  //  --价格
      + " from code_item a,plus_item b "
      + " where b.type = '3' and a.item_code = b.item_code and a.disable = 'F') h where h.nh_code is null and rownum < 10";
                DataTable dt = DBUtil.queryExecute(sql);
                return dt;
            }
            catch (Exception ex)
            {
                XnhLogger.log("his药品数据提取：sql=" + sql + " 异常：" + ex.StackTrace);
            }
            return null;
        }

        ///药品
        public static DataTable fetchHIS3()
        {

            string sql = "";

            try
            {
                sql = "select * from (select a.item_code,  " //--his编码
                          + "a.item_name,  " // --his项目名称
       + "(select wydm from xnh_dm where xnh_dm.item_code = a.item_code) as nh_code, "  //  --农合中心编码
       + " (select ypmc from xnh_ypzl,xnh_dm where xnh_ypzl.wydm = xnh_dm.wydm and xnh_dm.item_code = a.item_code) as nh_name, "      //--农合名称
       + " a.item_cls, "    //--项目类别123药品，其他诊疗
       + " a.py_code, "   // --拼音码
       + " b.standard, "   //--规格
       + "b.small_unit, "  //  --单位
       + " b.ret_price  "  //  --价格
      + " from code_item a,plus_item b "
      + " where b.type = '3' and a.item_code = b.item_code  and a.disable = 'F' and a.item_cls <>4 and a.item_cls <>5 and a.item_cls <>6 and a.item_cls <>7 and a.item_cls <>9 ) h where h.nh_code is null and rownum < 10";
                DataTable dt = DBUtil.queryExecute(sql);
                return dt;
            }
            catch (Exception ex)
            {
                XnhLogger.log("his药品数据提取：sql=" + sql + " 异常：" + ex.StackTrace);
            }
            return null;
        }
        ///诊疗
        public static DataTable fetchHIS4()
        {

            string sql = "";

            try
            {
                sql = "select * from (select a.item_code,  " //--his编码
                          + "a.item_name,  " // --his项目名称
       + "(select wydm from xnh_dm where xnh_dm.item_code = a.item_code) as nh_code, "  //  --农合中心编码
       + " (select ypmc from xnh_ypzl,xnh_dm where xnh_ypzl.wydm = xnh_dm.wydm and xnh_dm.item_code = a.item_code) as nh_name, "      //--农合名称
       + " a.item_cls, "    //--项目类别123药品，其他诊疗
       + " a.py_code, "   // --拼音码
       + " b.standard, "   //--规格
       + "b.small_unit, "  //  --单位
       + " b.ret_price  "  //  --价格
      + " from code_item a,plus_item b "
      + " where b.type = '3' and a.item_code = b.item_code  and a.disable = 'F' and a.item_cls <>1 and a.item_cls <>2 and a.item_cls <>3 and a.item_cls <>4 ) h where h.nh_code is null and rownum < 10";
                DataTable dt = DBUtil.queryExecute(sql);
                return dt;
            }
            catch (Exception ex)
            {
                XnhLogger.log("his药品数据提取：sql=" + sql + " 异常：" + ex.StackTrace);
            }
            return null;
        }
        ///以对吗
        public static DataTable fetchHIS2()
        {

            string sql = "";

            try
            {
                sql = "select * from (select a.item_code,  " //--his编码
                          + "a.item_name,  " // --his项目名称
       + "(select wydm from xnh_dm where xnh_dm.item_code = a.item_code) as nh_code, "  //  --农合中心编码
       + " (select ypmc from xnh_ypzl,xnh_dm where xnh_ypzl.wydm = xnh_dm.wydm and xnh_dm.item_code = a.item_code) as nh_name, "      //--农合名称
       + " a.item_cls, "    //--项目类别123药品，其他诊疗
       + " a.py_code, "   // --拼音码
       + " b.standard, "   //--规格
       + "b.small_unit, "  //  --单位
       + " b.ret_price  "  //  --价格
      + " from code_item a,plus_item b "
      + " where b.type = '3' and a.item_code = b.item_code) h where h.nh_code is not null";
                DataTable dt = DBUtil.queryExecute(sql);
                return dt;
            }
            catch (Exception ex)
            {
                XnhLogger.log("his药品数据提取：sql=" + sql + " 异常：" + ex.StackTrace);
            }
            return null;
        }
        ///提取农合药品编码
        /// </summary>
        public static DataTable fetchXNH(string ypdm)
        {

            string sql = "";
            try
            {
                sql ="select  cxgjz, " // --拼音码
       + "wydm, "   //--农合中心编码
       + " ypfl,"   //--药品分类
       + " ypmc, "  //--药品名称
       + " gg, "    //--规格
       + " dw, "    //--单位
       + " dj, "     //--单价
       + " bxbl, "  //--报销比例
       + " bnbw, "  //--1报内0保外
       + "jyl, "    //--甲乙类（0：甲类，1乙类，2：不区分）
       + "jyfjy "    //--基药非基药(0：基药 1：非基药)
       + " from xnh_ypzl a "
       + " where a.cxgjz like '%" + ypdm + "%' or a.wydm like '%" + ypdm + "%' or a.ypmc like '%" + ypdm + "%' ";
                DataTable dt = DBUtil.queryExecute(sql);
                return dt;
            }
            catch (Exception ex)
            {
                XnhLogger.log("农合药品数据提取：sql=" + sql + " 异常：" + ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        ///增加对码功能
        /// </summary>
        /// <param name="sqlParam"></param>
        public static void zjdmgn(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("bcZYBC sqlParam参数为空");
                return;
            }
           
            string sql = "insert into xnh_dm (item_code,wydm) values ('$HIS$','$NH$')";
            try
            {
                sql = sql.Replace("$HIS$", (sqlParam.ContainsKey("HIS") == true ? sqlParam["HIS"] : "")); 
                sql = sql.Replace("$NH$", (sqlParam.ContainsKey("NH") == true ? sqlParam["NH"] : "")); 

                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }
        

        /// <summary>
        ///增加取消对码功能
        /// </summary>
        /// <param name="sqlParam"></param>
        public static void qxdmgn(Dictionary<string, string> sqlParam)
        {
            if (sqlParam == null || sqlParam.Count == 0)
            {
                XnhLogger.log("bcZYBC sqlParam参数为空");
                return;
            }
            //UPDATE zybc SET D504_21=D504_21,D504_01=D504_01 WHERE D401_10 = D401_10 AND D401_21 = D401_21;
            string sql = "delete from xnh_dm  where  item_code = '$HIS$' and wydm = '$NH$' ";
            try
            {
                sql = sql.Replace("$HIS$", (sqlParam.ContainsKey("item_code") == true ? sqlParam["item_code"] : ""));
                sql = sql.Replace("$NH$", (sqlParam.ContainsKey("wydm") == true ? sqlParam["wydm"] : ""));

                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }
        ///住院发票打印
        /// </summary>
        public static DataTable printZYFP(string fpdy)
        {

            string sql = "";
            try
            {
                sql = "select a.name, "    //--姓名
                      + "a.ip_no, "      //--住院号
                      + "a.D505_02, "    //-- 住院登记流水号
                      + "a.TOTAL_COSTS, " // --住院总费用
                      + "a.TOTAL_CHAGE, "  //--住院可补偿金额
                      + "a.ZF_COSTS,  "   // --住院自费费用
                      + "b.area_code "   //--地区编码
                      + "from ZYJS a left join ZYBC b on a.ip_no = b.D504_01 where ip_no = '"+fpdy+"'"; 
                DataTable dt = DBUtil.queryExecute(sql);
                return dt;
            }
            catch (Exception ex)
            {
                XnhLogger.log("发票信息提取：sql=" + sql + " 异常：" + ex.StackTrace);
            }
            return null;
        }
        #endregion

       
    }
}