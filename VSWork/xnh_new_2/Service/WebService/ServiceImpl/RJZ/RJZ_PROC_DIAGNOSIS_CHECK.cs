
using Service.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.WebService.ServiceImpl.RJZ
{
    public class RJZ_PROC_DIAGNOSIS_CHECK : ServiceBase
    {
        public RJZ_PROC_DIAGNOSIS_CHECK()
        {
            this.sqlStr = "PROC_DIAGNOSIS_CHECK";
        }
        /// <summary>
        /// 验证出院诊断是否在疾病库中存在
        /// </summary>
        /// <param name="SqlStr">PROC_DIAGNOSIS_CHECK</param>
        /// <param name="parames">DIAGNOSIS_CODE 疾病代码</param>
        /// <param name="split">分割符&</param>
        /// <returns></returns>

        // 0	成功
        // 1	此疾病在疾病库中不存在
        // 2	程序异常
        // 程序异常：  S_Returns =1;错误信息（分号分隔）


    }
}
 