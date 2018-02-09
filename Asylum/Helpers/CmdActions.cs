using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asylum.Helpers {
    /// <summary>
    /// Cmd 辅助
    /// <author>ychost</author>
    /// <date>2018-2-9</date>
    /// </summary>
    public static class CmdActions {

        /// <summary>
        /// 启动 HmiPro 软件
        /// </summary>
        public class StartHmiPro {
            /// <summary>
            /// 是否强制启动（如果 HmiPro 存在，先关闭进程再启动进程）
            /// </summary>
            public bool IsForced { get; set; }
            /// <summary>
            /// 启动的参数
            /// </summary>
            public string StartArgs { get; set; }
        }

        /// <summary>
        /// 关闭 HmiPro 软件
        /// </summary>
        public class CloseHmiPro {
            /// <summary>
            /// 是否强制关闭
            /// 强制关闭：直接 kill 进程
            /// 普通关闭：发送消息给 HmiPro 让它自己关闭
            /// </summary>
           public bool IsForced { get; set; }
        }

        /// <summary>
        /// 获取 HmiPro 的状态
        /// </summary>
        public class GetHmiProStatus {
            
        }
        
    }
}
