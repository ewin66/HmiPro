using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentScheduler;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using Reducto;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// 调度器
    /// <href>https://github.com/fluentscheduler/FluentScheduler</href>
    /// <date>2017-12-20</date>
    /// <author>ychost</author>
    /// </summary>
    public class SchCore : Registry {
        private readonly SysEffects sysEffects;
        private readonly MqEffects mqEffects;
        private readonly OeeEffects oeeEffects;
        public readonly LoggerService Logger;
        public SchCore(SysEffects sysEffects, MqEffects mqEffects, OeeEffects oeeEffects) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.sysEffects = sysEffects;
            this.mqEffects = mqEffects;
            this.oeeEffects = oeeEffects;
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }

        /// <summary>
        /// 配置文件加载之后才能对其初始化
        /// 1. 每隔指定时间(15分钟)关闭显示器
        /// 2. 每天8:00打开显示器
        /// 3. 定时上传Cpm到Mq
        /// </summary>
        public async Task Init() {
            await App.Store.Dispatch(sysEffects.StartCloseScreenTimer(new SysActions.StartCloseScreenTimer(HmiConfig.CloseScreenInterval)));
            //启动定时上传Cpms到Mq定时器
            await App.Store.Dispatch(mqEffects.StartUploadCpmsInterval(new MqActiions.StartUploadCpmsInterval(HmiConfig.QueUpdateWebBoard, HmiConfig.UploadWebBoardInterval)));
            //每天8点打开显示器
            Schedule(() => {
                App.Store.Dispatch(new SysActions.OpenScreen());
            }).ToRunEvery(1).Days().At(8, 0);

            bool isContainSpeed = false;
            foreach (var pair in MachineConfig.MachineDict) {
                if (pair.Value.LogicToCpmDict.ContainsKey(CpmInfoLogic.Speed)) {
                    isContainSpeed = true;
                }
            }
            //每10分钟计算一次 Oee
            if (isContainSpeed) {
                var interval = 10 * 60 * 1000;
                await App.Store.Dispatch(oeeEffects.StartCalcOeeTimer(new OeeActions.StartCalcOeeTimer(interval)));
            } else {
                Logger.Error($"机台 {MachineConfig.AllMachineName} 未配置速度逻辑，将不会计算 Oee - 时间效率 - 速度效率");
            }

        }
    }
}
