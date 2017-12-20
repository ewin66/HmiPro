using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
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
        public SchTaskDoing MqSchTaskDoing = new SchTaskDoing();
        public readonly LoggerService Logger;
        readonly IDictionary<string, Action<AppState>> actionExecDict = new Dictionary<string, Action<AppState>>();

        public DMesCore(DbEffects dbEffects, MqEffects mqEffects) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.dbEffects = dbEffects;
            this.mqEffects = mqEffects;
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }

        /// <summary>
        /// 配置文件加载之后才能对其初始化
        /// </summary>
        public void Init() {
            actionExecDict[CpmActions.CPMS_UPDATED_ALL] = saveCpmsToInfluxDb;
            actionExecDict[MqActiions.SCH_TASK_ACCEPT] = treatNewSchTaskAccept;
            actionExecDict[CpmActions.NOTE_METER_ACCEPT] = treatNoteMeter;
            actionExecDict[AlarmActions.CHECK_CPM_BOM_ALARM] = checkCpmBomAlarm;
            actionExecDict[CpmActions.SPARK_DIFF_ACCEPT] = checkSparkDiffAlarm;
            App.Store.Subscribe(s => {
                //保存机台数据
                if (actionExecDict.TryGetValue(s.Type, out var exec)) {
                    exec(s);
                }
            });
        }

        /// <summary>
        /// 推送数据到influxDb
        /// </summary>
        /// <param name="state"></param>
        void saveCpmsToInfluxDb(AppState state) {
            var machineCode = state.CpmState.MachineCode;
            var updatedCpms = state.CpmState.UpdatedCpmsAllDict[machineCode];
            App.Store.Dispatch(
                dbEffects.UploadCpmsInfluxDb(new DbActions.UploadCpmsInfluxDb(machineCode, updatedCpms)));
        }

        /// <summary>
        /// 接受到新的任务
        /// </summary>
        /// <param name="state"></param>
        void treatNewSchTaskAccept(AppState state) {
            var machineCode = state.MqState.MachineCode;
            var task = state.MqState.MqSchTaskDict[machineCode];
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

        /// <summary>
        /// 检查火花报警
        /// </summary>
        /// <param name="state"></param>
        void checkSparkDiffAlarm(AppState state) {
            var machineCode = state.CpmState.MachineCode;
            var sparkCpm = state.CpmState.SparkDiffDict[machineCode];
            if ((int)sparkCpm.Value == 1) {
                var mqAlarm = createMqAlarm(machineCode, sparkCpm.PickTimeStampMs, AlarmType.SparkErr);
                dispatchAlarmAction(machineCode, mqAlarm);
            }
        }

        /// <summary>
        /// 对报警数据进行处理
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="mqAlarm"></param>
        void dispatchAlarmAction(string machineCode, MqAlarm mqAlarm) {
            if (mqAlarm == null) {
                Logger.Debug("任务未开始，报警数据无效");
                return;
            }
            //打开屏幕
            App.Store.Dispatch(new SysActions.OpenScreen());
            //通知系统报警
            App.Store.Dispatch(new AlarmActions.NotifyAlarm(machineCode, mqAlarm));
            //上传报警到Mq
            App.Store.Dispatch(mqEffects.UploadAlarm(new MqActiions.UploadAlarm(HmiConfig.QueWebSrvException, mqAlarm)));
            //保存报警到Mongo
            App.Store.Dispatch(dbEffects.UploadAlarmsMongo(new DbActions.UploadAlarmsMongo(machineCode, "Alarms", mqAlarm)));
        }

        MqAlarm createMqAlarm(string machineCode, long time, string alarmType) {
            if (!MqSchTaskDoing.IsStarted) {
                return null;
            }
            App.Store.GetState().CpmState.NoteMeterDict.TryGetValue(machineCode, out var meter);
            MqAlarm mqAlarm = new MqAlarm() {
                alarmType = alarmType,
                axisCode = MqSchTaskDoing?.MqSchAxis?.axiscode,
                machineCode = machineCode,
                meter = meter,
                time = time,
                workCode = MqSchTaskDoing?.MqSchTask?.workcode,
            };
            return mqAlarm;
        }

        /// <summary>
        /// 检查报警
        /// </summary>
        /// <param name="state"></param>
        void checkCpmBomAlarm(AppState state) {
            if (MqSchTaskDoing.MqSchTask == null) {
                return;
            }
            var machineCode = state.AlarmState.MachineCode;
            var checkAlarm = state.AlarmState.AlarmBomCheckDict[machineCode];
            var boms = MqSchTaskDoing.MqSchTask.bom;
            float? max = null;
            float? min = null;
            float? std = null;
            //从Bom表中求出最大、最小值
            foreach (var bom in boms) {
                bom.TryGetValue(checkAlarm.MaxBomKey, out var maxObj);
                bom.TryGetValue(checkAlarm.MinBomKey, out var minObj);
                bom.TryGetValue(checkAlarm.StdBomKey, out var stdObj);
                try {
                    max = maxObj != null ? (float?)maxObj : null;
                    min = minObj != null ? (float?)minObj : null;
                    std = stdObj != null ? (float?)stdObj : null;
                } catch (Exception e) {
                    Logger.Error($"任务 id={MqSchTaskDoing.MqSchTaskId} 的Bom表上下限有误" +
                                 $"{checkAlarm.MaxBomKey}: {maxObj},{checkAlarm.MinBomKey}:{minObj},{checkAlarm.StdBomKey}: {stdObj}");
                    return;
                }
            }
            //根据标准值求最小值
            if (std.HasValue && max.HasValue) {
                min = 2 * std - max;
            }
            //报警
            if (max.HasValue && min.HasValue) {
                var cpmVal = (float)checkAlarm.Cpm.Value;
                if (cpmVal > max || cpmVal < min) {
                    MqAlarm mqAlarm = createMqAlarm(machineCode, checkAlarm.Cpm.PickTimeStampMs, AlarmType.OdErr);
                    dispatchAlarmAction(machineCode, mqAlarm);
                }
            } else {
                Logger.Error($"未能从任务 Id={MqSchTaskDoing.MqSchTaskId}的Bom表中求出上下限，Max: {max},Min {min},Std: {std}");
            }
        }

        /// <summary>
        /// 记米相关处理
        /// </summary>
        /// <param name="state"></param>
        void treatNoteMeter(AppState state) {
            var machineCode = state.CpmState.MachineCode;
            var noteMeter = state.CpmState.NoteMeterDict[machineCode];
        }

        /// <summary>
        /// 根据轴号设置当前任务
        /// </summary>
        /// <param name="axisCode"></param>
        public void SetSchTaskDoing(string axisCode) {
            //搜索任务
            foreach (var st in MqSchTasks) {
                for (var i = 0; i < st.axisParam.Count; i++) {
                    var axis = st.axisParam[i];
                    if (axis.axiscode == axisCode) {
                        MqSchTaskDoing.MqSchTask = st;
                        MqSchTaskDoing.MqSchTaskId = st.id;
                        MqSchTaskDoing.MqSchAxisIndex = i;
                        MqSchTaskDoing.MqSchAxis = axis;
                        MqSchTaskDoing.IsStarted = true;
                        break;
                    }
                }
                if (MqSchTaskDoing.MqSchAxis != null) {
                    break;
                }
            }

        }
    }
}
