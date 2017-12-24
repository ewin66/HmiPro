using System;
using System.Collections.Generic;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.ViewModels.DMes.Tab;

namespace HmiPro.ViewModels.DMes {
    /// <summary>
    /// 任务列表，含每轴的计划
    /// </summary>
    [POCOViewModel]
    public class SchTaskAxisViewModel:BaseTab {

        public virtual string MachineCode { get; set; }
        public virtual string WorkCode { get; set; }
        public virtual IList<MqTaskAxis> TaskAxisList { get; set; }

        public SchTaskAxisViewModel() {

        }

        public SchTaskAxisViewModel(string machineCode, string workCode, IList<MqTaskAxis> taskAxisList) {
            MachineCode = machineCode;
            WorkCode = workCode;
            TaskAxisList = taskAxisList;
        }

        [Command(Name = "StartTaskAxisDoingCommand")]
        public void StartTaskAxisDoing(object row) {
            var axis = (MqTaskAxis)row;
            App.Store.Dispatch(new DMesActions.StartSchTaskAxis(axis.maccode, axis.axiscode));
        }

        public static SchTaskAxisViewModel Create(string machineCode, string workCode, IList<MqTaskAxis> taskAxisList) {
            return ViewModelSource.Create(() => new SchTaskAxisViewModel(machineCode, workCode, taskAxisList));
        }

    }
}