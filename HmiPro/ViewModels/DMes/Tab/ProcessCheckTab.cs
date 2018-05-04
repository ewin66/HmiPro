using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core.Native;
using HmiPro.Config.Models;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.ViewModels.Sys;
using Newtonsoft.Json;
using Reducto;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 制程质检
    /// <author>ychost</author>
    /// <date>2018-4-20</date>
    /// </summary>
    public class ProcessCheckTab : BaseTab {
        private string machineCode;
        /// <summary>
        /// 任务
        /// </summary>
        public ObservableCollection<MqSchTask> MqSchTasks { get; set; }
        /// <summary>
        /// 任务工单集合
        /// </summary>
        public ObservableCollection<string> Workcodes { get; set; }
        /// <summary>
        /// 班次
        /// </summary>
        public string[] ClassType { get; set; }
        /// <summary>
        /// 选择的班次
        /// </summary>
        public string SelectedClassType { get; set; }
        /// <summary>
        /// 质检项
        /// </summary>
        public IList<MqProcessCheck> ProcessCheckItems { get; set; }

        private bool canSubmit;

        public bool CanSubmit {
            get => canSubmit;
            set {
                if (canSubmit != value) {
                    canSubmit = value;
                    RaisePropertyChanged(nameof(CanSubmit));
                }
            }
        }

        /// <summary>
        /// 选择工单索引
        /// </summary>
        private int? selectedWorkCodeIndex = null;
        public int? SelectedWorkCodeIndex {
            get => selectedWorkCodeIndex;
            set {
                if (value.HasValue && selectedWorkCodeIndex != value) {
                    selectedWorkCodeIndex = value;
                    RaisePropertyChanged(nameof(SelectedWorkCodeIndex));
                    if (SelectedWorkCodeIndex < 0 || selectedWorkCodeIndex >= Workcodes.Count) {
                        return;
                    }
                    ProcessCheckItems = MqSchTasks[selectedWorkCodeIndex.Value].iqcList;
                    RaisePropertyChanged(nameof(ProcessCheckItems));
                    if (ProcessCheckItems.Count > 0) {
                        CanSubmit = true;
                    } else {
                        CanSubmit = false;
                    }
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public ProcessCheckTab() {
            Workcodes = new ObservableCollection<string>();
            ClassType = new[] { "白班", "夜班" };
            var time = YUtil.GetKeystoneWorkTime();
            if (time.Hour == 8) {
                SelectedClassType = ClassType[0];
            } else {
                SelectedClassType = ClassType[1];
            }
        }
        /// <summary>
        /// 页面加载事件命令
        /// </summary>
        ICommand submitCommand;
        public ICommand SubmitCommand {
            get {
                if (submitCommand == null)
                    submitCommand = new DelegateCommand(Submit);
                return submitCommand;
            }
        }



        /// <summary>
        /// 提交质检数据到服务器
        /// </summary>
        public async void Submit() {
            CanSubmit = false;
            if (this.ProcessCheckItems != null && ProcessCheckItems.Count > 0) {
                foreach (var item in ProcessCheckItems) {
                    item.macCode = machineCode;
                    item.classType = SelectedClassType;
                    if (selectedWorkCodeIndex.HasValue) {
                        item.workCode = MqSchTasks[selectedWorkCodeIndex.Value].workcode;
                    }
                }
                MqCall mqCall = new MqCall() {
                    callAction = MqCallAction.ProcessCheck,
                    callType = MqCallType.QualityCheck,
                    callArgs = ProcessCheckItems,
                    CallId = YUtil.GetTimeStampSec(DateTime.Now),
                    machineCode = machineCode
                };

                var mqEffects = UnityIocService.ResolveDepend<MqEffects>();
                var callSuccess = await App.Store.Dispatch(mqEffects.CallSystem(new MqActions.CallSystem(machineCode, mqCall)));
                if (callSuccess) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "通知",
                        Content = "提交成功"
                    }));
                    App.Logger.Info("提交制程质检数据: " + JsonConvert.SerializeObject(mqCall));
                } else {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "警告",
                        Content = "提交失败，请检查网络连接，稍后重试"
                    }));
                    App.Logger.Error("提交制程质检数据失败: " + JsonConvert.SerializeObject(mqCall));
                }

            }
            CanSubmit = true;
        }

        /// <summary>
        /// 绑定数据，返回的委托是解除本 Tab 的引用
        /// </summary>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        public Unsubscribe BindSource(string machineCode) {
            this.machineCode = machineCode;
            MqSchTasks = App.Store.GetState().DMesState.MqSchTasksDict[machineCode];
            whenSchTaskChanged(null, null);
            MqSchTasks.CollectionChanged += whenSchTaskChanged;
            return () => {
                MqSchTasks.CollectionChanged -= whenSchTaskChanged;
            };
        }

        /// <summary>
        /// 任务集合发生变化的时候，因更新当前 tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void whenSchTaskChanged(object sender, NotifyCollectionChangedEventArgs args) {
            Workcodes.Clear();
            SelectedWorkCodeIndex = null;
            foreach (var mqSchTask in MqSchTasks) {
                Workcodes.Add(mqSchTask.workcode);
            }
        }

    }
}
