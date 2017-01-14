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

        public static string getReturnJson(string flag, string msg)
        {
            string msgTemp = msg;
            if (!string.IsNullOrEmpty(msgTemp))
            {
                msgTemp = msgTemp.Replace(System.Environment.NewLine, ""); //防止回车符导致JSON无法解析
            }

            //return "{\"flag\":\"" + flag + "\",\"msg\":\"" + msgTemp + "\"}";

            string str_temp;

            if (msgTemp.Substring(0, 1) == "{")
                str_temp = "{\"flag\":\"" + flag + "\",\"msg\":" + msgTemp + "}";
            else
                str_temp = "{\"flag\":\"" + flag + "\",\"msg\":\"" + msgTemp + "\"}";

            return str_temp;
        }
    }
}