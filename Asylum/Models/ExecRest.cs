using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asylum.Models {
    /// <summary>
    /// 命令执行后得到的数据
    /// <author>ychost</author>
    /// <date>2018-2-9</date>
    /// </summary>
    public class ExecRest {
        /// <summary>
        /// 执行结果编码
        /// </summary>
        public ExecCode Code { get; set; } = ExecCode.Unknown;
        /// <summary>
        /// 执行结果参考消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 执行结果附加数据
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 调试的时候附加的一些消息
        /// </summary>
        public string DebugMessage { get; set; }
        /// <summary>
        /// 发送的时间
        /// </summary>
        public long? SendTime { get; set; }
    }

    public enum ExecCode {
        Unknown = -1,
        Ok = 1,
        NotFoundAction = 1001,
        NotFoundType = 1002,
        MapManyTypes = 1003,

        StartHmiProFailed = 2001,
        CloseHmiProFailed = 2002,

        ExecFailed = 3001,

        FormatError = 4001
    }
}
