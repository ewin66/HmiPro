using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.UI;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using YCsharp.Util;

namespace HmiPro.ViewModels.DMes.Tab {
    public class SchTaskTab : BaseTab, INotifyPropertyChanged {
        public ObservableCollection<MqSchTask> MqSchTasks { get; set; }
        public ObservableCollection<BaseTab> MqSchTaskDetails { get; set; } = new ObservableCollection<BaseTab>();
        private MqSchTask _selectedTask;

        public string EmployeeStr { get; set; } = "/";
        private HashSet<string> employees = new HashSet<string>();

        public MqSchTask SelectedTask {
            get => _selectedTask;
            set {
                if (_selectedTask != value && value != null) {
                    MqSchTaskDetails.Clear();
                    _selectedTask = value;
                    SchTaskAxisViewModel = SchTaskAxisViewModel.Create(_selectedTask.maccode, _selectedTask.workcode, _selectedTask.axisParam);
                    SchTaskAxisViewModel.Header = "任务";
                    RaisePropertyChanged(nameof(SchTaskAxisViewModel));

                    CraftBomViewModel = CraftBomViewModel.Create(_selectedTask.maccode, _selectedTask.workcode, _selectedTask.bom);
                    CraftBomViewModel.Header = "Bom";
                    RaisePropertyChanged(nameof(CraftBomViewModel));

                    MqSchTaskDetails.Add(SchTaskAxisViewModel);
                    MqSchTaskDetails.Add(CraftBomViewModel);
                    RaisePropertyChanged(nameof(SelectedTask));

                    ViewStore.TaskSelectedWorkCode = _selectedTask.workcode;

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
            SelectedTask = mqSchTasks.FirstOrDefault(t => t.workcode == ViewStore.TaskSelectedWorkCode) ?? mqSchTasks.FirstOrDefault();
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

    }
}
