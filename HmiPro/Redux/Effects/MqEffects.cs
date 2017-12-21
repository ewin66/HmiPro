using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Data.Helpers;
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
        public StorePro<AppState>.AsyncActionNeedsParam<MqActiions.StartListenSchTask, bool> StartListenSchTask;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActiions.UploadCpms> UploadCpms;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActiions.UploadAlarm> UploadAlarm;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActiions.StartUploadCpmsInterval> StartUploadCpmsInterval;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActiions.StartListenScanMaterial, bool> StartListenScanMaterial;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActiions.UploadSchTaskManu, bool> UploadSchTaskManu;


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
        }

        void initUploadSchTaskManu() {
            UploadSchTaskManu = App.Store.asyncAction<MqActiions.UploadSchTaskManu, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() => {
                        try {
                            activeMq.SendP2POneMessage(instance.QueueName, JsonConvert.SerializeObject(instance.MqUploadManu));
                        } catch (Exception e) {

                        }
                        return false;
                    });
                });
        }

        void initUploadAlarm() {
            UploadAlarm =
                App.Store.asyncActionVoid<MqActiions.UploadAlarm>(async (dispatch, getState, instance) => {
                    await Task.Run(() => {
                        dispatch(instance);
                        try {
                            activeMq.SendP2POneMessage(instance.QueueName, JsonConvert.SerializeObject(instance.MqAlarm));
                            App.Store.Dispatch(new SimpleAction(MqActiions.UPLOAD_ALARM_SUCCESS));
                        } catch (Exception e) {
                            App.Store.Dispatch(new SimpleAction(MqActiions.UPLOAD_ALARM_FAILED));
                            Logger.Error("上传报警到Mq失败", e);
                        }
                    });
                });
        }

        void initStartListenScanMaterial() {
            StartListenScanMaterial =
                App.Store.asyncAction<MqActiions.StartListenScanMaterial, bool>(async (dispatch, getState, instance) => {
                    return await Task.Run(() => {
                        dispatch(instance);
                        try {
                            activeMq.ListenP2PMessage(instance.QueueName, mqService.ScanMaterialAccept);
                            App.Store.Dispatch(new MqActiions.StartListenScanMaterialSuccess(instance.MachineCode));
                            return true;
                        } catch (Exception e) {
                            App.Store.Dispatch(new MqActiions.StartListenScanMaterialFailed() { Exp = e, MachineCode = instance.MachineCode });
                        }
                        return false;
                    });
                });
        }

        void initSchTaskEffect() {
            StartListenSchTask =
                App.Store.asyncAction<MqActiions.StartListenSchTask, bool>(async (dispatch, getState, instance) => {
                    var result = await Task.Run(() => {
                        dispatch(instance);
                        try {
                            activeMq.ListenP2PMessage(instance.QueueName, this.mqService.SchTaskAccept);
                            dispatch(new MqActiions.StartListenSchTaskSuccess(instance.MachineCode));
                            return true;
                        } catch (Exception e) {
                            dispatch(new MqActiions.StartListenSchTaskFailed() { Exp = e, MachineCode = instance.MachineCode });
                        }
                        return false;
                    });
                    return result;
                });
        }

        void initStartUploadCpmsIntervalEffect() {
            StartUploadCpmsInterval =
                App.Store.asyncActionVoid<MqActiions.StartUploadCpmsInterval>(async (dipatch, getState, instance) => {
                    await Task.Run(() => {
                        dipatch(instance);
                        YUtil.SetInterval(instance.Interval, () => {
                            App.Store.Dispatch(UploadCpms(new MqActiions.UploadCpms(getState().CpmState.OnlineCpmsDict, instance.QueueName)));
                        });
                    });
                });
        }

        void initUploadCpmsEffect() {
            UploadCpms = App.Store.asyncActionVoid<MqActiions.UploadCpms>(async (dispatch, getState, instance) => {
                await Task.Run(() => {
                    var cpmsDict = instance.CpmsDict;
                    foreach (var pair in cpmsDict) {
                        var machineCode = pair.Key;
                        var machineCpms = pair.Value;
                        MqUploadCpms uCpms = new MqUploadCpms();
                        uCpms.machineCode = machineCode;
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
                            activeMq.SendP2POneMessage(instance.QueueName, JsonConvert.SerializeObject(uCpms));
                            App.Store.Dispatch(new MqActiions.UploadCpmsSuccess());
                        } catch (Exception e) {
                            App.Store.Dispatch(new MqActiions.UploadCpmsFailed() { Exp = e });
                        }
                    }
                });
            });
        }

    }
}
