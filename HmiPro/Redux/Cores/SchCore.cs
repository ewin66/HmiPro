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
using HmiPro.Redux.Models;
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
        public readonly LoggerService Logger;

        public SchCore(SysEffects sysEffects, MqEffects mqEffects) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.sysEffects = sysEffects;
            this.mqEffects = mqEffects;
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }

        /// <summary>
        /// 配置文件加载之后才能对其初始化
        /// 1. 每隔指定时间(15分钟)关闭显示器
        /// 2. 每天8:00打开显示器
        /// 3. 定时上传Cpm到Mq
        /// </summary>
        public async Task Init() {
            await App.Store.Dispatch(
                sysEffects.StartCloseScreenTimer(new SysActions.StartCloseScreenTimer(HmiConfig.CloseScreenInterval)));
            //启动定时上传Cpms到Mq定时器
            await App.Store.Dispatch(mqEffects.StartUploadCpmsInterval(
                new MqActions.StartUploadCpmsInterval(HmiConfig.QueUpdateWebBoard, HmiConfig.UploadWebBoardInterval)));

            //每天8点打开显示器
            Schedule(() => {
                App.Store.Dispatch(new SysActions.OpenScreen());
            }).ToRunEvery(1).Days().At(8, 0);
            JobManager.Initialize(this);
        }


        void FluentSchTest() {
            Schedule(() => {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "测试Fluent Schedule Task",
                    Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }));
                Logger.Debug("测试Flunt Schedule");
            }).ToRunEvery(1).Days().At(10, 40);

        }
    }
}
