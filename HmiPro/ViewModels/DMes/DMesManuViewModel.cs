using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.ViewModels.DMes.Tab;
using Reducto;
using YCsharp.Service;

namespace HmiPro.ViewModels.DMes {
    [POCOViewModel]
    public class DMesManuViewModel : IDocumentContent {
        public virtual ObservableCollection<BaseTab> ViewSource { get; set; } = new ObservableCollection<BaseTab>();
        public virtual IDispatcherService DispatcherService => null;
        public readonly IDictionary<string, CpmsTab> CpmsTabDict = new Dictionary<string, CpmsTab>();
        public readonly IDictionary<string, SchTaskTab> SchTaskTabDict = new Dictionary<string, SchTaskTab>();
        public readonly IDictionary<string, AlarmTab> AlarmTabDict = new Dictionary<string, AlarmTab>();
        readonly IDictionary<string, Action<AppState>> execActionDict = new ConcurrentDictionary<string, Action<AppState>>();
        public string OeeStr = "";
        private Unsubscribe unsubscribe;
        public readonly LoggerService Logger;

        public DMesManuViewModel() {
            foreach (var pair in MachineConfig.MachineDict) {
                var cpmsTab = new CpmsTab() { Header = pair.Key + "参数" };
                var schTaskTab = new SchTaskTab() { Header = pair.Key + "任务" };
                var alarmTab = new AlarmTab() { Header = pair.Key + "报警" };
                //视图显示
                ViewSource.Add(cpmsTab);
                ViewSource.Add(schTaskTab);
                ViewSource.Add(alarmTab);
                //用字典提高查找效率
                CpmsTabDict[pair.Key] = cpmsTab;
                SchTaskTabDict[pair.Key] = schTaskTab;
                AlarmTabDict[pair.Key] = alarmTab;
            }
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }


        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            //绑定实时参数页面
            var onlineCpmsDict = App.Store.GetState().CpmState.OnlineCpmsDict;
            foreach (var pair in onlineCpmsDict) {
                CpmsTabDict[pair.Key].BindSource(pair.Key, pair.Value);
            }
            //绑定报警页面
            var alarmDict = App.Store.GetState().AlarmState.NotifyAlarmDict;
            foreach (var pair in alarmDict) {
                AlarmTabDict[pair.Key].BindSource(pair.Value);
            }
            //绑定Oee
            var oeeDict = App.Store.GetState().OeeState.OeeDict;
            foreach (var pair in oeeDict) {
                //OeeDict[pair.Key] = pair.Value;
            }
            //绑定任务页面
            var mqTasks = App.Store.GetState().DMesState.MqSchTasksDict;
            foreach (var pair in mqTasks) {
                if (SchTaskTabDict.TryGetValue(pair.Key, out var tab)) {
                    tab.BindSource(pair.Value);
                }
            }
            //监听系统信息
            unsubscribe = App.Store.Subscribe((state, action) => {
                if (execActionDict.TryGetValue(state.Type, out var exec)) {
                    exec(state);
                }
            });
        }

        [Command(Name = "StartTaskAxisDoingCommand")]
        public void StartTaskAxisDoing(string machineCode_axis_isStarted) {
            if (machineCode_axis_isStarted?.Split('_')?.Length == 3) {
                var arr = machineCode_axis_isStarted.Split('_');
                var machineCode = arr[0];
                var axisCode = arr[1];
                bool.TryParse(arr[2], out var isStarted);
                App.Store.Dispatch(new DMesActions.StartSchTaskAxis(machineCode, axisCode));
            } else {
                Logger.Error("派发的工单数据有误  machineCode_axis_isStarted:=" + machineCode_axis_isStarted);
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "派单系统故障",
                    Content = "任务数据有误，请联系管理员"
                }));
            }

        }

        public void OnClose(CancelEventArgs e) {
            //取消订阅
            if (unsubscribe != null) {
                unsubscribe();
            }
        }

        public void OnDestroy() {
        }

        public IDocumentOwner DocumentOwner { get; set; }
        public object Title { get; }
    }
}