using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Services;
using Reducto;
using YCsharp.Model.Procotol.SmParam;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// cpm相关状态处理逻辑
    /// <date>2017-12-17</date>
    /// <author>ychost</author>
    /// </summary>
    public static class CpmReducer {

        /// <summary>
        /// 采集参数存储的状态和数据
        /// </summary>
        public struct State {
            /// <summary>
            /// 只保存差异更新参数
            /// </summary>
            public IDictionary<string, IDictionary<int, Cpm>> UpdatedCpmsDiffDict;
            /// <summary>
            /// 当前包中解析出来的参数值
            /// </summary>
            public IDictionary<string, List<Cpm>> UpdatedCpmsAllDict;
            /// <summary>
            /// 保存所有最新参数
            /// </summary>
            public IDictionary<string, IDictionary<int, Cpm>> OnlineCpmsDict;
            /// <summary>
            /// 每个机台的记米值
            /// </summary>
            public IDictionary<string, float> NoteMeterDict;
            /// <summary>
            /// Ip最后活跃时间字典
            /// </summary>
            public IDictionary<string, DateTime> IpActivedDict;
            /// <summary>
            /// 当前的火花值
            /// </summary>
            public IDictionary<string, Cpm> SparkDiffDict;
            /// <summary>
            /// 开关状态
            /// </summary>
            public IDictionary<string, ObservableCollection<MachineState>> MachineStateDict;
            /// <summary>
            /// 调机状态
            /// </summary>
            public IDictionary<string, ObservableCollection<MachineDebugState>> MachineDebugStateDict;
            /// <summary>
            /// 计算速度导数算法
            /// </summary>
            public IDictionary<string, Func<double, double>> SpeedCalcDerDict;
            /// <summary>
            /// 保存机台出线速度
            /// </summary>
            public IDictionary<string, Cpm> SpeedDict;
            /// <summary>
            /// 机台编码
            /// </summary>
            public string MachineCode;

            public LoggerService Logger;
        }


        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>().When<CpmActions.Init>((state, action) => {
                state.OnlineCpmsDict = new Dictionary<string, IDictionary<int, Cpm>>();
                state.UpdatedCpmsDiffDict = new Dictionary<string, IDictionary<int, Cpm>>();
                state.UpdatedCpmsAllDict = new Dictionary<string, List<Cpm>>();
                state.NoteMeterDict = new Dictionary<string, float>();
                state.IpActivedDict = new Dictionary<string, DateTime>();
                state.SparkDiffDict = new Dictionary<string, Cpm>();
                state.MachineStateDict = new Dictionary<string, ObservableCollection<MachineState>>();
                state.MachineDebugStateDict = new Dictionary<string, ObservableCollection<MachineDebugState>>();
                state.SpeedCalcDerDict = new Dictionary<string, Func<double, double>>();
                state.SpeedDict = new Dictionary<string, Cpm>();
                state.Logger = LoggerHelper.CreateLogger(typeof(CpmReducer).ToString());
                foreach (var pair in MachineConfig.MachineDict) {
                    var machineCode = pair.Key;
                    var machine = pair.Value;
                    var cpmsDict = new ConcurrentDictionary<int, Cpm>();
                    foreach (var mPair in machine.CodeToAllCpmDict) {
                        var info = mPair.Value;
                        cpmsDict[mPair.Key] = new Cpm() {
                            Name = info.Name,
                            Unit = info.Unit,
                            Code = info.Code,
                            Value = "暂无"
                        };
                    }
                    state.OnlineCpmsDict[machineCode] = cpmsDict;
                    state.MachineStateDict[machineCode] = new ObservableCollection<MachineState>();
                    state.SpeedCalcDerDict[machineCode] = YUtil.CreateExecDerFunc();
                    state.SpeedDict[machineCode] = new Cpm() { Value = 0, ValueType = SmParamType.Signal };
                }
                return state;
            }).When<CpmActions.StartServerSuccess>((state, action) => {
                return state;

            }).When<CpmActions.StartServerFailed>((state, action) => {
                return state;

            }).When<CpmActions.CpmUpdateDiff>((state, action) => {
                state.MachineCode = action.MachineCode;
                state.UpdatedCpmsDiffDict[state.MachineCode] = action.CpmsDict;
                return state;

            }).When<CpmActions.CpmUpdatedAll>((state, action) => {
                state.MachineCode = action.MachineCode;
                state.UpdatedCpmsAllDict[action.MachineCode] = action.Cpms;
                return state;

            }).When<CpmActions.NoteMeterAccept>((state, action) => {
                state.MachineCode = action.MachineCode;
                state.NoteMeterDict[state.MachineCode] = action.Meter;
                return state;
            }).When<CpmActions.CpmIpActivted>((state, action) => {
                state.IpActivedDict[action.Ip] = action.ActivedTime;
                return state;
            }).When<CpmActions.SparkDiffAccept>((state, action) => {
                state.MachineCode = action.MachineCode;
                state.SparkDiffDict[action.MachineCode] = action.SparkCpm;
                return state;
            }).When<CpmActions.SpeedDiffAccpet>((state, action) => {
                state.MachineCode = action.MachineCode;
                var speed = (float)action.SpeedCpm.Value;
                var speedDer = state.SpeedCalcDerDict[state.MachineCode](speed);
                //更新速度表
                state.SpeedDict[state.MachineCode] = action.SpeedCpm;
                //速度下降到 0 了，对其机台状态进行分析
                if ((int)speed == 0) {
                    var machineStates = state.MachineStateDict[state.MachineCode];
                    //导数大于0，则认为是启动阶段
                    MachineState newMachineSatte = null;
                    if (speedDer > 0) {
                        newMachineSatte = new MachineState() {
                            StatePoint = MachineState.State.Start,
                            Time = action.SpeedCpm.PickTime
                        };
                        //导数小于0，则认为是关闭阶段
                    } else if (speedDer < 0) {
                        newMachineSatte = new MachineState() {
                            StatePoint = MachineState.State.Stop,
                            Time = action.SpeedCpm.PickTime
                        };
                    }
                    var lastMachineState = machineStates.LastOrDefault();
                    if (lastMachineState.StatePoint == newMachineSatte.StatePoint) {
                        state.Logger.Error($"机台 {state.MachineCode} 状态分析有误,两次状态一样均为：{lastMachineState.StatePoint}");
                    } else {
                        machineStates.Add(newMachineSatte);
                    }

                    var workTime = YUtil.GetWorkTime(8, 20);
                    //删除上一班的机台状态数据
                    var removeList = machineStates.Where(m => m.Time < workTime).ToList();
                    foreach (var item in removeList) {
                        machineStates.Remove(item);
                    }
                }
                return state;
            });
        }
    }
}
