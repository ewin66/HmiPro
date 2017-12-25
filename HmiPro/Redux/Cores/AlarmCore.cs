using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
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
        private readonly MqEffects mqEffects;
        private readonly DbEffects dbEffects;

        public AlarmCore(MqEffects mqEffects, DbEffects dbEffects) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.mqEffects = mqEffects;
            this.dbEffects = dbEffects;
            actionsExecDict[AlarmActions.GENERATE_ONE_ALARM] = doGenerateOneAlarm;
        }

        private void doGenerateOneAlarm(AppState state, IAction action) {
            var alarmAction = (AlarmActions.GenerateOneAlarm)action;
            var machineCode = alarmAction.MachineCode;
            var alarmAdd = alarmAction.MqAlarm;
            if (alarmAdd == null) {
                return;
            }
            var historyAlarms = historyAlarmsDict[machineCode];
            var alarmRemove = historyAlarms.FirstOrDefault(a => a.code == alarmAdd.code);
            AlarmActions.UpdateAction updateAction = AlarmActions.UpdateAction.Add;
            if (alarmRemove != null) {
                updateAction = AlarmActions.UpdateAction.Change;
                historyAlarms.Remove(alarmRemove);
            }
            historyAlarms.Add(alarmAdd);
            //通知报警历史记录改变
            App.Store.Dispatch(new AlarmActions.UpdateHistoryAlarms(machineCode, updateAction, alarmAdd, alarmRemove));
            //打开报警灯5秒
            App.Store.Dispatch(new AlarmActions.OpenAlarmLights(machineCode, 5000));
            //打开屏幕
            App.Store.Dispatch(new SysActions.OpenScreen());
            //显示消息通知
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                Title = "警报",
                Content = machineCode + ":" + alarmAdd.alarmType
            }));
            //上传报警到Mq
            App.Store.Dispatch(mqEffects.UploadAlarm(new MqActions.UploadAlarmMq(HmiConfig.QueWebSrvException, alarmAdd)));
            //保存报警到Mongo
            App.Store.Dispatch(dbEffects.UploadAlarmsMongo(new DbActions.UploadAlarmsMongo(machineCode, "Alarms", alarmAdd)));
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
