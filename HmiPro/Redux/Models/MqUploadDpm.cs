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
        public string commType { get; set; } = "setParam";
        public string macCode { get; set; }
        public string proGgxh { get; set; }
        public Dictionary<string,string> paramJson { get; set; }
    }
}
