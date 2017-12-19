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
        public StorePro<AppState>.AsyncActionNeedsParam<MqActiions.StartListenSchTask> StartListenSchTask;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActiions.UploadCpms> UploadCpms;
        public StorePro<AppState>.AsyncActionNeedsParam<MqActiions.StartUploadCpmsInterval> StartUploadCpmsInterval;

        public MqEffects(MqService mqService) {
            UnityIocService.AssertIsFirstInject(GetType());

            activeMq = ActiveMqHelper.GetActiveMqService();
            this.mqService = mqService;
            initSchTaskEffect();
            initUploadCpmsEffect();
            initStartUploadCpmsIntervalEffect();
        }

        void initSchTaskEffect() {
            StartListenSchTask =
                App.Store.asyncActionVoid<MqActiions.StartListenSchTask>(async (dispatch, getState, instance) => {
                    await Task.Run(() => {
                        dispatch(instance);
                        try {
                            activeMq.ListenP2PMessage(instance.QueueName, this.mqService.SchTaskAccept);
                            dispatch(new MqActiions.StartListenSchTaskSuccess());
                        } catch (Exception e) {
                            dispatch(new MqActiions.StartListenSchTaskFailed() { Exp = e });
                        }
                    });
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
