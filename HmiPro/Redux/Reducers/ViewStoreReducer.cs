﻿using System;
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
using YCsharp.Util;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// 保存所有视图状态
    /// <author>ychost</author>
    /// <date>2018-01-15</date>
    /// </summary>
    public static class ViewStoreReducer {
        /// <summary>
        /// 保存视图的一些临时数据，比如导航数据，列表选择数据等等，让用户下一次进入到此路由的时候恢复上次离开的时候的状态
        /// </summary>
        public struct Store {
            /// <summary>
            /// DMesCoreView 视图模型数据
            /// </summary>
            public IDictionary<string, DMesCoreViewStore> DMewCoreViewDict;
            /// <summary>
            /// 导航视图M模型数据（目前只在 DMesCoreView 中使用到，顶部的那个机台导航）
            /// </summary>
            public NavViewStore NavView;
            /// <summary>
            /// DMesCoreView -> CpmDetailView 视图模型数据
            /// </summary>
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
                        var cpmDetail = state.CpmDetailViewDict[pair.Key];

                        cpmDetail.ChartCpmSourceDict = new Dictionary<int, ObservableCollection<Cpm>>();
                        cpmDetail.MaxThresholdDict = new Dictionary<int, ObservableCollection<CpmChartThreshold>>();
                        cpmDetail.MinThresholdDict = new Dictionary<int, ObservableCollection<CpmChartThreshold>>();
                        cpmDetail.CPKDict = new Dictionary<int, ObservableCollection<CPK>>();
                        cpmDetail.AvgDict = new Dictionary<int, ObservableCollection<CpmAvg>>();
                        cpmDetail.Avgcalculator = new Dictionary<int, Func<double, double>>();
                        cpmDetail.AvgLast = new Dictionary<int, double?>();

                        foreach (var cpmPair in pair.Value.CodeToAllCpmDict) {
                            cpmDetail.ChartCpmSourceDict[cpmPair.Key] = new ObservableCollection<Cpm>();
                            cpmDetail.MaxThresholdDict[cpmPair.Key] = new ObservableCollection<CpmChartThreshold>();
                            cpmDetail.MinThresholdDict[cpmPair.Key] = new ObservableCollection<CpmChartThreshold>();
                            cpmDetail.CPKDict[cpmPair.Key] = new ObservableCollection<CPK>();
                            cpmDetail.AvgDict[cpmPair.Key] = new ObservableCollection<CpmAvg>();
                            cpmDetail.Avgcalculator[cpmPair.Key] = YUtil.CreateExecAvgFunc();
                            cpmDetail.AvgLast[cpmPair.Key] = null;
                        }
                    }
                    return state;
                }).When<ViewStoreActions.ChangeDMesSelectedMachineCode>((state, action) => {
                    state.NavView.DMesSelectedMachineCode = action.MachineCode;
                    return state;
                }).When<CpmActions.CpmUpdatedAll>((state, action) => {
                    try {
                        var cpmDetail = state.CpmDetailViewDict[action.MachineCode];
                        //当前是否处于曲线界面
                        //这个 6 表示为曲线界面 PageView 的 Index 为 6
                        var isInChartView = state.DMewCoreViewDict[action.MachineCode].TabSelectedIndex == 5;
                        var machineCode = action.MachineCode;
                        foreach (var cpm in action.Cpms) {
                            if (cpm.ValueType != SmParamType.Signal) {
                                continue;
                            }
                            //过滤掉时间有问题的点
                            var lastTime = cpmDetail.ChartCpmSourceDict[cpm.Code].LastOrDefault()?.PickTime;
                            if (lastTime.HasValue && lastTime.Value >= cpm.PickTime) {
                                continue;
                            }
                            var (maxThreshold, minThreshold) = getMaxMinThreshold(state, machineCode, cpm);
                            var cpk = getCPK(cpm, maxThreshold, minThreshold);
                            //减少主线程调用次数
                            //只有更新的参数Id为选中的 && 当前处于曲线界面才唤起主线程 && 当前导航机台位参数机台
                            if (state.NavView.DMesSelectedMachineCode == action.MachineCode &&
                                cpm.Code == cpmDetail.SelectedCpm?.Code && isInChartView) {
                                Application.Current.Dispatcher.Invoke(() => {
                                    //有的机台会抛错，别问我为什么
                                    try {
                                        updateChartView(cpmDetail, cpm, maxThreshold, minThreshold, cpk);
                                    }
                                    catch (Exception e) {
                                        //App.Logger.Warn("更新曲线异常" + e, true);
                                    }
                                });
                            }
                            else {
                                //防止多线程错误
                                try {
                                    updateChartView(cpmDetail, cpm, maxThreshold, minThreshold, cpk);
                                }
                                catch { }
                            }
                        }
                    }
                    catch (Exception e) {
                        App.Logger.Error("实时曲线异常",e);
                    }
                    return state;
                });
        }


        /// <summary>
        /// 获取参数的最值
        /// </summary>
        /// <param name="state"></param>
        /// <param name="machineCode"></param>
        /// <param name="cpm"></param>
        /// <returns></returns>
        private static (CpmChartThreshold, CpmChartThreshold) getMaxMinThreshold(Store state, string machineCode, Cpm cpm) {
            CpmChartThreshold maxThreshold = null;
            CpmChartThreshold minThreshold = null;

            var (plcMax, plcMin) = getThresholdFromPlc(state, machineCode, cpm);
            var (expMax, expMin) = getThresholdFromExp(state, machineCode, cpm);

            //首先考虑Plc的最值，再考虑经验最值，最后考虑历史保存最值
            maxThreshold = plcMax ?? expMax ?? state.CpmDetailViewDict[machineCode].MaxThresholdDict[cpm.Code].LastOrDefault()?.Clone();
            minThreshold = plcMin ?? expMin ?? state.CpmDetailViewDict[machineCode].MinThresholdDict[cpm.Code].LastOrDefault()?.Clone(); ;

            return (maxThreshold, minThreshold);
        }

        /// <summary>
        /// 从 Plc 中读取参数的最值
        /// </summary>
        /// <param name="state"></param>
        /// <param name="machineCode"></param>
        /// <param name="cpm"></param>
        /// <returns></returns>
        private static (CpmChartThreshold, CpmChartThreshold) getThresholdFromPlc(Store state, string machineCode, Cpm cpm) {
            CpmChartThreshold maxThreshold = null;
            CpmChartThreshold minThreshold = null;
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
        /// 读取参数的经验最值
        /// </summary>
        /// <param name="state"></param>
        /// <param name="machineCode"></param>
        /// <param name="cpm"></param>
        /// <returns></returns>
        private static (CpmChartThreshold, CpmChartThreshold) getThresholdFromExp(Store state, string machineCode, Cpm cpm) {
            CpmChartThreshold maxThreshold = null;
            CpmChartThreshold minThreshold = null;

            if (MachineConfig.MachineDict[machineCode].CodeToExpAlarmDict.TryGetValue(cpm.Code, out var expAlarm)) {
                if (expAlarm.Max.HasValue) {
                    maxThreshold = new CpmChartThreshold() { Value = expAlarm.Max.Value };
                }
                if (expAlarm.Min.HasValue) {
                    minThreshold = new CpmChartThreshold() { Value = expAlarm.Min.Value };
                }
            }

            return (maxThreshold, minThreshold);
        }



        /// <summary>
        /// 更新曲线数据
        /// 该函数被多线程调用，所以给每个 Code 对应的 Chart 都上了一把锁
        /// </summary>
        private static void updateChartView(CpmDetailViewStore cpmDetail, Cpm cpm, CpmChartThreshold maxThreshold, CpmChartThreshold minThreshold, CPK cpk) {
            lock (cpmDetail.ChartCpmSourceDict[cpm.Code]) {
                if (cpm.ValueType == SmParamType.Signal) {
                    AsureChartPointNums(cpmDetail, cpm);
                    cpmDetail.ChartCpmSourceDict[cpm.Code].Add(cpm);
                    cpmDetail.Avgcalculator[cpm.Code](cpm.GetFloatVal());
                    if (!cpmDetail.AvgLast[cpm.Code].HasValue) {
                        cpmDetail.AvgLast[cpm.Code] = cpm.GetFloatVal();
                    }
                    cpmDetail.AvgDict[cpm.Code].Add(new CpmAvg() {
                        Value = cpmDetail.AvgLast[cpm.Code].Value,
                        UpdateTime = cpm.PickTime
                    });
                    //同步最值和实时曲线的时间
                    if (maxThreshold != null) {
                        maxThreshold.UpdateTime = cpm.PickTime;
                        cpmDetail.MaxThresholdDict[cpm.Code].Add(maxThreshold);
                    }
                    if (minThreshold != null) {
                        minThreshold.UpdateTime = cpm.PickTime;
                        cpmDetail.MinThresholdDict[cpm.Code].Add(minThreshold);
                    }
                    if (cpk != null) {
                        cpk.UpdateTime = cpm.PickTime;
                        cpmDetail.CPKDict[cpm.Code].Add(cpk);
                    }
                }
                if (cpm.Code == cpmDetail.SelectedCpm.Code) {
                    cpmDetail.SelectedPointNums = "点数：" + cpmDetail.SelectedCpmChartSource.Count;
                    //保证实时曲线的动态绘制
                    cpmDetail.SelectedVisualMax = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 一个曲线图最多绘制的点数
        /// </summary>
        private static int maxChartPointNums = 200;
        /// <summary>
        /// 当点数超过 maxChartPointNums 之后移除的点数
        /// </summary>
        private static int removePointNums = 50;

        /// <summary>
        /// 确保参数值的个数不会超限
        /// </summary>
        /// <param name="cpmDetail"></param>
        /// <param name="cpm"></param>
        private static void AsureChartPointNums(CpmDetailViewStore cpmDetail, Cpm cpm) {
            if (cpmDetail.ChartCpmSourceDict[cpm.Code].Count > maxChartPointNums) {
                cpmDetail.ChartCpmSourceDict[cpm.Code].RemoveRange(0, removePointNums);
                cpmDetail.AvgDict[cpm.Code].RemoveRange(0, removePointNums);
                //更新平均值计算器
                cpmDetail.AvgLast[cpm.Code] = cpmDetail.Avgcalculator[cpm.Code](cpm.GetFloatVal());
                cpmDetail.Avgcalculator[cpm.Code] = YUtil.CreateExecAvgFunc();
            }
            if (cpmDetail.MaxThresholdDict[cpm.Code].Count > maxChartPointNums) {
                cpmDetail.MaxThresholdDict[cpm.Code].RemoveRange(0, removePointNums);
            }
            if (cpmDetail.MinThresholdDict[cpm.Code].Count > maxChartPointNums) {
                cpmDetail.MinThresholdDict[cpm.Code].RemoveRange(0, removePointNums);
            }
            if (cpmDetail.CPKDict[cpm.Code].Count > maxChartPointNums) {
                cpmDetail.CPKDict[cpm.Code].RemoveRange(0, removePointNums);
            }
        }

        private static Func<double> randCpk = YUtil.GetRandomGen();
        /// <summary>
        /// <todo>计算 CPK</todo>
        /// </summary>
        /// <param name="cpm"></param>
        /// <param name="maxThreshold"></param>
        /// <param name="minThreshold"></param>
        /// <returns></returns>
        private static CPK getCPK(Cpm cpm, CpmChartThreshold maxThreshold, CpmChartThreshold minThreshold) {
            CPK cpk = new CPK() { Value = 1.8 };
            return cpk;
        }
    }
}
