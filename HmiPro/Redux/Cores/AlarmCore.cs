using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly IDictionary<string, Action<AppState, IAction>> actionExecutors = new Dictionary<string, Action<AppState, IAction>>();
        private IDictionary<string, ObservableCollection<MqAlarm>> historyAlarmsDict;
        private readonly MqEffects mqEffects;
        private readonly DbEffects dbEffects;

        public AlarmCore(MqEffects mqEffects, DbEffects dbEffects) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.mqEffects = mqEffects;
            this.dbEffects = dbEffects;
            actionExecutors[AlarmActions.GENERATE_ONE_ALARM] = doGenerateOneAlarm;
        }

        private void doGenerateOneAlarm(AppState state, IAction action) {
            var alarmAction = (AlarmActions.GenerateOneAlarm)action;
            var machineCode = alarmAction.MachineCode;
            var alarmAdd = alarmAction.MqAlarm;
            if (alarmAdd == null) {
                return;
            }
            var key = alarmAction.MachineCode + alarmAdd.message;
            if (AlarmActions.GenerateOneAlarm.LastGenerateTimeDict.TryGetValue(key, out var lastTime)) {
                if ((DateTime.Now - lastTime).TotalSeconds < alarmAction.MinGapSec) {
                    return;
                }
            }
            AlarmActions.GenerateOneAlarm.LastGenerateTimeDict[key] = DateTime.Now;
            var historyAlarms = historyAlarmsDict[machineCode];
            var alarmRemove = historyAlarms.FirstOrDefault(a => a.code == alarmAdd.code);
            //fixed: 2018-01-15
            // 直接调用 UI Dispatcher 来更新
            Application.Current.Dispatcher.Invoke(() => {
                if (alarmRemove != null) {
                    historyAlarms.Remove(alarmRemove);
                }
                historyAlarms.Add(alarmAdd);
            });
            //打开报警灯5秒
            App.Store.Dispatch(new AlarmActions.OpenAlarmLights(machineCode, 5000));
            //打开屏幕
            App.Store.Dispatch(new SysActions.OpenScreen());
            //显示消息通知，相同通知10秒间隔只显示一次
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                Title = "警报",
                Content = machineCode + ":" + alarmAdd.message,
                MinGapSec = 10
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
            //订阅
            App.Store.Subscribe(actionExecutors);
        }
    }
}
