using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 上传电能
    /// <author>ychost</author>
    /// <date> 2018-4-13</date>
    /// </summary>
    public class MqUploadElec {
        /// <summary>
        /// 总电能
        /// </summary>
        public float elec { get; set; }
        /// <summary>
        /// 操作手
        /// </summary>
        public string employees { get; set; }
        /// <summary>
        /// 机台编码
        /// </summary>
        public string machinecode { get; set; }
        /// <summary>
        /// 工单号
        /// </summary>
        public string workcoder { get; set; }
    }
}
