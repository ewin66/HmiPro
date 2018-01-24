using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using HmiPro.Annotations;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using Reducto;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// 
    /// </summary>
    public static class DMesReducer {
        public struct State {
            public IDictionary<string, ObservableCollection<MqSchTask>> MqSchTasksDict;
            public IDictionary<string, SchTaskDoing> SchTaskDoingDict;
            public IDictionary<string, MqScanMaterial> MqScanMaterialDict;
            public string MachineCode;
            //人员卡信息
            public IDictionary<string, List<MqEmpRfid>> MqEmpRfidDict;

            /// <summary>
            /// 栈板目前存放的数量
            /// key 为机台 code
            /// </summary>
            public IDictionary<string, Pallet> PalletDict;

        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>()
                .When<DMesActions.Init>((state, action) => {
                    state.SchTaskDoingDict = new ConcurrentDictionary<string, SchTaskDoing>();
                    state.MqSchTasksDict = new ConcurrentDictionary<string, ObservableCollection<MqSchTask>>();
                    state.MqScanMaterialDict = new ConcurrentDictionary<string, MqScanMaterial>();
                    state.MqEmpRfidDict = new ConcurrentDictionary<string, List<MqEmpRfid>>();
                    state.PalletDict = new ConcurrentDictionary<string, Pallet>();
                    foreach (var pair in MachineConfig.MachineDict) {
                        state.SchTaskDoingDict[pair.Key] = new SchTaskDoing();
                        state.MqSchTasksDict[pair.Key] = new ObservableCollection<MqSchTask>();
                        state.MqEmpRfidDict[pair.Key] = new List<MqEmpRfid>();
                    }
                    foreach (var machinieCode in GlobalConfig.PalletMachineCodes) {
                        state.PalletDict[machinieCode] = new Pallet();
                    }
                    return state;
                });

        }
    }

    /// <summary>
    /// 栈板
    /// </summary>
    public class Pallet : MongoDoc, INotifyPropertyChanged {

        private int axisNum;

        /// <summary>
        /// 栈板上面的轴数量
        /// </summary>
        public int AxisNum {
            get { return axisNum; }
            set {
                if (axisNum != value) {
                    lock (this) {
                        axisNum = value;
                    }
                    OnPropertyChanged(nameof(AxisNum));
                }
            }
        }

        /// <summary>
        /// 栈板的 RFID
        /// </summary>
        public string Rfid { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
