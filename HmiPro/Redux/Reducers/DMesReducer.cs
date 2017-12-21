using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
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
            public string MachineCode;
        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>()
                .When<DMesActions.Init>((state, action) => {
                    state.SchTaskDoingDict = new ConcurrentDictionary<string, SchTaskDoing>();
                    state.MqSchTasksDict = new ConcurrentDictionary<string, ObservableCollection<MqSchTask>>();
                    foreach (var pair in MachineConfig.MachineDict) {
                        state.SchTaskDoingDict[pair.Key] = new SchTaskDoing();
                        state.MqSchTasksDict[pair.Key] = new ObservableCollection<MqSchTask>();
                    }
                    return state;
                });

        }
    }
}
