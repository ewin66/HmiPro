using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.CodeView;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using Reducto;
using YCsharp.Model.Procotol.SmParam;

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
            public IDictionary<string, CpmDetailViewStore> CpmDetailsiewDict;
        }

        public static SimpleReducer<Store> Create() {
            return new SimpleReducer<Store>()
                .When<ViewStoreActions.Init>((state, action) => {
                    state.NavView = new NavViewStore();
                    state.NavView.DMesSelectedMachineCode = MachineConfig.MachineDict.Keys.FirstOrDefault();
                    state.DMewCoreViewDict = new Dictionary<string, DMesCoreViewStore>();
                    state.CpmDetailsiewDict = new Dictionary<string, CpmDetailViewStore>();
                    foreach (var pair in MachineConfig.MachineDict) {
                        state.DMewCoreViewDict[pair.Key] = new DMesCoreViewStore(pair.Key);

                        state.CpmDetailsiewDict[pair.Key] = new CpmDetailViewStore(pair.Key);
                        state.CpmDetailsiewDict[pair.Key].ChartCpmSourceDict = new Dictionary<int, ObservableCollection<Cpm>>();

                        foreach (var cpmPair in pair.Value.CodeToAllCpmDict) {
                            state.CpmDetailsiewDict[pair.Key].ChartCpmSourceDict[cpmPair.Key] = new ObservableCollection<Cpm>();
                        }
                    }
                    return state;
                }).When<ViewStoreActions.ChangeDMesSelectedMachineCode>((state, action) => {
                    state.NavView.DMesSelectedMachineCode = action.MachineCode;
                    return state;
                }).When<CpmActions.CpmUpdatedAll>((state, action) => {
                    var cpmDetail = state.CpmDetailsiewDict[action.MachineCode];
                    //当前是否处于曲线界面
                    var isInChartView = state.DMewCoreViewDict[action.MachineCode].TabSelectedIndex == 6;
                    lock (cpmDetail.opLock) {
                        foreach (var cpm in action.Cpms) {
                            //减少主线程调用次数
                            //只有更新的参数Id为选中的 && 当前处于曲线界面才唤起主线程
                            if (cpm.Code == cpmDetail.SelectedCpm?.Code && isInChartView) {
                                Application.Current.Dispatcher.Invoke(() => {
                                    updateChartView(cpmDetail, cpm);
                                });
                            } else {
                                try {
                                    updateChartView(cpmDetail, cpm);
                                } catch {

                                }
                            }
                        }
                    }

                    return state;
                });
        }

        /// <summary>
        /// 更新曲线数据
        /// </summary>
        /// <param name="cpmDetail"></param>
        /// <param name="cpm"></param>
        private static void updateChartView(CpmDetailViewStore cpmDetail, Cpm cpm) {
            if (cpmDetail.ChartCpmSourceDict[cpm.Code].Count > 1000) {
                cpmDetail.ChartCpmSourceDict[cpm.Code].RemoveRange(0, 100);
            }
            if (cpm.ValueType == SmParamType.Signal) {
                cpmDetail.ChartCpmSourceDict[cpm.Code].Add(cpm);
            }
        }
    }
}
