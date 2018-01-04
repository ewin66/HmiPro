using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using HmiPro.ViewModels.DMes.Tab;
using HmiPro.ViewModels.Sys;
using Reducto;
using YCsharp.Model.Procotol.SmParam;
using YCsharp.Util;

namespace HmiPro.ViewModels.DMes {
    /// <summary>
    /// Dmes系统核心页面，含排产、报警、来料页面的合集
    /// <author>ychost</author>
    /// <date>2017-12-27</date>
    /// </summary>
    [POCOViewModel]
    public class DMesCoreViewModel : IDocumentContent {
        public virtual IDispatcherService DispatcherService => null;
        public virtual INavigationService NavigationSerivce => null;
        public virtual string MachineCode { get; set; }
        public virtual ObservableCollection<BaseTab> ViewSource { get; set; } = new ObservableCollection<BaseTab>();
        public virtual CpmsTab CpmsTab { get; set; } = new CpmsTab() { Header = "参数" };
        public virtual SchTaskTab SchTaskTab { get; set; } = new SchTaskTab() { Header = "任务" };
        public virtual AlarmTab AlarmTab { get; set; } = new AlarmTab() { Header = "报警" };
        public virtual ScanMaterialTab ScanMaterialTab { get; set; } = new ScanMaterialTab() { Header = "来料" };
        public virtual Com485Tab Com485Tab { get; set; } = new Com485Tab() { Header = "通讯" };
        private Unsubscribe unsubscribe;
        readonly IDictionary<string, Action<AppState, IAction>> actionExecDict = new Dictionary<string, Action<AppState, IAction>>();
        public virtual string Header { get; set; }

        public DMesCoreViewModel(string machineCode) : this() {
            MachineCode = machineCode;
            Header = MachineCode + " 生产管理";
        }

        public DMesCoreViewModel() {
            ViewSource.Add(CpmsTab);
            ViewSource.Add(SchTaskTab);
            ViewSource.Add(AlarmTab);
            ViewSource.Add(ScanMaterialTab);
            ViewSource.Add(Com485Tab);

            actionExecDict[AlarmActions.UPDATE_HISTORY_ALARMS] = whenUpdateHistoryAlarms;
            actionExecDict[DMesActions.RFID_ACCPET] = whenRfidAccept;
            actionExecDict[MqActions.SCAN_MATERIAL_ACCEPT] = whenScanMaterialAccpet;
        }


        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            //绑定实时参数
            var onlineCpmsDict = App.Store.GetState().CpmState.OnlineCpmsDict;
            CpmsTab.BindSource(MachineCode, onlineCpmsDict[MachineCode]);
            //初始化报警
            var alarmsDict = App.Store.GetState().AlarmState.AlarmsDict;
            AlarmTab.Init(alarmsDict[MachineCode]);
            //绑定任务
            var mqTaskDict = App.Store.GetState().DMesState.MqSchTasksDict;
            SchTaskTab.BindSource(mqTaskDict[MachineCode]);
            //初始化人员卡
            var mqEmpRfids = App.Store.GetState().DMesState.MqEmpRfidDict;
            SchTaskTab.InitEmployees(mqEmpRfids[MachineCode]);
            //初始化来料
            var scanMaterialDict = App.Store.GetState().DMesState.MqScanMaterialDict;
            if (scanMaterialDict.TryGetValue(MachineCode, out var material)) {
                ScanMaterialTab.Update(material);
            }
            //绑定485通讯状态
            var com485Dict = App.Store.GetState().CpmState.Com485StatusDict;
            var status = com485Dict.Where(c => MachineConfig.MachineCodeToIpsDict[MachineCode].Contains(c.Key))
                .Select(c => c.Value).ToList();
            Com485Tab.BindSource(status);

            //订阅派发事件
            unsubscribe = App.Store.Subscribe((state, action) => {
                if (actionExecDict.TryGetValue(action.Type(), out var exec)) {
                    exec(state, action);
                }
            });
        }

        /// <summary>
        /// 接收到Rfid数据
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenRfidAccept(AppState state, IAction action) {
            var dmesAction = (DMesActions.RfidAccpet)action;
            if (dmesAction.MachineCode != MachineCode) {
                return;
            }
            //上机卡
            if (dmesAction.RfidType == DMesActions.RfidType.EmpStartMachine) {
                var mqRfid = (MqEmpRfid)dmesAction.MqData;
                SchTaskTab.AddEmployee(mqRfid.name);
            }
            //下机卡
            if (dmesAction.RfidType == DMesActions.RfidType.EmpEndMachine) {
                var mqRfid = (MqEmpRfid)dmesAction.MqData;
                SchTaskTab.RemoveEmployee(mqRfid.name);
            }
        }

        /// <summary>
        /// 报警用的DataGrid每次都会add or remove 必须通过 UI 调度器
        /// 这里是保持 UI 显示和 State 的报警数据一致
        /// </summary>
        void whenUpdateHistoryAlarms(AppState state, IAction action) {
            var alarmAction = (AlarmActions.UpdateHistoryAlarms)action;
            if (alarmAction.MachineCode != MachineCode) {
                return;
            }
            DispatcherService.BeginInvoke(() => {
                if (alarmAction.UpdateAction == AlarmActions.UpdateAction.Add) {
                    AlarmTab.Alarms.Add(alarmAction.MqAlarmAdd);

                } else if (alarmAction.UpdateAction == AlarmActions.UpdateAction.Change) {
                    if (!AlarmTab.Alarms.Remove(alarmAction.MqAlarmRemove)) {
                        //fixed:2017-12-22
                        // 直接remove alarmActon.MqAlarmRemove 会失败
                        var removeItem = AlarmTab.Alarms.FirstOrDefault(s => s.code == alarmAction.MqAlarmRemove.code);
                        AlarmTab.Alarms.Remove(removeItem);
                    }
                    AlarmTab.Alarms.Add(alarmAction.MqAlarmAdd);
                }
            });
        }
        /// <summary>
        /// 扫描来料
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenScanMaterialAccpet(AppState state, IAction action) {
            var mqAction = (MqActions.ScanMaterialAccpet)action;
            if (mqAction.MachineCode != MachineCode) {
                return;
            }
            ScanMaterialTab.Update(mqAction.ScanMaterial);
        }

        [Command(Name = "NavigateCommand")]
        public void Navigate(Navigator nav) {
            if (nav.MachineCode != NavigatorViewModel.NavMachineCodeInDoing) {
                var vm = DMesCoreViewModel.Create(nav.MachineCode);
                NavigatorViewModel.NavMachineCodeInDoing = nav.MachineCode;
                NavigationSerivce.Navigate("DMesCoreView", vm, null, this, false);
            }

        }

        public static DMesCoreViewModel Create(string machineCode) {
            return ViewModelSource.Create<DMesCoreViewModel>(() => new DMesCoreViewModel(machineCode));
        }

        public void OnClose(CancelEventArgs e) {
            unsubscribe?.Invoke();
        }

        public void OnDestroy() {

        }

        public IDocumentOwner DocumentOwner { get; set; }
        public object Title { get; }
    }
}