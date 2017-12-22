using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// <author>ychost</author>
    /// <date>2017-12-22</date>
    /// </summary>
    public class AlarmCore {
        private readonly IDictionary<string, Action<AppState, IAction>> actionsExecDict = new Dictionary<string, Action<AppState, IAction>>();
        private IDictionary<string, ObservableCollection<MqAlarm>> historyAlarmsDict;

        public AlarmCore() {
            UnityIocService.AssertIsFirstInject(GetType());
            actionsExecDict[AlarmActions.GENERATE_ONE_ALARM] = doGenerateOneAlarm;
        }

        private void doGenerateOneAlarm(AppState state, IAction action) {
            var alarmAction = (AlarmActions.GenerateOneAlarm)action;
            var macineCode = alarmAction.MachineCode;
            var alarmAdd = alarmAction.MqAlarm;
            if (alarmAdd == null) {
                return;
            }
            var historyAlarms = historyAlarmsDict[macineCode];
            var alarmRemove = historyAlarms.FirstOrDefault(a => a.code == alarmAdd.code);
            AlarmActions.UpdateAction updateAction = AlarmActions.UpdateAction.Add;
            if (alarmRemove != null) {
                updateAction = AlarmActions.UpdateAction.Change;
                historyAlarms.Remove(alarmRemove);
            }
            historyAlarms.Add(alarmAdd);
            App.Store.Dispatch(new AlarmActions.UpdateHistoryAlarms(macineCode, updateAction, alarmAdd, alarmRemove));
        }

        /// <summary>
        /// 只有配置加载无误之后才能调用此初始化
        /// </summary>
        public void Init() {
            //绑定
            historyAlarmsDict = App.Store.GetState().AlarmState.AlarmsDict;
            //派发
            App.Store.Subscribe((state, action) => {
                if (actionsExecDict.TryGetValue(action.Type(), out var exec)) {
                    exec(state, action);
                }
            });
        }
    }
}
