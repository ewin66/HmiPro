﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config.Models;
using HmiPro.Redux.Actions;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Config {
    /// <summary>
    /// 一些固有的全局属性配置，方便管理
    /// <author>ychost</author>
    /// <date>2018-01-09</date>
    /// </summary>
    public static class GlobalConfig {
        /// <summary>
        /// 所有机台的全局设定
        /// </summary>
        public static IDictionary<string, MachineSetting> MachineSettingDict;
        /// <summary>
        /// Hmi电脑的ip
        /// <example>
        /// 192.168.110.66:DE_DF
        /// </example>
        /// </summary>
        public static IDictionary<string, string> IpToHmiDict;
        /// <summary>
        /// 含栈板的机台
        /// </summary>
        public static string[] PalletMachineCodes = new string[0];

        /// <summary>
        /// 某些机台必须重启才能正常更新
        /// </summary>
        public static string[] UpdateMustReartHmiNames = new string[0];

        /// <summary>
        /// 加载 Globa.xls 配置文件
        /// </summary>
        /// <param name="path"></param>
        public static void Load(string path) {
            path = YUtil.GetAbsolutePath(path);
            MachineSettingDict = new Dictionary<string, MachineSetting>();
            IpToHmiDict = new Dictionary<string, string>();
            using (var xlsOp = new XlsService(path)) {
                var speedDt = xlsOp.ExcelToDataTable("逻辑配置", true);
                foreach (DataRow row in speedDt.Rows) {
                    MachineSetting setting = new MachineSetting();
                    setting.Code = row["Code"].ToString();
                    if (string.IsNullOrEmpty(setting.Code)) {
                        continue;
                    }
                    setting.OeeSpeed = row["OeeSpeed"].ToString();
                    var oeeSpeedMax = row["OeeSpeedMax"].ToString();
                    if (!string.IsNullOrEmpty(oeeSpeedMax) && !string.IsNullOrEmpty(setting.OeeSpeed)) {
                        //从Mq接受最大速度
                        if (oeeSpeedMax.ToUpper().StartsWith("MQ_")) {
                            setting.OeeSpeedMax = oeeSpeedMax.Split('_')[1];
                            setting.OeeSpeedType = OeeActions.CalcOeeSpeedType.MaxSpeedMq;
                            //从Plc中读取最大速度
                        } else if (oeeSpeedMax.ToUpper().StartsWith("PLC_")) {
                            setting.OeeSpeedMax = oeeSpeedMax.Split('_')[1];
                            setting.OeeSpeedType = OeeActions.CalcOeeSpeedType.MaxSpeedPlc;
                            //最大速度为设定值
                        } else if (float.TryParse(row["OeeSpeedMax"].ToString(), out var maxSettingVal)) {
                            if (maxSettingVal == 0) {
                                throw new Exception($"机台{setting.Code} 的 OeeSpeedMax1Setting 为0，请检查");
                            }
                            setting.OeeSpeedMax = maxSettingVal;
                            setting.OeeSpeedType = OeeActions.CalcOeeSpeedType.MaxSpeedSetting;
                        }
                    }
                    setting.MqNeedSpeed = row["MqNeedSpeed"].ToString();
                    setting.StateSpeed = row["StateSpeed"].ToString();
                    setting.NoteMeter = row["NoteMeter"].ToString();
                    setting.Spark = row["Spark"].ToString();
                    setting.Od = row["Od"].ToString();
                    setting.CpmModuleIps = row["CpmModuleIps"].ToString().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    setting.DPms = row["Dpms"].ToString().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    setting.ProcessName = row["Process"]?.ToString();
                    //setting.StartTrayNum = int.Parse(row["StartTrayNum"].ToString());
                    MachineSettingDict[setting.Code] = setting;
                }

                var ipDt = xlsOp.ExcelToDataTable("Ip配置", true);
                foreach (DataRow row in ipDt.Rows) {
                    IpToHmiDict[row["Ip"].ToString()] = row["Hmi"].ToString();
                    var hmi = row["hmi"].ToString().Split('_');
                    //保证后续逻辑不会出现空指针
                    foreach (var s in hmi) {
                        if (!MachineSettingDict.ContainsKey(s)) {
                            MachineSettingDict[s] = new MachineSetting();
                        }
                    }
                }

                var otherDt = xlsOp.ExcelToDataTable("其它配置", true);
                foreach (DataRow row in otherDt.Rows) {
                    var name = row["Name"].ToString().ToUpper();
                    var value = row["Value"].ToString().ToUpper();
                    //RC、RF这种收线盘不贴卡，放栈板上面的
                    //即它们的收线盘的 Rfid 就是栈板的 Rfid，多个收线盘共用一个 Rfid
                    if (name == "栈板机台") {
                        PalletMachineCodes = value.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    } else if (name == "重启机台") {
                        UpdateMustReartHmiNames = value.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    }

                }
            }
        }
    }
}
