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
        /// 更新住院流水号
        /// </summary>
        /// <param name="sqlParam"></param>
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

            string sql = "insert into zybc " +
                    "select '$D401_10$','$D401_21$','$D504_01$','$D504_21$' from dual "
                    + "where not exists (select * from zybc t where t.D401_10='$D401_10$' and t.D401_21='$D401_21$' )";
            try
            {
                sql = sql.Replace("$D401_10$", (sqlParam.ContainsKey("D401_10") == true ? sqlParam["D401_10"] : ""));
                sql = sql.Replace("$D401_21$", (sqlParam.ContainsKey("D401_21") == true ? sqlParam["D401_21"] : ""));
                sql = sql.Replace("$D504_21$", (sqlParam.ContainsKey("D504_21") == true ? sqlParam["D504_21"] : ""));
                sql = sql.Replace("$D504_01$", (sqlParam.ContainsKey("D504_01") == true ? sqlParam["D504_01"] : ""));

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
            string sql = "UPDATE zybc t SET t.D504_21='$D504_21$',t.D504_01='$D504_01$' WHERE t.D401_10 = '$D401_10$' AND t.D401_21 = '$D401_21$'";
            try
            {
                sql = sql.Replace("$D401_10$", (sqlParam.ContainsKey("D401_10") == true ? sqlParam["D401_10"] : ""));
                sql = sql.Replace("$D401_21$", (sqlParam.ContainsKey("D401_21") == true ? sqlParam["D401_21"] : ""));
                sql = sql.Replace("$D504_21$", (sqlParam.ContainsKey("D504_21") == true ? sqlParam["D504_21"] : ""));
                sql = sql.Replace("$D504_01$", (sqlParam.ContainsKey("D504_01") == true ? sqlParam["D504_01"] : ""));

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
                sql = sql.Replace("$D504_21$", (sqlParam.ContainsKey("D504_21") == true ? sqlParam["D504_21"] : ""));

                DBUtil.updateExecute(sql);
            }
            catch (Exception ex)
            {
                XnhLogger.log(ex.ToString() + " SQL:" + sql);
            }
        }


        #endregion
    }
}