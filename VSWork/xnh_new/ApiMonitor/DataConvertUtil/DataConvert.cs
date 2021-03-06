﻿using System;
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
                str_temp = "{\"flag\":\"" + flag + "\",\"msg\":" + msgTemp + "}"; //msgTemp是json字符串的情况
            else
                str_temp = "{\"flag\":\"" + flag + "\",\"msg\":\"" + msgTemp + "\"}"; //msgTemp是值的情况

            return str_temp;
        }

        /// <summary>
        /// 将Dictionary<string,string>转为json格式输出，方便前台使用
        /// </summary>
        /// <param name="srcDict">输入Dictionary<string,string></param>
        /// <returns>json字符串</returns>
        public static string Dict2Json(Dictionary<string, string> srcDict)
        {
            if (srcDict != null && srcDict.Count > 0)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(srcDict);
            }

            return "";
        }

        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="src">明文</param>
        /// <returns></returns>
        public static string Base64Encode(string src)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(src);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="src">密文</param>
        /// <returns></returns>
        public static string Base64Decode(string src)
        {
            byte[] outputb = Convert.FromBase64String(src);
            return Encoding.UTF8.GetString(outputb);
        }
    }
}