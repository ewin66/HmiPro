using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using HmiPro.ViewModels.DMes.Form;
using YCsharp.Util;

namespace HmiPro.ViewModels.DMes.Tab {
    public class SchTaskTab : BaseTab, INotifyPropertyChanged {
        public ObservableCollection<MqSchTask> MqSchTasks { get; set; }
        public ObservableCollection<BaseTab> MqSchTaskDetails { get; set; } = new ObservableCollection<BaseTab>();
        private MqSchTask _selectedTask;
        private Visibility palletVisibility = Visibility.Collapsed;
        /// <summary>
        /// 是否含有栈板
        /// </summary>
        public Visibility PalletVisibility {
            get => palletVisibility;
            set {
                if (palletVisibility != value) {
                    palletVisibility = value;
                    RaisePropertyChanged(nameof(PalletVisibility));
                }
            }
        }

        public Pallet Pallet { get; set; }

        public SchTaskTab() {

        }

        public SchTaskTab(string header) {
            Header = header;
        }


        public string EmployeeStr { get; set; } = "/";
        private HashSet<string> employees = new HashSet<string>();

        public MqSchTask SelectedTask {
            get => _selectedTask;
            set {
                if (_selectedTask != value) {
                    MqSchTaskDetails.Clear();
                    _selectedTask = value;
                    if (_selectedTask == null) {
                        return;
                    }
                    SchTaskAxisViewModel = SchTaskAxisViewModel.Create(_selectedTask?.maccode, _selectedTask?.workcode, _selectedTask?.axisParam);
                    SchTaskAxisViewModel.Header = "任务";
                    RaisePropertyChanged(nameof(SchTaskAxisViewModel));

                    CraftBomViewModel = CraftBomViewModel.Create(_selectedTask?.maccode, _selectedTask?.workcode, _selectedTask?.bom);
                    CraftBomViewModel.Header = "Bom";
                    RaisePropertyChanged(nameof(CraftBomViewModel));

                    MqSchTaskDetails.Add(SchTaskAxisViewModel);
                    MqSchTaskDetails.Add(CraftBomViewModel);
                    RaisePropertyChanged(nameof(SelectedTask));

                    ViewStore.TaskSelectedWorkCode = _selectedTask?.workcode;

                }
            }
        }

        /// <summary>
        /// 清空任务
        /// </summary>
        public void Clear() {
            Application.Current.Dispatcher.Invoke(() => {
                foreach (var detail in MqSchTaskDetails) {
                    if (detail is CraftBomViewModel bomDetail) {
                        bomDetail?.Clear();
                    } else if (detail is SchTaskAxisViewModel axisDetail) {

                    }
                }
                MqSchTaskDetails.Clear();
            });
        }
        public SchTaskAxisViewModel SchTaskAxisViewModel { get; set; }
        public CraftBomViewModel CraftBomViewModel { get; set; }
        public string MachineCode { get; set; }
        public DMesCoreViewStore ViewStore { get; set; }

        /// <summary>
        /// 初始化任务数据
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="mqSchTasks">排产任务</param>
        public void BindSource(string machineCode, ObservableCollection<MqSchTask> mqSchTasks) {
            MachineCode = machineCode;
            MqSchTasks = mqSchTasks;
            //设置选中的工单
            ViewStore = App.Store.GetState().ViewStoreState.DMewCoreViewDict[machineCode];
            SetDefaultSelected();
            //绑定栈板
            if (GlobalConfig.PalletMachineCodes.Contains(machineCode)) {
                PalletVisibility = Visibility.Visible;
                Pallet = App.Store.GetState().DMesState.PalletDict[machineCode];
            }
        }

        /// <summary>
        /// 设置默认选中任务
        /// </summary>
        public void SetDefaultSelected() {
            SelectedTask = MqSchTasks.FirstOrDefault(t => t.workcode == ViewStore.TaskSelectedWorkCode) ?? MqSchTasks.FirstOrDefault();
        }

        /// <summary>
        /// 上机人员打卡后应添加在里面
        /// </summary>
        /// <param name="name"></param>
        public void AddEmployee(string name) {
            employees.Add(name);
            if (employees.Count > 0) {
                EmployeeStr = string.Join(",", employees);
            } else {
                EmployeeStr = "/";
            }
            RaisePropertyChanged(nameof(EmployeeStr));
        }

        /// <summary>
        /// 下机人员打卡后应删除
        /// </summary>
        /// <param name="name"></param>
        public void RemoveEmployee(string name) {
            employees.Remove(name);
            if (employees.Count > 0) {
                EmployeeStr = string.Join(",", employees);
            } else {
                EmployeeStr = "/";
            }
            RaisePropertyChanged(nameof(EmployeeStr));
        }

        /// <summary>
        /// 初始化人员卡信息
        /// </summary>
        /// <param name="mqEmpRfids"></param>
        public void InitEmployees(List<MqEmpRfid> mqEmpRfids) {
            foreach (var rfid in mqEmpRfids) {
                AddEmployee(rfid.name);
            }
        }

        /// <summary>
        /// 显示确定栈板上面轴数量
        /// </summary>
        [Command(Name = "ShowPalletViewCommand")]
        public void ShowPalletFormView() {
            if (!GlobalConfig.PalletMachineCodes.Contains(MachineCode)) {
                return;
            }
            var pallet = App.Store.GetState().DMesState.PalletDict[MachineCode];
            if (string.IsNullOrEmpty(pallet.Rfid)) {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "警告",
                    Content = "未扫描栈板的Rfid"
                }));
            }
            var workcode = App.Store.GetState().DMesState.SchTaskDoingDict[MachineCode].MqSchTask?.workcode;
            var form = new PalletConfirmForm(MachineCode, pallet.Rfid, pallet.AxisNum, workcode);
            App.Store.Dispatch(new SysActions.ShowFormView("确认栈板轴数量", form));
        }

        public static SchTaskTab Create(string header) {
            return ViewModelSource.Create(() => new SchTaskTab(header));
        }
    }
}
