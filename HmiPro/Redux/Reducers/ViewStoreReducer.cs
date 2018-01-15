using System;
using System.Collections.Concurrent;
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
    /// 保存所有视图状态
    /// <date>2018-01-15</date>
    /// <author>ychost</author>
    /// </summary>
    public static class ViewStoreReducer {
        public struct Store {
            public IDictionary<string, DMesCoreViewStore> DMewCoreViewDict;
            public NavViewStore NavView;
        }

        public static SimpleReducer<Store> Create() {
            return new SimpleReducer<Store>()
                .When<ViewStoreActions.Init>((state, action) => {
                    state.NavView = new NavViewStore();
                    state.NavView.DMesSelectedMachineCode = MachineConfig.MachineDict.Keys.FirstOrDefault();
                    state.DMewCoreViewDict = new Dictionary<string, DMesCoreViewStore>();
                    foreach (var pair in MachineConfig.MachineDict) {
                        state.DMewCoreViewDict[pair.Key] = new DMesCoreViewStore(pair.Key);
                    }
                    return state;
                }).When<ViewStoreActions.ChangeDMesSelectedMachineCode>((state, action) => {
                    state.NavView.DMesSelectedMachineCode = action.MachineCode;
                    return state;
                });
        }
    }
}
