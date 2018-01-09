using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config.Models;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Config {
    /// <summary>
    /// 一些固有的全局属性配置，方便管理
    /// <author>ychost</author>
    /// <date>2018-01-09</date>
    /// </summary>
    public static class GlobalConfig {
        public static IDictionary<string, MachineOeeSetting> MachineOeeSettingDict;

        public static void Load(string path) {
            path = YUtil.GetAbsolutePath(path);
            MachineOeeSettingDict = new Dictionary<string, MachineOeeSetting>();
            using (var xlsOp = new XlsService(path)) {
                var dt = xlsOp.ExcelToDataTable("速度配置", true);
                foreach (DataRow row in dt.Rows) {
                    MachineOeeSetting setting = new MachineOeeSetting();
                    setting.Code = row["Code"].ToString();
                    setting.OeeSpeedCpmName = row["OeeSpeedCpmName"].ToString();
                    if (float.TryParse(row["OeeSpeedMax1Setting"].ToString(), out var max1Setting)) {
                        setting.OeeSpeedMax1Setting = max1Setting;
                        if (setting.OeeSpeedMax1Setting.Value == 0) {
                            throw new Exception($"机台{setting.Code} 的 OeeSpeedMax1Setting 为0，请检查");
                        }
                    }
                    setting.OeeSpeedMax2CpmName = row["OeeSpeedMax2CpmName"].ToString();
                    setting.OeeSpeedMax3MqKey = row["OeeSpeedMax3MqKey"].ToString();
                    setting.MqNeedSpeedCpmName = row["MqNeedSpeedCpmName"].ToString();
                    MachineOeeSettingDict[setting.Code] = setting;
                }
            }
        }

    }
}
