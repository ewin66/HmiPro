using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.UI;
using HmiPro.Redux.Models;
using YCsharp.Util;

namespace HmiPro.ViewModels.DMes.Tab {
    public class SchTaskTab : BaseTab,INotifyPropertyChanged {
        public virtual ObservableCollection<MqSchTask> MqSchTasks { get; set; }
        public ObservableCollection<BaseTab> MqSchTaskDetails { get; set; } = new ObservableCollection<BaseTab>();
        private MqSchTask schTask;

        public string EmployeeStr { get; set; } = "/";
        private HashSet<string> employees = new HashSet<string>();

        public MqSchTask SelectedTask {
            get => schTask;
            set {
                if (schTask != value && value != null) {
                    MqSchTaskDetails.Clear();
                    schTask = value;
                    SchTaskAxisViewModel = SchTaskAxisViewModel.Create(schTask.maccode, schTask.workcode, schTask.axisParam);
                    SchTaskAxisViewModel.Header = "任务";
                    OnPropertyChanged(nameof(SchTaskAxisViewModel));

                    CraftBomViewModel = CraftBomViewModel.Create(schTask.maccode, schTask.workcode, schTask.bom);
                    CraftBomViewModel.Header = "Bom";
                    OnPropertyChanged(nameof(CraftBomViewModel));

                    MqSchTaskDetails.Add(SchTaskAxisViewModel);
                    MqSchTaskDetails.Add(CraftBomViewModel);

                    OnPropertyChanged(nameof(SelectedTask));
                }
            }
        }

        public SchTaskAxisViewModel SchTaskAxisViewModel { get; set; }
        public CraftBomViewModel CraftBomViewModel { get; set; }

        /// <summary>
        /// 初始化任务数据
        /// </summary>
        /// <param name="mqSchTasks">排产任务</param>
        public void BindSource(ObservableCollection<MqSchTask> mqSchTasks) {
            MqSchTasks = mqSchTasks;
            if (mqSchTasks.Count > 0) {
                SelectedTask = mqSchTasks.FirstOrDefault();
            }
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
            OnPropertyChanged(nameof(EmployeeStr));
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
            OnPropertyChanged(nameof(EmployeeStr));
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
