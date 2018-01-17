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
    public class MqUploadDpm {
        public string macCode { get; set; }
        public string proGgxh { get; set; }
        public string paramName { get; set; }
        public string paramValue { get; set; }
    }
}
