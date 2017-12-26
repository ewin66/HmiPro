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
            string[] machineName = new[] { "CA", "CB", "CC", "DA", "DB", "DE_DF", "DM", "ED", "EJ", "EK_EL", "EP", "RC_RF", "RG", "SA", "SB", "SI_SG", "TA_RE", "TB_TG", "VA_VC", "VB_VG", "VD", "VE_VF", "VH_VI", "VJ_VK","EB" };
            string[] netSegs = new[] { "192.168.130.66","192.168.131.66","192.168.132.66","192.168.200.66","192.168.201.66","192.168.110.66","192.168.202.66","192.168.180.66","192.168.183.66",
                "192.168.186.66","182.168.184.66","192.168.150.66","192.168.152.66","192.168.190.66","192.168.191.66","192.168.120.66","192.168.220.66","192.168.221.66",
                "192.168.139.66","192.168.140.66","192.168.142.66","192.168.144.66","192.168.146.66","192.168.149.66","1192.168.178.66"};
            int port = 8899;
            int i = 0;
            foreach (var ns in netSegs) {
                var url = $"http://{ns}:{port}";
                KsUpdateHttpGet(url, cmd, machineName[i++]);
            }
            YUtil.ExitWithQ();
        }

        static async void KsUpdateHttpGet(string url, string cmd, string macineName) {
            using (var client = new HttpClient()) {
                Console.WriteLine($"向 {macineName}:{url} 发出请求 ... {cmd}");
                try {
                    url += "/" + cmd;
                    var responseString = await client.GetStringAsync(url);
                    Console.WriteLine(macineName+":  "+responseString);
                } catch (Exception e) {
                    Console.WriteLine(macineName + " : " + e.Message);
                }
            }
        }
    }
}
