using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FSLib.App.SimpleUpdater;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.Redux.Services;
using Newtonsoft.Json;
using YCsharp.Service;

namespace HmiPro.Redux.Effects {
    /// <summary>
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public class SysEffects {
        public readonly StorePro<AppState>.AsyncActionNeedsParam<SysActions.StartHttpSystem> StartHttpSystem;
        public readonly LoggerService Logger;

        public SysEffects(StorePro<AppState> store, SysService sysService) {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            //启动http解析服务
            StartHttpSystem = store.asyncActionVoid<SysActions.StartHttpSystem>(
                async (dispatch, getState, startHttpSystem) => {
                    dispatch(startHttpSystem);
                    var isStarted = await sysService.StartHttpSystem(startHttpSystem);
                    if (isStarted) {
                        App.Store.Dispatch(new SysActions.StartHttpSystemSuccess());
                    } else {
                        App.Store.Dispatch(new SysActions.StartHttpSystemFailed());
                    }
                });
        }

    }
}
