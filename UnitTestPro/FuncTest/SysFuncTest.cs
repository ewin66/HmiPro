using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestPro.FuncTest {
    /// <summary>
    /// 系统功能函数测试
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    [TestClass]
    public class SysFuncTest {
        [TestMethod]
        public void RefTest() {
            string apple = "apple";
            string appleRef = apple;
            apple = "苹果";
            Console.WriteLine(appleRef);
            TestClass test = new TestClass() { Name = "origin struct" };
            var testRef = test;
            test = new TestClass() { Name = "changed struct" };
            Console.WriteLine(testRef.Name);

            TestStruct tStruct = new TestStruct() { Name = "origin struct", TestClass = new TestClass() };
            var refStruct = tStruct;
            refStruct.Name = "changed struct";
            refStruct.TestClass = new TestClass();
            Console.WriteLine(tStruct.TestClass.Name);
        }

        [TestMethod]
        public void ObservableCollectionRemoveTest() {
            ObservableCollection<TestClass> TestObs = new ObservableCollection<TestClass>();
            for (int i = 0; i < 20; i++) {
                TestObs.Add(new TestClass() { Name = i.ToString() });
            }
            var removeList = TestObs.Where(t => int.Parse(t.Name) < 7).ToList();
            foreach (var item in removeList) {
                TestObs.Remove(item);
            }
            Assert.AreEqual(TestObs.Count, 13);
            var removeList2 = TestObs.Where(t => int.Parse(t.Name) < -1).ToList();
            foreach (var item in removeList2) {
                TestObs.Remove(item);
            }
            TestObs.Add(null);
            TestObs.Add(null);
            Assert.AreEqual(TestObs.Count, 15);
            TestObs[0] = null;
            TestObs.Remove(null);
            TestObs.Remove(null);
            TestObs.Remove(null);
            TestObs.Remove(null);
            TestObs.Remove(null);

            Assert.AreEqual(TestObs.Count, 12);
        }

        [TestMethod]
        public void FirstOrSignleTest() {
            List<TestClass> TestObs = new List<TestClass>() { new TestClass() { Name = "1" }, new TestClass() { Name = "2" } };
            var test = TestObs.FirstOrDefault(s => s.Name == "2");
            Assert.AreEqual(test.Name, "2");
            var test2 = TestObs.FirstOrDefault(s => s.Name == "3");
            Assert.AreEqual(test2, null);
        }

        public static object LockObj = new Object();
        [TestMethod]
        public void LockTest() {
            lock (LockObj) {
                Task.Run(() => {
                    lock (LockObj) {
                        Console.WriteLine("进入 New Thread LockTest");
                        LockTestCall();
                    }
                });
                Thread.Sleep(200);
                Console.WriteLine("进入 LockTest");
                LockTestCall();
            }
        }

        public void LockTestCall() {
            lock (LockObj) {
                Console.WriteLine("进入 Lock Test Call");
            }

        }

        [TestMethod]
        public void AddRemoveInManyListTest() {
            TestClass o1 = new TestClass() { Name = "1" };
            TestClass o2 = new TestClass() { Name = "2" };
            ObservableCollection<TestClass> list1 = new ObservableCollection<TestClass>() { o1, o2 };
            ObservableCollection<TestClass> list2 = new ObservableCollection<TestClass>();
            foreach (var t in list1) {
                list2.Add(t);
            }
            var removeItem = list1.FirstOrDefault(t => t.Name == "1");
            list1.Remove(removeItem);
            Assert.AreEqual(list1.Count, 1);
            list2.Remove(removeItem);
            Assert.AreEqual(list2.Count, 1);
        }

        [TestMethod]
        public void ConvertTest() {
            object zero = 0.357;
            var zeroF = float.Parse(zero.ToString());
            Console.WriteLine(zeroF);
        }

        [TestMethod]
        public void ReplaceTest() {
            string str = "/////fsfsf";
            var rpStr = str.Replace("/", "");
            Assert.AreEqual(rpStr, "fsfsf");
        }

        [TestMethod]
        public void PingTest() {
            Ping ping = new Ping();
            var ret = ping.Send("192.168.88.15", 1000);
            Console.WriteLine(ret.Status);
        }

        [TestMethod]
        public void EnumTest() {
            object e = TestEnum.Hello;
            Console.Write((TestEnum)e);
        }

        [TestMethod]
        public async Task TaskTest() {
            var task = Task.Run(() => {
                Thread.Sleep(900);
                Console.WriteLine("执行完毕");
            });

            if (await Task.WhenAny(task, Task.Delay(1000)) == task) {
                Console.WriteLine("在1s之内完成");
            } else {
                Console.WriteLine("超时1s");
            }
        }

        public enum TestEnum {
            Hello = 1,
            World = 2
        }

        [TestMethod]
        public void TryFinallyTest() {
            TestClass test() {
                TestClass testObj = new TestClass();
                try {
                    testObj.Name = "hello";
                    return testObj;
                } finally {
                    testObj.Name = "world";
                }
            }

            Console.WriteLine(test().Name);
        }

        [TestMethod]
        public void LockDictTest() {
            IDictionary<string, object> lockDict = new ConcurrentDictionary<string, object>();
            for (int i = 0; i < 10; i++) {
                lockDict[i.ToString()] = new object();
            }

            Task.Run(() => {
                lock (lockDict["1"]) {
                    Thread.Sleep(1000);
                    Console.WriteLine("Task1 Completed");
                }
            });

            Task.Run(() => {
                lock (lockDict["1"]) {
                    Console.WriteLine("Task2.Completed");
                }
            });

            Thread.Sleep(1003);
        }

        [TestMethod]
        public void FloatTest() {
            float? a = 1;
            float? b = 1;
            float? c = a * b;
            Assert.AreEqual(c, 1);
            Console.WriteLine(a);
        }

        [TestMethod]
        public void FinallyTest() {
            int test() {
                try {
                    throw new Exception("haha");
                    return 0;
                } catch {
                    return -1;
                } finally {
                    Console.WriteLine("Enter Finally");
                }
                return 3;
            }

            var s = test();
            Assert.AreEqual(s, -1);
        }

        [TestMethod]
        public void LockTest2() {
            object lockObj = new object();

            int monitorTest() {
                if (Monitor.TryEnter(lockObj)) {
                    try {
                        Console.WriteLine("MonitorTest in");
                        return 3;
                    } finally {
                        Monitor.Exit(lockObj);
                    }
                }
                return 4;
            }

            if (Monitor.TryEnter(lockObj)) {
                try {
                    Console.WriteLine("lockin");
                    var s = monitorTest();
                    Assert.AreEqual(s, 3);
                } finally {
                    Monitor.Exit(lockObj);
                }
            }
        }

        [TestMethod]
        public void GenericTest() {
            List<int> intList = new List<int>();
            intList.Add(1);
            var objList = intList.OfType<object>().ToList();
            objList.Add("hello");
            objList.Add(2);
            int a = 2;
            switch (a) {
                case 1:
                    Console.WriteLine("ll");
                    break;
                default:
                    break;
            }

            objList.ForEach(Console.Write);
            Console.WriteLine();
            intList.ForEach(Console.Write);
        }

        [TestMethod]
        public void ExtendTest() {
            Father father = new Father();
            Son son = new Son();
            father.DoVirtualMethod(); father.DoNormalMethod();
            father.PrintFather();

            son.DoVirtualMethod();
            son.DoNormalMethod();
            son.PrintSon();
            son.PrintFather();

            Father father2 = son;
            father2.PrintFather();
            father2.DoNormalMethod();
        }

        [TestMethod]
        public async Task LockDeadTest() {
            object locker = new object();
            object locker2 = new object();

            async Task<Boolean> timeConsume() {
                Console.WriteLine("time consume in");
                return await Task.Run(() => {
                    lock (locker) {
                        Thread.Sleep(1000);
                        Console.WriteLine("time consume execing");
                        Thread.Sleep(1000);
                        Console.WriteLine("time consume completed");
                        return true;
                    }
                });
            }

            async Task dispatch() {
                lock (locker2) {
                    timeConsume();
                }
            }

            for (int i = 0; i < 3; i++) {
                await Task.Run(() => {
                    lock (locker) {
                        dispatch();
                    }
                });
            }

            Thread.Sleep(9000);

        }

        [TestMethod]
        public void SmtcProtocolTest() {
            var data = 0xff & 0x03;
            Assert.AreEqual(data, 3);
            data = (0xff & 0x0c) >> 2;
            Assert.AreEqual(data, 3);
            data = (0xff & 0x30) >> 4;
            Assert.AreEqual(data, 3);
            data = (0xff >> 4) & 0x03;
            Assert.AreEqual(data, 3);
        }

    }


    public class Father {
        public string PubField;
        private string priField;

        public virtual void DoVirtualMethod() {
            PubField = "father virual public";
            priField = "father virtual private field";
            Console.WriteLine("father virtual method");
        }

        public void DoNormalMethod() {
            PubField = "father normal public";
            priField = "father normal private";
            Console.WriteLine("father normal method");
        }

        public string GetFatherPrivate() {
            return this.priField;
        }

        public void PrintFather() {
            Console.WriteLine($"Father: public:{PubField} ,private:{priField}");
        }
    }

    public class Son : Father {
        public string PubField;
        private string priField;
        public override void DoVirtualMethod() {
            PubField = "son override public";
            priField = " son override private";
            Console.WriteLine("son override method");
        }

        public new void DoNormalMethod() {
            PubField = "son new public";
            priField = "son new private";
            Console.WriteLine("son new method");
        }

        public string GetSonPrivate() {
            return priField;
        }

        public void PrintSon() {
            Console.WriteLine($"Son: public:{PubField} ,private:{priField}");
        }
    }

}
