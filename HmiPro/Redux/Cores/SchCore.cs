using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using FluentScheduler;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using Newtonsoft.Json;
using Reducto;
using YCsharp.Model.Procotol.SmParam;
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
        /// <summary>
        /// 启动定时关闭显示器
        /// </summary>
        private readonly SysEffects sysEffects;
        /// <summary>
        /// 定时上传采集参数到 Mq
        /// </summary>
        private readonly MqEffects mqEffects;
        /// <summary>
        /// 日志
        /// </summary>
        public readonly LoggerService Logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sysEffects"></param>
        /// <param name="mqEffects"></param>
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
            JobManager.JobException += info => Logger.Error("An error just happened with a scheduled job: " + info.Exception);
            //自动关闭显示器
            await App.Store.Dispatch(sysEffects.StartCloseScreenTimer(new SysActions.StartCloseScreenTimer(HmiConfig.CloseScreenInterval)));
            //启动定时上传Cpms到Mq定时器
            await App.Store.Dispatch(mqEffects.StartUploadCpmsInterval(new MqActions.StartUploadCpmsInterval(HmiConfig.QueUpdateWebBoard, HmiConfig.UploadWebBoardInterval)));

            //定义完成一些额外的任务
            //YUtil.SetInterval(HmiConfig.UploadWebBoardInterval, () => {
            //    updateGrafana();
            //});

            //一小时缓存一次485状态
            YUtil.SetInterval(3600000, () => {
                persistCom485State();
            });

            //每天8点打开显示器
            Schedule(() => {
                App.Store.Dispatch(new SysActions.OpenScreen());
                App.Logger.Info("8 点开启显示器");
            }).ToRunEvery(1).Days().At(8, 0);

            JobManager.Initialize(this);
        }

        /// <summary>
        /// 将485状态持久化到 Sqlite 中去
        /// </summary>
        void persistCom485State() {
            SqliteHelper.DoAsync(ctx => {
                ctx.SavePersist(new Persist("com485", JsonConvert.SerializeObject(App.Store.GetState().CpmState.Com485StatusDict)));
            });
        }

        /// <summary>
        /// 更新 Grafana 所需要的实时数据
        /// </summary>
        void updateGrafana() {
            foreach (var pair in MachineConfig.MachineDict) {
                IDictionary<string, string> dict = new Dictionary<string, string>();
                string machineCode = pair.Key;
                var onlineCpms = App.Store.GetState().CpmState.OnlineCpmsDict[machineCode];
                updateGrafanaDict(dict, onlineCpms, DefinedParamCode.OeeTime, "oee_or");
                updateGrafanaDict(dict, onlineCpms, DefinedParamCode.OeeSpeed, "oee_pr");
                updateGrafanaDict(dict, onlineCpms, DefinedParamCode.OeeQuality, "oee_qr");
                updateGrafanaDict(dict, onlineCpms, DefinedParamCode.Oee, "oee");
                updateGrafanaDict(dict, onlineCpms, DefinedParamCode.StopTime, "stop_time");
                updateGrafanaDict(dict, onlineCpms, DefinedParamCode.RunTime, "run_time");
                updateGrafanaDict(dict, onlineCpms, DefinedParamCode.DutyTime, "total_time");
                updateGrafanaDict(dict, onlineCpms, DefinedParamCode.Od, "fact_diameter");
                updateGrafanaDict(dict, onlineCpms, DefinedParamCode.NoteMeter, "pro_length");

                Task.Run(() => {
                    HttpHelper.UpdateGrafana(dict, machineCode);
                });
            }
        }

        /// <summary>
        /// 更新 Grafana 字典
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="onlieCpms"></param>
        /// <param name="cpmCode"></param>
        /// <param name="key"></param>
        /// <param name="defaultval"></param>
        void updateGrafanaDict(IDictionary<string, string> dict, IDictionary<int, Cpm> onlieCpms, int cpmCode,
            string key, string defaultval = "未知") {
            if (onlieCpms.TryGetValue(cpmCode, out var cpm)) {
                if (cpm.ValueType == SmParamType.Signal) {
                    dict[key] = cpm.GetFloatVal().ToString("0.00");
                } else {
                    dict[key] = defaultval;
                }
            }
        }
    }
}
