using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Windows;
using HmiPro.Redux.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using YCsharp.Util;

namespace UnitTestPro {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void TestMethod1() {
            var source = new Subscriber2();
            source.Test();
        }
        [TestMethod()]
        public void javaTimeTest() {
            Console.WriteLine(YUtil.UtcTimestampToLocalTime(1524712304000));
        }

        [TestMethod()]
        public void desJsonTest()
        {
            string json = "";
            var pcs = JsonConvert.DeserializeObject<ObservableCollection<MqSchTask>>(json);
            Console.WriteLine(pcs.Count);
        }
    }

    public class Subscriber1 {
        public Action<string> actions;

        public Action AddListener(Action<string> action) {
            actions += action;
            return () => {
                actions -= action;
            };
        }

        public void Raise(string message) {
            actions?.Invoke(message);
        }

        public void Test() {
            var rmLsr = AddListener(msg => {
                Console.WriteLine("test received message: " + msg);
            });
            Raise("Hello world");
            rmLsr();
            Raise("Goodbay");
        }
    }

    /// <summary>
    /// event 模式的事件源
    /// </summary>
    public class SubEventSource : EventSource {
        public event Action<object, SubEventArgs> Event;

        public void Raise(SubEventArgs args) {
            Event?.Invoke(this, args);
        }
    }

    /// <summary>
    /// event 模式的事件参数
    /// </summary>
    public class SubEventArgs : EventArgs {
        public object Data;
    }

    public class Subscriber2 {
        /// <summary>
        /// 事件源
        /// </summary>
        private readonly SubEventSource evnetSource = new SubEventSource();
        /// <summary>
        /// 添加订阅，可不比手动取消
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public Action AddListener(EventHandler<SubEventArgs> handler) {
            WeakEventManager<SubEventSource, SubEventArgs>.AddHandler(evnetSource, nameof(evnetSource.Event), handler);
            //手动取消订阅
            return () => {
                WeakEventManager<SubEventSource, SubEventArgs>.RemoveHandler(evnetSource, nameof(evnetSource.Event),
                    handler);
            };
        }
        /// <summary>
        /// 测试自动取消订阅
        /// </summary>
        public void Test() {
            var handler = new TestHandler();
            AddListener(handler.Handle);
            evnetSource.Raise(new SubEventArgs() { Data = "hello" });
            handler = null;
            //垃圾回收
            GC.Collect();
            GC.WaitForFullGCComplete();
            evnetSource.Raise(new SubEventArgs() { Data = "world" });
            //output:
            //hello
        }

        /// <summary>
        /// 事件处理方
        /// </summary>
        public class TestHandler {
            public void Handle(object sender, SubEventArgs args) {
                Console.WriteLine(args?.Data?.ToString());
            }
        }

    }



}

