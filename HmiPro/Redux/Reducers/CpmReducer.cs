using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Annotations;
using HmiPro.Config;
using HmiPro.Config.Models;
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
            public IDictionary<string, float> SparkDiffDict;
            /// <summary>
            /// 开关状态
            /// </summary>
            public IDictionary<string, ObservableCollection<MachineState>> MachineStateDict;
            public static readonly IDictionary<string, object> OeeLocks = new Dictionary<string, object>();
            /// <summary>
            /// 调机状态
            /// </summary>
            public IDictionary<string, ObservableCollection<MachineDebugState>> MachineDebugStateDict;
            /// <summary>
            /// 保存机台出线速度
            /// </summary>
            public IDictionary<string, float> SpeedDict;
            /// <summary>
            /// 保存当前速度的前一个速度
            /// </summary>
            public IDictionary<string, float> PreSpeedDict;
            /// <summary>
            /// 机台编码
            /// </summary>
            public string MachineCode;
            /// <summary>
            /// 日志工具
            /// </summary>
            public LoggerService Logger;
            /// <summary>
            /// 每个Ip对应的485状态表
            /// </summary>
            public IDictionary<string, Com485SingleStatus> Com485StatusDict;
        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>().When<CpmActions.Init>((state, action) => {
                state.OnlineCpmsDict = new Dictionary<string, IDictionary<int, Cpm>>();
                state.UpdatedCpmsDiffDict = new Dictionary<string, IDictionary<int, Cpm>>();
                state.UpdatedCpmsAllDict = new Dictionary<string, List<Cpm>>();
                state.NoteMeterDict = new Dictionary<string, float>();
                state.IpActivedDict = new Dictionary<string, DateTime>();
                state.SparkDiffDict = new Dictionary<string, float>();
                state.MachineStateDict = new ConcurrentDictionary<string, ObservableCollection<MachineState>>();
                state.MachineDebugStateDict = new Dictionary<string, ObservableCollection<MachineDebugState>>();
                state.Com485StatusDict = new ConcurrentDictionary<string, Com485SingleStatus>();
                state.SpeedDict = new ConcurrentDictionary<string, float>();
                state.PreSpeedDict = new Dictionary<string, float>();
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
                            Value = "暂无",
                            ValueType = SmParamType.Unknown
                        };
                    }
                    state.OnlineCpmsDict[machineCode] = cpmsDict;
                    state.MachineStateDict[machineCode] = new ObservableCollection<MachineState>();
                    state.SpeedDict[machineCode] = 0f;
                    state.NoteMeterDict[machineCode] = 0f;
                    state.PreSpeedDict[machineCode] = 0f;
                    state.SparkDiffDict[machineCode] = 0f;
                    State.OeeLocks[machineCode] = new object();
                }
                //初始化所有ip的通讯状态为未知
                foreach (var pair in MachineConfig.IpToMachineCodeDict) {
                    var ip = pair.Key;
                    state.Com485StatusDict[ip] = new Com485SingleStatus() { Status = SmSingleStatus.Ok, Time = DateTime.Now, Ip = ip };
                }
                //ping一下所有ip
                Task.Run(() => {
                    foreach (var pair in state.Com485StatusDict) {
                        Ping ping = new Ping();
                        var ret = ping.Send(pair.Key, 1000);
                        if (ret?.Status != IPStatus.Success) {
                            pair.Value.Status = SmSingleStatus.Offline;
                            pair.Value.Time = DateTime.Now;
                        }
                    }
                });
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
                state.SparkDiffDict[action.MachineCode] = action.Spark;
                return state;
            }).When<CpmActions.SpeedDiffAccpet>((state, action) => {
                lock (State.OeeLocks[action.MachineCode]) {
                    state.MachineCode = action.MachineCode;
                    var currentSpeed = action.Speed;
                    var lastMachineState = state.MachineStateDict[action.MachineCode].LastOrDefault();
                    //如果机台处于维修状态则不更改其状态，直到其维修完毕
                    if (lastMachineState?.StatePoint == MachineState.State.Repair) {
                        return state;
                    }

                    //开机阶段，上一个速度为0 ，此时的速度大于0
                    MachineState newMachineState = null;
                    var machineCode = action.MachineCode;
                    state.SpeedDict[machineCode] = currentSpeed;
                    if (currentSpeed > 0 && (int)state.PreSpeedDict[machineCode] == 0) {
                        newMachineState = new MachineState() {
                            StatePoint = MachineState.State.Start,
                            Time = DateTime.Now
                        };
                    }
                    //关机阶段，上一个速度大于0，此时速度等于0
                    else if ((int)currentSpeed == 0 && state.PreSpeedDict[machineCode] > 0) {
                        newMachineState = new MachineState() {
                            StatePoint = MachineState.State.Stop,
                            Time = DateTime.Now
                        };
                    }
                    //赋值前一个速度
                    state.PreSpeedDict[action.MachineCode] = action.Speed;
                    if (newMachineState == null) {
                        return state;
                    }
                    var machineStates = state.MachineStateDict[machineCode];

                    if (machineStates.Count == 0) {
                        machineStates.Add(newMachineState);
                    } else if (machineStates.Count > 0) {
                        var lastState = machineStates.LastOrDefault();
                        if (lastState?.StatePoint == newMachineState.StatePoint) {
                            state.Logger.Error($"机台 {machineCode} 分析开关机有误，两次都为 {lastState?.StatePoint}");
                        } else {
                            machineStates.Add(newMachineState);
                        }
                    }
                    return state;
                }
                //更新ip的485状态
            }).When<CpmActions.Com485SingleStatusAccept>((state, action) => {
                state.Com485StatusDict[action.Ip].Status = action.Status;
                state.Com485StatusDict[action.Ip].Time = DateTime.Now;
                return state;
            });
        }
    }

    public class Com485SingleStatus : INotifyPropertyChanged {
        private SmSingleStatus status;
        public SmSingleStatus Status {
            get => status;
            set {
                if (status != value) {
                    status = value;
                    OnPropertyChanged(nameof(StatusStr));
                }
            }

        }
        public string StatusStr {
            get {
                switch (status) {
                    case SmSingleStatus.Error:
                        return "错误";
                    case SmSingleStatus.Ok:
                        return "正常";
                    case SmSingleStatus.Unknown:
                        return "初始化";
                    case SmSingleStatus.Offline:
                        return "离线";
                    default:
                        return "初始化";
                }
            }
        }

        private DateTime time;
        public DateTime Time {
            get => time;
            set {
                if (time != value) {
                    time = value;
                    OnPropertyChanged(nameof(TimeStr));
                }
            }
        }
        public string TimeStr => time.ToString("yyyy-MM-dd HH:mm:ss");

        public string Ip { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
