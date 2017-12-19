using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.ViewModels.DMes.Tab;
using Reducto;
using YCsharp.Service;

namespace HmiPro.ViewModels.DMes {
    [POCOViewModel]
    public class DMesManuViewModel : IDocumentContent {
        private readonly List<Unsubscribe> subscribes = new List<Unsubscribe>();
        public virtual ObservableCollection<BaseTab> ViewSource { get; set; } = new ObservableCollection<BaseTab>();
        public virtual IDispatcherService DispatcherService => null;
        public readonly IDictionary<string, CpmsTab> CpmsTabDict = new Dictionary<string, CpmsTab>();
        public readonly IDictionary<string, SchTaskTab> SchTaskTabDict = new Dictionary<string, SchTaskTab>();
     
        public DMesManuViewModel() {
            foreach (var pair in MachineConfig.MachineDict) {
                var cpmsTab = new CpmsTab() { Header = pair.Key + "_参数" };
                var schTaskTab = new SchTaskTab() { Header = pair.Key + "_任务" };
                //视图显示
                ViewSource.Add(cpmsTab);
                ViewSource.Add(schTaskTab);
                //用字典提高查找效率
                CpmsTabDict[pair.Key] = cpmsTab;
                SchTaskTabDict[pair.Key] = schTaskTab;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            initSubscribes();
            //将最新的所有参数更新到显示
            var onlineCpmsDict = App.Store.GetState().CpmState.OnlineCpmsDict;
            foreach (var pair in onlineCpmsDict) {
                var code = pair.Key;
                CpmsTabDict[code].BindSource(onlineCpmsDict[code]);
            }
            var mqSchTask = App.Store.GetState().MqState.MqSchTaskDict;
            foreach (var pair in mqSchTask) {
                if (SchTaskTabDict.TryGetValue(pair.Key, out var tab)) {
                    tab.Update(pair.Value);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void initSubscribes() {
            var sub1 = App.Store.Subscribe(s => {
                if (s.Type == DMesActions.DMES_SCH_TASK_ASSIGN) {
                    foreach (var pair in s.DMesState.MqSchTaskDict) {
                        var machineCode = pair.Key;
                        if (SchTaskTabDict.TryGetValue(machineCode, out var tab)) {
                            tab.Update(pair.Value);
                        }
                    }
                }
            });
            subscribes.Add(sub1);
        }

        public void OnClose(CancelEventArgs e) {
            //取消订阅
            subscribes.ForEach(cancel => cancel());
        }

        public void OnDestroy() {
        }

        public IDocumentOwner DocumentOwner { get; set; }
        public object Title { get; }
    }
}