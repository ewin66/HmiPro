using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using YCsharp.Service;
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
                args = new string[] { "StartHmiPro", "update-app", "clear-task" };
            }
            var cmd = new Cmd();
            cmd.Action = args[0];
            cmd.Args = new { IsForced = true };

            var hmis = loadHmi(YUtil.GetAbsolutePath(".\\Global.xls"), "Ip配置");

            int asylumPort = 9988;
            int hmiPort = 8899;
            foreach (var pair in hmis) {
                string ip = pair.Value;
                var name = pair.Key;
                var hmiProUrl = $"http://{ip}:{hmiPort}";
                var asylumUrl = $"http://{ip}:{asylumPort}";
                var url = asylumUrl;
                SendToAsylum(url, cmd, name);
                //var url = hmiProUrl;
                //SendToHmiPro(url, args[2], name);
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
                    var content = new StringContent(JsonConvert.SerializeObject(cmd), Encoding.UTF8,
                        "application/json");
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


        /// <summary>
        /// 获取所有 Hmi信息，名字:IP
        /// </summary>
        /// <param name="xlsPath"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        static IDictionary<string, string> loadHmi(string xlsPath, string sheetName) {
            var dict = new Dictionary<string, string>();
            using (var xlsOp = new XlsService(xlsPath)) {
                var speedDt = xlsOp.ExcelToDataTable(sheetName, true);
                foreach (DataRow row in speedDt.Rows) {
                    dict[row["Hmi"].ToString()] = row["Ip"].ToString();
                }
            }
            return dict;
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
}
