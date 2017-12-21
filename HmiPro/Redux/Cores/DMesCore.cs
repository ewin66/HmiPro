using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using MongoDB.Bson;
using NeoSmart.AsyncLock;
using Newtonsoft.Json;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// DMes 系统的核心逻辑
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public class DMesCore {
        private readonly DbEffects dbEffects;
        private readonly MqEffects mqEffects;
        /// <summary>
        /// 每个机台接受到的所有任务
        /// </summary>
        public IDictionary<string, ObservableCollection<MqSchTask>> MqSchTasksDict;
        /// <summary>
        /// 每个机台的当前工作任务
        /// </summary>
        public IDictionary<string, SchTaskDoing> SchTaskDoingDict;
        /// <summary>
        /// 日志辅助
        /// </summary>
        public readonly LoggerService Logger;
        /// <summary>
        /// 命令派发执行的动作
        /// </summary>
        readonly IDictionary<string, Action<AppState, IAction>> actionExecDict = new Dictionary<string, Action<AppState, IAction>>();
        /// <summary>
        /// 任务锁
        /// </summary>
        public static readonly IDictionary<string, AsyncLock> SchTaskDoingLockDict = new Dictionary<string, AsyncLock>();

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
            actionExecDict[DMesActions.START_SCH_TASK_AXIS] = startSchTaskDoing;

            App.Store.Subscribe((state, action) => {
                if (actionExecDict.TryGetValue(state.Type, out var exec)) {
                    exec(state, action);
                }
            });
            //绑定全局的值
            SchTaskDoingDict = App.Store.GetState().DMesState.SchTaskDoingDict;
            MqSchTasksDict = App.Store.GetState().DMesState.MqSchTasksDict;
            foreach (var pair in MachineConfig.MachineDict) {
                SchTaskDoingLockDict[pair.Key] = new AsyncLock();
            }
        }

        /// <summary>
        /// 推送数据到influxDb
        /// </summary>
        /// <param name="state"></param>
        void saveCpmsToInfluxDb(AppState state, IAction action) {
            var machineCode = state.CpmState.MachineCode;
            var updatedCpms = state.CpmState.UpdatedCpmsAllDict[machineCode];
            App.Store.Dispatch(dbEffects.UploadCpmsInfluxDb(new DbActions.UploadCpmsInfluxDb(machineCode, updatedCpms)));
        }

        /// <summary>
        /// 接受到新的任务
        /// </summary>
        /// <param name="state"></param>
        void treatNewSchTaskAccept(AppState state, IAction action) {
            var machineCode = state.MqState.MachineCode;
            var mqTasks = MqSchTasksDict[machineCode];
            var task = state.MqState.MqSchTaskAccpetDict[machineCode];
            foreach (var cacheTask in mqTasks) {
                if (cacheTask.id == task.id) {
                    Logger.Error($"任务id重复,id={cacheTask.id}");
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "系统异常",
                        Content = $"接收到重复任务，请联系管理员，任务Id {task.id},工单 {task.workcode}"
                    }));
                    return;
                }
            }
            //将任务添加到任务队列里面
            mqTasks.Add(task);
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                ctx.SavePersist(new Persist(@"task_" + machineCode, JsonConvert.SerializeObject(mqTasks)));
            }
        }

        /// <summary>
        /// 检查火花报警
        /// </summary>
        /// <param name="state"></param>
        void checkSparkDiffAlarm(AppState state, IAction action) {
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
            //显示消息通知
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                Title = "警报",
                Content = machineCode + ":" + mqAlarm.alarmType
            }));
            //上传报警到Mq
            App.Store.Dispatch(mqEffects.UploadAlarm(new MqActiions.UploadAlarm(HmiConfig.QueWebSrvException, mqAlarm)));
            //保存报警到Mongo
            App.Store.Dispatch(dbEffects.UploadAlarmsMongo(new DbActions.UploadAlarmsMongo(machineCode, "Alarms", mqAlarm)));
        }

        private MqAlarm createMqAlarm(string machineCode, long time, string alarmType) {
            using (SchTaskDoingLockDict[machineCode].Lock()) {
                var taskDoing = SchTaskDoingDict[machineCode];
                if (!SchTaskDoingDict[machineCode].IsStarted) {
                    return null;
                }
                App.Store.GetState().CpmState.NoteMeterDict.TryGetValue(machineCode, out var meter);
                var mqAlarm = new MqAlarm() {
                    alarmType = alarmType,
                    axisCode = taskDoing?.MqSchAxis?.axiscode,
                    machineCode = machineCode,
                    meter = meter,
                    time = time,
                    workCode = taskDoing?.MqSchTask?.workcode,
                };
                return mqAlarm;
            }
        }

        /// <summary>
        /// 检查报警
        /// </summary>
        /// <param name="state"></param>
        void checkCpmBomAlarm(AppState state, IAction action) {
            var machineCode = state.AlarmState.MachineCode;
            var taskDoing = SchTaskDoingDict[machineCode];
            if (taskDoing.MqSchTask == null) {
                return;
            }
            var checkAlarm = state.AlarmState.AlarmBomCheckDict[machineCode];
            var boms = taskDoing.MqSchTask.bom;
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
                    Logger.Error($"任务 id={taskDoing.MqSchTaskId} 的Bom表上下限有误" +
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
                Logger.Error($"未能从任务 Id={taskDoing.MqSchTaskId}的Bom表中求出上下限，Max: {max},Min {min},Std: {std}");
            }
        }

        /// <summary>
        /// 记米相关处理
        /// </summary>
        /// <param name="state"></param>
        void treatNoteMeter(AppState state, IAction action) {
            var machineCode = state.CpmState.MachineCode;
            var noteMeter = state.CpmState.NoteMeterDict[machineCode];


        }

        /// <summary>
        /// 根据轴号设置当前任务开始
        /// </summary>
        public void startSchTaskDoing(AppState state, IAction action) {
            var startParam = (DMesActions.StartSchTaskAxis)action;
            var machineCode = startParam.MachineCode;
            var axisCode = startParam.AxisCode;
            //搜索任务
            using (SchTaskDoingLockDict[machineCode].Lock()) {
                bool hasFound = false;
                var mqTasks = MqSchTasksDict[machineCode];
                foreach (var st in mqTasks) {
                    for (var i = 0; i < st.axisParam.Count; i++) {
                        var axis = st.axisParam[i];
                        if (axis.axiscode == axisCode) {
                            if (axis.IsStarted == true) {
                                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                                    Title = "请勿重复启动任务",
                                    Content = $"机台 {machineCode} 轴号： {axisCode}"
                                }));
                                App.Store.Dispatch(new SimpleAction(DMesActions.START_SCH_TASK_AXIS_FAILED));
                                return;
                            }
                            var taskDoing = SchTaskDoingDict[machineCode];
                            if (taskDoing.IsStarted) {
                                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                                    Title = $"尚有任务未完成，请先完成任务再启动新任务",
                                    Content = $"机台 {machineCode} 任务 {taskDoing.MqSchAxis.axiscode} 未完成"
                                }));
                                App.Store.Dispatch(new SimpleAction(DMesActions.START_SCH_TASK_AXIS_FAILED));
                                return;
                            }
                            taskDoing.MqSchTask = st;
                            taskDoing.MqSchTaskId = st.id;
                            taskDoing.MqSchAxisIndex = i;
                            taskDoing.MqSchAxis = axis;
                            taskDoing.IsStarted = true;
                            taskDoing.Step = st.step;
                            taskDoing.WorkCode = st.workcode;
                            hasFound = true;
                            axis.IsStarted = true;
                            break;
                        }
                    }
                    if (hasFound) {
                        break;
                    }
                }
                if (hasFound) {
                    App.Store.Dispatch(new SimpleAction(DMesActions.START_SCH_TASK_AXIS_SUCCESS));
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "开始任务",
                        Content = $"机台 {machineCode} 轴号： {axisCode}"
                    }));
                } else {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "启动任务失败，请联系管理员",
                        Content = $"机台 {machineCode} 轴号： {axisCode}"
                    }));
                    App.Store.Dispatch(new SimpleAction(DMesActions.START_SCH_TASK_AXIS_FAILED));
                }
            }
        }

        /// <summary>
        /// 完成某轴
        /// </summary>
        public async void CompleteOneAxis(string machineCode, string axisCode) {
            using (await SchTaskDoingLockDict[machineCode].LockAsync()) {
                //显示完成消息
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = $"机台 {machineCode} 达成新任务",
                    Content = $"轴号 {axisCode} 任务达成"
                }));

                var taskDoing = SchTaskDoingDict[machineCode];
                if (taskDoing.MqSchTask == null) {
                    Logger.Error($"机台 {machineCode} 没有进行中的任务");
                    return;
                }
                if (taskDoing.MqSchAxis.axiscode != axisCode) {
                    Logger.Error($"机台 {machineCode} 当前正在生产的轴号:{taskDoing.MqSchAxis?.axiscode}与设置完成轴号{axisCode}不一致");
                    return;
                }
                var uManu = new MqUploadManu() {
                    actualBeginTime = YUtil.GetUtcTimestampMs(taskDoing.StartTime),
                    actualEndTime = YUtil.GetUtcTimestampMs(taskDoing.EndTime),
                    axisName = axisCode,
                    macCode = machineCode,
                    axixLen = taskDoing.Meter,
                    courseCode = taskDoing.WorkCode,
                    empRfid = string.Join(",", taskDoing.EmpRfids),
                    rfids_begin = string.Join(",", taskDoing.StartRfids),
                    rfid_end = string.Join(",", taskDoing.EndRfids),
                    acutalDispatchTime = YUtil.GetUtcTimestampMs(taskDoing.StartTime),
                    mqType = "yes",
                    step = taskDoing.Step,
                    testLen = 0,
                    testTime = 0,
                    speed = 0,
                };
                var uploadResult = await App.Store.Dispatch(
                    mqEffects.UploadSchTaskManu(new MqActiions.UploadSchTaskManu(HmiConfig.QueWebSrvPropSave, uManu)));
                if (uploadResult) {
                    //一个工单任务完成
                    if (taskDoing.MqSchAxisIndex >= taskDoing.MqSchTask.axisParam.Count - 1) {
                        CompleteOneSchTask(machineCode, taskDoing.WorkCode);
                    }
                    //上传落轴数据失败，对其进行缓存
                } else {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = $"机台 {machineCode} 上传任务达成进度失败",
                        Content = $"轴号 {axisCode} 任务 上传服务器失败，请检查网络连接"
                    }));
                    using (var ctx = SqliteHelper.CreateSqliteService()) {
                        ctx.UploadManuFailures.Add(uManu);
                    }
                }
            }
        }

        /// <summary>
        /// 完成某个工单
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="workCode"></param>
        public void CompleteOneSchTask(string machineCode, string workCode) {
            var mqTasks = MqSchTasksDict[machineCode];
            var removeTask = mqTasks.FirstOrDefault(t => t.workcode == workCode);
            mqTasks.Remove(removeTask);
            //更新缓存
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                ctx.SavePersist(new Persist($"task_{machineCode}", JsonConvert.SerializeObject(mqTasks)));
            }
        }
    }
}
