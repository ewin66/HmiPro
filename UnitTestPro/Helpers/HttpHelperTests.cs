using Microsoft.VisualStudio.TestTools.UnitTesting;
using HmiPro.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            //PrintCardWithRfid pc = new PrintCardWithRfid() {
            //    macCode = "DA",
            //    type = MqRfidType.EmpStartMachine
            //};
            //IDictionary<string, string> dict = new Dictionary<string, string>();
            //dict[nameof(pc.rfid)] = "S80403000331";
            //dict[nameof(pc.type)] = pc.type;
            //dict[nameof(pc.macCode)] = pc.macCode;
            //var rep = await HttpHelper.Get(
            //    @"http://192.168.0.14:8080/mes/rest/mauEmployeeManageAction/saveMauEmployeeRecord", dict);
            //Console.WriteLine(rep);
        }

        [TestMethod()]
        public void TestFileExist() {
            var url = "http://192.168.0.15:9898/images/田书停.jpg";
            HttpWebResponse response = null;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";


            try {
                response = (HttpWebResponse)request.GetResponse();
            } catch (WebException ex) {
                /* A WebException will be thrown if the status of the response is not `200 OK` */
            } finally {
                // Don't forget to close your response.
                if (response != null) {
                    response.Close();
                }
            }
            Console.WriteLine(response?.StatusCode == HttpStatusCode.OK);
        }
    }
}