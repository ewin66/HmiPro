using Microsoft.VisualStudio.TestTools.UnitTesting;
using HmiPro.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Helpers.Tests {
    [TestClass()]
    public class HttpHelperTests {
        [TestMethod()]
        public void UpdateGrafanaTest() {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict["mac_code"] = "DA";
            dict["oee_or"] = "0.23";
            Console.WriteLine(HttpHelper.UpdateGrafana(dict, "DA"));
        }
    }
}