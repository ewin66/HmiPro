using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using Reducto;

namespace HmiPro.Redux.Reducers {

    /// <summary>
    /// Mq 数据核心，排产任务数据、启动监听状态等等
    /// <author>ychost</author>
    /// <date>2017-12-1</date>
    /// </summary>
    public static class MqReducer {
        public struct State {
            /// <summary>
            /// 当前接受到的任务
            /// </summary>
            public IDictionary<string, MqSchTask> MqSchTaskAccpetDict;
            public bool LsnUploadCpmsInterval;
            public string MachineCode;
            public IDictionary<string, bool> LsnScanMaterialDict;
            public IDictionary<string, bool> LsnSchTaskDict;
        }

        public static SimpleReducer<MqReducer.State> Create() {
            return new SimpleReducer<State>(() => new State() {
                MqSchTaskAccpetDict = new ConcurrentDictionary<string, MqSchTask>(),
                LsnScanMaterialDict = new ConcurrentDictionary<string, bool>(),
                LsnSchTaskDict = new ConcurrentDictionary<string, bool>(),
            }).When<MqActions.StartListenSchTaskSuccess>((state, action) => {
                state.LsnSchTaskDict[action.MachineCode] = true;
                return state;
            }).When<MqActions.StartListenSchTask>((state, action) => {
                if (state.LsnSchTaskDict.TryGetValue(action.MachineCode, out var lsn)) {
                    if (lsn) {
                        throw new Exception($"请勿重复监听务 [Mq] 排产任务 Machine {action.MachineCode}");
                    }
                }
                return state;
            }).When<MqActions.StartListenSchTaskFailed>((state, action) => {
                state.LsnScanMaterialDict[action.MachineCode] = false;
                return state;
            }).When<MqActions.SchTaskAccept>((state, action) => {
                state.MachineCode = action.MqSchTask.maccode;
                state.MqSchTaskAccpetDict[state.MachineCode] = action.MqSchTask;
                return state;
            }).When<MqActions.StartUploadCpmsInterval>((state, action) => {
                if (state.LsnUploadCpmsInterval) {
                    throw new Exception("请勿重复开启采集参数周期上传定时器");
                }
                state.LsnUploadCpmsInterval = true;
                return state;
            })
            .When<MqActions.StartListenScanMaterial>((state, action) => {
                if (state.LsnScanMaterialDict.TryGetValue(action.MachineCode, out var lsn)) {
                    if (lsn) {
                        throw new Exception($"请勿重复监听 [Mq] 扫描来料 Machine {action.MachineCode}");
                    }
                }
                return state;
            })
            .When<MqActions.StartListenScanMaterialSuccess>((state, action) => {
                state.LsnScanMaterialDict[action.MachineCode] = true;
                return state;
            }).When<MqActions.StartListenScanMaterialFailed>((state, action) => {
                state.LsnScanMaterialDict[action.MachineCode] = false;
                return state;
            });
        }
    }
}
