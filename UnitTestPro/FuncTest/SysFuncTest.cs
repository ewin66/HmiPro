using System;
using System.Collections.Generic;
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
    }
}
