﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
            Assert.AreEqual(TestObs.Count, 14);
            TestObs.Remove(null);
            TestObs.Remove(null);
            TestObs.Remove(null);
            TestObs.Remove(null);
            Assert.AreEqual(TestObs.Count, 13);
        }

        [TestMethod]
        public void FirstOrSignleTest() {
            List<TestClass> TestObs = new List<TestClass>() { new TestClass() { Name = "1" }, new TestClass() { Name = "2" } };
            var test = TestObs.FirstOrDefault(s => s.Name == "2");
            Assert.AreEqual(test.Name,"2");
            var test2 = TestObs.FirstOrDefault(s => s.Name == "3");
            Assert.AreEqual(test2,null);
        }
    }
}
