﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Cores;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.Redux.Services;
using Reducto;

namespace HmiPro.Redux.Effects {
    public class CpmEffects {
        public readonly StorePro<AppState> StorePro;
        /// <summary>
        /// 异步启动服务
        /// </summary>
        public readonly StorePro<AppState>.AsyncActionNeedsParam<CpmActions.StartServer> StartServer;
        public readonly CpmCore CpmCore;

        public CpmEffects(StorePro<AppState> store, CpmCore cpmCore) {
            CpmCore = cpmCore;
            StorePro = store;
            StartServer = store.asyncActionVoid<CpmActions.StartServer>(async (dispatch, getState, startServer) => {
                dispatch(startServer);
                try {
                    await cpmCore.StartAsync(startServer.Ip,startServer.Port);
                    dispatch(new CpmActions.StartServerSuccess());
                } catch (Exception e) {
                    dispatch(new CpmActions.StartServerFailed() { Exception = e });
                }
            });

          
        }
    }
}
