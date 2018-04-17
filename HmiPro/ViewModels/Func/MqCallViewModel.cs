using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using HmiPro.ViewModels.DMes.Form;
using Newtonsoft.Json;
using YCsharp.Service;
using YCsharp.Util;

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
            MqCallDict = new Dictionary<string, List<MqCall>>();
            foreach (var pair in MachineConfig.MachineDict) {
                var machineCode = pair.Key;
                if (!MqCallDict.ContainsKey(machineCode)) {
                    MqCallDict[machineCode] = new List<MqCall>();
                }
                var callRepair = new MqCall() {
                    CallIcon = AssetsHelper.GetAssets().IconRepair,
                    CallTxt = $"{machineCode} 报修",
                    MachineCode = machineCode,
                    Data = null,
                    CallType = MqCallType.Repair,
                };

                var callForklift = new MqCall() {
                    CallIcon = AssetsHelper.GetAssets().IconForklift,
                    CallTxt = $"{machineCode} 叉车",
                    MachineCode = machineCode,
                    Data = null,
                    CallType = MqCallType.Forklift
                };

                var callRepairComplete = new MqCall() {
                    CallIcon = AssetsHelper.GetAssets().IconV,
                    CallTxt = $"{machineCode} 完成维修",
                    MachineCode = machineCode,
                    Data = null,
                    CallType = MqCallType.RepairComplete
                };

                MqCallDict[machineCode].Add(callRepair);
                MqCallDict[machineCode].Add(callForklift);
                MqCallDict[machineCode].Add(callRepairComplete);
            }
        }

        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            ;
        }

        [Command(Name = "CallCommand")]
        public void Call(MqCall selectCall) {
            if (selectCall.CallType == MqCallType.Repair) {
                execCallRepair(selectCall);
            } else if (selectCall.CallType == MqCallType.RepairComplete) {
                execCallRepairComplete(selectCall);
            } else if (selectCall.CallType == MqCallType.Forklift) {
                execCallForklift(selectCall);
            }
        }

        /// <summary>
        /// 呼叫叉车
        /// </summary>
        /// <param name="selectCall"></param>
        /// <returns></returns>
        async Task execCallForklift(MqCall selectCall) {
            var call = new Redux.Models.MqCall() {
                machineCode = selectCall.MachineCode,
                callType = Redux.Models.MqCallType.Forklift,
                CallId = Guid.NewGuid().GetHashCode(),
            };
            App.Logger.Info("呼叫你叉车: " + JsonConvert.SerializeObject(call));
            var callSuccess = await App.Store.Dispatch(UnityIocService.ResolveDepend<MqEffects>().CallSystem(new MqActions.CallSystem(selectCall.MachineCode, call)));
            if (callSuccess) {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "通知",
                    Content = $"{selectCall.MachineCode} 呼叫叉车成功"
                }));
            } else {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "警告",
                    Content = $"{selectCall.MachineCode} 呼叫叉车失败，请重试"
                }));
            }
        }

        /// <summary>
        /// 呼叫维修完成
        /// </summary>
        /// <param name="selectCall"></param>
        /// <returns></returns>
        async Task execCallRepairComplete(MqCall selectCall) {
            var call = new Redux.Models.MqCall() {
                machineCode = selectCall.MachineCode,
                callType = Redux.Models.MqCallType.RepairComplete,
                CallId = Guid.NewGuid().GetHashCode(),
            };
            App.Logger.Info("维修完成: " + JsonConvert.SerializeObject(call));
            var callSuccess = await App.Store.Dispatch(UnityIocService.ResolveDepend<MqEffects>().CallSystem(new MqActions.CallSystem(selectCall.MachineCode, call)));
            if (callSuccess) {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "通知",
                    Content = $"{selectCall.MachineCode} 维修完成"
                }));
            } else {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "警告",
                    Content = $"{selectCall.MachineCode} 维修完成状态上传失败，请重试"
                }));
            }
        }

        /// <summary>
        /// 呼叫维修
        /// </summary>
        /// <param name="selectCall"></param>
        void execCallRepair(MqCall selectCall) {
            var frm = new CallRepairForm() {
                OnOkPressed = async f => {
                    var repairForm = f as CallRepairForm;
                    var call = new Redux.Models.MqCall() {
                        machineCode = selectCall.MachineCode,
                        callType = Redux.Models.MqCallType.Repair,
                        callAction = repairForm.RepairType.GetAttribute<DisplayAttribute>().Name,
                        CallId = Guid.NewGuid().GetHashCode(),
                    };
                    App.Logger.Info("呼叫维修: " + JsonConvert.SerializeObject(call));
                    var callSuccess = await App.Store.Dispatch(UnityIocService.ResolveDepend<MqEffects>().CallSystem(new MqActions.CallSystem(selectCall.MachineCode, call)));
                    if (callSuccess) {
                        App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                            Title = "通知",
                            Content = $"{selectCall.MachineCode} 申报维修成功 {repairForm.RepairType.GetAttribute<DisplayAttribute>().Name }"
                        }));
                    } else {
                        App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                            Title = "警告",
                            Content = $"{selectCall.MachineCode} 申报维修失败 {repairForm.RepairType.GetAttribute<DisplayAttribute>().Name }，请重试"
                        }));
                    }
                }
            };
            App.Store.Dispatch(new SysActions.ShowFormView("选择故障类型", frm));
        }
    }
}