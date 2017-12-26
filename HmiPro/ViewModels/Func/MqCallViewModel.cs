using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Config;
using HmiPro.Helpers;
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

        public virtual IDictionary<string, List<MqCall>> MqCallDict { get; set; }
        readonly IDictionary<string, Action<AppState, IAction>> actionExecDict = new Dictionary<string, Action<AppState, IAction>>();

        public MqCallViewModel() {


        }

        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            MqCallDict = new Dictionary<string, List<MqCall>>();
            foreach (var pair in MachineConfig.MachineDict) {
                var machineCode = pair.Key;
                if (!MqCallDict.ContainsKey(machineCode)) {
                    MqCallDict[machineCode] = new List<MqCall>();
                }
                var callRepair = new MqCall() {
                    CallIcon = AssetsHelper.GetAssets().IconCallUp,
                    CallTxt = $"{machineCode} 报修",
                    MachineCode = machineCode,
                    Data = null,
                    CallType = MqCallType.Repair,
                };

                var callForklift = new MqCall() {
                    CallIcon = AssetsHelper.GetAssets().IconCallUp,
                    CallTxt = $"{machineCode} 叉车",
                    MachineCode = machineCode,
                    Data = null,
                    CallType = MqCallType.Forklift
                };

                MqCallDict[machineCode].Add(callRepair);
                MqCallDict[machineCode].Add(callForklift);
            }
        }

        [Command(Name = "CallCommand")]
        public void Call(MqCall mqCall) {
            //mqCall.CanCall = false;
        }
    }
}