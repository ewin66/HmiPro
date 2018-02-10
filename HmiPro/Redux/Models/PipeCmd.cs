using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Util;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 往 Pipe 里面发送的命令
    /// <author>ychost</author>
    /// <date>2018-2-10</date>
    /// </summary>
    public class PipeCmd {
        /// <summary>
        /// 命令格式
        /// </summary>
        /// <summary>
        /// 动作
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public object Args { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public long? ExecTime { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public long? SendTime { get; set; }

        public PipeCmd() {
            SendTime = YUtil.GetUtcTimestampMs(DateTime.Now);
        }
    }
}
