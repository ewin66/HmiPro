using System;
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
    /// <summary>
    /// 开启参数采集服务等等
    /// <author>ychost</author>
    /// <date>2017-12-19</date>
    /// </summary>
    public class CpmEffects {
        public readonly StorePro<AppState> StorePro;
        /// <summary>
        /// 异步启动服务
        /// </summary>
        public readonly StorePro<AppState>.AsyncActionNeedsParam<CpmActions.StartServer,bool> StartServer;

        /// <summary>
        /// 主要是数据采集服务是在 CpmCore 中启动的
        /// </summary>
        public readonly CpmCore CpmCore;

        public CpmEffects(StorePro<AppState> store, CpmCore cpmCore) {
            CpmCore = cpmCore;
            StorePro = store;
            StartServer = store.asyncAction<CpmActions.StartServer,bool>(async (dispatch, getState, startServer) => {
                dispatch(startServer);
                try {
                    await cpmCore.StartAsync(startServer.Ip,startServer.Port);
                    dispatch(new CpmActions.StartServerSuccess());
                    return true;
                } catch (Exception e) {
                    dispatch(new CpmActions.StartServerFailed() { Exception = e });
                }
                return false;
            });

          
        }
    }
}
