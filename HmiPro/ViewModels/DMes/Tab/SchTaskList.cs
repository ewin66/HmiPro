using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Annotations;

namespace HmiPro.ViewModels.DMes.Tab {
    public class SchTaskList : INotifyPropertyChanged {

        private string machineCode;
        /// <summary>
        /// 机台编号
        /// </summary>
        public string MachineCode {
            get { return machineCode; }
            set {
                if (machineCode != value) {
                    machineCode = value;
                    OnPropertyChanged(nameof(MachineCode));
                }
            }
        }


        /// <summary>
        /// 工单号
        /// </summary>
        private string order;
        public string Order {
            get { return order; }
            set {
                if (order != value) {
                    order = value;
                    OnPropertyChanged(nameof(Order));
                }

            }
        }

        /// <summary>
        /// 计划开始时间
        /// </summary>
        private DateTime planStartTime;
        public DateTime PlanStartTime {
            get => planStartTime;
            set {
                if (value != planStartTime) {
                    planStartTime = value;
                    OnPropertyChanged(nameof(PlanStartTime));
                    OnPropertyChanged(nameof(PlanStartTimeStr));
                }
            }
        }

        /// <summary>
        /// 计划结束时间
        /// </summary>
        private DateTime planEndTime;
        public DateTime PlanEndTime {
            get { return planEndTime; }
            set {
                if (value != planEndTime) {
                    planEndTime = value;
                    OnPropertyChanged(nameof(PlanEndTime));
                    OnPropertyChanged(PlanEndTimeStr);

                }
            }
        }

        public string PlanStartTimeStr => planStartTime.ToString("yyyy-MM-dd HH:mm");
        public string PlanEndTimeStr => planEndTime.ToString("yyyy-MM-dd HH:mm");

        /// <summary>
        /// 操作手
        /// </summary>
        private string @operator;
        public string Operator {
            get { return @operator; }
            set {
                if (value != @operator) {
                    @operator = value;
                    OnPropertyChanged(nameof(Operator));
                }

            }
        }

        private ObservableCollection<AxisListDetail> axisListDetails;
        /// <summary>
        /// 任务列表
        /// </summary>
        public ObservableCollection<AxisListDetail> AxisListDetails {
            get { return axisListDetails; }
            set {
                if (axisListDetails != value) {
                    axisListDetails = value;
                    OnPropertyChanged(nameof(AxisListDetails));
                }
            }
        }

        private string doingAxis;
        /// <summary>
        /// 当前工作轴号
        /// </summary>
        public string DoingAxis {
            get => doingAxis;
            set {
                if (doingAxis != value) {
                    doingAxis = value;
                    OnPropertyChanged(nameof(DoingAxis));
                }
            }
        }


        private string canDoingTxt;

        public string CanDoingTxt {
            get { return canDoingTxt; }
            set {
                if (canDoingTxt != value) {
                    canDoingTxt = value;
                    OnPropertyChanged(nameof(CanDoingTxt));
                }
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 轴号列表信息
    /// </summary>
    public class AxisListDetail : INotifyPropertyChanged {

        private string axisNo;
        /// <summary>
        /// 轴号
        /// </summary>
        public string AxisNo {
            get { return axisNo; }
            set {
                if (axisNo != value) {
                    axisNo = value;
                    OnPropertyChanged(nameof(AxisNo));
                }
            }
        }

        private string color;
        /// <summary>
        /// 颜色
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

        private double length;
        /// <summary>
        /// 米数
        /// </summary>
        public double Length {
            get { return length; }
            set {
                if (length != value) {
                    length = value;
                    OnPropertyChanged(nameof(Length));
                }
            }
        }

        private string completedRate;
        /// <summary>
        /// 完成率
        /// </summary>
        public string CompletedRate {
            get { return completedRate; }

            set {
                if (value != completedRate) {
                    completedRate = value;
                    OnPropertyChanged(nameof(CompletedRate));
                }
            }
        }





        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
