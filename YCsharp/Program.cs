using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Util;

namespace YCsharp {
    /// <summary>
    /// <date>2017-09-27</date>
    /// <author>ychost</author>
    /// <detail>
    ///     这是在做Csharp项目的时候总结的一些库
    /// </detail>
    /// </summary>
    public class Program {
        static void Main(string[] args) {
            if (args?.Length == 0) {
                Console.WriteLine("默认为更新命令 update-app");
                args = new string[] { "update-app" };
            }
            var cmd = args[0];
            string[] machineName = new[] { "DE", "DA",  "VB",  "ED",  "RC_RF", "SI", "VA",  "DM", "SB" };
            string[] netSegs = new[] {    "110", "200", "140", "180", "150",  "120", "139", "202", "191" };
            int port = 8899;
            int i = 0;
            foreach (var ns in netSegs) {
                var url = $"http://192.168.{ns}.66:{port}";
                KsUpdateHttpGet(url, cmd, machineName[i++]);
            }
            KsUpdateHttpGet("http://192.168.0.2:8899", "update-app", "da");
            YUtil.ExitWithQ();
        }

        static async void KsUpdateHttpGet(string url, string cmd, string macineName) {
            using (var client = new HttpClient()) {
                Console.WriteLine($"向 {macineName}:{url} 发出请求 ... {cmd}");
                try {
                    url += "/" + cmd;
                    var responseString = await client.GetStringAsync(url);
                    Console.WriteLine(responseString);
                } catch (Exception e) {
                    Console.WriteLine(macineName + " : " + e.Message);
                }
            }
        }
    }
}
