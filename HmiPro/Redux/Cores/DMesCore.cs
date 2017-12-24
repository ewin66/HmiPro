using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HmiPro.Annotations;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Mocks;
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
        public static readonly IDictionary<string, object> SchTaskDoingLocks = new Dictionary<string, object>();

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
            actionExecDict[CpmActions.CPMS_UPDATED_ALL] = whenCpmsUpdateAll;
            actionExecDict[MqActiions.SCH_TASK_ACCEPT] = whenSchTaskAccept;
            actionExecDict[CpmActions.NOTE_METER_ACCEPT] = whenNoteMeterAccept;
            actionExecDict[AlarmActions.CHECK_CPM_BOM_ALARM] = doCheckCpmBomAlarm;
            actionExecDict[CpmActions.SPARK_DIFF_ACCEPT] = whenSparkDiffAccept;
            actionExecDict[DMesActions.START_SCH_TASK_AXIS] = doStartSchTaskAxis;
            actionExecDict[CpmActions.SPEED_DIFF_ZERO_ACCEPT] = whenSpeedDiffZeroAccept;
            actionExecDict[CpmActions.SPEED_ACCEPT] = whenSpeedAccept;

            App.Store.Subscribe((state, action) => {
                if (actionExecDict.TryGetValue(state.Type, out var exec)) {
                    exec(state, action);
                }
            });
            //绑定全局的值
            SchTaskDoingDict = App.Store.GetState().DMesState.SchTaskDoingDict;
            MqSchTasksDict = App.Store.GetState().DMesState.MqSchTasksDict;
            foreach (var pair in MachineConfig.MachineDict) {
                SchTaskDoingLocks[pair.Key] = new object();
            }
        }

        /// <summary>
        /// 计算平均速度
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenSpeedAccept(AppState state, IAction action) {
            var speedAction = (CpmActions.SpeedAccept)action;
            var machineCode = speedAction.MachineCode;
            var speed = speedAction.Speed;
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                if (taskDoing.IsStarted && taskDoing.CalcAvgSpeed != null) {
                    taskDoing.SpeedAvg = (float)taskDoing.CalcAvgSpeed(speed);
                }
            }

        }

        /// <summary>
        /// 当速度变化为0
        /// 任务完成率大于 0.98 的时候则认为一轴的任务完成
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenSpeedDiffZeroAccept(AppState state, IAction action) {
            var speedAction = (CpmActions.SpeedDiffZeroAccept)action;
            var machineCode = speedAction.MachineCode;
            lock (SchTaskDoingDict[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                if (!taskDoing.IsStarted) {
                    return;
                }
                //一轴生成完成时候速度为0
                if (taskDoing.CompleteRate >= 0.98) {
                    CompleteOneAxis(machineCode, taskDoing?.MqSchAxis?.axiscode);
                    //调试完成的时候速度为0
                } else if (taskDoing.CompleteRate > 0) {
                    DebugOneAxisEnd(machineCode, taskDoing?.MqSchAxis.axiscode);
                }
            }
        }

        /// <summary>
        /// 推送数据到influxDb
        /// </summary>
        void whenCpmsUpdateAll(AppState state, IAction action) {
            var cpmAction = (CpmActions.CpmUpdatedAll)action;
            var machineCode = cpmAction.MachineCode;
            var updatedCpms = cpmAction.Cpms;
            App.Store.Dispatch(dbEffects.UploadCpmsInfluxDb(new DbActions.UploadCpmsInfluxDb(machineCode, updatedCpms)));
        }

        /// <summary>
        /// 接受到新的任务
        /// </summary>
        void whenSchTaskAccept(AppState state, IAction action) {
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
        void whenSparkDiffAccept(AppState state, IAction action) {
            var machineCode = state.CpmState.MachineCode;
            var sparkCpm = state.CpmState.SparkDiffDict[machineCode];
            if ((int)sparkCpm.Value == 1) {
                var mqAlarm = createMqAlarm(machineCode, sparkCpm.PickTimeStampMs, sparkCpm.Name, AlarmType.SparkErr);
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
            //产生一个报警
            App.Store.Dispatch(new AlarmActions.GenerateOneAlarm(machineCode, mqAlarm));
        }

        /// <summary>
        /// 创建一个报警对象
        /// 因为很多地方都要创建，这里提取其公共属性
        /// </summary>
        /// <param name="machineCode">机台编码</param>
        /// <param name="time">报警时间戳</param>
        /// <param name="alarmType">报警类型</param>
        /// <param name="cpmName">报警参数名</param>
        /// <returns></returns>
        private MqAlarm createMqAlarm(string machineCode, long time, string cpmName, string alarmType) {
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                //如果当前没有正在执行的任务，则报警无意义
                if (!SchTaskDoingDict[machineCode].IsStarted) {
                    return null;
                }
                App.Store.GetState().CpmState.NoteMeterDict.TryGetValue(machineCode, out var meter);
                var mqAlarm = new MqAlarm() {
                    CpmName = cpmName,
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
        /// 从Bom表中去出上下限，然后判断参数是否异常
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void doCheckCpmBomAlarm(AppState state, IAction action) {
            var checkAlarmAction = (AlarmActions.CheckCpmBomAlarm)action;
            var machineCode = checkAlarmAction.MachineCode;
            var taskDoing = SchTaskDoingDict[machineCode];
            lock (SchTaskDoingLocks[machineCode]) {
                //没有正在执行的任务，则无Bom，终止检查
                if (taskDoing.MqSchTask == null) {
                    return;
                }
                var checkAlarm = checkAlarmAction.AlarmBomCheck;
                var boms = taskDoing.MqSchTask.bom;
                if (boms == null) {
                    //10 分钟通知一次
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "检查报警失败",
                        Content = $"工单{taskDoing.WorkCode} 没有配置 Bom，则无法实现报警",
                        MinGapSec = 600
                    }));
                    return;
                }
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
                        var logDetail = $"任务 id={taskDoing.MqSchTaskId} 的Bom表上下限有误" +
                                     $"{checkAlarm.MaxBomKey}: {maxObj},{checkAlarm.MinBomKey}:{minObj},{checkAlarm.StdBomKey}: {stdObj}";
                        //10分钟通知一次
                        App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                            Title = $"机台 {machineCode} 报警失败",
                            Content = $"工单 {taskDoing.WorkCode} Bom表上下限有误",
                            MinGapSec = 600,
                            LogDetail = logDetail
                        }));
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
                        MqAlarm mqAlarm = createMqAlarm(machineCode, checkAlarm.Cpm.PickTimeStampMs, checkAlarm.Cpm.Name, AlarmType.OdErr);
                        dispatchAlarmAction(machineCode, mqAlarm);
                    }
                } else {
                    Logger.Error($"未能从任务 Id={taskDoing.MqSchTaskId}的Bom表中求出上下限，Max: {max},Min {min},Std: {std}");
                }
            }
        }

        /// <summary>
        /// 记米相关处理
        /// </summary>
        void whenNoteMeterAccept(AppState state, IAction action) {
            var meterAction = (CpmActions.NoteMeterAccept)action;
            var machineCode = meterAction.MachineCode;
            var noteMeter = meterAction.Meter;
            lock (SchTaskDoingDict[machineCode]) {
                var doingTask = SchTaskDoingDict[machineCode];
                if (SchTaskDoingDict[machineCode].IsStarted) {
                    doingTask.MeterWork = noteMeter;
                    var rate = noteMeter / doingTask.MeterPlan;
                    doingTask.CompleteRate = rate;
                }
            }

        }

        /// <summary>
        /// 根据轴号设置当前任务开始
        /// </summary>
        public void doStartSchTaskAxis(AppState state, IAction action) {
            var startParam = (DMesActions.StartSchTaskAxis)action;
            var machineCode = startParam.MachineCode;
            var axisCode = startParam.AxisCode;
            //搜索任务
            lock (SchTaskDoingLocks[machineCode]) {
                bool hasFound = false;
                var mqTasks = MqSchTasksDict[machineCode];
                foreach (var st in mqTasks) {
                    for (var i = 0; i < st.axisParam.Count; i++) {
                        var axis = st.axisParam[i];
                        if (axis.axiscode == axisCode) {
                            //重复启动任务
                            if (axis.IsStarted == true) {
                                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                                    Title = "请勿重复启动任务",
                                    Content = $"机台 {machineCode} 轴号： {axisCode}"
                                }));
                                App.Store.Dispatch(new SimpleAction(DMesActions.START_SCH_TASK_AXIS_FAILED));
                                return;
                            }
                            var taskDoing = SchTaskDoingDict[machineCode];
                            //其它任务在运行中
                            if (taskDoing.IsStarted) {
                                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                                    Title = $"尚有任务未完成，请先完成任务再启动新任务",
                                    Content = $"机台 {machineCode} 任务 {taskDoing.MqSchAxis.axiscode} 未完成"
                                }));
                                App.Store.Dispatch(new SimpleAction(DMesActions.START_SCH_TASK_AXIS_FAILED));
                                return;
                            }
                            //记米没有清零
                            var noteMeter = App.Store.GetState().CpmState.NoteMeterDict[machineCode];
                            if ((int)noteMeter != 0) {
                                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                                    Title = $"请先清零记米，再开始任务",
                                    Content = $"机台 {machineCode} 记米没有清零，请先清零"
                                }));
                                return;
                            }
                            setSchTaskDoing(taskDoing, st, axis, i);
                            hasFound = true;
                            break;
                        }
                    }
                    if (hasFound) {
                        break;
                    }
                }
                if (hasFound) {
                    //设置其它任务不能启动
                    SetOtherTaskAxisCanStart(machineCode, axisCode, false);
                    App.Store.Dispatch(new SimpleAction(DMesActions.START_SCH_TASK_AXIS_SUCCESS));
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "启动任务成功",
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
        /// 给当前进行的任务赋值，通过排产任务转换成进行任务
        /// </summary>
        /// <param name="taskDoing"></param>
        /// <param name="st"></param>
        /// <param name="axis"></param>
        /// <param name="axisIndex"></param>
        void setSchTaskDoing([NotNull]SchTaskDoing taskDoing, [NotNull] MqSchTask st, [NotNull] MqTaskAxis axis, int axisIndex) {
            taskDoing.MqSchTask = st;
            taskDoing.MqSchTaskId = st.id;
            taskDoing.MqSchAxisIndex = axisIndex;
            taskDoing.MqSchAxis = axis;
            taskDoing.IsStarted = true;
            taskDoing.Step = st.step;
            taskDoing.WorkCode = st.workcode;
            taskDoing.MeterPlan = axis.length;
            taskDoing.StartTime = DateTime.Now;
            taskDoing.CalcAvgSpeed = YUtil.CreateExecAvgFunc();
            axis.IsStarted = true;
            axis.State = MqSchTaskAxisState.Doing;
        }

        /// <summary>
        /// 设置其它轴的任务不能被启动
        /// </summary>
        /// <param name="machineCode">任务机台编码</param>
        /// <param name="startAxisCode">当前启动的轴任务</param>
        /// <param name="canStart">其它轴任务能否启动</param>
        public void SetOtherTaskAxisCanStart(string machineCode, string startAxisCode, bool canStart) {
            var tasks = MqSchTasksDict[machineCode];
            foreach (var task in tasks) {
                foreach (var axis in task.axisParam) {
                    if (axis.axiscode != startAxisCode) {
                        axis.CanStart = canStart;
                    }
                }
            }
        }


        /// <summary>
        /// 调试一轴结束
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="axisCode"></param>
        public void DebugOneAxisEnd(string machineCode, string axisCode) {
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                if (taskDoing.IsStarted) {
                    var meter = App.Store.GetState().CpmState.NoteMeterDict[machineCode];
                    taskDoing.MeterDebug = meter;
                    taskDoing.DebugTimestampMs = (long)(DateTime.Now - taskDoing.StartTime).TotalMilliseconds;
                }
            }
        }

        /// <summary>
        /// 完成某轴
        /// </summary>
        public async void CompleteOneAxis(string machineCode, string axisCode) {
            var taskDoing = SchTaskDoingDict[machineCode];
            MqUploadManu uManu = null;
            lock (SchTaskDoingLocks[machineCode]) {
                if (taskDoing.MqSchTask == null) {
                    Logger.Error($"机台 {machineCode} 没有进行中的任务");
                    return;
                }
                if (taskDoing.MqSchAxis.axiscode != axisCode) {
                    Logger.Error($"机台 {machineCode} 当前正在生产的轴号:{taskDoing.MqSchAxis?.axiscode}与设置完成轴号{axisCode}不一致");
                    return;
                }

                //显示完成消息
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = $"机台 {machineCode} 达成任务",
                    Content = $"轴号 {axisCode} 任务达成"
                }));

                //标志位改变
                taskDoing.MqSchAxis.State = MqSchTaskAxisState.Completed;
                taskDoing.MqSchAxis.IsCompleted = true;
                SetOtherTaskAxisCanStart(machineCode, axisCode, true);
                //更新当前任务完成进度
                var completedAxis = taskDoing.MqSchTask.axisParam.Count(a => a.IsCompleted == true);
                taskDoing.CompleteRate = completedAxis / taskDoing.MqSchTask.axisParam.Count;

                uManu = new MqUploadManu() {
                    actualBeginTime = YUtil.GetUtcTimestampMs(taskDoing.StartTime),
                    actualEndTime = YUtil.GetUtcTimestampMs(taskDoing.EndTime),
                    axisName = axisCode,
                    macCode = machineCode,
                    axixLen = taskDoing.MeterPlan,
                    courseCode = taskDoing.WorkCode,
                    empRfid = string.Join(",", taskDoing.EmpRfids),
                    rfids_begin = string.Join(",", taskDoing.StartRfids),
                    rfid_end = string.Join(",", taskDoing.EndRfids),
                    acutalDispatchTime = YUtil.GetUtcTimestampMs(taskDoing.StartTime),
                    mqType = "yes",
                    step = taskDoing.Step,
                    testLen = taskDoing.MeterDebug,
                    testTime = taskDoing.DebugTimestampMs,
                    speed = taskDoing.SpeedAvg,
                };
                //重新初始化
                taskDoing.Init();
            }
            var uploadResult = await App.Store.Dispatch(mqEffects.UploadSchTaskManu(new MqActiions.UploadSchTaskManu(HmiConfig.QueWebSrvPropSave, uManu)));
            if (uploadResult) {
                //一个工单任务完成
                if (taskDoing.CompleteRate >= 1) {
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
