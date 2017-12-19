using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
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
using HmiPro.Config.Models;
using HmiPro.Redux.Effects;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// 数据采集逻辑服务
    /// <date>2017-12-17</date>
    /// <author>ychost</author>
    /// </summary>
    public class CpmCore {
        public YSmParamTcp SmParamTcp;
        public LoggerService Logger;
        //编码：值（在线数据）
        //加float增加算法参数计算效率
        //计算相关参数的时候需要使用
        readonly IDictionary<string, IDictionary<int, float>> onlineFloatDict = new ConcurrentDictionary<string, IDictionary<int, float>>();
        /// <summary>
        /// 订阅指令：执行逻辑
        /// </summary>
        readonly IDictionary<string, Action<AppState>> actionExecDict = new ConcurrentDictionary<string, Action<AppState>>();

        //编码：采集参数
        //外部可能会对此进行采样
        public IDictionary<string, IDictionary<int, Cpm>> OnlineCpmDict;

        public CpmCore() {
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            foreach (var pair in MachineConfig.MachineDict) {
                onlineFloatDict[pair.Key] = new ConcurrentDictionary<int, float>();
            }
            actionExecDict[AlarmActions.OPEN_ALARM_LIGHTS] = openAlarmLights;
            actionExecDict[AlarmActions.CLOSE_ALARM_LIGHTS] = closeAlarmLights;
        }

        /// <summary>
        /// 异步启动服务，将在线参数字典与AppState全局字典进行绑定
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public Task StartAsync(string ip, int port) {
            return Task.Run(() => {
                if (SmParamTcp == null) {
                    SmParamTcp = new YSmParamTcp(ip, port);
                    SmParamTcp.OnDataReceivedAction += smModelsHandler;
                    OnlineCpmDict = App.Store.GetState().CpmState.OnlineCpmsDict;
                    App.Store.Subscribe(s => {
                        if (actionExecDict.TryGetValue(s.Type, out var exec)) {
                            exec(s);
                        }
                    });
                }
                SmParamTcp.Start();

            });
        }

        /// <summary>
        /// 停止掉Tcp监听服务
        /// </summary>
        public void Stop() {
            SmParamTcp?.StopSoft();
        }


        /// <summary>
        /// 打开报警灯，报警Ip必须配置
        /// </summary>
        /// <param name="s"></param>
        void openAlarmLights(AppState s) {
            var machineCode = s.AlarmState.MachineCode;
            if (MachineConfig.AlarmIpDict.TryGetValue(machineCode, out var ip)) {
                SmParamTcp?.OpenAlarm(ip);
            } else {
                Logger.Error($"机台 {machineCode} 没有报警 ip");
            }
        }

        /// <summary>
        /// 关闭报警灯，报警Ip必须配置
        /// </summary>
        /// <param name="s"></param>
        void closeAlarmLights(AppState s) {
            var machineCode = s.AlarmState.MachineCode;
            if (MachineConfig.AlarmIpDict.TryGetValue(machineCode, out var ip)) {
                SmParamTcp?.CloseAlarm(ip);
            } else {
                Logger.Error($"机台 {machineCode} 没有报警 ip");
            }
        }


        /// <summary>
        /// 来自底层数据处理
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="smModels"></param>
        void smModelsHandler(string ip, List<SmModel> smModels) {
            if (!MachineConfig.IpToMachineCodeDict.TryGetValue(ip, out var code)) {
                Logger.Error($"ip {ip} 未注册");
                return;
            }
            App.Store.Dispatch(new CpmActions.CpmIpActivted(ip, DateTime.Now));
            smModels?.ForEach(sm => {
                //处理参数包
                if (sm.PackageType == SmPackageType.ParamPackage) {
                    paramPkgHandler(code, sm);
                }
            });
        }

        /// <summary>
        /// 处理参数包，将底层的包转成上层需要的Cpm
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="sm"></param>
        void paramPkgHandler(string machineCode, SmModel sm) {
            var cpmsDirect = Cpm.ConvertBySmModel(machineCode, sm);
            var cpmsRelate = new List<Cpm>();
            var cpms = new List<Cpm>();
            IDictionary<int, Cpm> updatedCpmsDiffDict = new Dictionary<int, Cpm>();
            //简洁算法计算出来的参数
            cpmsDirect?.ForEach(cpm => {
                if (cpm.ValueType == SmParamType.Signal) {
                    var floatVal = (float)cpm.Value;
                    onlineFloatDict[machineCode][cpm.Code] = floatVal;
                    var relateCpms = calcRelateCpm(machineCode, cpm.Code);
                    cpmsRelate.AddRange(relateCpms);
                }
            });
            if (cpmsDirect != null) {
                cpms.AddRange(cpmsDirect);
                cpms.AddRange(cpmsRelate);
            }
            //更新参数字典
            //fixed:2017-10-20
            //      一包参数的时间应该一致
            var pickTime = DateTime.Now;

            void update(Cpm cpm) {
                cpm.PickTime = pickTime;
                //直接更新，会更新界面的数据
                if (OnlineCpmDict[machineCode].TryGetValue(cpm.Code, out var oldCpm)) {
                    oldCpm.Update(cpm.Value, cpm.ValueType, pickTime);
                } else {
                    Logger.Error($"参数 {cpm.Code} 未注册");
                }
                updatedCpmsDiffDict[cpm.Code] = cpm;
            }

            //差异更新，使用linq 2017-11-13
            (from c in cpms
             where (
                 c.Value != null
                 && (!OnlineCpmDict[machineCode].ContainsKey(c.Code) ||
                     OnlineCpmDict[machineCode][c.Code].ToString() != c.Value.ToString())
             )
             select c
            ).ForEach(update);
            //一定要先派遣所有更新，再派遣部分更新
            //这样就保证了reducer里面数据的唯一性
            if (cpms.Count > 0) {
                App.Store.Dispatch(new CpmActions.CpmUpdatedAll(machineCode, cpms));
                dispatchNoteMeter(machineCode, cpms, (meter) => {
                    App.Store.Dispatch(new CpmActions.NoteMeterAccept(machineCode, meter));
                });
            }
            if (updatedCpmsDiffDict.Count > 0) {
                App.Store.Dispatch(new CpmActions.CpmUpdateDiff(machineCode, updatedCpmsDiffDict));
                dispatchNoteMeter(machineCode, updatedCpmsDiffDict.Values.ToList(), (meter) => {
                    App.Store.Dispatch(new CpmActions.NoteMeterDiffAccept(machineCode, meter));
                });
            }
            //检查报警
            dispatchCheckCpm(machineCode, cpms);
        }

        /// <summary>
        /// 将需要报警检查的Cpm发送出去
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="cpms"></param>
        void dispatchCheckCpm(string machineCode, List<Cpm> cpms) {
            cpms?.ForEach(cpm => {
                if (MachineConfig.MachineDict[machineCode].CodeToAlarmCpmDict.TryGetValue(cpm.Code, out var alarmCpm)) {
                    App.Store.Dispatch(new AlarmActions.CheckCpm(cpm));
                }
            });
        }

        /// <summary>
        /// 派遣记米相关指令
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="Cpms"></param>
        /// <param name="dispatch"></param>
        void dispatchNoteMeter(string machineCode, List<Cpm> Cpms, Action<float> dispatch) {
            Cpms?.ForEach(cpm => {
                if (MachineConfig.MachineDict[machineCode].CodeToLogicDict.TryGetValue(cpm.Code, out var logicCpm)) {
                    if (logicCpm == CpmInfoLogic.NoteMeter) {
                        if (cpm.ValueType == SmParamType.Signal) {
                            dispatch((float)cpm.Value);
                        } else {
                            Logger.Error($"记米参数 {cpm.Name} 的值不是浮点，值为：{cpm.Value}");
                        }
                    }
                }
            });
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
