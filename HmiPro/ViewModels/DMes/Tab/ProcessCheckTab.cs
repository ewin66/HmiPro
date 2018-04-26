using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpf.Core.Native;
using HmiPro.Config.Models;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.ViewModels.Sys;
using Newtonsoft.Json;
using Reducto;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 制程质检
    /// <author>ychost</author>
    /// <date>2018-4-20</date>
    /// </summary>
    public class ProcessCheckTab : BaseTab {
        string machineCode;
        public ObservableCollection<MqSchTask> MqSchTasks { get; set; }
        public ObservableCollection<string> Workcodes { get; set; }
        private IList<MqProcessCheck> processCheckItems;

        public IList<MqProcessCheck> ProcessCheckItems {
            get => processCheckItems;
            set {
                if (processCheckItems != value) {
                    processCheckItems = value;
                    RaisePropertyChanged(nameof(ProcessCheckItems));
                }
            }
        }

        private int? selectedIndex = null;
        public int? SelectedIndex {
            get => selectedIndex;
            set {
                if (value.HasValue && selectedIndex != value) {
                    selectedIndex = value;
                    RaisePropertyChanged(nameof(SelectedIndex));
                    if (SelectedIndex < 0 || selectedIndex >= Workcodes.Count) {
                        return;
                    }
                    ProcessCheckItems = MqSchTasks[selectedIndex.Value].iqcList;
                }
            }
        }

        public ProcessCheckTab() {
            Workcodes = new ObservableCollection<string>();
        }

        public Unsubscribe BindSource(string machineCode) {
            this.machineCode = machineCode;
            MqSchTasks = App.Store.GetState().DMesState.MqSchTasksDict[machineCode];
            whenSchTaskChanged(null, null);
            MqSchTasks.CollectionChanged += whenSchTaskChanged;
            ;
            return () => {
                MqSchTasks.CollectionChanged -= whenSchTaskChanged;
            };

        }

        void whenSchTaskChanged(object sender, NotifyCollectionChangedEventArgs args) {
            Workcodes.Clear();
            SelectedIndex = null;
            foreach (var mqSchTask in MqSchTasks) {
                Workcodes.Add(mqSchTask.workcode);
            }
        }

        public ProcessCheckViewModel ProcessCheckViewModel { get; set; }
    }
}
