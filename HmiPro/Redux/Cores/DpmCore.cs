﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using Newtonsoft.Json;
using YCsharp.Service;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// 回填参数核心逻辑
    /// <author>ychost</author>
    /// <date>2017-01-17</date>
    /// </summary>
    public class DpmCore {
        private readonly MqEffects mqEffects;

        public DpmCore(MqEffects mqEffects) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.mqEffects = mqEffects;
        }

        public void Init() {
            App.Store.Subscribe(new Dictionary<string, Action<AppState, IAction>>() {
                [DpmActions.SUBMIT] = doSubmitDpms,
            });
        }

        /// <summary>
        /// 提交回填参数
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void doSubmitDpms(AppState state, IAction action) {
            var dpmAction = (DpmActions.Submit)action;
            List<MqUploadDpm> mqUploadDpms = new List<MqUploadDpm>();
            foreach (var dpm in dpmAction.Dpms) {
                mqUploadDpms.Add(new MqUploadDpm() {
                    macCode = dpmAction.MachineCode,
                    paramName = dpm.Name,
                    paramValue = dpm.Value
                });
            }
            //提交给Mq
            App.Store.Dispatch(mqEffects.UploadDpms(new MqActions.UploadDpms(dpmAction.MachineCode, mqUploadDpms)));
        }
    }
}
