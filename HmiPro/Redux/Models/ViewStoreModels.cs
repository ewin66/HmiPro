using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Annotations;
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
}
