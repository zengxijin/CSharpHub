using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Text;

namespace DataImport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string filePath = ConfigurationManager.AppSettings["filePath"];
                string sheetName = ConfigurationManager.AppSettings["sheetName"];

                NPOIUtil npoi = new NPOIUtil();
                DataTable dt = npoi.ExcelToDataTable(filePath, sheetName, true);

                string insertSql = "insert into xnl_sys_cfg(id,cfg_key,cfg_val,cfg_type,create_time) values(seq_xnl_sys_cfg.nextval,'{0}','{1}','{2}',sysdate)";

                int count = 0;

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string key = dr["key"] as string;
                        string value = dr["value"] as string;
                        string type = dr["type"] as string;

                        string sql = string.Format(insertSql, key, value, type);

                        updateExecute(sql);
                        count++;
                        Console.WriteLine("已成功导入" + count + " " + key);
                    }
                    Console.WriteLine("共导入" + count + "条");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误：" + ex.ToString());
            }

            Console.WriteLine("请输入任意键回车结束");
            Console.ReadKey();

        }

        public static int updateExecute(string updateSql)
        {
            string oracleConnString = ConfigurationManager.AppSettings["oracleConnection"];
            OracleConnection conn = null;
            OracleCommand cmd = null;
            try
            {
                conn = new OracleConnection(oracleConnString);
                conn.Open();
                cmd = new OracleCommand(updateSql, conn);
                return cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
