using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
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
                args = new string[] { "StartHmiPro" };
            }
            var cmd = new Cmd();
            cmd.Action = args[0];
            cmd.Args = new { IsForced = true };

            string[] hmi = new[] { "CA", "CB", "CC", "DA", "DB", "DE_DF", "DM", "ED", "EJ", "EL", "EP", "RC_RF", "RG", "SA", "SB_SD", "SI_SG_BB", "TA_RE", "TB_TG", "VA_VC", "VB_VG", "VD", "VE_VF", "VH_VI", "VJ_VK", "EB", "CD", "MB", "PB", "MA", "E2", "E1", "EC", "SC", "EK" };
            string[] ips = new[] { "192.168.130.66","192.168.131.66","192.168.132.66","192.168.200.66","192.168.201.66","192.168.110.66","192.168.202.66","192.168.180.66","192.168.183.66",
                "192.168.186.66","192.168.184.66","192.168.150.66","192.168.152.66","192.168.190.66","192.168.191.66","192.168.120.66","192.168.220.66","192.168.221.66",
                "192.168.139.66","192.168.140.66","192.168.142.66","192.168.144.66","192.168.146.66","192.168.149.66","192.168.178.66","192.168.133.66","192.168.211.66","192.168.213.66","192.168.212.66","192.168.187.66","192.168.181.66","192.168.179.66","192.168.192.66","192.168.182.66"};
            if (hmi.Length != ips.Length) {
                throw new Exception("Hmi和Ip长度不匹配");
            }

            int port = 9988;
            for (var i = 0; i < ips.Length; i++) {
                var des = ips[i].Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries)[2];
                var url = $"http://192.168.{des}.66:{port}";
                SendToAsylum(url, cmd, hmi[i]);
            }

            YUtil.ExitWithQ();
        }

        static int recI = 0;
        private static int sendI = 0;
        private static object recLock = new Object();

        public static async void SendToAsylum(string url, Cmd cmd, string machineName) {
            using (var client = new HttpClient()) {
                Console.WriteLine($"[{++sendI}] 向 {machineName}:{url} 发出请求 ... {cmd}");
                try {
                    url += "/" + cmd;
                    var content = new StringContent(JsonConvert.SerializeObject(cmd), Encoding.UTF8, "application/json");
                    var rep = await client.PostAsync(url, content);
                    var str = await rep.Content.ReadAsStringAsync();
                    Console.WriteLine(machineName + ":  " + str);
                } catch (Exception e) {
                    lock (recLock) {
                        Console.WriteLine($"[{++recI}] {machineName}：{e.Message}");
                    }
                }
            }
        }


        public static async void SendToHmiPro(string url, string cmd, string machineName) {
            using (var client = new HttpClient()) {
                Console.WriteLine($"[{++sendI}] 向 {machineName}:{url} 发出请求 ... {cmd}");
                try {
                    url += "/" + cmd;
                    var responseString = await client.GetStringAsync(url);
                    Console.WriteLine(machineName + ":  " + responseString);
                } catch (Exception e) {
                    lock (recLock) {
                        Console.WriteLine($"[{++recI}] {machineName}：{e.Message}");
                    }
                }
            }
        }
    }
    /// <summary>
    /// 命令格式
    /// </summary>
    public class Cmd {
        /// <summary>
        /// 动作
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public object Args { get; set; }
        /// <summary>
        /// 执行时间
        /// </summary>
        public long? ExecTime { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public long? SendTime { get; set; }
    }

}
