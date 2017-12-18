using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YCsharp.Util;

namespace UnitTestPro.FuncTest {
    [TestClass]
    public class ReflectTest {
        public class BaseClass {
            private readonly string privateField;
            public string publicField;

            public BaseClass() {
                privateField = "base private";
                publicField = "base public";
            }
        }

        public class DerivedClass : BaseClass {
            private string privateField;
            public new string publicField;
            public DerivedClass() {
                privateField = "derived private";
                publicField = "derived public";

                Console.WriteLine("父类privateField: " + this.GetPrivateField<string>("privateField", typeof(BaseClass)));

                Console.WriteLine("父类publicFiled: " + base.publicField);
                Console.WriteLine("子类publicFiled: " + this.publicField);

            }
        }

        [TestMethod]
        public void GetBaseClassPrivateTest() {
            var der = new DerivedClass();

        }

    }
}
