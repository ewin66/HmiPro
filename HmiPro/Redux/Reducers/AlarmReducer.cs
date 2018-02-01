using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using Reducto;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// 报警字典
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public static class AlarmReducer {
        public struct State {
            public string MachineCode;
            /// <summary>
            /// 待检查报警的参数
            /// </summary>
            public IDictionary<string, AlarmBomCheck> AlarmBomCheckDict;
            /// <summary>
            /// 已产生的报警
            /// </summary>
            public IDictionary<string, ObservableCollection<MqAlarm>> AlarmsDict;

            /// <summary>
            /// 最近产生的报警
            /// </summary>
            public IDictionary<string, MqAlarm> LatestAlarmDict;
        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>()
                .When<AlarmActions.Init>((state, action) => {
                    state.AlarmsDict = new ConcurrentDictionary<string, ObservableCollection<MqAlarm>>();
                    state.AlarmBomCheckDict = new ConcurrentDictionary<string, AlarmBomCheck>();
                    state.LatestAlarmDict = new ConcurrentDictionary<string, MqAlarm>();
                    foreach (var pair in MachineConfig.MachineDict) {
                        state.AlarmsDict[pair.Key] = new ObservableCollection<MqAlarm>();
                    }
                    return state;
                }).When<AlarmActions.OpenAlarmLights>((state, action) => {
                    state.MachineCode = action.MachineCode;
                    return state;
                }).When<AlarmActions.CloseAlarmLights>((state, action) => {
                    state.MachineCode = action.MachineCode;
                    return state;
                }).When<AlarmActions.CheckCpmBomAlarm>((state, action) => {
                    state.MachineCode = action.MachineCode;
                    state.AlarmBomCheckDict[action.MachineCode] = action.AlarmBomCheck;
                    return state;
                }).When<AlarmActions.GenerateOneAlarm>((state, action) => {
                    state.MachineCode = action.MachineCode;
                    state.LatestAlarmDict[action.MachineCode] = action.MqAlarm;
                    return state;
                });
        }
    }
}
