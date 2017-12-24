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
    public class SchTaskTab : BaseTab {
        public virtual ObservableCollection<MqSchTask> MqSchTasks { get; set; }
        public ObservableCollection<BaseTab> MqSchTaskDetails { get; set; } = new ObservableCollection<BaseTab>();
        private MqSchTask schTask;




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

    }
}
