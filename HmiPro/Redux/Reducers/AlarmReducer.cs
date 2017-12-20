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
            public IDictionary<string, ObservableCollection<MqAlarm>> NotifyAlarmDict;
        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>(() => {
                var state = new State() {
                    AlarmBomCheckDict = new ConcurrentDictionary<string, AlarmBomCheck>(),
                    NotifyAlarmDict = new ConcurrentDictionary<string, ObservableCollection<MqAlarm>>()
                };

                return state;
            }).When<AlarmActions.Init>((state, action) => {
                foreach (var pair in MachineConfig.MachineDict) {
                    state.NotifyAlarmDict[pair.Key] = new ObservableCollection<MqAlarm>();
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
            }).When<AlarmActions.NotifyAlarm>((state, action) => {
                state.MachineCode = action.MachineCode;
                //相同参数报警已经存在
                var oldAlarm = state.NotifyAlarmDict[action.MachineCode].Where(a => a.code == action.MqAlarm.code)
                        .FirstOrDefault();
                //删除之前的报警
                if (oldAlarm != null) {
                    state.NotifyAlarmDict[action.MachineCode].Remove(oldAlarm);
                }
                //添新的报警
                state.NotifyAlarmDict[action.MachineCode].Add(action.MqAlarm);
                return state;
            });
        }
    }
}
