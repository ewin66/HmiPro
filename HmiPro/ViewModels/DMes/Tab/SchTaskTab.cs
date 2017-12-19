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
        public virtual SchTaskList SchTaskList { get; set; } = new SchTaskList();
        private string machineCode;
        /// <summary>
        /// 机台编码
        /// </summary>
        public string MachineCode {
            get { return machineCode; }
            set {
                if (machineCode != value) {
                    machineCode = value;
                    SchTaskList.MachineCode = value;
                    OnPropertyChanged(nameof(MachineCode));
                }
            }
        }

        private string canDoingTxt;
        /// <summary>
        /// “开始生产” 按钮文本
        /// </summary>
        public string CanDoingTxt {
            get { return canDoingTxt; }
            set {
                if (canDoingTxt != value) {
                    SchTaskList.CanDoingTxt = value;
                    OnPropertyChanged(nameof(CanDoingTxt));
                }
            }
        }




        /// <summary>
        /// 接受到排产任务
        /// </summary>
        /// <param name="dispatchTask">排产任务</param>
        public void Update(MqSchTask dispatchTask) {
            //排产任务模型==>视图显示模型
            SchTaskList.MachineCode = dispatchTask.maccode;
            SchTaskList.Order = dispatchTask.workcode;
            SchTaskList.PlanStartTime = YUtil.UtcTimestampToLocalTime(dispatchTask.pstime.time);
            SchTaskList.PlanEndTime = YUtil.UtcTimestampToLocalTime(dispatchTask.pdtime.time);
            SchTaskList.AxisListDetails = new ObservableCollection<AxisListDetail>();
            dispatchTask.axisParam.ForEach(axis => {
                SchTaskList.AxisListDetails.Add(new AxisListDetail() {
                    AxisNo = axis.axiscode,
                    Color = axis.color,
                    CompletedRate = "0%",
                    Length = axis.length
                });
            });
        }
    }
}
