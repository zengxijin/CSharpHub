using ApiMonitor.log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

namespace ApiMonitor.DB
{
    public class DBUtil
    {
        //连接数据库格式（无需配置tnsnames.ora）：
        //User ID=IFSAPP;Password=IFSAPP;Data Source=(DESCRIPTION = (ADDRESS_LIST= (ADDRESS = (PROTOCOL = TCP)(HOST = 127.0.0.1)(PORT = 1521))) (CONNECT_DATA = (SERVICE_NAME = RACE)));
        private static string oracleConnString = ConfigurationManager.AppSettings["oracleConnection"];
        /// <summary>
        /// 数据库查询
        /// </summary>
        /// <param name="querySql">查询SQL</param>
        /// <returns>DataTable</returns>
        public static DataTable queryExecute(string querySql)
        {
            OracleConnection conn = null;
            OracleCommand    cmd  = null;
            try
            {
                conn = new OracleConnection(oracleConnString);
                conn.Open();
                cmd = new OracleCommand(querySql, conn);
                OracleDataAdapter oda = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                oda.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                XnhLogger.log("DBUtil.queryExecute" + ex.ToString());
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return null;
        }

        /// <summary>
        /// 更新数据库操作
        /// </summary>
        /// <param name="updateSql">更新SQL</param>
        /// <returns>修改的行数</returns>
        public static int updateExecute(string updateSql)
        {
            OracleConnection conn = null;
            OracleCommand    cmd  = null;
            try
            {
                conn = new OracleConnection(oracleConnString);
                conn.Open();
                cmd = new OracleCommand(updateSql, conn);
                return cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                XnhLogger.log("DBUtil.updateExecute" + ex.ToString());
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return -1;

        }

    }
}