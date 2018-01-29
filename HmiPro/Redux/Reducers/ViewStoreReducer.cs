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
            public IDictionary<string, CpmDetailViewStore> CpmDetailViewDict;
        }

        public static SimpleReducer<Store> Create() {
            return new SimpleReducer<Store>()
                .When<ViewStoreActions.Init>((state, action) => {
                    state.NavView = new NavViewStore();
                    state.NavView.DMesSelectedMachineCode = MachineConfig.MachineDict.Keys.FirstOrDefault();
                    state.DMewCoreViewDict = new Dictionary<string, DMesCoreViewStore>();
                    state.CpmDetailViewDict = new Dictionary<string, CpmDetailViewStore>();
                    foreach (var pair in MachineConfig.MachineDict) {
                        state.DMewCoreViewDict[pair.Key] = new DMesCoreViewStore(pair.Key);
                        state.CpmDetailViewDict[pair.Key] = new CpmDetailViewStore(pair.Key);
                        state.CpmDetailViewDict[pair.Key].ChartCpmSourceDict = new Dictionary<int, ObservableCollection<Cpm>>();
                        state.CpmDetailViewDict[pair.Key].MaxThresholdDict = new Dictionary<int, ObservableCollection<CpmChartThreshold>>();
                        state.CpmDetailViewDict[pair.Key].MinThresholdDict = new Dictionary<int, ObservableCollection<CpmChartThreshold>>();
                        foreach (var cpmPair in pair.Value.CodeToAllCpmDict) {
                            state.CpmDetailViewDict[pair.Key].ChartCpmSourceDict[cpmPair.Key] = new ObservableCollection<Cpm>();
                            state.CpmDetailViewDict[pair.Key].MaxThresholdDict[cpmPair.Key] = new ObservableCollection<CpmChartThreshold>();
                            state.CpmDetailViewDict[pair.Key].MinThresholdDict[cpmPair.Key] = new ObservableCollection<CpmChartThreshold>();
                        }
                    }
                    return state;
                }).When<ViewStoreActions.ChangeDMesSelectedMachineCode>((state, action) => {
                    state.NavView.DMesSelectedMachineCode = action.MachineCode;
                    return state;
                }).When<CpmActions.CpmUpdatedAll>((state, action) => {
                    var cpmDetail = state.CpmDetailViewDict[action.MachineCode];
                    //当前是否处于曲线界面
                    //这个 6 表示为曲线界面 PageView 的 Index 为 6
                    var isInChartView = state.DMewCoreViewDict[action.MachineCode].TabSelectedIndex == 6;
                    var machineCode = action.MachineCode;
                    lock (cpmDetail.opLock) {
                        foreach (var cpm in action.Cpms) {
                            var (maxThreshold, minThreshold) = getMaxAndMinCpmThreshold(state, machineCode, cpm);
                            //减少主线程调用次数
                            //只有更新的参数Id为选中的 && 当前处于曲线界面才唤起主线程 && 当前导航机台位参数机台
                            if (state.NavView.DMesSelectedMachineCode == action.MachineCode && cpm.Code == cpmDetail.SelectedCpm?.Code && isInChartView) {
                                Application.Current.Dispatcher.Invoke(() => {
                                    updateChartView(cpmDetail, cpm, maxThreshold, minThreshold);
                                });
                            } else {
                                //防止多线程错误
                                try {
                                    updateChartView(cpmDetail, cpm, maxThreshold, minThreshold);
                                } catch {

                                }
                            }
                        }
                    }

                    return state;
                });
        }


        /// <summary>
        /// 获取参数的最大最小值
        /// </summary>
        /// <param name="state"></param>
        /// <param name="machineCode"></param>
        /// <param name="cpm"></param>
        /// <returns></returns>
        private static (CpmChartThreshold, CpmChartThreshold) getMaxAndMinCpmThreshold(Store state, string machineCode, Cpm cpm) {
            //从 Plc 中获取
            //最大值和最小值，默认为上一次的值
            CpmChartThreshold maxThreshold = state.CpmDetailViewDict[machineCode].MaxThresholdDict[cpm.Code].LastOrDefault();
            CpmChartThreshold minThreshold = state.CpmDetailViewDict[machineCode].MinThresholdDict[cpm.Code].LastOrDefault();
            if (MachineConfig.MachineDict[machineCode].CodeToPlcAlarmDict.TryGetValue(cpm.Code, out var plc)) {
                if (plc.MaxCode.HasValue) {
                    var maxCpm = App.Store.GetState().CpmState.OnlineCpmsDict[machineCode][plc.MaxCode.Value];
                    if (maxCpm.ValueType == SmParamType.Signal) {
                        maxThreshold = new CpmChartThreshold() { Value = maxCpm.FloatValue, UpdateTime = cpm.PickTime };
                    }
                }
                if (plc.MinCode.HasValue) {
                    var minCpm = App.Store.GetState().CpmState.OnlineCpmsDict[machineCode][plc.MinCode.Value];
                    if (minCpm.ValueType == SmParamType.Signal) {
                        minThreshold = new CpmChartThreshold() { Value = minCpm.FloatValue, UpdateTime = cpm.PickTime };
                    }
                }
            }
            return (maxThreshold, minThreshold);
        }

        /// <summary>
        /// 更新曲线数据
        /// </summary>
        private static void updateChartView(CpmDetailViewStore cpmDetail, Cpm cpm, CpmChartThreshold maxThreshold, CpmChartThreshold minThreshold) {
            //最多保留 1000 个点
            if (cpmDetail.ChartCpmSourceDict[cpm.Code].Count > 1000) {
                cpmDetail.ChartCpmSourceDict[cpm.Code].RemoveRange(0, 100);
            }
            if (cpmDetail.MaxThresholdDict[cpm.Code].Count > 1000) {
                cpmDetail.MaxThresholdDict[cpm.Code].RemoveRange(0, 100);
            }
            if (cpmDetail.MinThresholdDict[cpm.Code].Count > 1000) {
                cpmDetail.MinThresholdDict[cpm.Code].RemoveRange(0, 100);
            }

            if (cpm.ValueType == SmParamType.Signal) {
                cpmDetail.ChartCpmSourceDict[cpm.Code].Add(cpm);
                if (maxThreshold != null) {
                    cpmDetail.MaxThresholdDict[cpm.Code].Add(maxThreshold);
                }
                if (minThreshold != null) {
                    cpmDetail.MinThresholdDict[cpm.Code].Add(minThreshold);
                }
            }
        }
    }
}
