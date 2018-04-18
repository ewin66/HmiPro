using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 生产属性，含机台，Rfid，轴号，人员，od，米数等等
    /// 实现未排产的状态质量回溯
    /// <date>2018-4-18</date>
    /// <author>ychost</author>
    /// </summary>
    public class ProdProp : MongoDoc {
        /// <summary>
        /// 机台编码
        /// </summary>
        public string MahcineCode { get; set; }

        /// <summary>
        /// 操作人员 Rfid
        /// </summary>
        public HashSet<string> EmpRfids { get; set; }

        /// <summary>
        /// 放线盘，可能有多个
        /// </summary>
        public HashSet<string> StartRfids { get; set; }

        /// <summary>
        /// 收线盘，只能有一个
        /// </summary>
        public string EndRfid { get; set; }

        /// <summary>
        /// 这轴线开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 这轴线结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

    }
}
