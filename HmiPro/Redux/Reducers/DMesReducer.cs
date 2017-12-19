using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using Reducto;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// 
    /// </summary>
    public static class DMesReducer {
        public class State {
            public IDictionary<string, MqSchTask> MqSchTaskDict;
            public string MachineCode;
        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>(() => new State() { MqSchTaskDict = new ConcurrentDictionary<string, MqSchTask>() })
                .When<DMesActions.DMesSchTaskAssign>((state, action) => {
                    state.MachineCode = action.MachineCode;
                    state.MqSchTaskDict[state.MachineCode] = action.SchTask;
                    return state;
                });
        }
    }
}
