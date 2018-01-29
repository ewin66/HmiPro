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
            get { return taskPanelIsSelected = true; }
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
        public readonly string MachineCode;

        public CpmDetailViewStore(string machineCode) {
            MachineCode = machineCode;
        }

        private Cpm selectedCpm;
        public object opLock = new object();
        /// <summary>
        /// 用户选择的参数
        /// </summary>
        public Cpm SelectedCpm {
            get { return selectedCpm; }
            set {
                if (selectedCpm != value && value != null) {
                    selectedCpm = value;
                    RaisePropertyChanged(nameof(SelectedCpm));
                    lock (opLock) {
                        SelectedCpmChartSource = ChartCpmSourceDict[selectedCpm.Code];
                    }
                    RaisePropertyChanged(nameof(SelectedCpmChartSource));
                }
            }
        }

        public IDictionary<int, ObservableCollection<Models.Cpm>> ChartCpmSourceDict { get; set; }

        public ObservableCollection<Cpm> SelectedCpmChartSource { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// chart 适用的数据模型
    /// <author>DevExpress</author>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChartDataCollection<T> : ObservableCollection<T> {
        public void AddRange(IList<T> items) {
            foreach (T item in items)
                Items.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)items, Items.Count - items.Count));
        }
        public void RemoveFromBegin(int count) {
            IList<T> removedItems = new List<T>(count);
            for (int i = 0; i < count; i++) {
                removedItems.Add(Items[0]);
                Items.RemoveAt(0);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)removedItems, 0));
        }
    }
}
