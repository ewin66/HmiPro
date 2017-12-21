using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraExport.Xls;
using HmiPro.Annotations;
using HmiPro.Redux.Models;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// <date>2017-12-21</date>
    /// <author>ychost</author>
    /// </summary>
    public class SchTaskViewModel : INotifyPropertyChanged {

        private string workCode;
        /// <summary>
        /// 工单
        /// </summary>
        public string WorkCode {
            get { return workCode; }
            set {
                if (workCode != value) {
                    workCode = value;
                    OnPropertyChanged(nameof(WorkCode));
                }
            }
        }

        public ObservableCollection<SchTaskAxis> TaskAxises { get; set; }

        public void Init(MqSchTask mqSchTasks) {
            WorkCode = mqSchTasks.workcode;
            TaskAxises = new ObservableCollection<SchTaskAxis>();
            foreach (var axis in mqSchTasks.axisParam)
            {
                TaskAxises.Add(new SchTaskAxis()
                {
                    AxisCode = axis.axiscode,
                    Color = axis.color,
                    CompletedRate = axis.CompletedRate
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// <date>2017-12-21</date>
    /// <author>ychost</author>
    /// </summary>
    public class SchTaskAxis : INotifyPropertyChanged {

        private string axisCode;
        /// <summary>
        /// 轴号
        /// </summary>
        public string AxisCode {
            get { return axisCode; }
            set {
                if (axisCode != value) {
                    axisCode = value;
                    OnPropertyChanged(nameof(AxisCode));
                }
            }
        }

        private string color;
        /// <summary>
        /// 轴号颜色
        /// </summary>
        public string Color {
            get { return color; }
            set {
                if (color != value) {
                    color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        private float completedRate;
        /// <summary>
        /// 完成率，浮点型比如：0.56
        /// </summary>
        public float CompletedRate {
            get { return completedRate; }
            set {
                if (completedRate != value) {
                    completedRate = value;
                    OnPropertyChanged(nameof(CompletedRate));
                    CompletedRatePercent = (completedRate * 100).ToString("#0.00") + "%";
                    OnPropertyChanged(nameof(CompletedRatePercent));
                }
            }
        }

        /// <summary>
        /// 完成率字符串，比如：56%，自动生成的
        /// </summary>
        public string CompletedRatePercent { get; private set; } = "0.00%";




        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
