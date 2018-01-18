using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 上传回填参数
    /// <author>ychot</author>
    /// <date>2018-1-17</date>
    /// </summary>
    public class MqUploadDpm : MqUploadRest {
        public string commType { get; set; } = "getParam";
        public string macCode { get; set; }
        public string proGgxh { get; set; }
        public List<DpmUpload> paramJson { get; set; }
    }

    public class DpmUpload {
        /** 机台code */
        public String macCode { get; set; }
        /** 产品规格 */
        public String proGgxh { get; set; }
        /** 参数名称 */
        public String paramName { get; set; }
        /** 设定值 */
        public String paramValue { get; set; }
    }


}
