using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using Reducto;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// <date>2017-12-20</date>
    /// <author>ychost</author>
    /// </summary>
    public static class OeeReducer {
        public struct State {
            public string MachineCode;
            public IDictionary<string, Oee> OeeDict;
        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>()
                .When<OeeActions.Init>((state, action) => {
                    state.OeeDict = new Dictionary<string, Oee>();
                    foreach (var pair in MachineConfig.MachineDict) {
                        state.OeeDict[pair.Key] = new Oee();
                    }
                    return state;
                }).When<OeeActions.UpdateOeePartialValue>((state, action) => {
                    state.MachineCode = action.MachineCode;
                    var oee = state.OeeDict[action.MachineCode];
                    //每次通知不一定oee三个数据都有值，更新其中有值项便是
                    if (action.TimeEff.HasValue) {
                        oee.TimeEff = action.TimeEff.Value;
                    }
                    if (action.QualityEff.HasValue) {
                        oee.QualityEff = action.QualityEff.Value;
                    }
                    if (action.SpeedEff.HasValue) {
                        oee.SpeedEff = action.SpeedEff.Value;
                    }
                    return state;
                });
        }
    }
}
