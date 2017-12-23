using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        readonly IDictionary<string, Action<AppState, IAction>> actionsExecDict = new ConcurrentDictionary<string, Action<AppState, IAction>>();
        public string OeeStr = "";
        private Unsubscribe unsubscribe;
        public readonly LoggerService Logger;
        public virtual INavigationService NavigationSerivce => null;

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
            actionsExecDict[AlarmActions.UPDATE_HISTORY_ALARMS] = whenUpdateHistoryAlarms;
        }

        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            //绑定实时参数页面
            var onlineCpmsDict = App.Store.GetState().CpmState.OnlineCpmsDict;
            foreach (var pair in onlineCpmsDict) {
                CpmsTabDict[pair.Key].BindSource(pair.Key, pair.Value);
            }

            //初始化报警界面
            //将已经产生的历史报警显示在界面上
            var alarmsDict = App.Store.GetState().AlarmState.AlarmsDict;
            foreach (var pair in alarmsDict) {
                var machineCode = pair.Key;
                if (AlarmTabDict.TryGetValue(machineCode, out var tab)) {
                    tab.Init(pair.Value);
                }
            }

            //绑定Oee
            var oeeDict = App.Store.GetState().OeeState.OeeDict;
            foreach (var pair in oeeDict) {
                //OeeDict[pair.Key] = pair.Value;
            }
            //DataGrid不支持非UI线程add or remove
            //但是任务页面里面的任务详细列表一旦初始化后项目就不会增加减少
            //所以可以直接绑定
            var mqTasks = App.Store.GetState().DMesState.MqSchTasksDict;
            foreach (var pair in mqTasks) {
                if (SchTaskTabDict.TryGetValue(pair.Key, out var tab)) {
                    tab.BindSource(pair.Value);
                }

            }
            //绑定Bom

            //监听系统信息
            unsubscribe = App.Store.Subscribe((state, action) => {
                if (actionsExecDict.TryGetValue(action.Type(), out var exec)) {
                    exec(state, action);
                }
            });
        }

        /// <summary>
        /// 报警用的DataGrid每次都会add or remove 必须通过 UI 调度器
        /// 这里是保持 UI 显示和 State 的报警数据一致
        /// </summary>
        public void whenUpdateHistoryAlarms(AppState state, IAction action) {
            DispatcherService.BeginInvoke(() => {
                var alarmAction = (AlarmActions.UpdateHistoryAlarms)action;
                if (alarmAction.UpdateAction == AlarmActions.UpdateAction.Add) {
                    AlarmTabDict[alarmAction.MachineCode].Alarms.Add(alarmAction.MqAlarmAdd);

                } else if (alarmAction.UpdateAction == AlarmActions.UpdateAction.Change) {
                    if (!AlarmTabDict[alarmAction.MachineCode].Alarms.Remove(alarmAction.MqAlarmRemove)) {
                        //fixed:2017-12-22
                        // 直接remove alarmActon.MqAlarmRemove 会失败
                        var removeItem = AlarmTabDict[alarmAction.MachineCode].Alarms.FirstOrDefault(s => s.code == alarmAction.MqAlarmRemove.code);
                        AlarmTabDict[alarmAction.MachineCode].Alarms.Remove(removeItem);
                    }
                    AlarmTabDict[alarmAction.MachineCode].Alarms.Add(alarmAction.MqAlarmAdd);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="machineCode_axis_isStarted"></param>
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

        /// <summary>
        /// 导航到Bom表参数页面
        /// </summary>
        /// <param name="params"></param>
        [Command(Name = "NavigateToBomViewCommand")]
        public void NavigateToBomView(object[] @params) {
            if (@params?.Length == 2) {
                var machineCode = @params[0].ToString();
                var workCode = @params[1].ToString();
                var mqSchTasksDict = App.Store.GetState().DMesState.MqSchTasksDict;
                if (mqSchTasksDict.TryGetValue(machineCode, out var tasks)) {
                    var task = tasks.FirstOrDefault(t => t.workcode == workCode);
                    var vm = CraftBomViewModel.Create(machineCode, workCode, task?.bom);
                    //var vm = new CraftBomViewModel(machineCode,workCode,task?.bom);
                    NavigationSerivce.Navigate("CraftBomView", vm, null, this,true);
                }

            }
        }

        [Command(Name = "NavigateToTaskAxisViewCommand")]
        public void NavigateToTaskAxisView(object[] @params) {
            if (@params?.Length == 2) {
                var machineCode = @params[0].ToString();
                var workCode = @params[1].ToString();
                var mqSchTasksDict = App.Store.GetState().DMesState.MqSchTasksDict;
                if (mqSchTasksDict.TryGetValue(machineCode, out var tasks)) {
                    var task = tasks.FirstOrDefault(t => t.workcode == workCode);
                    var vm = SchTaskAxisViewModel.Create(machineCode, workCode, task?.axisParam);
                    //var vm = new CraftBomViewModel(machineCode,workCode,task?.bom);
                    NavigationSerivce.Navigate("SchTaskAxisView", vm, null, this,true);
                }

            }
        }

        [Command(Name = "GoBackCommand")]
        public void GoBack() {
            NavigationSerivce.GoBack(null);
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