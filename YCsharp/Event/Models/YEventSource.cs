using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCsharp.Event.Models {
    /// <summary>
    /// 事件源
    /// <author>ychost</author>
    /// <date>2018-2-10</date>
    /// </summary>
    public class YEventSource : EventSource {
        /// <summary>
        /// 触发的事件
        /// </summary>
        public event Action<object, YEventArgs> Event;
        /// <summary>
        /// 调用者的信息
        /// </summary>
        public StackFrame CallFrame;

        public YEventSource() {
        }

        public void RaiseEvent(YEventArgs args) {
            Event?.Invoke(this, args);
        }
    }
}
