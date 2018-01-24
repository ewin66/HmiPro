using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using HmiPro.Annotations;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Cores;
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
    public class DMesCoreViewModel : IDocumentContent, INotifyPropertyChanged {
        public virtual IDispatcherService DispatcherService => null;
        public virtual INavigationService NavigationSerivce => null;
        public virtual string MachineCode { get; set; }


        public virtual DMesCoreViewStore ViewStore { get; set; }

        public virtual ObservableCollection<BaseTab> ViewSource { get; set; } = new ObservableCollection<BaseTab>();
        public virtual CpmsTab CpmsTab { get; set; } = new CpmsTab() { Header = "参数" };
        public virtual SchTaskTab SchTaskTab { get; set; } = new SchTaskTab() { Header = "任务" };
        public virtual AlarmTab AlarmTab { get; set; } = new AlarmTab() { Header = "报警" };
        public virtual ScanMaterialTab ScanMaterialTab { get; set; } = new ScanMaterialTab() { Header = "来料" };
        public virtual Com485Tab Com485Tab { get; set; } = new Com485Tab() { Header = "通讯" };
        public virtual DpmsTab DpmsTab { get; set; } = new DpmsTab() { Header = "设置" };
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
            ViewSource.Add(DpmsTab);

            actionExecDict[DMesActions.RFID_ACCPET] = whenRfidAccept;
            actionExecDict[MqActions.SCAN_MATERIAL_ACCEPT] = whenScanMaterialAccpet;
            actionExecDict[DMesActions.CLEAR_SCH_TASKS] = clearSchTask;
            actionExecDict[CpmActions.UNREGISTERED_IP_ACTIVE] = unRegIpActived;
            actionExecDict[MqActions.SCH_TASK_REPLACED] = whenSchTaskReplaced;
        }


        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            //绑定实时参数
            var onlineCpmsDict = App.Store.GetState().CpmState.OnlineCpmsDict;
            CpmsTab.BindSource(MachineCode, onlineCpmsDict[MachineCode]);
            //绑定报警
            var alarmsDict = App.Store.GetState().AlarmState.AlarmsDict;
            AlarmTab.BindSource(MachineCode, alarmsDict[MachineCode]);
            //绑定任务
            var mqTaskDict = App.Store.GetState().DMesState.MqSchTasksDict;
            SchTaskTab.BindSource(MachineCode, mqTaskDict[MachineCode]);
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
            Com485Tab.BindSource(MachineCode, status);
            //回填参数
            var dpms = App.Store.GetState().DpmStore.DpmsDict;
            DpmsTab.BindSource(dpms[MachineCode]);

            //绑定选中的tab
            ViewStore = App.Store.GetState().ViewStoreState.DMewCoreViewDict[MachineCode];

            unsubscribe = App.Store.Subscribe(actionExecDict);
        }


        /// <summary>
        /// 有工单被顶掉，更新选中的任务
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenSchTaskReplaced(AppState state, IAction action) {
            var mqAction = (MqActions.SchTaskReplaced)action;
            //设置默认选中的工单任务
            if (mqAction.MachineCode == MachineCode) {
                DispatcherService.BeginInvoke(() => {
                    SchTaskTab.SetDefaultSelected();
                });
            }
        }

        /// <summary>
        /// 未注册 Ip 有活动
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void unRegIpActived(AppState state, IAction action) {
            var cpmAction = (CpmActions.UnregIpActived)action;
            var status = Com485Tab.Com485Status.FirstOrDefault(c => c.Ip == cpmAction.Ip);
            //添加未注册的 Ip 在界面上面显示
            if (status == null) {
                DispatcherService.BeginInvoke(() => {
                    if (state.CpmState.Com485StatusDict.TryGetValue(cpmAction.Ip, out var s)) {
                        Com485Tab.Com485Status.Add(s);
                    }
                });
            }
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
        /// 清空任务
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void clearSchTask(AppState state, IAction action) {
            SchTaskTab.Clear();
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
            //来料出问题
            if (mqAction.ScanMaterial?.type == false) {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "警告",
                    Content = $"{mqAction.MachineCode} 来料错误，请检查",
                }));
                App.Store.Dispatch(new AlarmActions.OpenAlarmLights(mqAction.MachineCode, 5000));
            }
            ScanMaterialTab.Update(mqAction.ScanMaterial);
        }

        [Command(Name = "NavigateCommand")]
        public void Navigate(Navigator nav) {
            var navViewStore = App.Store.GetState().ViewStoreState.NavView;
            if (nav.MachineCode != navViewStore.DMesSelectedMachineCode) {
                App.Store.Dispatch(new ViewStoreActions.ChangeDMesSelectedMachineCode(nav.MachineCode));
                var vm = DMesCoreViewModel.Create(nav.MachineCode);
                NavigationSerivce.Navigate("DMesCoreView", vm, null, this, false);
            }

        }

        /// <summary>
        /// 显示虚拟键盘
        /// </summary>
        [Command(Name = "CallOskCommand")]
        public void CallOsk() {
            YUtil.CallOskAsync();
        }

        /// <summary>
        /// 提交回传参数
        /// </summary>
        [Command(Name = "SubmitDpmsCommand")]
        public void SubmitDpms() {
            App.Store.Dispatch(new DpmActions.Submit(MachineCode, DpmsTab.Dpms));
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}