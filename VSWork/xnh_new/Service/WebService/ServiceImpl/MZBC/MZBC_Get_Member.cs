using Service.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.WebService.ServiceImpl.MZBC
{
    public class MZBC_Get_Member : ServiceBase
    {

        public MZBC_Get_Member()
        {
            this.sqlStr = "Get_Member";
        }
        
        /// <summary>
        /// 根据新医疗证号查询家庭成员
        /// </summary>
        /// <param name="SqlStr">Get_Member</param>
        /// <param name="parames">AREA_NO&D401_10 病人地区编码(取前台选择的地区编码)&医疗证号</param>
        /// <param name="split">分割符&</param>
        /// <returns></returns>

        //成功：S_Returns =0;家庭成员字符集
        //如 0;D401_21/D401_02;D401_21/D401_02
        //成员序号：D401_21  CHAR(2)
        //成员姓名：D401_02  VARCHAR2(24)
        //成员序号D401_21需要存储
        /// <summary>
        /// 覆盖接口的特殊实现
        /// </summary>
        /// <returns></returns>
        public new object getResponseResultOtherWrapper()
        {
            object retObj = null;
            if (this.getExecuteStatus() == true && this.executeResult.Length > 2)
            {
                //直接返回D401_21/D401_02;D401_21/D401_02
                return this.executeResult.Substring(2);
            }
            return retObj;
        }

    }
}
