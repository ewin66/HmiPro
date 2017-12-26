using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using YCsharp.Model.Procotol.SmParam;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// <date>2017-12-20</date>
    /// <author>ychost</author>
    /// </summary>
    public class OeeCore {
        public readonly LoggerService Logger;
        public OeeCore() {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }
        /// <summary>
        /// 计算Oee的开机率，Or
        /// </summary>
        /// <returns></returns>
        public float? CalcOeeTimeEff(string machineCode, IList<MachineState> machineStates) {
            float? timeEff = null;
            if (!MachineConfig.MachineDict[machineCode].LogicToCpmDict.ContainsKey(CpmInfoLogic.OeeSpeed)) {
                Logger.Debug($"机台 {machineCode} 未配置速度逻辑，无法判断开停机，无法计算 Oee - 时间效率", ConsoleColor.Red);
                return null;
            }
            float currentSpeed = 0;
            currentSpeed = App.Store.GetState().CpmState.SpeedDict[machineCode];
            var runTimeSec = getMachineRunTimeSec(machineStates, currentSpeed);
            var debugTimeSec = getMachineDebugTimeSec();
            var workTime = YUtil.GetKeystoneWorkTime();
            //计算时间效率
            if (runTimeSec < 0) {
                Logger.Error($"计算时间效率失败，有效时间 {runTimeSec} < 0 ");
            } else {
                timeEff = (float)((runTimeSec - debugTimeSec) / (DateTime.Now - workTime).TotalSeconds);
                Console.WriteLine($"当班时间：{(DateTime.Now - workTime).TotalSeconds} 秒，机台运行时间 {runTimeSec} 秒");
            }
            return timeEff;
        }

        /// <summary>
        /// 计算速度率，即 Pr
        /// </summary>
        /// <returns></returns>
        public float? CalcOeeSpeedEff(string machineCode, OeeActions.CalcOeeSpeedType oeeSpeedType) {
            if (oeeSpeedType == OeeActions.CalcOeeSpeedType.MaxSpeedPlc) {
                return CalcOeeSpeedEffByPlc(machineCode);
            } else if (oeeSpeedType == OeeActions.CalcOeeSpeedType.MaxSpeedMq) {
                return CalcOeeSpeedEffByMq(machineCode);
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
        /// <todo>通过从Mq获取到的最大速度来计算Oee速度效率</todo>
        /// </summary>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        public float? CalcOeeSpeedEffByMq(string machineCode) {
            return null;
        }

        /// <summary>
        /// 从Plc获取最大速度来计算速度效率
        /// </summary>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        public float? CalcOeeSpeedEffByPlc(string machineCode) {
            float? speedEff = null;
            var maxSpeedCode = MachineConfig.MachineDict[machineCode].LogicToCpmDict[CpmInfoLogic.MaxSpeedPlc].Code;
            var maxSpeedCpm = App.Store.GetState().CpmState.OnlineCpmsDict[machineCode][maxSpeedCode];
            if (maxSpeedCpm.ValueType != SmParamType.Signal) {
                Logger.Error($"机台 {machineCode} 未采集到 {maxSpeedCpm.Name} 的值，将无法计算 Oee 速度效率");
                return null;
            }
            var speedCode = MachineConfig.MachineDict[machineCode].LogicToCpmDict[CpmInfoLogic.OeeSpeed].Code;
            var speedCpm = App.Store.GetState().CpmState.OnlineCpmsDict[machineCode][speedCode];
            if (speedCpm.ValueType != SmParamType.Signal) {
                Logger.Error($"机台 {machineCode} 未采集到 {maxSpeedCpm.Name} 的值，将无法计算 Oee 速度效率");
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
        private double getMachineRunTimeSec(IList<MachineState> machineStates, float currentSpeed) {
            double runTimeSec = -1;
            //开工时间
            var workTime = YUtil.GetKeystoneWorkTime();
            //只有一个状态的情况
            if (machineStates.Count == 1) {
                //一个开机点，则认为之前都是关机状态
                if (machineStates[0].StatePoint == MachineState.State.Start) {
                    runTimeSec = (DateTime.Now - machineStates[0].Time).TotalSeconds;
                    //一个关机点，则认为之前都是开机状态
                } else {
                    runTimeSec = (machineStates[0].Time - workTime).TotalSeconds;
                }
                //多个状态的情况
            } else if (machineStates.Count > 1) {
                for (var i = 0; i < machineStates.Count - 1; i += 1) {
                    var preeState = machineStates[i];
                    var nextState = machineStates[i + 1];
                    if (preeState.StatePoint == MachineState.State.Start && nextState.StatePoint == MachineState.State.Stop) {
                        runTimeSec += (nextState.Time - preeState.Time).TotalSeconds;
                    }
                }
                //开机时间为开工之前
                if (machineStates[0].StatePoint == MachineState.State.Stop) {
                    runTimeSec += (machineStates[0].Time - workTime).TotalSeconds;
                }
                //当前正在运转
                if (machineStates.Last().StatePoint == MachineState.State.Start) {
                    runTimeSec += (DateTime.Now - machineStates[0].Time).TotalSeconds;
                }
                //没有保留的历史状态
            } else if (machineStates.Count == 0) {
                //机台当前正在运转，则认为从上班时间到现在未停过机
                if (currentSpeed > 0) {
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
