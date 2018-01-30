﻿using System;
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
    /// 机台采集参数配置
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
        public static string HmiName;

        /// <summary>
        /// 每个机台的报警ip字典
        /// </summary>
        public static IDictionary<string, string> AlarmIpDict;

        public static void Load(string path) {
            MachineDict = new Dictionary<string, Machine>();
            IpToMachineCodeDict = new Dictionary<string, string>();
            AlarmIpDict = new Dictionary<string, string>();
            MachineCodeToIpsDict = new Dictionary<string, List<string>>();
            var codes = Path.GetFileNameWithoutExtension(path).Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            HmiName = Path.GetFileNameWithoutExtension(path);
            foreach (var code in codes) {
                var upperCode = code.ToUpper();
                var machine = new Machine();
                machine.Code = upperCode;
                machine.InitCpmDict(path, $"{upperCode}_采集参数");
                machine.CpmIps = GlobalConfig.MachineSettingDict[upperCode].CpmModuleIps;
                MachineDict[upperCode] = machine;
                foreach (var ip in machine.CpmIps) {
                    IpToMachineCodeDict[ip] = upperCode;
                    if (ip.EndsWith("100")) {
                        AlarmIpDict[upperCode] = ip;
                    }
                }
                MachineCodeToIpsDict[upperCode] = machine.CpmIps.ToList();
            }
        }

        /// <summary>
        /// 根据ip来寻找其配置文件的路径
        /// 如果指定了 hmiXlsPath，则直接使用
        /// </summary>
        public static void LoadFromGlobal(string hmiXlsPath) {
            if (!string.IsNullOrEmpty(hmiXlsPath)) {
                Load(hmiXlsPath);
                return;
            }

            string configPath = null;
            if (!string.IsNullOrEmpty(CmdOptions.GlobalOptions.HmiName)) {

                Console.WriteLine("指定配置Hmi：" + CmdOptions.GlobalOptions.HmiName);
                //Global.xls中根据ip来指定Hmi配置
            } else {
                var ips = YUtil.GetAllIps();
                foreach (var ip in ips) {
                    if (GlobalConfig.IpToHmiDict.TryGetValue(ip, out var hmi)) {
                        configPath =
                            YUtil.GetAbsolutePath(CmdOptions.GlobalOptions.ConfigFolder + "\\Machines\\" + hmi +
                                                  ".xls");
                    }
                }
            }
            if (string.IsNullOrEmpty(configPath)) {
                throw new Exception($"本机ip{string.Join(",", YUtil.GetAllIps())}未在 Global.xls中配置");
            }
            Load(configPath);
            Console.WriteLine("加载机台配置文件路径：-" + configPath);
        }

    }
}
