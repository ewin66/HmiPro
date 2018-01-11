using System;
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
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Effects {
    /// <summary>
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public class MqEffects {
        private readonly ActiveMqService activeMq;
        private readonly MqService mqService;
        public readonly LoggerService Logger;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartListenSchTask, bool> StartListenSchTask;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.UploadCpms> UploadCpms;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.UploadAlarmMq> UploadAlarm;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartUploadCpmsInterval> StartUploadCpmsInterval;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartListenScanMaterial, bool> StartListenScanMaterial;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.UploadSchTaskManu, bool> UploadSchTaskManu;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartListenEmpRfid, bool> StartListenEmpRfid;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActions.StartListenAxisRfid, bool> StartListenAxisRfid;




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
        }

        void initStartListenAxisRfid() {
            StartListenAxisRfid = App.Store.asyncAction<MqActions.StartListenAxisRfid, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() => {
                        try {
                            activeMq.ListenTopic(instance.TopicName, null,
                                mqService.AxisRfidAccpet)();
                            dispatch(new SimpleAction(MqActions.START_LISTEN_AXIS_RFID_SUCCESS, null));
                            return true;
                        } catch (Exception e) {
                            dispatch(new SimpleAction(MqActions.START_LISTEN_AXIS_RFID_FAILED, null));
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
                            activeMq.ListenTopic(instance.TopicName, null, mqService.EmpRfidAccept)();
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
                    //dispatch(instance);
                    return await Task.Run(() => {
                        try {
                            activeMq.SendP2POneMessage(instance.QueueName, JsonConvert.SerializeObject(instance.MqUploadManu));
                            return true;
                        } catch (Exception e) {

                        }
                        return false;
                    });
                });
        }

        void initUploadAlarm() {
            UploadAlarm =
                App.Store.asyncActionVoid<MqActions.UploadAlarmMq>(async (dispatch, getState, instance) => {
                    await Task.Run(() => {
                        dispatch(instance);
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
                    return await Task.Run(() => {
                        dispatch(instance);
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
                    var result = await Task.Run(() => {
                        dispatch(instance);
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
                    await Task.Run(() => {
                        dipatch(instance);
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
                        if (getState().CpmState.OnlineCpmsDict[machineCode].TryGetValue(cpmNameToCodeDict[setting.MqNeedSpeed], out var speedCpm)) {
                            uCpms.macSpeed = speedCpm.Value;
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
                        foreach (var cpmPair in machineCpms) {
                            var cpm = cpmPair.Value;
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
                            Console.WriteLine($"上传Mq：Speed {uCpms.macSpeed} Timeff: {uCpms.TimeEff},SpeedEff：{uCpms.SpeedEff},QualityEff：{uCpms.QualityEff}");
                            activeMq.SendP2POneMessage(instance.QueueName, JsonConvert.SerializeObject(uCpms));
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
                    Console.WriteLine($"上传Mq超时{awaitTime / 1000}s");
                    App.Store.Dispatch(new MqActions.UploadCpmsFailed() { Exp = new Exception($"上传超时 {awaitTime / 1000}s ") });
                }
                task.Dispose();
            });
        }

    }
}
