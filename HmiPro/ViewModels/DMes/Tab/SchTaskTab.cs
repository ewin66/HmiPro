using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.UI;
using HmiPro.Redux.Models;
using YCsharp.Util;

namespace HmiPro.ViewModels.DMes.Tab {
    public class SchTaskTab : BaseTab {
        public virtual ObservableCollection<MqSchTask> MqSchTasks { get; set; }
        /// <summary>
        /// 初始化任务数据
        /// </summary>
        /// <param name="mqSchTasks">排产任务</param>
        public void BindSource(ObservableCollection<MqSchTask> mqSchTasks) {
            MqSchTasks = mqSchTasks;
        }

    }
}
