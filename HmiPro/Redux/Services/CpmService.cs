using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using Reducto;
using YCsharp.Model.Procotol;
using YCsharp.Model.Procotol.SmParam;

namespace HmiPro.Redux.Services {
    /// <summary>
    /// 数据采集逻辑服务
    /// <date>2017-12-17</date>
    /// <author>ychost</author>
    /// </summary>
    public class CpmService {

        public YSmParamTcp SmParamTcp;
        public readonly StorePro<AppState> StorePro;

        public CpmService(StorePro<AppState> store) {
            StorePro = store;
        }

        public Task StartAsync(string ip, int port) {
            return Task.Run(() => {
                if (SmParamTcp == null) {
                    SmParamTcp = new YSmParamTcp(ip, port);
                    SmParamTcp.OnDataReceivedAction += smModelsHandler;
                }
                SmParamTcp.Start();
            });
        }

        public void Stop() {
            SmParamTcp?.StopSoft();
        }

        void smModelsHandler(string ip, List<SmModel> smModels) {
            List<Cpm> cpms = new List<Cpm>();
            //StorePro.Dispatch(new CpmActions.CpmReceived() { Cpms = cpms });
        }
    }
}
