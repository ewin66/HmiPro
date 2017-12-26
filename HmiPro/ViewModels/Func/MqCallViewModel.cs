using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Reducers;

namespace HmiPro.ViewModels.Func {
    /// <summary>
    /// 呼叫叉车、呼叫维修、呼叫质检的功能页面
    /// <date>2017-12-26</date>
    /// <author>ychost</author>
    /// </summary>
    [POCOViewModel]
    public class MqCallViewModel {

        public ObservableCollection<MqCallViewModel> MqCalls { get; set; } = new ObservableCollection<MqCallViewModel>();

        readonly IDictionary<string, Action<AppState, IAction>> actionExecDict = new Dictionary<string, Action<AppState, IAction>>();

        public MqCallViewModel() {

        }

        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            foreach (var pair in MachineConfig.MachineDict) {
                var machineCode = pair.Key;
            }
        }
    }
}