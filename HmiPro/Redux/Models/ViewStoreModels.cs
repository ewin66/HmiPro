using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraExport.Xls;
using HmiPro.Annotations;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Reducers;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// DMesCoreView需要界面保存的数据
    /// 当导航变更的时候保存这些数据，等再次导航回来的时候回到上次的状态
    /// <author>ychost</author>
    /// <date>2017-01-15</date>
    /// </summary>
    public class DMesCoreViewStore : INotifyPropertyChanged {


        public readonly string MachineCode;

        public DMesCoreViewStore(string machineCode) {
            MachineCode = machineCode;
        }

        private string tabSlectedWorkCode;
        /// <summary>
        /// 选定的工单号
        /// </summary>
        public string TaskSelectedWorkCode {
            get => tabSlectedWorkCode;
            set {
                if (tabSlectedWorkCode != value) {
                    tabSlectedWorkCode = value;
                    OnPropertyChanged(nameof(TaskSelectedWorkCode));
                    OnPropertyChanged(nameof(AxisSelectedRow));
                }
            }
        }

        private int tabSelectedIndex;
        /// <summary>
        /// 选定最外层Tab序号
        /// </summary>
        public int TabSelectedIndex {
            get => tabSelectedIndex;
            set {
                if (tabSelectedIndex != value) {
                    tabSelectedIndex = value;
                    OnPropertyChanged(nameof(TabSelectedIndex));
                }
            }
        }

        private int taskTabSelectedIndex;
        /// <summary>
        /// 选定工单任务的Tab序号
        /// </summary>
        public int TaskTabSelectedIndex {
            get => taskTabSelectedIndex;
            set {
                if (taskTabSelectedIndex != value) {
                    taskTabSelectedIndex = value;
                    OnPropertyChanged(nameof(TaskTabSelectedIndex));
                }
            }
        }



        private string axisSelectedCode;
        /// <summary>
        /// 选定具体轴任务的某行
        /// </summary>
        public MqTaskAxis AxisSelectedRow {
            get {
                var currentTask = App.Store.GetState().DMesState.MqSchTasksDict[MachineCode]?.FirstOrDefault(m => m.workcode == TaskSelectedWorkCode);
                return currentTask?.axisParam?.FirstOrDefault(a => a?.axiscode == axisSelectedCode) ?? currentTask?.axisParam?.FirstOrDefault();
            }
            set {
                if (axisSelectedCode != value?.axiscode) {
                    axisSelectedCode = value.axiscode;
                }
            }
        }

        /// <summary>
        /// 通讯栏选择行
        /// </summary>
        private Com485SingleStatus com485SelectedRow;
        public Com485SingleStatus Com485SelectedRow {
            get => com485SelectedRow;
            set {
                if (com485SelectedRow != value) {
                    com485SelectedRow = value;
                    OnPropertyChanged(nameof(Com485SelectedRow));
                }
            }
        }


        private MqAlarm alarmSelctedRow;
        /// <summary>
        /// 报警栏选择行
        /// </summary>
        public MqAlarm AlarmSelectedRow {
            get => alarmSelctedRow;
            set {
                if (alarmSelctedRow != value) {
                    alarmSelctedRow = value;
                    OnPropertyChanged(nameof(AlarmSelectedRow));
                }

            }
        }

        private bool taskPanelIsSelected = true;

        /// <summary>
        /// 控制工单列表的显示/隐藏
        /// </summary>
        public bool TaskPanelIsSelected {
            get { return taskPanelIsSelected; }
            set {
                if (taskPanelIsSelected != value) {
                    taskPanelIsSelected = value;
                    OnPropertyChanged(nameof(TaskPanelIsSelected));
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
    /// 导航视图数据
    /// </summary>
    public class NavViewStore {
        /// <summary>
        /// DMes界面中选定的某个机台
        /// </summary>
        public string DMesSelectedMachineCode;
    }

    /// <summary>
    /// 参数详细数据
    /// </summary>
    public class CpmDetailViewStore : INotifyPropertyChanged {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public readonly string MachineCode;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="machineCode"></param>
        public CpmDetailViewStore(string machineCode) {
            MachineCode = machineCode;
            SelectedVisualMax = DateTime.Now;
            SelectedVisualMin = SelectedVisualMax.AddMinutes(-10);
        }

        private Cpm selectedCpm;
        /// <summary>
        /// 用户选择的参数
        /// </summary>
        public Cpm SelectedCpm {
            get { return selectedCpm; }
            set {
                if (selectedCpm != value && value != null) {
                    selectedCpm = value;
                    SelectedCpmChartSource = ChartCpmSourceDict[selectedCpm.Code];
                    SelectedMaxThreshold = MaxThresholdDict[selectedCpm.Code];
                    SelectedMinThreshold = MinThresholdDict[SelectedCpm.Code];
                    SelectedCPK = CPKDict[selectedCpm.Code];
                    SelectedAvg = AvgDict[selectedCpm.Code];

                    RaisePropertyChanged(nameof(SelectedCpm));
                    RaisePropertyChanged(nameof(SelectedCpmChartSource));
                    RaisePropertyChanged(nameof(SelectedMaxThreshold));
                    RaisePropertyChanged(nameof(SelectedMinThreshold));
                    RaisePropertyChanged(nameof(SelectedCPK));
                    RaisePropertyChanged(nameof(SelectedAvg));

                    SelectedPointNums = "点数：" + SelectedCpmChartSource.Count;
                    SelectedVisualMax = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 选中取钱的平均值
        /// </summary>
        public ObservableCollection<CpmAvg> SelectedAvg { get; set; }
        /// <summary>
        /// 选中曲线的最大值
        /// </summary>
        public ObservableCollection<CpmChartThreshold> SelectedMaxThreshold { get; set; }

        /// <summary>
        /// 当前选中的 CPK
        /// </summary>
        public ObservableCollection<CPK> SelectedCPK { get; set; }

        /// <summary>
        /// 所有的 CPK
        /// </summary>
        public IDictionary<int, ObservableCollection<CPK>> CPKDict { get; set; }

        /// <summary>
        /// 参数的平均数
        /// </summary>
        public IDictionary<int, ObservableCollection<CpmAvg>> AvgDict { get; set; }

        /// <summary>
        /// 选中曲线的最小值
        /// </summary>
        public ObservableCollection<CpmChartThreshold> SelectedMinThreshold { get; set; }

        /// <summary>
        /// 选中的曲线的点数
        /// </summary>
        private string selectedPointNums;
        public string SelectedPointNums {
            get { return selectedPointNums; }
            set {
                if (selectedPointNums != value) {
                    selectedPointNums = value;
                    RaisePropertyChanged(nameof(SelectedPointNums));
                }
            }
        }

        private DateTime selectedVisualMax;
        /// <summary>
        /// 下面滚动条的最大时间点
        /// </summary>
        public DateTime SelectedVisualMax {
            get { return selectedVisualMax; }
            set {
                if (selectedVisualMax != value) {
                    selectedVisualMax = value;
                    RaisePropertyChanged(nameof(SelectedVisualMax));
                }
            }
        }


        private DateTime selectedVisualMin;
        /// <summary>
        /// 下面滚动条的最小值
        /// </summary>
        public DateTime SelectedVisualMin {
            get { return selectedVisualMin; }
            set {
                if (selectedVisualMin != value) {
                    selectedVisualMin = value;
                    RaisePropertyChanged(nameof(SelectedVisualMin));
                }
            }
        }



        /// <summary>
        /// 每个参数的最大值，同参数一起更新
        /// </summary>
        public IDictionary<int, ObservableCollection<CpmChartThreshold>> MaxThresholdDict { get; set; }

        /// <summary>
        /// 每个参数的最小值，同参数一起更新
        /// </summary>
        public IDictionary<int, ObservableCollection<CpmChartThreshold>> MinThresholdDict { get; set; }

        /// <summary>
        /// 每个参数的历史数据
        /// </summary>
        public IDictionary<int, ObservableCollection<Models.Cpm>> ChartCpmSourceDict { get; set; }

        /// <summary>
        /// 平均值计算器
        /// </summary>
        public IDictionary<int, Func<double, double>> Avgcalculator { get; set; }

        /// <summary>
        /// 主要保存上次的平均值，平均值每次显示应该为一直线，而不能变动，故
        /// 后 200 个点其实是显示前 200 个点的平均值
        /// </summary>
        public IDictionary<int, double?> AvgLast { get; set; }


        /// <summary>
        /// 选择参数的历史数据
        /// </summary>
        public ObservableCollection<Cpm> SelectedCpmChartSource { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 参数曲线的最值
    /// 最值可动态变更，并非一条常量曲线，它和正常曲线一起绘制的
    /// </summary>
    public class CpmChartThreshold {
        /// <summary>
        /// 最值
        /// </summary>
        public float Value { get; set; }
        /// <summary>
        /// 最值更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        public CpmChartThreshold Clone() {
            return new CpmChartThreshold() { Value = this.Value, UpdateTime = this.UpdateTime };
        }
    }

    /// <summary>
    /// CPK 图形属性
    /// </summary>
    public class CPK {
        public double Value { get; set; }
        public DateTime UpdateTime { get; set; } = DateTime.Now;


    }

    /// <summary>
    /// 每个参数的平均值
    /// </summary>
    public class CpmAvg {
        public double Value { get; set; }
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}
