using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asylum.Models {
    /// <summary>
    /// 命令格式
    /// </summary>
    public class Cmd {
        /// <summary>
        /// 动作
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public object Args { get; set; }
        /// <summary>
        /// Cmd 是从哪里发出来的
        /// </summary>
        public CmdWhere Where { get; set; } = CmdWhere.Unknown;
        /// <summary>
        /// 执行时间
        /// </summary>
        public long? ExecTime { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public long? SendTime { get; set; }
    }

    public enum CmdWhere {
        FromHttp,
        FromMq,
        FromTcp,
        FromUdp,
        Unknown
    }
}
