using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using Reducto;
using YCsharp.Model.Procotol;
using YCsharp.Model.Procotol.SmParam;
using YCsharp.Service;
using DevExpress.XtraPrinting.Native;

namespace HmiPro.Redux.Services {
    /// <summary>
    /// 数据采集逻辑服务
    /// <date>2017-12-17</date>
    /// <author>ychost</author>
    /// </summary>
    public class CpmService {

        public YSmParamTcp SmParamTcp;
        public readonly StorePro<AppState> Store;
        public LoggerService Logger;
        //编码：值（在线数据）
        //加float增加算法参数计算效率
        //计算相关参数的时候需要使用
        readonly IDictionary<string, IDictionary<int, float>> onlineFloatDict = new ConcurrentDictionary<string, IDictionary<int, float>>();

        //编码：采集参数
        //外部可能会对此进行采样
        public readonly IDictionary<string, IDictionary<int, Cpm>> OnlineCpmDict = new ConcurrentDictionary<string, IDictionary<int, Cpm>>();

        public CpmService(StorePro<AppState> store) {
            Store = store;
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
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
            string machineCode = "";
            if (MachineConfig.IpToMachineCodeDict.TryGetValue(ip, out var code)) {
                machineCode = code;
            } else {
                Logger.Error($"ip {ip} 未注册");
                return;
            }
            smModels?.ForEach(sm => {
                //处理参数包
                if (sm.PackageType == SmPackageType.ParamPackage) {
                    paramPkgHandler(code, sm);
                }
                //处理心跳包
                else if (sm.PackageType == SmPackageType.HeartbeatPackage) {
                }
            });
        }

        void paramPkgHandler(string code, SmModel sm) {
            var cpmsDirect = Cpm.ConvertBySmModel(code, sm);
            var cpmsRelate = new List<Cpm>();
            var cpms = new List<Cpm>();
            //简洁算法计算出来的参数
            cpmsDirect?.ForEach(cpm => {
                if (cpm.ValueType == SmParamType.Signal) {
                    var floatVal = (float)cpm.Value;
                    onlineFloatDict[code][cpm.Code] = floatVal;
                    var relateCpms = calcRelateCpm(code, cpm.Code);
                    cpmsRelate.AddRange(relateCpms);
                }
            });
            if (cpmsDirect != null) {
                cpms.AddRange(cpmsDirect);
                cpms.AddRange(cpmsRelate);
            }
            //更新参数字典
            var changedCpms = new List<Cpm>();
            //fixed:2017-10-20
            //      一包参数的时间应该一致
            var pickTime = DateTime.Now;

            void update(Cpm cpm) {
                cpm.PickTime = pickTime;
                OnlineCpmDict[code][cpm.Code] = cpm;
                changedCpms.Add(cpm);
            }

            //差异更新，使用linq 2017-11-13
            (from c in cpms
             where (
                 c.Value != null
                 && (!OnlineCpmDict[code].ContainsKey(c.Code) ||
                     OnlineCpmDict[code][c.Code].ToString() != c.Value.ToString())
             )
             select c
            ).ForEach(update);

            //推送采集参数数据
            if (changedCpms.Count > 0) {
                Store.Dispatch(new CpmActions.CpmUpdateDiff(code, changedCpms));
            }
            if (cpms.Count > 0) {
                Store.Dispatch(new CpmActions.CpmUpdatedAll(code, cpms));
            }

        }


        /// <summary>
        /// 更新需要算法计算的参数，并返回转换后的集合
        /// </summary>
        /// <returns></returns>
        List<Cpm> calcRelateCpm(string code, int updatedCode) {
            List<Cpm> cpms = new List<Cpm>();
            foreach (var infoPair in MachineConfig.MachineDict[code].CodeToRelateCpmDict) {
                var val = infoPair.Value.ExecCalc(onlineFloatDict[code], updatedCode);
                if (val.HasValue) {
                    //设置精度
                    val = (float)Math.Round(val.Value, HmiConfig.MathRound);
                    //实时更新在线数据表
                    onlineFloatDict[code][infoPair.Key] = val.Value;
                    cpms.Add(new Cpm() {
                        Code = infoPair.Key,
                        Value = val,
                        ValueType = SmParamType.Signal,
                        Name = infoPair.Value.Name
                    });
                }
            }
            return cpms;
        }
    }
}
