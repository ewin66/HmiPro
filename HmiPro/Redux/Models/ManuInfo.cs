using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 生产过程中的属性
    /// <author>ychost</author>
    /// <date>2018-2-1</date>
    /// </summary>
    public class ManuInfo : MongoDoc {
        /// <summary>
        /// 机台编码
        /// </summary>
        public string MachineCode { get; set; }
        /// <summary>
        /// 放线轴
        /// </summary>
        public HashSet<string> StartAxisList { get; set; }
        /// <summary>
        /// 收线轴
        /// </summary>
        public HashSet<string> EndAxisList { get; set; }
        /// <summary>
        /// 操作人员
        /// </summary>
        public HashSet<MqEmpRfid> EmpList{ get; set; }
        /// <summary>
        /// 任务开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 任务结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

    }
}
