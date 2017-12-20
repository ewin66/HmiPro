using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using FSLib.App.SimpleUpdater;
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
    public class SysEffects {
        public readonly StorePro<AppState>.AsyncActionNeedsParam<SysActions.StartHttpSystem> StartHttpSystem;
        public readonly StorePro<AppState>.AsyncActionNeedsParam<SysActions.StartCloseScreenTimer> StartCloseScreenTimer;
        public readonly StorePro<AppState>.AsyncActionNeedsParam<SysActions.StopCloseScreenTimer> StopCloseScrenTimer;
        public readonly LoggerService Logger;
        public Timer CloseScrrenTimer;

        public SysEffects(SysService sysService) {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            //启动http解析服务
            StartHttpSystem = App.Store.asyncActionVoid<SysActions.StartHttpSystem>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    var isStarted = await sysService.StartHttpSystem(instance);
                    if (isStarted) {
                        App.Store.Dispatch(new SysActions.StartHttpSystemSuccess());
                    } else {
                        App.Store.Dispatch(new SysActions.StartHttpSystemFailed());
                    }
                });
            //启动关闭显示器定时器
            StartCloseScreenTimer = App.Store.asyncActionVoid<SysActions.StartCloseScreenTimer>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    if (CloseScrrenTimer != null) {
                        YUtil.RecoveryTimeout(CloseScrrenTimer);
                    } else {
                        CloseScrrenTimer = YUtil.SetInterval(instance.Interval, () => {
                            App.Store.Dispatch(new SysActions.CloseScreen());
                        });
                    }
                });

            //停止关闭显示器定时器
            StopCloseScrenTimer = App.Store.asyncActionVoid<SysActions.StopCloseScreenTimer>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    if (CloseScrrenTimer != null) {
                        YUtil.ClearTimeout(CloseScrrenTimer);
                    }
                });
        }

    }
}
