using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using Reducto;
using YCsharp.Model.Procotol;
using YCsharp.Model.Procotol.SmParam;
using YCsharp.Service;
using DevExpress.XtraPrinting.Native;
using HmiPro.Redux.Effects;

namespace HmiPro.Redux.Services {
    /// <summary>
    /// 数据采集逻辑服务
    /// <date>2017-12-17</date>
    /// <author>ychost</author>
    /// </summary>
    public class CpmService {

        public YSmParamTcp SmParamTcp;
        public LoggerService Logger;
        //编码：值（在线数据）
        //加float增加算法参数计算效率
        //计算相关参数的时候需要使用
        IDictionary<string, IDictionary<int, float>> onlineFloatDict = new ConcurrentDictionary<string, IDictionary<int, float>>();

        //编码：采集参数
        //外部可能会对此进行采样
        public IDictionary<string, IDictionary<int, Cpm>> OnlineCpmDict;

        public CpmService() {
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            foreach (var pair in MachineConfig.MachineDict) {
                onlineFloatDict[pair.Key] = new ConcurrentDictionary<int, float>();
            }
        }

        public Task StartAsync(string ip, int port) {
            return Task.Run(() => {
                if (SmParamTcp == null) {
                    SmParamTcp = new YSmParamTcp(ip, port);
                    SmParamTcp.OnDataReceivedAction += smModelsHandler;
                }
                SmParamTcp.Start();
                OnlineCpmDict = App.Store.GetState().CpmState.OnlineCpmsDict;
            });
        }

        public void Stop() {
            SmParamTcp?.StopSoft();
        }

        void smModelsHandler(string ip, List<SmModel> smModels) {
            if (!MachineConfig.IpToMachineCodeDict.TryGetValue(ip, out var code)) {
                Logger.Error($"ip {ip} 未注册");
                return;
            }
            smModels?.ForEach(sm => {
                //处理参数包
                if (sm.PackageType == SmPackageType.ParamPackage) {
                    paramPkgHandler(code, sm);
                }
                //处理心跳包
                else if (sm.PackageType == SmPackageType.HeartbeatPackage) {
                }
            });
        }

        void paramPkgHandler(string machineCode, SmModel sm) {
            var cpmsDirect = Cpm.ConvertBySmModel(machineCode, sm);
            var cpmsRelate = new List<Cpm>();
            var cpms = new List<Cpm>();
            IDictionary<int, Cpm> updatedCpmsDiffDict = new Dictionary<int, Cpm>();
            //简洁算法计算出来的参数
            cpmsDirect?.ForEach(cpm => {
                if (cpm.ValueType == SmParamType.Signal) {
                    var floatVal = (float)cpm.Value;
                    onlineFloatDict[machineCode][cpm.Code] = floatVal;
                    var relateCpms = calcRelateCpm(machineCode, cpm.Code);
                    cpmsRelate.AddRange(relateCpms);
                }
            });
            if (cpmsDirect != null) {
                cpms.AddRange(cpmsDirect);
                cpms.AddRange(cpmsRelate);
            }
            //更新参数字典
            //fixed:2017-10-20
            //      一包参数的时间应该一致
            var pickTime = DateTime.Now;

            void update(Cpm cpm) {
                cpm.PickTime = pickTime;
                //直接更新，会更新界面的数据
                if (OnlineCpmDict[machineCode].TryGetValue(cpm.Code, out var oldCpm)) {
                    oldCpm.Update(cpm.Value, cpm.ValueType, pickTime);
                } else {
                    OnlineCpmDict[machineCode][cpm.Code] = cpm;
                }
                updatedCpmsDiffDict[cpm.Code] = cpm;
            }

            //差异更新，使用linq 2017-11-13
            (from c in cpms
             where (
                 c.Value != null
                 && (!OnlineCpmDict[machineCode].ContainsKey(c.Code) ||
                     OnlineCpmDict[machineCode][c.Code].ToString() != c.Value.ToString())
             )
             select c
            ).ForEach(update);

            if (updatedCpmsDiffDict.Count > 0) {
                App.Store.Dispatch(new CpmActions.CpmUpdateDiff(machineCode, updatedCpmsDiffDict));
            }
            if (cpms.Count > 0) {
                App.Store.Dispatch(new CpmActions.CpmUpdatedAll(machineCode, cpms));
            }
        }


        /// <summary>
        /// 更新需要算法计算的参数，并返回转换后的集合
        /// </summary>
        /// <returns></returns>
        List<Cpm> calcRelateCpm(string code, int updatedCode) {
            List<Cpm> cpms = new List<Cpm>();
            foreach (var infoPair in MachineConfig.MachineDict[code].CodeToRelateCpmDict) {
                var val = infoPair.Value.ExecCalc(onlineFloatDict[code], updatedCode);
                if (val.HasValue) {
                    //设置精度
                    val = (float)Math.Round(val.Value, HmiConfig.MathRound);
                    //实时更新在线数据表
                    onlineFloatDict[code][infoPair.Key] = val.Value;
                    cpms.Add(new Cpm() {
                        Code = infoPair.Key,
                        Value = val,
                        ValueType = SmParamType.Signal,
                        Name = infoPair.Value.Name
                    });
                }
            }
            return cpms;
        }
    }
}
