using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using YCsharp.Service;

namespace HmiPro.Config.Models {

    /// <summary>
    /// 单个机台的配置
    /// </summary>
    public class Machine {
        public string Code { get; set; }
        /// <summary>
        /// 底层Ip
        /// </summary>
        public string[] CpmIps { get; set; }
        //编码：采集参数（所有）
        public IDictionary<int, CpmInfo> CodeToAllCpmDict = new Dictionary<int, CpmInfo>();
        //编码：采集参数（需要计算，如最大值）
        public IDictionary<int, CpmInfo> CodeToRelateCpmDict = new Dictionary<int, CpmInfo>();
        //编码：采集参数（直接获取）
        public IDictionary<int, CpmInfo> CodeToDirectCpmDict = new Dictionary<int, CpmInfo>();
        //编码：[算法参数编码]
        public IDictionary<int, List<int>> CodeMethodDict = new Dictionary<int, List<int>>();

        //mq检验报警
        public IDictionary<int, CpmInfo> CodeToMqBomAlarmCpmDict = new Dictionary<int, CpmInfo>();
        //plc检验报警
        public IDictionary<int, PlcAlarmCpm> CodeToPlcAlarmDict = new Dictionary<int, PlcAlarmCpm>();
        //经验报警配置
        public IDictionary<int, ExpAlarm> CodeToExpAlarmDict = new Dictionary<int, ExpAlarm>();

        /// <summary>
        /// 参数名称：编码
        /// </summary>
        public IDictionary<string, int> CpmNameToCodeDict = new Dictionary<string, int>();

        /// <summary>
        /// 初始化采集参数字典
        /// </summary>
        public void InitCpmDict(string path, string sheetName) {
            CpmLoader cpmLoader = new CpmLoader(path, sheetName);
            List<CpmInfo> cpms = cpmLoader.Load();
            //添加Oee显示
            cpms.Add(new CpmInfo() { Code = DefinedParamCode.StartAxisRfid, Name = "放线卡" });
            cpms.Add(new CpmInfo() { Code = DefinedParamCode.EndAxisRfid, Name = "收线卡" });
            cpms.Add(new CpmInfo() { Code = DefinedParamCode.EmpRfid, Name = "人员卡" });
            //cpms.Add(new CpmInfo() { Code = DefinedParamCode.MaterialRfid, Name = "物料卡" });

            cpms.Add(new CpmInfo() { Code = DefinedParamCode.Oee, Name = "Oee" });
            cpms.Add(new CpmInfo() { Code = DefinedParamCode.OeeTime, Name = "开机率" });
            cpms.Add(new CpmInfo() { Code = DefinedParamCode.OeeSpeed, Name = "速度率" });
            cpms.Add(new CpmInfo() { Code = DefinedParamCode.OeeQuality, Name = "质量率" });
            validUnique(cpms);
            cpms.ForEach(cpm => {
                CpmNameToCodeDict[cpm.Name] = cpm.Code;
                //所有参数
                if (CodeToAllCpmDict.ContainsKey(cpm.Code)) {
                    throw new Exception($"参数编码 [{cpm.Code}] 重复了");
                }
                CodeToAllCpmDict[cpm.Code] = cpm;

                //计算参数
                if (cpmLoader.IsRelateMethod(cpm.MethodName)) {
                    CodeToRelateCpmDict[cpm.Code] = cpm;
                } else {
                    CodeToDirectCpmDict[cpm.Code] = cpm;
                }
                //算法参数
                if (cpm.MethodParamInts?.Count > 0) {
                    var codeList = new List<int>();
                    CodeMethodDict[cpm.Code] = codeList;
                    cpm.MethodParamInts.ForEach(code => {
                        codeList.Add(code);
                    });
                }
                //mq报警设置
                if (cpm.MqAlarmBomKeys != null) {
                    CodeToMqBomAlarmCpmDict[cpm.Code] = cpm;
                }
                //plc报警参数设置
                if (!string.IsNullOrEmpty(cpm.PlcAlarmKey)) {
                    //报警参数
                    if (!cpm.PlcAlarmKey.ToLower().Contains("_max") && !cpm.PlcAlarmKey.ToLower().Contains("_min")) {
                        CodeToPlcAlarmDict[cpm.Code] = new PlcAlarmCpm() { Code = cpm.Code, AlarmKey = cpm.PlcAlarmKey };
                    }
                }
                //经验报警配置
                //max_150|min_130   ||    max_150   || min_130 || min_130|max_150
                if (cpm.ExpAlarms != null) {
                    float? max = null, min = null;
                    var maxStr = cpm.ExpAlarms.FirstOrDefault(c => c.ToLower().Contains("max"));
                    var minStr = cpm.ExpAlarms.FirstOrDefault(c => c.ToLower().Contains("min"));
                    if (!string.IsNullOrEmpty(maxStr)) {
                        max = float.Parse(maxStr.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    }
                    if (!string.IsNullOrEmpty(minStr)) {
                        min = float.Parse(minStr.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    }
                    CodeToExpAlarmDict[cpm.Code] = new ExpAlarm() {
                        Max = max,
                        Min = min
                    };
                }
            });
            //更新Plc报警参数
            foreach (var pair in CodeToPlcAlarmDict) {
                var plcAlarm = pair.Value;
                var max = cpms.FirstOrDefault(cpm => cpm.PlcAlarmKey?.ToLower() == plcAlarm.AlarmKey + "_max");
                if (max != null) {
                    plcAlarm.MaxCode = max.Code;
                }
                var min = cpms.FirstOrDefault(cpm => cpm.PlcAlarmKey?.ToLower() == plcAlarm.AlarmKey + "_min");
                if (min != null) {
                    plcAlarm.MinCode = min.Code;
                }

            }
            validCodeMethodDict();
            validPlcAlarm();
        }

        /// <summary>
        /// 参数名称和参数编码不能重复
        /// </summary>
        /// <param name="cpms"></param>
        void validUnique(List<CpmInfo> cpms) {
            var repeatNames = cpms.GroupBy(c => c.Name).Where(c => c.Count() > 1).ToList();
            if (repeatNames?.Count > 0) {
                throw new Exception($"机台 {Code} 参数名: " + repeatNames.FirstOrDefault().Key + " 重复！");
            }
            var repeatCodes = cpms.GroupBy(c => c.Code).Where(c => c.Count() > 1).ToList();
            if (repeatCodes?.Count > 0) {
                throw new Exception($"机台 {Code} 参数编码：{repeatCodes.FirstOrDefault().Key} 重复！");
            }
        }

        /// <summary>
        /// 校验Plc报警配置
        /// </summary>
        void validPlcAlarm() {

        }

        /// <summary>
        /// 校验算法参数的编码是有效的
        /// </summary>
        void validCodeMethodDict() {
            foreach (var methodPair in CodeMethodDict) {
                methodPair.Value.ForEach(method => {
                    if (!CodeToAllCpmDict.ContainsKey(method)) {
                        throw new Exception($"算法参数编码 [{method}] 不存在");
                    }
                });
            }

        }
    }
}
