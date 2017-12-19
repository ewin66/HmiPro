using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Services;
using Reducto;

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
            public IDictionary<string, double> NoteMeterDict;
            /// <summary>
            /// Ip最后活跃时间字典
            /// </summary>
            public IDictionary<string, DateTime> IpActivedDict;
            /// <summary>
            /// 机台编码
            /// </summary>
            public string MachineCode;
        }


        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>().When<CpmActions.Init>((state, action) => {
                state.OnlineCpmsDict = new ConcurrentDictionary<string, IDictionary<int, Cpm>>();
                state.UpdatedCpmsDiffDict = new ConcurrentDictionary<string, IDictionary<int, Cpm>>();
                state.UpdatedCpmsAllDict = new ConcurrentDictionary<string, List<Cpm>>();
                state.NoteMeterDict = new ConcurrentDictionary<string, double>();
                state.IpActivedDict = new ConcurrentDictionary<string, DateTime>();
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
            });
        }
    }
}
