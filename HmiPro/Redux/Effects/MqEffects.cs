﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Data.Helpers;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.Redux.Services;
using Newtonsoft.Json;
using Reducto;
using YCsharp.Model.Procotol.SmParam;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Effects {
    /// <summary>
    /// 监听排产任务、机台指令、上传采集参数等等
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public class MqEffects {
        private readonly ActiveMqService activeMq;
        private readonly MqService mqService;
        public readonly LoggerService Logger;
        /// <summary>
        /// 监听排产任务
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartListenSchTask, bool> StartListenSchTask;
        /// <summary>
        /// 上传实时参数
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.UploadCpms> UploadCpms;
        /// <summary>
        /// 上传报警数据
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.UploadAlarmMq> UploadAlarm;
        /// <summary>
        /// 周期上传实时参数
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartUploadCpmsInterval> StartUploadCpmsInterval;
        /// <summary>
        /// 监听来料信息
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartListenScanMaterial, bool> StartListenScanMaterial;
        /// <summary>
        /// 上传排产落轴数据
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.UploadSchTaskManu, bool> UploadSchTaskManu;
        /// <summary>
        /// 回传电能数据
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.UploadElecPower, bool> UploadElecPower;

        /// <summary>
        /// 监听人员卡
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartListenEmpRfid, bool> StartListenEmpRfid;
        /// <summary>
        /// 监听轴号卡
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartListenAxisRfid, bool> StartListenAxisRfid;
        /// <summary>
        /// 上传回填数据（员工手工录入的数据）
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.UploadDpms, bool> UploadDpms;
        /// <summary>
        /// 呼叫叉车、维修等等
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.CallSystem, bool> CallSystem;
        /// <summary>
        /// 监听命令
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartListenCmd, bool> StartListenCmd;

        /// <summary>
        /// 初始化各个 Effect
        /// </summary>
        /// <param name="mqService"></param>
        public MqEffects(MqService mqService) {
            UnityIocService.AssertIsFirstInject(GetType());

            activeMq = ActiveMqHelper.GetActiveMqService();
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            this.mqService = mqService;
            initSchTaskEffect();
            initUploadCpmsEffect();
            initStartUploadCpmsIntervalEffect();
            initStartListenScanMaterial();
            initUploadAlarm();
            initUploadSchTaskManu();
            initStartListenEmpRfid();
            initStartListenAxisRfid();
            initUploadDpms();
            initCallSystem();
            initStartListenCmd();
            initUploadElecPower();
        }


        /// <summary>
        /// 上传能耗
        /// </summary>
        void initUploadElecPower() {
            UploadElecPower = App.Store.asyncAction<MqActions.UploadElecPower, bool>(
                async (dispatch, getState, instance) => {
                    return await Task.Run(() => {
                        try {
                            String str = JsonConvert.SerializeObject(instance.elec);
                            App.Logger.Info("回传总电能：" + str);
                            activeMq.SendP2POneMessage(HmiConfig.QueUploadPowerElec, str);
                            return true;
                        } catch (Exception e) {
                            App.Logger.Error("上传能耗失败", e);
                        }
                        return false;
                    });
                });
        }

        void initStartListenCmd() {
            StartListenCmd = App.Store.asyncAction<MqActions.StartListenCmd, bool>(
                        async (dispatch, getState, instance) => {
                            dispatch(instance);
                            return await Task.Run(() => {
                                try {
                                    activeMq.ListenTopic(instance.TopicName, instance.Id, null, mqService.CmdAccept);
                                    return true;
                                } catch (Exception e) {
                                    dispatch(new SimpleAction(MqActions.START_LISTEN_CMD_FAILED, e));
                                }
                                return false;
                            });
                        });
        }
        /// <summary>
        /// 连接服务器
        /// </summary>
        public void Start() {
            activeMq.Start();
        }

        void initCallSystem() {
            CallSystem = App.Store.asyncAction<MqActions.CallSystem, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() => {
                        try {
                            activeMq.SendP2POneMessage(HmiConfig.QueueCallSystem, JsonConvert.SerializeObject(instance.MqCall));
                            dispatch(new SimpleAction(MqActions.CALL_SYSTEM_SUCCESS));
                            return true;
                        } catch (Exception e) {
                            dispatch(new SimpleAction(MqActions.CALL_SYSTEM_FAILED, e));
                        }
                        return false;
                    });
                });
        }

        void initUploadDpms() {
            UploadDpms = App.Store.asyncAction<MqActions.UploadDpms, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() => {
                        try {
                            activeMq.SendP2POneMessage(HmiConfig.QueueDpms, JsonConvert.SerializeObject(instance.MqUploadDpms));
                            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                                Title = "提交成功"
                            }));
                            App.Store.Dispatch(new SimpleAction(MqActions.UPLOAD_DPMS_SUCCESS));
                            return true;
                        } catch {
                            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                                Title = "提交失败",
                                Content = "请检查网络连接或联系管理员"
                            }));
                            App.Store.Dispatch(new SimpleAction(MqActions.UPLOAD_DPMS_FAILED));
                        }
                        return false;
                    });
                }
            );
        }

        void initStartListenAxisRfid() {
            StartListenAxisRfid = App.Store.asyncAction<MqActions.StartListenAxisRfid, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() => {
                        try {
                            activeMq.ListenTopic(instance.TopicName, instance.Id, null, mqService.AxisRfidAccpet);
                            dispatch(new SimpleAction(MqActions.START_LISTEN_AXIS_RFID_SUCCESS, null));
                            return true;
                        } catch (Exception e) {
                            dispatch(new SimpleAction(MqActions.START_LISTEN_AXIS_RFID_FAILED, e));
                        }
                        return false;
                    });
                });
        }

        void initStartListenEmpRfid() {
            StartListenEmpRfid = App.Store.asyncAction<MqActions.StartListenEmpRfid, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() => {
                        try {
                            activeMq.ListenTopic(instance.TopicName, instance.Id, null, mqService.EmpRfidAccept);
                            dispatch(new MqActions.StartListenEmpRfidSuccess());
                            return true;
                        } catch (Exception e) {
                            dispatch(new MqActions.StartListenEmpRfidFailed(e));
                        }
                        return false;
                    });
                }

            );

        }

        void initUploadSchTaskManu() {
            UploadSchTaskManu = App.Store.asyncAction<MqActions.UploadSchTaskManu, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() => {
                        try {
                            activeMq.SendP2POneMessage(instance.QueueName, JsonConvert.SerializeObject(instance.MqUploadManu));
                            return true;
                        } catch {

                        }
                        return false;
                    });
                });
        }

        void initUploadAlarm() {
            UploadAlarm = App.Store.asyncActionVoid<MqActions.UploadAlarmMq>(async (dispatch, getState, instance) => {
                dispatch(instance);
                await Task.Run(() => {
                    try {
                        activeMq.SendP2POneMessage(instance.QueueName, JsonConvert.SerializeObject(instance.MqAlarm));
                        App.Store.Dispatch(new SimpleAction(MqActions.UPLOAD_ALARM_SUCCESS));
                    } catch (Exception e) {
                        App.Store.Dispatch(new SimpleAction(MqActions.UPLOAD_ALARM_FAILED));
                        Logger.Error("上传报警到Mq失败", e);
                    }
                });
            });
        }

        void initStartListenScanMaterial() {
            StartListenScanMaterial =
                App.Store.asyncAction<MqActions.StartListenScanMaterial, bool>(async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() => {
                        try {
                            activeMq.ListenP2PMessage(instance.QueueName, json => {
                                mqService.ScanMaterialAccept(instance.MachineCode, json);
                            });
                            App.Store.Dispatch(new MqActions.StartListenScanMaterialSuccess(instance.MachineCode));
                            return true;
                        } catch (Exception e) {
                            App.Store.Dispatch(new MqActions.StartListenScanMaterialFailed() { Exp = e, MachineCode = instance.MachineCode });
                        }
                        return false;
                    });
                });
        }

        void initSchTaskEffect() {
            StartListenSchTask =
                App.Store.asyncAction<MqActions.StartListenSchTask, bool>(async (dispatch, getState, instance) => {
                    dispatch(instance);
                    var result = await Task.Run(() => {
                        try {
                            activeMq.ListenP2PMessage(instance.QueueName, this.mqService.SchTaskAccept);
                            dispatch(new MqActions.StartListenSchTaskSuccess(instance.MachineCode));
                            return true;
                        } catch (Exception e) {
                            dispatch(new MqActions.StartListenSchTaskFailed() { Exp = e, MachineCode = instance.MachineCode });
                        }
                        return false;
                    });
                    return result;
                });
        }

        void initStartUploadCpmsIntervalEffect() {
            StartUploadCpmsInterval =
                App.Store.asyncActionVoid<MqActions.StartUploadCpmsInterval>(async (dipatch, getState, instance) => {
                    dipatch(instance);
                    await Task.Run(() => {
                        YUtil.SetInterval(instance.Interval, () => {
                            App.Store.Dispatch(UploadCpms(new MqActions.UploadCpms(getState().CpmState.OnlineCpmsDict, instance.QueueName)));
                        });
                    });
                });
        }

        void initUploadCpmsEffect() {
            UploadCpms = App.Store.asyncActionVoid<MqActions.UploadCpms>(async (dispatch, getState, instance) => {
                var task = Task.Run(() => {
                    var cpmsDict = instance.CpmsDict;
                    foreach (var pair in cpmsDict) {
                        var machineCode = pair.Key;
                        var machineCpms = pair.Value;
                        MqUploadCpms uCpms = new MqUploadCpms();
                        uCpms.machineCode = machineCode;
                        var setting = GlobalConfig.MachineSettingDict[machineCode];
                        var cpmNameToCodeDict = MachineConfig.MachineDict[machineCode].CpmNameToCodeDict;
                        //fix: 2018-1-24
                        if (cpmNameToCodeDict.ContainsKey(setting.MqNeedSpeed)) {
                            if (getState().CpmState.OnlineCpmsDict[machineCode]
                                .TryGetValue(cpmNameToCodeDict[setting.MqNeedSpeed], out var speedCpm)) {
                                uCpms.macSpeed = speedCpm.Value;
                            }
                        }
                        // fix: 2018-1-24
                        if (cpmNameToCodeDict.ContainsKey(setting.Od)) {
                            if (getState().CpmState.OnlineCpmsDict[machineCode]
                                .TryGetValue(cpmNameToCodeDict[setting.Od], out var od)) {
                                uCpms.diameter = od.Value.ToString();
                            }
                        }
                        //update:2018-4-13，添加总电能
                        if (cpmNameToCodeDict.ContainsKey(setting.totalPower)) {
                            if (getState().CpmState.OnlineCpmsDict[machineCode]
                                .TryGetValue(cpmNameToCodeDict[setting.totalPower], out var tp)) {
                                uCpms.totalPower = tp.Value.ToString();
                            }
                        }

                        uCpms.TimeEff = getState().OeeState.OeeDict[machineCode].TimeEff.ToString("0.00");
                        uCpms.SpeedEff = getState().OeeState.OeeDict[machineCode].SpeedEff.ToString("0.00");
                        uCpms.QualityEff = getState().OeeState.OeeDict[machineCode].QualityEff.ToString("0.00");
                        //机台状态
                        string state = MqUploadCpms.MachineState.Running;
                        var machineState = getState().CpmState.MachineStateDict[machineCode];
                        if (machineState.Count > 0) {
                            if (machineState.Last().StatePoint == MachineState.State.Start) {
                                state = MqUploadCpms.MachineState.Running;
                            } else if (machineState.Last().StatePoint == MachineState.State.Stop) {
                                state = MqUploadCpms.MachineState.Closed;
                            } else if (machineState.Last().StatePoint == MachineState.State.Repair) {
                                state = MqUploadCpms.MachineState.Repairing;
                            }
                        }
                        uCpms.machineState = state;
                        uCpms.paramInfoList = new List<UploadParamInfo>();
                        //如果底层没有采集到数据则不上传
                        bool canUpload = false;
                        foreach (var cpmPair in machineCpms) {
                            var cpm = cpmPair.Value;
                            if (cpm.ValueType == SmParamType.Signal && cpm.Code >= 500 && cpm.Code < 1000) {
                                canUpload = true;
                            }
                            var uInfo = new UploadParamInfo() {
                                paramCode = cpm.Code.ToString(),
                                paramName = cpm.Name,
                                paramValue = cpm.Value,
                                valueType = cpm.ValueType,
                                pickTimeStamp = cpm.PickTimeStampMs,
                            };
                            uCpms.paramInfoList.Add(uInfo);
                        }
                        try {
                            if (canUpload) {
                                activeMq.SendP2POneMessage(instance.QueueName, JsonConvert.SerializeObject(uCpms));
                            } else {
                                App.Logger.Info("所有数据为空，不上传 Mq",false,ConsoleColor.White);
                            }
                            App.Store.Dispatch(new MqActions.UploadCpmsSuccess());
                        } catch (Exception e) {
                            App.Store.Dispatch(new MqActions.UploadCpmsFailed() { Exp = e });
                        }
                    }
                });

                //每个机台等待3s
                var awaitTime = MachineConfig.MachineDict.Count * 3000;
                if (await Task.WhenAny(task, Task.Delay(awaitTime)) == task) {
                    //在时间之内完成了task
                } else {
                    //超时
                    Logger.Error("上传实时参数到 Mq 超时", 36000);
                    App.Store.Dispatch(new MqActions.UploadCpmsFailed() { Exp = new Exception($"上传超时 {awaitTime / 1000}s ") });
                }
                task.Dispose();
            });
        }

    }
}
