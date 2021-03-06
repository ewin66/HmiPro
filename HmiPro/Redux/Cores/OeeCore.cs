﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using MongoDB.Driver.Linq;
using YCsharp.Model.Procotol.SmParam;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// 计算实时 Oee
    /// <date>2017-12-20</date>
    /// <author>ychost</author>
    /// </summary>
    public class OeeCore {
        /// <summary>
        /// 日志
        /// </summary>
        public readonly LoggerService Logger;
        /// <summary>
        /// 事件处理器
        /// </summary>
        private readonly IDictionary<string, Action<AppState, IAction>> actionExecutors = new Dictionary<string, Action<AppState, IAction>>();

        /// <summary>
        /// 初始化 日志、事件处理器
        /// </summary>
        public OeeCore() {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            actionExecutors[CpmActions.OEE_SPEED_ACCEPT] = whenOeeSpeedAccept;
        }

        /// <summary>
        /// 初始化订阅
        /// </summary>
        public void Init() {
            App.Store.Subscribe(actionExecutors);
        }

        /// <summary>
        /// 计算Oee
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">Oee 速度值</param>
        private void whenOeeSpeedAccept(AppState state, IAction action) {
            //每当速度变化则计算一次Oee
            var oeeAction = (CpmActions.OeeSpeedAccept)action;
            var machineCode = oeeAction.MachineCode;
            var machineStates = state.CpmState.MachineStateDict[machineCode];
            var timeEff = CalcOeeTimeEff(machineCode, machineStates);
            var speedEff = CalcOeeSpeedEff(machineCode, GlobalConfig.MachineSettingDict[machineCode].OeeSpeedType);
            var qualityEff = CalcOeeQualityEff(machineCode);
            //更新Oee字典
            App.Store.Dispatch(new OeeActions.UpdateOeePartialValue(
                machineCode,
                timeEff,
                speedEff,
                qualityEff));
        }

        /// <summary>
        /// 计算Oee的开机率，Or
        /// </summary>
        /// <returns></returns>
        public float? CalcOeeTimeEff(string machineCode, IList<MachineState> machineStates) {
            float? timeEff = null;
            float stateSpeed = 0;
            stateSpeed = App.Store.GetState().CpmState.StateSpeedDict[machineCode];
            //删除上一班的机台状态数据
            var workTime = YUtil.GetKeystoneWorkTime();
            removeBeforeWorkTime(machineStates, workTime);
            var runTimeSec = getMachineRunTimeSec(machineStates, stateSpeed);
            var debugTimeSec = getMachineDebugTimeSec();
            //计算时间效率
            if (runTimeSec < 0) {
                Logger.Error($"计算时间效率失败，有效时间 {runTimeSec} < 0 ");
            } else {

                var cpms = App.Store.GetState().CpmState.OnlineCpmsDict[machineCode];
                cpms[DefinedParamCode.RunTime].Value = TimeSpan.FromSeconds(runTimeSec - debugTimeSec).TotalHours;
                cpms[DefinedParamCode.DutyTime].Value = (DateTime.Now - workTime).TotalHours;
                cpms[DefinedParamCode.StopTime].Value = (double)cpms[DefinedParamCode.DutyTime].Value - (double)cpms[DefinedParamCode.RunTime].Value;
                timeEff = (float)((runTimeSec - debugTimeSec) / (DateTime.Now - workTime).TotalSeconds);


                Logger.Debug($"当班时间：{(DateTime.Now - workTime).TotalHours.ToString("0.00")} 小时，机台运行时间 {cpms[DefinedParamCode.RunTime].GetFloatVal().ToString("0.00")} 小时");
            }
            return timeEff;
        }

        /// <summary>
        /// 删除当班时间前的机台状态数据
        /// </summary>
        /// <param name="machineStates"></param>
        /// <param name="workTime"></param>
        private void removeBeforeWorkTime(IList<MachineState> machineStates, DateTime workTime) {
            //删除上一班的机台状态数据
            var removeList = machineStates.Where(m => m.Time < workTime).ToList();
            //如果最后一个状态是维修，且该状态在上一班，则表示该维修状态超过了一班时间
            //保留该状态，直到维修完毕
            if (removeList?.LastOrDefault()?.StatePoint == MachineState.State.Repair) {
                removeList.Remove(removeList.Last());
            }
            foreach (var item in removeList) {
                machineStates.Remove(item);
            }
        }

        /// <summary>
        /// 计算速度率，即 Pr
        /// </summary>
        /// <returns></returns>
        public float? CalcOeeSpeedEff(string machineCode, OeeActions.CalcOeeSpeedType oeeSpeedType) {
            var setting = GlobalConfig.MachineSettingDict[machineCode];

            if (oeeSpeedType == OeeActions.CalcOeeSpeedType.MaxSpeedPlc) {
                return CalcOeeSpeedEffByPlc(machineCode);

            } else if (oeeSpeedType == OeeActions.CalcOeeSpeedType.MaxSpeedMq) {
                return CalcOeeSpeedEffByMq(machineCode);

            } else if (oeeSpeedType == OeeActions.CalcOeeSpeedType.MaxSpeedSetting) {
                return CalcOeeSpeedEffBySetting(machineCode, float.Parse(setting.OeeSpeedMax.ToString()));
            }
            return null;
        }

        /// <summary>
        /// <todo>计算 Oee 质量效率，即 Qr</todo>
        /// </summary>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        public float? CalcOeeQualityEff(string machineCode) {
            return null;
        }

        /// <summary>
        /// 通过排产任务来计算 Oee 速度效率
        /// </summary>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        public float? CalcOeeSpeedEffByMq(string machineCode) {
            //获取最大速度
            var forceSpeed = App.Store.GetState().DMesState.SchTaskDoingDict[machineCode]?.MqSchTask?.forceSpeed;
            if (!forceSpeed.HasValue) {
                return null;
            }
            if (forceSpeed.Value == 0) {
                Logger.Debug($"机台 {machineCode} 的 forceSpeed 为0，无法计算 Oee 速度效率");
                return null;
            }
            return CalcOeeSpeedEffBySetting(machineCode, forceSpeed.Value);
        }

        /// <summary>
        /// 通过设定值来计算速度效率
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        public float? CalcOeeSpeedEffBySetting(string machineCode, float maxVal) {
            if (maxVal == 0) {
                return null;
            }
            var setting = GlobalConfig.MachineSettingDict[machineCode];
            var speedCode = MachineConfig.MachineDict[machineCode].CpmNameToCodeDict[setting.OeeSpeed];
            var speedCpm = App.Store.GetState().CpmState.OnlineCpmsDict[machineCode][speedCode];
            if (speedCpm.ValueType == SmParamType.Signal) {
                return (float)speedCpm.GetFloatVal() / maxVal;
            }
            return null;
        }

        /// <summary>
        /// 从Plc获取最大速度来计算速度效率
        /// </summary>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        public float? CalcOeeSpeedEffByPlc(string machineCode) {
            float? speedEff = null;
            var setting = GlobalConfig.MachineSettingDict[machineCode];
            var maxSpeedCode = MachineConfig.MachineDict[machineCode].CpmNameToCodeDict[setting.OeeSpeedMax.ToString()];
            var maxSpeedCpm = App.Store.GetState().CpmState.OnlineCpmsDict[machineCode][maxSpeedCode];
            if (maxSpeedCpm.ValueType != SmParamType.Signal) {
                Logger.Error($"机台 {machineCode} 未采集到 {maxSpeedCpm.Name} 的值，将无法计算 Oee 速度效率", 36000);
                return null;
            }
            var speedCode = MachineConfig.MachineDict[machineCode].CpmNameToCodeDict[setting.OeeSpeed];
            var speedCpm = App.Store.GetState().CpmState.OnlineCpmsDict[machineCode][speedCode];
            if (speedCpm.ValueType != SmParamType.Signal) {
                Logger.Error($"机台 {machineCode} 未采集到 {maxSpeedCpm.Name} 的值，将无法计算 Oee 速度效率", 36000);
                return null;
            }
            var maxSpeed = (float)maxSpeedCpm.Value;
            var speed = (float)speedCpm.Value;
            if (maxSpeed == 0) {
                App.Store.Dispatch(new SysNotificationMsg() {
                    Title = "无法计算 Oee-速度效率",
                    Content = $"机台 {machineCode} 的 {maxSpeedCpm.Name} ==0",
                    Level = NotifyLevel.Warn
                });
                return null;
            }
            speedEff = speed / maxSpeed;
            return speedEff;
        }

        /// <summary>
        /// 获取机台开机运行时间
        /// </summary>
        /// <returns></returns>
        private double getMachineRunTimeSec(IList<MachineState> machineStates, float stateSpeed) {
            double runTimeSec = 0;
            //开工时间
            var workTime = YUtil.GetKeystoneWorkTime();
            //只有一个状态的情况
            if (machineStates.Count == 1) {
                //一个开机点，则认为之前都是关机状态
                if (machineStates[0].StatePoint == MachineState.State.Start) {
                    runTimeSec = (DateTime.Now - machineStates[0].Time).TotalSeconds;
                    //一个关机点，则认为之前都是开机状态
                } else if (machineStates[0].StatePoint == MachineState.State.Stop) {
                    runTimeSec = (machineStates[0].Time - workTime).TotalSeconds;
                    //一个维修点
                } else if (machineStates[0].StatePoint == MachineState.State.Repair) {
                    runTimeSec = 0;
                }
                //多个状态的情况
            } else if (machineStates.Count > 1) {
                for (var i = 0; i < machineStates.Count - 1; i += 1) {
                    var preeState = machineStates[i];
                    var nextState = machineStates[i + 1];
                    if (preeState.StatePoint == MachineState.State.Start && nextState.StatePoint != MachineState.State.Start) {
                        var diffSec = (nextState.Time - preeState.Time).TotalSeconds;
                        runTimeSec += diffSec;
                    }
                }
                //第一个点为关机，则开机时间在上班时间之前
                //加上当班时间点-->第一个关机时间点
                if (machineStates[0].StatePoint == MachineState.State.Stop) {
                    runTimeSec += (machineStates[0].Time - workTime).TotalSeconds;
                }
                //最后一个点为开机，则机台还在正常运转
                //加上最后一个开机时间点--->当前时间
                if (machineStates.Last().StatePoint == MachineState.State.Start) {
                    runTimeSec += (DateTime.Now - machineStates.Last().Time).TotalSeconds;
                }
                //没有保留的历史状态
            } else if (machineStates.Count == 0) {
                //机台当前正在运转，则认为从上班时间到现在未停过机
                if (stateSpeed > 0) {
                    runTimeSec = (DateTime.Now - workTime).TotalSeconds;
                    //机台未运转，则认为从上班时间到现在未开过机
                } else {
                    runTimeSec = 0;
                }
            }
            return runTimeSec;
        }

        /// <summary>
        ///<todo>获取机台调机时间</todo> 
        /// </summary>
        /// <returns></returns>
        private double getMachineDebugTimeSec() {
            return 0;
        }
    }
}
