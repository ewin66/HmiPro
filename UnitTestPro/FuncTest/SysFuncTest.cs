using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
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
            var task = Task.Run(() => {Thread.Sleep(900);
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

    }
}
