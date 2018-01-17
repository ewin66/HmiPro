using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using Reducto;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// 回填参数 Store
    /// <author>ychost</author>
    /// <date>2018-1-17</date>
    /// </summary>
    public static class DpmReducer {
        public struct Store {
            public IDictionary<string, ObservableCollection<Dpm>> DpmsDict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SimpleReducer<Store> Create() {
            return new SimpleReducer<Store>().
                When<DMesActions.Init>((state, action) => {
                    state.DpmsDict = new SortedDictionary<string, ObservableCollection<Dpm>>();
                    //恢复之前的设置数据
                    foreach (var pair in GlobalConfig.MachineSettingDict) {
                        var machineCode = pair.Key;
                        state.DpmsDict[machineCode] = new ObservableCollection<Dpm>();
                        foreach (var name in pair.Value.DPms) {
                            state.DpmsDict[machineCode].Add(new Dpm() { Name = name, Value = "暂无" });
                        }
                    }
                    return state;
                });
        }
    }
}
