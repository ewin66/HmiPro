using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using Newtonsoft.Json;
using YCsharp.Service;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// DMes 系统的核心逻辑
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public class DMesCore {
        private readonly DbEffects dbEffects;
        private readonly MqEffects mqEffects;
        public readonly List<MqSchTask> MqSchTasks = new List<MqSchTask>();
        public readonly LoggerService Logger;
        public IDictionary<string, Action<AppState>> ExecDict = new Dictionary<string, Action<AppState>>();

        public DMesCore(DbEffects dbEffects, MqEffects mqEffects) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.dbEffects = dbEffects;
            this.mqEffects = mqEffects;
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }

        public void Init() {
            ExecDict[CpmActions.CPMS_UPDATED_ALL] = saveCpmsToInfluxDb;
            ExecDict[MqActiions.SCH_TASK_ACCEPT] = treatNewSchTaskAccept;
            ExecDict[CpmActions.NOTE_METER_ACCEPT] = treatNoteMeter;


            App.Store.Subscribe(s => {
                //保存机台数据
                if (ExecDict.TryGetValue(s.Type, out var exec)) {
                    exec(s);
                }
            });
        }

        /// <summary>
        /// 推送数据到influxDb
        /// </summary>
        /// <param name="s"></param>
        void saveCpmsToInfluxDb(AppState s) {
            var machineCode = s.CpmState.MachineCode;
            var updatedCpms = s.CpmState.UpdatedCpmsAllDict[machineCode];
            App.Store.Dispatch(dbEffects.UploadCpmsInfluxDb(new DbActions.UploadCpmsInfluxDb(machineCode, updatedCpms)));
        }

        /// <summary>
        /// 接受到新的任务
        /// </summary>
        /// <param name="s"></param>
        void treatNewSchTaskAccept(AppState s) {
            var machineCode = s.MqState.MachineCode;
            var task = s.MqState.MqSchTaskDict[machineCode];
            foreach (var cache in MqSchTasks) {
                if (cache.id == task.id) {
                    Logger.Error($"任务id重复,id={cache.id}");
                }
                return;
            }

            //将任务添加到任务队列里面
            MqSchTasks.Add(task);
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                ctx.SavePersist(new Persist(@"task_" + machineCode, JsonConvert.SerializeObject(MqSchTasks)));
            }
        }

        void treatNoteMeter(AppState s) {
            var machineCode = s.CpmState.MachineCode;
            var noteMeter = s.CpmState.NoteMeterDict[machineCode];

        }
    }
}
