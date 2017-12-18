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
        private readonly StorePro<AppState> store;
        private readonly List<Unsubscribe> unsubscribes = new List<Unsubscribe>();
        public virtual ObservableCollection<BaseTab> ViewSource { get; set; } = new ObservableCollection<BaseTab>();
        public readonly IDictionary<string, CpmsTab> CpmsTabDict = new Dictionary<string, CpmsTab>();

        public DMesManuViewModel() {
            store = UnityIocService.ResolveDepend<StorePro<AppState>>();
            foreach (var pair in MachineConfig.MachineDict) {
                var cpmsTab = new CpmsTab() { Header = pair.Key + "_参数" };
                cpmsTab.Init(pair.Value);
                //视图显示
                ViewSource.Add(cpmsTab);
                //用字典提高查找效率
                CpmsTabDict[pair.Key] = cpmsTab;
            }
        }

        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            initSubscribes();
        }

        private void initSubscribes() {
            var sub1 = store.Subscribe(s => {
                if (s.Type == CpmActions.CPMS_UPDATED_DIFF) {
                    var updatedCpms = s.CpmState.UpdatedCpmsDiff;
                    var tab = CpmsTabDict[s.CpmState.MachineCode];

                }
            });
            unsubscribes.Add(sub1);
        }

        public void OnClose(CancelEventArgs e) {
            //取消订阅
            unsubscribes.ForEach(cancel => cancel());
        }

        public void OnDestroy() {
        }

        public IDocumentOwner DocumentOwner { get; set; }
        public object Title { get; }
    }
}