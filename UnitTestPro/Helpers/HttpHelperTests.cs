using Microsoft.VisualStudio.TestTools.UnitTesting;
using HmiPro.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;
using Newtonsoft.Json;

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

        [TestMethod()]
        public async void PostTest() {
            PrintCardWithRfid pc = new PrintCardWithRfid() {
                macCode = "DA",
                type = MqRfidType.EmpStartMachine
            };
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict[nameof(pc.rfid)] = "S80403000331";
            dict[nameof(pc.type)] = pc.type;
            dict[nameof(pc.macCode)] = pc.macCode;
            var rep = await HttpHelper.Get(
                @"http://192.168.0.14:8080/mes/rest/mauEmployeeManageAction/saveMauEmployeeRecord", dict);
            Console.WriteLine(rep);
        }
    }
}