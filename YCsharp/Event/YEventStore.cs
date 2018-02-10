using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NPOI.HSSF.Util;
using YCsharp.Event.Models;

namespace YCsharp.Event {
    public delegate void Unsubscibe();

    /// <summary>
    /// 事件总线模式的一种实现
    /// [x] 使用的是 WeakEvent ，支持（手动/自动）取消订阅
    /// [x] 所有操作都是同步的，所以注意每个操作不要耗费太多事件
    /// <author>ychost</author>
    /// <date>2018-2-10</date>
    /// </summary>
    public class YEventStore {
        /// <summary>
        /// 所有操作上的锁
        /// </summary>
        private readonly object locker = new object();
        /// <summary>
        /// 所有事件源代
        /// </summary>
        private readonly IDictionary<Type, YEventSource> eventSource;
        /// <summary>
        /// 最近 Dispatch 的对象
        /// </summary>
        private object latestPayload;
        /// <summary>
        /// 初始化
        /// </summary>
        public YEventStore() {
            eventSource = new Dictionary<Type, YEventSource>();
        }

        /// <summary>
        /// 派遣事件
        /// </summary>
        /// <param name="payload"></param>
        public void Dispatch(object payload) {
            if (payload == null) {
                return;
            }
            var type = payload.GetType();
            lock (locker) {
                latestPayload = payload;
                if (eventSource.TryGetValue(type, out var source)) {
                    StackTrace trace = new StackTrace();
                    StackFrame frame = trace.GetFrame(1);
                    source.CallFrame = frame;
                    source.RaiseEvent(new YEventArgs(payload));
                }
            }
        }

        /// <summary>
        /// 单一订阅
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="handler"></param>
        /// <param name="useLatestPayload"></param>
        /// <returns></returns>
        public Unsubscibe Subscribe(Type actionType, EventHandler<YEventArgs> handler, bool useLatestPayload = true) {
            lock (locker) {
                if (!eventSource.ContainsKey(actionType)) {
                    eventSource[actionType] = new YEventSource();
                }
                var source = eventSource[actionType];
                WeakEventManager<YEventSource, YEventArgs>.AddHandler(source, nameof(YEventSource.Event), handler);
                if (useLatestPayload && latestPayload?.GetType() == actionType) {
                    handler?.Invoke(new YEventSource() { CallFrame = new StackTrace().GetFrame(0) }, new YEventArgs(latestPayload));
                }
            }
            return () => {
                lock (locker) {
                    var source = eventSource[actionType];
                    WeakEventManager<YEventSource, YEventArgs>.RemoveHandler(source, nameof(YEventSource.Event), handler);
                }
            };
        }

        /// <summary>
        /// 集合订阅
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="useLatestPayload"></param>
        /// <returns></returns>
        public Unsubscibe Subscribe(IDictionary<Type, EventHandler<YEventArgs>> handlers, bool useLatestPayload = true) {
            lock (locker) {
                foreach (var pair in handlers) {
                    var actionType = pair.Key;
                    var handler = pair.Value;
                    if (!eventSource.ContainsKey(actionType)) {
                        eventSource[actionType] = new YEventSource();
                    }
                    var source = eventSource[actionType];
                    WeakEventManager<YEventSource, YEventArgs>.AddHandler(source, nameof(YEventSource.Event), handler);
                    if (useLatestPayload && latestPayload?.GetType() == actionType) {
                        handler?.Invoke(this, new YEventArgs(latestPayload));
                    }
                }
            }
            return () => {
                lock (locker) {
                    foreach (var pair in handlers) {
                        var source = eventSource[pair.Key];
                        WeakEventManager<YEventSource, YEventArgs>.RemoveHandler(source, nameof(YEventSource.Event), pair.Value);
                    }
                }
            };
        }
      
    }
}
