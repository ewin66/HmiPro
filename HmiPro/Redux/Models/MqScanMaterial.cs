using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    public class MqScanMaterial {
        /// <summary>
        /// 最终结果
        /// </summary>
        public bool type { get; set; }
        /// <summary>
        /// 最后的原因
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 规格型号：重量
        /// </summary>
        public Dictionary<string, double> materMap { get; set; }
        /// <summary>
        /// 规格型号：用量
        /// </summary>
        public Dictionary<string, double> taskMaterMap { get; set; }
        /// <summary>
        /// 中途出现的小问题
        /// </summary>
        public List<string> msgList { get; set; }
    }
}
