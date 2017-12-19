using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using Reducto;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public static class AlarmReducer {
        public struct State {
            public string MachineCode;
        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>()
                .When<AlarmActions.OpenAlarmLights>((state, action) => {
                    state.MachineCode = action.MachineCode;
                    return state;
                });
        }
    }
}
