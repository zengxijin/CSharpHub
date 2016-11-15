using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace ApiMonitor.DataConvertUtil
{
    public class DataConvert
    {
        /// <summary>
        /// DataTable数据转为Json数组
        /// 方便前台JavaScript数据绑定
        /// </summary>
        /// <param name="dt">DataTable数据</param>
        /// <returns>JSON格式数组</returns>
        public static string DataTable2Json(DataTable dt)
        {
            string retVal = "";

            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{\"data\":[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(dt.Columns[j].ColumnName);
                    jsonBuilder.Append("\":\"");
                    jsonBuilder.Append(dt.Rows[i][j].ToString());
                    jsonBuilder.Append("\",");
                }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]");
            jsonBuilder.Append("}");

            retVal = jsonBuilder.ToString();

            return retVal;
        }
    }
}