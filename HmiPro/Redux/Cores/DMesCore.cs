using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HmiPro.Annotations;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Helpers;
using HmiPro.Mocks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using MongoDB.Bson;
using NeoSmart.AsyncLock;
using Newtonsoft.Json;
using YCsharp.Model.Procotol.SmParam;
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
        /// 每个机台的来料信息
        /// </summary>
        public IDictionary<string, MqScanMaterial> MqScanMaterialDict;
        /// <summary>
        /// 人员卡信息
        /// </summary>
        public IDictionary<string, List<MqEmpRfid>> MqEmpRfidDict;

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
            actionExecDict[MqActions.SCH_TASK_ACCEPT] = whenSchTaskAccept;
            actionExecDict[CpmActions.NOTE_METER_ACCEPT] = whenNoteMeterAccept;
            actionExecDict[AlarmActions.CHECK_CPM_BOM_ALARM] = doCheckCpmBomAlarm;
            actionExecDict[CpmActions.SPARK_DIFF_ACCEPT] = whenSparkDiffAccept;
            actionExecDict[DMesActions.START_SCH_TASK_AXIS] = doStartSchTaskAxis;
            actionExecDict[CpmActions.STATE_SPEED_ZERRO_ACCEPT] = whenSpeedZeroAccept;
            actionExecDict[CpmActions.STATE_SPEED_ACCEPT] = whenSpeedAccept;
            actionExecDict[DMesActions.RFID_ACCPET] = doRfidAccept;
            actionExecDict[MqActions.SCAN_MATERIAL_ACCEPT] = whenScanMaterialAccept;
            actionExecDict[AlarmActions.CPM_PLC_ALARM_OCCUR] = whenCpmPlcAlarm;
            actionExecDict[AlarmActions.COM_485_SINGLE_ERROR] = whenCom485SingleError;
            actionExecDict[DMesActions.COMPLETED_SCH_AXIS] = doCompleteSchAxis;
            actionExecDict[DMesActions.CLEAR_SCH_TASKS] = clearSchTasks;

            App.Store.Subscribe(actionExecDict);

            //绑定全局的值
            SchTaskDoingDict = App.Store.GetState().DMesState.SchTaskDoingDict;
            MqSchTasksDict = App.Store.GetState().DMesState.MqSchTasksDict;
            MqScanMaterialDict = App.Store.GetState().DMesState.MqScanMaterialDict;
            MqEmpRfidDict = App.Store.GetState().DMesState.MqEmpRfidDict;
            foreach (var pair in MachineConfig.MachineDict) {
                SchTaskDoingLocks[pair.Key] = new object();
            }
            //恢复任务
            if (!CmdOptions.GlobalOptions.MockVal) {
                restoreTask();
            }
        }

        /// <summary>
        /// 清空机台任务
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void clearSchTasks(AppState state, IAction action) {
            var dmesAction = (DMesActions.ClearSchTasks)action;
            Application.Current.Dispatcher.Invoke(() => {
                foreach (var machineCode in dmesAction.Machines) {
                    lock (SchTaskDoingLocks[machineCode]) {
                        foreach (var task in state.DMesState.MqSchTasksDict[machineCode]) {
                            task?.axisParam?.Clear();
                            task?.bom?.Clear();
                        }
                        state.DMesState.MqSchTasksDict[machineCode]?.Clear();
                        state.DMesState.SchTaskDoingDict[machineCode]?.Clear();
                        state.MqState.MqSchTaskAccpetDict[machineCode] = null;
                        state.ViewStoreState.DMewCoreViewDict[machineCode].TaskSelectedWorkCode = null;
                        SqliteHelper.DoAsync(ctx => {
                            ctx.RemovePersist("task_" + machineCode);
                        });
                    }
                }
            });
        }

        /// <summary>
        /// 完成某个排产轴任务
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void doCompleteSchAxis(AppState state, IAction action) {
            var dmesAction = (DMesActions.CompletedSchAxis)action;
            //完成一轴任务
            completeOneAxis(dmesAction.MachineCode, dmesAction.AxisCode);
            //自动开始下一轴任务
            lock (SchTaskDoingLocks[dmesAction.MachineCode]) {
                var nextAxisCode = SchTaskDoingDict[dmesAction.MachineCode]?.MqSchTask?.axisParam
                                  ?.FirstOrDefault(s => s.IsCompleted == false)?.axiscode;
                Logger.Info("自动开始下一轴号：" + nextAxisCode);
                if (!string.IsNullOrEmpty(nextAxisCode)) {
                    DMesActions.StartSchTaskAxis startAxis = new DMesActions.StartSchTaskAxis(dmesAction.MachineCode, nextAxisCode);
                    doStartSchTaskAxis(state, startAxis, true);
                }
            }

        }

        /// <summary>
        /// 485通讯发生故障
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenCom485SingleError(AppState state, IAction action) {
            var alarmAction = (AlarmActions.Com485SingleError)action;
            var mqAlarm = createMqAlarmAnyway(alarmAction.MachineCode, alarmAction.CpmCode, alarmAction.CpmName,
                $"ip {alarmAction.Ip} 485故障");
            //1小时记录一次485故障到文件
            Logger.Error($"ip {alarmAction.Ip}，参数：{alarmAction.CpmName} 485故障", 3600);
            //485故障暂时不考虑上报
            //App.Store.Dispatch(new AlarmActions.GenerateOneAlarm(alarmAction.MachineCode, mqAlarm));
        }

        /// <summary>
        /// 创建报警对象，无论是否有任务进行
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        MqAlarm createMqAlarmAnyway(string machineCode, int code, string name, string message) {
            var meter = App.Store.GetState().CpmState.NoteMeterDict[machineCode];
            lock (SchTaskDoingLocks[machineCode]) {
                var mqAlarm = new MqAlarm() {
                    machineCode = machineCode,
                    alarmType = AlarmType.CpmErr,
                    axisCode = SchTaskDoingDict[machineCode].MqSchAxis?.axiscode ?? "/",
                    code = code,
                    CpmName = name,
                    message = message,
                    employees = SchTaskDoingDict[machineCode]?.EmpRfids,
                    endRfids = SchTaskDoingDict[machineCode]?.EndAxisRfids,
                    startRfids = SchTaskDoingDict[machineCode]?.StartAxisRfids,
                    meter = meter,
                    time = YUtil.GetUtcTimestampMs(DateTime.Now),
                };
                return mqAlarm;
            }
        }

        /// <summary>
        /// 采集参数超过Plc设定的最值
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenCpmPlcAlarm(AppState state, IAction action) {
            var alarmAction = (AlarmActions.CpmPlcAlarmOccur)action;
            var meter = App.Store.GetState().CpmState.NoteMeterDict[alarmAction.MachineCode];
            //记米小于等于0则不产生报警
            if (meter <= 0) {
                return;
            }
            MqAlarm mqAlarm = createMqAlarmAnyway(alarmAction.MachineCode, alarmAction.CpmCode, alarmAction.CpmName, alarmAction.Message);
            //Cpm 参数报警 10 秒触发一次
            App.Store.Dispatch(new AlarmActions.GenerateOneAlarm(alarmAction.MachineCode, mqAlarm, 10));
        }

        /// <summary>
        /// 监听到扫描来料信息
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenScanMaterialAccept(AppState state, IAction action) {
            var mqAction = (MqActions.ScanMaterialAccpet)action;
            MqScanMaterialDict[mqAction.MachineCode] = mqAction.ScanMaterial;
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                Title = $"机台{mqAction.MachineCode}通知",
                Content = $"接受到来料数据，请注意核实"
            }));

        }

        /// <summary>
        /// 计算平均速度
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenSpeedAccept(AppState state, IAction action) {
            var speedAction = (CpmActions.StateSpeedAccept)action;
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
        /// 当速度为0
        /// 任务完成率大于 0.98 的时候则认为一轴的任务完成
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenSpeedZeroAccept(AppState state, IAction action) {
            //var speedAction = (CpmActions.StateSpeedZeroAccept)action;
            //var machineCode = speedAction.MachineCode;
            ////速度为0的时候检查当前任务可否完成
            //checkAxisCanComplete(machineCode);
        }

        /// <summary>
        /// 检查当前任务可否完成，还是调试完毕
        /// </summary>
        /// <param name="machineCode"></param>
        private bool isCompleteOrDebugEnd(string machineCode) {
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                if (!taskDoing.IsStarted) {
                    return false;
                }
                // 完成率满足一定条件才行
                if (taskDoing.AxisCompleteRate >= 0.90) {
                    return true;
                }
                return false;
            }
        }


        /// <summary>
        /// 处理接收到的Rfid数据
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void doRfidAccept(AppState state, IAction action) {
            var dmesAction = (DMesActions.RfidAccpet)action;
            if (dmesAction.RfidType == DMesActions.RfidType.EmpStartMachine || dmesAction.RfidType == DMesActions.RfidType.EmpEndMachine) {
                var mqEmpRfid = (MqEmpRfid)dmesAction.MqData;
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "消息通知",
                    Content = $" {mqEmpRfid.name} 打{mqEmpRfid.type}卡成功, {dmesAction.MachineCode} 机台",
                    MinGapSec = 10
                }));

            } else if (dmesAction.RfidWhere == DMesActions.RfidWhere.FromMq) {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "消息通知",
                    Content = $"手持机扫卡成功",
                    MinGapSec = 10
                }));
            }

            lock (SchTaskDoingLocks[dmesAction.MachineCode]) {
                var doingTask = SchTaskDoingDict[dmesAction.MachineCode];

                //放线卡
                if (dmesAction.RfidType == DMesActions.RfidType.StartAxis) {
                    doingTask.StartAxisRfids.Add(dmesAction.Rfid);
                    //非采集参数，也更新其Rfid数据
                    if (dmesAction.RfidWhere != DMesActions.RfidWhere.FromCpm) {
                        if (state.CpmState.OnlineCpmsDict[dmesAction.MachineCode].TryGetValue(DefinedParamCode.StartAxisRfid, out var startRfidCpm)) {
                            startRfidCpm.Value = dmesAction.Rfid;
                            startRfidCpm.ValueType = SmParamType.StrRfid;
                        }
                    }
                    //收线卡
                } else if (dmesAction.RfidType == DMesActions.RfidType.EndAxis) {
                    doingTask.EndAxisRfids.Add(dmesAction.Rfid);
                    //更新界面显示
                    if (dmesAction.RfidWhere != DMesActions.RfidWhere.FromCpm) {
                        if (state.CpmState.OnlineCpmsDict[dmesAction.MachineCode]
                            .TryGetValue(DefinedParamCode.EndAxisRfid, out var endRfidCpm)) {
                            endRfidCpm.Value = dmesAction.Rfid;
                            endRfidCpm.ValueType = SmParamType.StrRfid;
                        }
                    }

                    //人员上机卡
                } else if (dmesAction.RfidType == DMesActions.RfidType.EmpStartMachine) {
                    var mqEmpRfid = (MqEmpRfid)dmesAction.MqData;
                    doingTask.EmpRfids.Add(dmesAction.Rfid);
                    //全局保存打卡信息
                    var isPrinted = MqEmpRfidDict[dmesAction.MachineCode].Exists(s => s.employeeCode == mqEmpRfid.name);

                    //如果没有打上机卡，则添加到全局保存
                    if (!isPrinted) {
                        MqEmpRfidDict[dmesAction.MachineCode].Add(mqEmpRfid);
                    }

                    //人员下机卡
                } else if (dmesAction.RfidType == DMesActions.RfidType.EmpEndMachine) {
                    doingTask.EmpRfids.Remove(dmesAction.Rfid);
                    var mqEmpRfid = (MqEmpRfid)dmesAction.MqData;
                    var removeItem = MqEmpRfidDict[dmesAction.MachineCode]
                        .FirstOrDefault(s => s.employeeCode == mqEmpRfid.employeeCode);
                    //从全局打卡信息中移除
                    MqEmpRfidDict[dmesAction.MachineCode].Remove(removeItem);
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
            MqSchTask removeTask = null;
            lock (SchTaskDoingLocks[machineCode]) {
                foreach (var cacheTask in mqTasks) {
                    if (cacheTask.workcode == task.workcode) {
                        //工单已经开始了不能冲掉
                        if (SchTaskDoingDict[machineCode].WorkCode == task.workcode) {
                            Logger.Error($"工单已经开始，无法替换,{cacheTask.workcode}");
                            return;
                        }
                        App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                            Title = "通知",
                            Content = $"即将更新工单任务,工单 {task.workcode}"
                        }));
                        //要被冲掉的工单
                        removeTask = cacheTask;
                        break;
                    }
                }
                //将任务添加到任务队列里面
                //fix: 2018-01-04
                // mqTasks被view引用了，所以用Ui线程来更新
                Application.Current.Dispatcher.Invoke(() => {
                    mqTasks.Remove(removeTask);
                    mqTasks.Add(task);
                });
            }
            //更新任务缓存
            SqliteHelper.DoAsync(ctx => {
                ctx.SavePersist(new Persist(@"task_" + machineCode, JsonConvert.SerializeObject(mqTasks)));
            });
        }

        /// <summary>
        /// 从sqlite中恢复任务
        /// </summary>
        void restoreTask() {
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                foreach (var pair in MachineConfig.MachineDict) {
                    var key = "task_" + pair.Key;
                    var tasks = ctx.Restore<ObservableCollection<MqSchTask>>(key);
                    if (tasks != null) {
                        //过滤掉重复、已完成、过期工单等等
                        HashSet<string> allCodes = new HashSet<string>();
                        HashSet<MqSchTask> delTasks = new HashSet<MqSchTask>();
                        foreach (var task in tasks) {
                            //重复工单
                            if (!allCodes.Add(task.workcode)) {
                                delTasks.Add(task);
                            }
                            //过期工单
                            try {
                                var dayDiff = (DateTime.Now - YUtil.UtcTimestampToLocalTime(task.pstime.time)).TotalDays;
                                if (dayDiff > HmiConfig.TaskPersistMaxDays) {
                                    delTasks.Add(task);
                                }
                            } catch (Exception e) {
                                Logger.Error("检查过期任务失败", e);
                            }
                            //已经完成工单
                            if (task.CompletedRate >= 1) {
                                delTasks.Add(task);
                            }
                        }
                        foreach (var delTask in delTasks) {
                            tasks.Remove(delTask);
                        }
                        //更新缓存
                        if (delTasks.Count > 0) {
                            ctx.SavePersist(new Persist(key, JsonConvert.SerializeObject(tasks)));
                        }
                        MqSchTasksDict[pair.Key] = tasks;
                    }
                }
            }
        }

        /// <summary>
        /// 检查火花报警
        /// </summary>
        void whenSparkDiffAccept(AppState state, IAction action) {
            var machineCode = state.CpmState.MachineCode;
            var spark = state.CpmState.SparkDiffDict[machineCode];
            if ((int)spark == 1) {
                var mqAlarm = createMqAlarmMustInTask(machineCode, YUtil.GetUtcTimestampMs(DateTime.Now), "火花报警", AlarmType.SparkErr);
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
        /// 必须要有任务进行才能创建
        /// </summary>
        /// <param name="machineCode">机台编码</param>
        /// <param name="time">报警时间戳</param>
        /// <param name="alarmType">报警类型</param>
        /// <param name="cpmName">报警参数名</param>
        /// <returns></returns>
        private MqAlarm createMqAlarmMustInTask(string machineCode, long time, string cpmName, string alarmType) {
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
                    bom.TryGetValue(checkAlarm.MaxBomKey ?? "__default", out var maxObj);
                    bom.TryGetValue(checkAlarm.MinBomKey ?? "__default", out var minObj);
                    bom.TryGetValue(checkAlarm.StdBomKey ?? "__default", out var stdObj);

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
                        MqAlarm mqAlarm = createMqAlarmMustInTask(machineCode, checkAlarm.Cpm.PickTimeStampMs,
                            checkAlarm.Cpm.Name, AlarmType.CpmErr);
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
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                if (SchTaskDoingDict[machineCode].IsStarted) {
                    //记米数减小了，则去检查任务是否达完成条件
                    if (noteMeter < taskDoing.MeterWork && noteMeter < 5) {
                        if (isCompleteOrDebugEnd(machineCode)) {
                            completeOneAxis(machineCode, taskDoing?.MqSchAxis?.axiscode);
                        } else {
                            debugOneAxisEnd(machineCode, taskDoing?.MqSchAxis?.axiscode);
                        }
                        //任务已经完成
                    } else {
                        taskDoing.MeterWork = noteMeter;
                        var rate = noteMeter / taskDoing.MeterPlan;
                        taskDoing.AxisCompleteRate = rate;
                    }
                }
            }
        }

        /// <summary>
        /// 重载下周
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void doStartSchTaskAxis(AppState state, IAction action) {
            doStartSchTaskAxis(state, action, false);
        }

        /// <summary>
        /// 根据轴号设置当前任务开始
        /// </summary>
        void doStartSchTaskAxis(AppState state, IAction action, bool isAutoStart) {
            var startParam = (DMesActions.StartSchTaskAxis)action;
            var machineCode = startParam.MachineCode;
            //准备工作
            if (!validTaskPrepare(machineCode)) {
                //
            }
            var axisCode = startParam.AxisCode;
            //搜索任务
            lock (SchTaskDoingLocks[machineCode]) {
                bool hasFound = false;
                var mqTasks = MqSchTasksDict[machineCode];
                foreach (var mqTask in mqTasks) {
                    for (var i = 0; i < mqTask.axisParam.Count; i++) {
                        var axis = mqTask.axisParam[i];
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
                            //var noteMeter = App.Store.GetState().CpmState.NoteMeterDict[machineCode];
                            //if (noteMeter != 0) {
                            //    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                            //        Title = $"请先清零记米，再开始任务",
                            //        Content = $"机台 {machineCode} 记米没有清零，请先清零"
                            //    }));
                            //    return;
                            //}
                            setSchTaskDoing(taskDoing, mqTask, axis, i);
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
                    setOtherTaskAxisCanStart(machineCode, axisCode, false);
                    App.Store.Dispatch(new DMesActions.StartAxisSuccess(machineCode, axisCode));
                    var title = "启动任务成功";
                    if (isAutoStart) {
                        title = "自动 " + title;
                    } else {
                        title = "手动 " + title;
                    }
                    //通知服务器任务开始了
                    notifyServerAxisStarted(machineCode,axisCode);
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = title,
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
        /// 通知服务器任务开始了
        /// </summary>
        /// <param name="machineCode"></param>
        void notifyServerAxisStarted(string machineCode, string axisCode) {
            MqUploadManu uManu = null;
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                uManu = new MqUploadManu() {
                    actualBeginTime = YUtil.GetUtcTimestampMs(taskDoing.StartTime),
                    actualEndTime = YUtil.GetUtcTimestampMs(taskDoing.EndTime),
                    axisName = axisCode,
                    macCode = machineCode,
                    axixLen = taskDoing.MeterPlan,
                    courseCode = taskDoing.WorkCode,
                    empRfid = string.Join(",", taskDoing.EmpRfids),
                    rfids_begin = string.Join(",", taskDoing.StartAxisRfids),
                    rfid_end = string.Join(",", taskDoing.EndAxisRfids),
                    acutalDispatchTime = YUtil.GetUtcTimestampMs(taskDoing.StartTime),
                    mqType = "no",
                    step = taskDoing.Step,
                    testLen = taskDoing.MeterDebug,
                    testTime = taskDoing.DebugTimestampMs,
                    speed = taskDoing.SpeedAvg,
                    seqCode = taskDoing.MqSchAxis.seqcode,
                    status = "开始生产",
                };
            }
            mqEffects.UploadSchTaskManu(new MqActions.UploadSchTaskManu(HmiConfig.QueWebSrvPropSave, uManu));
        }


        /// <summary>
        /// 检查任务准备工作是否做好
        /// </summary>
        bool validTaskPrepare(string machineCode) {
            bool isValid = true;
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                if (taskDoing.StartAxisRfids?.Count < 1) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "警告",
                        Content = $"请扫描 {machineCode} 的放线盘"
                    }));
                    isValid = false;
                }
                if (taskDoing.EndAxisRfids?.Count < 1) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "警告",
                        Content = $"请扫描 {machineCode} 的收线盘"
                    }));
                    isValid = false;
                }
                if (taskDoing.EmpRfids?.Count < 1) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "警告",
                        Content = $"请打 {machineCode} 的上机卡"
                    }));
                    isValid = false;
                }
            }
            //校验不通过打开报警灯
            if (!isValid) {
                //打开显示器
                App.Store.Dispatch(new SysActions.OpenScreen());
                //打开报警灯
                App.Store.Dispatch(new AlarmActions.OpenAlarmLights(machineCode, 5000));
            }
            return isValid;
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
        void setOtherTaskAxisCanStart(string machineCode, string startAxisCode, bool canStart) {
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
        void debugOneAxisEnd(string machineCode, string axisCode) {
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
        async void completeOneAxis(string machineCode, string axisCode) {
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
                //标志位改变
                taskDoing.MqSchAxis.State = MqSchTaskAxisState.Completed;
                taskDoing.MqSchAxis.IsCompleted = true;
                taskDoing.MqSchAxis.CanStart = false;
                taskDoing.EndTime = DateTime.Now;
                setOtherTaskAxisCanStart(machineCode, axisCode, true);
                //更新当前任务完成进度
                var completedAxis = taskDoing.MqSchTask.axisParam.Count(a => a.IsCompleted == true);
                taskDoing.MqSchTask.CompletedRate = (float)completedAxis / taskDoing.MqSchTask.axisParam.Count;
                //一个工单任务完成
                if (taskDoing.MqSchTask.CompletedRate >= 1) {
                    completeOneSchTask(machineCode, taskDoing.WorkCode);
                }
                uManu = new MqUploadManu() {
                    actualBeginTime = YUtil.GetUtcTimestampMs(taskDoing.StartTime),
                    actualEndTime = YUtil.GetUtcTimestampMs(taskDoing.EndTime),
                    axisName = axisCode,
                    macCode = machineCode,
                    axixLen = taskDoing.MeterPlan,
                    courseCode = taskDoing.WorkCode,
                    empRfid = string.Join(",", taskDoing.EmpRfids),
                    rfids_begin = string.Join(",", taskDoing.StartAxisRfids),
                    rfid_end = string.Join(",", taskDoing.EndAxisRfids),
                    acutalDispatchTime = YUtil.GetUtcTimestampMs(taskDoing.StartTime),
                    mqType = "yes",
                    step = taskDoing.Step,
                    testLen = taskDoing.MeterDebug,
                    testTime = taskDoing.DebugTimestampMs,
                    speed = taskDoing.SpeedAvg,
                    seqCode = taskDoing.MqSchAxis.seqcode,
                    status = "正常结束",
                };

                //重新初始化
                taskDoing.Init();
            }

            //更新缓存
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                //移除已经完成的任务轴
                //taskDoing.MqSchTask.axisParam.Remove(taskDoing.MqSchAxis);
                ctx.SavePersist(new Persist("task_" + machineCode, JsonConvert.SerializeObject(MqSchTasksDict[machineCode])));
            }
            var uploadResult = await App.Store.Dispatch(mqEffects.UploadSchTaskManu(new MqActions.UploadSchTaskManu(HmiConfig.QueWebSrvPropSave, uManu)));
            if (uploadResult) {
                //显示完成消息
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = $"机台 {machineCode} 达成任务",
                    Content = $"轴号 {axisCode} 任务达成"
                }));
                Logger.Info($"回传机台{machineCode},{axisCode}排产数据成功");
                Logger.Info(JsonConvert.SerializeObject(uManu));
                //上传落轴数据失败，对其进行缓存
            } else {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = $"机台 {machineCode} 上传任务达成进度失败",
                    Content = $"轴号 {axisCode} 任务 上传服务器失败，请检查网络连接"
                }));
                using (var ctx = SqliteHelper.CreateSqliteService()) {
                    ctx.UploadManuFailures.Add(uManu);
                }
                Logger.Error($"回传机台{machineCode},{axisCode}排产数据失败，已缓存");
            }
        }

        /// <summary>
        /// 完成某个工单
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="workCode"></param>
        void completeOneSchTask(string machineCode, string workCode) {
            var mqTasks = MqSchTasksDict[machineCode];
            var removeTask = mqTasks.FirstOrDefault(t => t.workcode == workCode);
            //移除已经完成的某个工单任务
            //fixed: 2018-01-14
            // mqTasks 是界面数据，所以要用 Dispatcher
            Application.Current.Dispatcher.Invoke(() => {
                //先清空轴任务数据
                removeTask.axisParam?.Clear();
                removeTask.bom?.Clear();
                //删除工单任务
                mqTasks.Remove(removeTask);
                //更新缓存
                SqliteHelper.DoAsync(ctx => {
                    ctx.SavePersist(new Persist($"task_{machineCode}", JsonConvert.SerializeObject(mqTasks)));
                });
            });

        }
    }
}
