using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config.Models;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Config {
    /// <summary>
    /// 机台配置
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public static class MachineConfig {
        /// <summary>
        /// 所有机台字典，键为机台编码
        /// </summary>
        public static IDictionary<string, Machine> MachineDict;
        public static IDictionary<string, string> IpToMachineCodeDict;
        public static IDictionary<string, List<string>> MachineCodeToIpsDict;
        public static string AllMachineName;

        /// <summary>
        /// 每个机台的报警ip字典
        /// </summary>
        public static IDictionary<string, string> AlarmIpDict;

        public static void Load(string path) {
            MachineDict = new Dictionary<string, Machine>();
            IpToMachineCodeDict = new Dictionary<string, string>();
            AlarmIpDict = new Dictionary<string, string>();
            MachineCodeToIpsDict = new Dictionary<string, List<string>>();
            var codes = Path.GetFileNameWithoutExtension(path).Split('_');
            AllMachineName = Path.GetFileNameWithoutExtension(path);
            foreach (var code in codes) {
                var machine = new Machine();
                machine.Code = code;
                machine.InitCpmDict(path, $"{code}_采集参数");
                machine.InitCodeAndIp(path, $"{code}_机台属性");
                MachineDict[code] = machine;
                foreach (var ip in machine.CpmIps) {
                    IpToMachineCodeDict[ip] = code;
                    if (ip.EndsWith("100")) {
                        AlarmIpDict[code] = ip;
                    }
                }
                MachineCodeToIpsDict[code] = machine.CpmIps.ToList();
            }
        }

        /// <summary>
        /// 根据ip来寻找其配置文件的路径
        /// </summary>
        public static void LoadFromGlobal() {
            string configPath = null;

            bool hasConfiged = false;
            if (!string.IsNullOrEmpty(CmdOptions.GlobalOptions.HmiName)) {
                configPath = YUtil.GetAbsolutePath(CmdOptions.GlobalOptions.ConfigFolder + "\\Machines\\" +
                                                   CmdOptions.GlobalOptions.HmiName + ".xls");
                Console.WriteLine("指定配置Hmi：" + CmdOptions.GlobalOptions.HmiName);
                hasConfiged = true;
            } else {

                var ips = YUtil.GetAllIps();
                foreach (var ip in ips) {
                    if (GlobalConfig.IpToHmiDict.TryGetValue(ip, out var hmi)) {
                        configPath =
                            YUtil.GetAbsolutePath(CmdOptions.GlobalOptions.ConfigFolder + "\\Machines\\" + hmi +
                                                  ".xls");
                        Load(configPath);
                        hasConfiged = true;
                    }
                }
            }
            if (!hasConfiged) {
                throw new Exception("未在Global中配置本机ip对应的Hmi");
            }

            Console.WriteLine("加载机台配置文件路径：-" + configPath);
        }
    }
}
