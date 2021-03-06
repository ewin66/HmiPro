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
        /// <summary>
        /// Mq 操作利器
        /// </summary>
        private readonly MqEffects mqEffects;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mqEffects"></param>
        public DpmCore(MqEffects mqEffects) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.mqEffects = mqEffects;
        }

        /// <summary>
        /// 初始化事件是力气
        /// </summary>
        public void Init() {
            App.Store.Subscribe(new Dictionary<string, Action<AppState, IAction>>() {
                [DpmActions.SUBMIT] = doSubmitDpms,
            });
        }

        /// <summary>
        /// 提交回填参数
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">回填的参数内容</param>
        void doSubmitDpms(AppState state, IAction action) {
            var dpmAction = (DpmActions.Submit)action;
            var mqUploadDpm = new MqUploadDpm();
            var taskDoing = state.DMesState.SchTaskDoingDict[dpmAction.MachineCode];
            mqUploadDpm.proGgxh = taskDoing?.MqSchAxis?.product;
            mqUploadDpm.macCode = dpmAction.MachineCode;
            mqUploadDpm.paramJson = new Dictionary<string,string>();
            foreach (var dpm in dpmAction.Dpms) {
                mqUploadDpm.paramJson[dpm.Name] = dpm.Value;
            }
            //提交给Mq
            App.Store.Dispatch(mqEffects.UploadDpms(new MqActions.UploadDpms(dpmAction.MachineCode, mqUploadDpm)));
        }
    }
}
