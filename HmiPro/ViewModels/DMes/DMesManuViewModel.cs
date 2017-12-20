using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Config;
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
            execActionDict[DMesActions.DMES_SCH_TASK_ASSIGN] = schTaskAssign;
        }

        /// <summary>
        /// 
        /// </summary>
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

            //更新任务页面
            var mqSchTask = App.Store.GetState().MqState.MqSchTaskDict;
            foreach (var pair in mqSchTask) {
                if (SchTaskTabDict.TryGetValue(pair.Key, out var tab)) {
                    tab.Update(pair.Value);
                }
            }

            //监听系统信息
            unsubscribe = App.Store.Subscribe(s => {
                if (execActionDict.TryGetValue(s.Type, out var exec)) {
                    exec(s);
                }
            });
        }



        /// <summary>
        /// 分配新的任务
        /// </summary>
        /// <param name="state"></param>
        void schTaskAssign(AppState state) {
            foreach (var pair in state.DMesState.MqSchTaskDict) {
                var machineCode = pair.Key;
                if (SchTaskTabDict.TryGetValue(machineCode, out var tab)) {
                    tab.Update(pair.Value);
                }
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