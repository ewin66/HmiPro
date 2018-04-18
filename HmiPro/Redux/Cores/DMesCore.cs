using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.Native;
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
using HmiPro.ViewModels;
using HmiPro.ViewModels.DMes.Form;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
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
        /// <summary>
        /// 异步操作数据库（MongoDb,InfluxDb)
        /// </summary>
        private readonly DbEffects dbEffects;
        /// <summary>
        /// 异步操作 Mq
        /// </summary>
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
        public LoggerService Logger;
        /// <summary>
        /// 命令派发执行的动作
        /// </summary>
        readonly IDictionary<string, Action<AppState, IAction>> actionExecutors = new Dictionary<string, Action<AppState, IAction>>();
        /// <summary>
        /// 任务锁
        /// </summary>
        public static readonly IDictionary<string, object> SchTaskDoingLocks = new Dictionary<string, object>();
        /// <summary>
        /// 栈板数据保存
        /// </summary>
        public IDictionary<string, Pallet> PalletDict;

        /// <summary>
        /// 注入必需的服务
        /// </summary>
        /// <param name="dbEffects"></param>
        /// <param name="mqEffects"></param>
        public DMesCore(DbEffects dbEffects, MqEffects mqEffects) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.dbEffects = dbEffects;
            this.mqEffects = mqEffects;
        }

        /// <summary>
        /// 配置文件加载之后才能对其初始化
        /// </summary>
        public void Init() {
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            actionExecutors[CpmActions.CPMS_UPDATED_ALL] = whenCpmsUpdateAll;
            actionExecutors[MqActions.SCH_TASK_ACCEPT] = whenSchTaskAccept;
            actionExecutors[CpmActions.NOTE_METER_ACCEPT] = whenNoteMeterAccept;
            actionExecutors[AlarmActions.CHECK_CPM_BOM_ALARM] = doCheckCpmBomAlarm;
            actionExecutors[CpmActions.SPARK_DIFF_ACCEPT] = whenSparkDiffAccept;
            actionExecutors[DMesActions.START_SCH_TASK_AXIS] = doStartSchTaskAxis;
            actionExecutors[CpmActions.STATE_SPEED_ZERRO_ACCEPT] = whenSpeedZeroAccept;
            actionExecutors[CpmActions.STATE_SPEED_ACCEPT] = whenSpeedAccept;
            actionExecutors[DMesActions.RFID_ACCPET] = doRfidAccept;
            actionExecutors[MqActions.SCAN_MATERIAL_ACCEPT] = whenScanMaterialAccept;
            actionExecutors[AlarmActions.CPM_PLC_ALARM_OCCUR] = whenCpmPlcAlarm;
            actionExecutors[AlarmActions.COM_485_SINGLE_ERROR] = whenCom485SingleError;
            actionExecutors[DMesActions.COMPLETED_SCH_AXIS] = doCompleteSchAxis;
            actionExecutors[DMesActions.CLEAR_SCH_TASKS] = doClearSchTasks;
            actionExecutors[SysActions.FORM_VIEW_PRESSED_OK] = doFormViewPressedOk;
            actionExecutors[MqActions.CMD_ACCEPT] = whenCmdAccept;
            actionExecutors[DMesActions.DEL_TASK] = doDelTask;
            actionExecutors[CpmActions.OD_ACCPET] = whenOdAccept;

            App.Store.Subscribe(actionExecutors);

            //绑定全局的值
            SchTaskDoingDict = App.Store.GetState().DMesState.SchTaskDoingDict;
            MqSchTasksDict = App.Store.GetState().DMesState.MqSchTasksDict;
            MqScanMaterialDict = App.Store.GetState().DMesState.MqScanMaterialDict;
            MqEmpRfidDict = App.Store.GetState().DMesState.MqEmpRfidDict;
            PalletDict = App.Store.GetState().DMesState.PalletDict;

            foreach (var pair in MachineConfig.MachineDict) {
                SchTaskDoingLocks[pair.Key] = new object();
            }
            //恢复任务
            if (!CmdOptions.GlobalOptions.MockVal) {
                restoreTask();
            }
        }

        /// <summary>
        /// 计算每个任务的平均 Od 值
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenOdAccept(AppState state, IAction action) {
            var odAction = (CpmActions.OdAccept)action;
            if (odAction.Od == 0) {
                return;
            }
            lock (SchTaskDoingLocks[odAction.MachineCode]) {
                var taskDoing = SchTaskDoingDict[odAction.MachineCode];
                if (taskDoing.IsStarted && taskDoing.CalcAvgSpeed != null) {
                    taskDoing.OdAvg = (float)taskDoing.CalcOdAvg(odAction.Od);
                    taskDoing.OdMax = Math.Max(odAction.Od, taskDoing.OdMax);
                    taskDoing.OdMin = Math.Min(odAction.Od, taskDoing.OdMin);
                }
            }
        }

        /// <summary>
        /// 删除某个大任务
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">待删除任务的属性，含任务 Id</param>
        void doDelTask(AppState state, IAction action) {
            var delTaskAction = (DMesActions.DelTask)action;
            var tasks = MqSchTasksDict[delTaskAction.MachineCode];
            MqSchTask delTask = null;
            lock (SchTaskDoingLocks[delTaskAction.MachineCode]) {
                //任务已经开始了，不能被删除
                if (SchTaskDoingDict[delTaskAction.MachineCode].IsStarted && SchTaskDoingDict[delTaskAction.MachineCode].TaskId == delTaskAction.TaskId) {
                    Logger.Warn($"任务 {delTaskAction.TaskId} 已经开始了，不能被删除");
                    return;
                }
                delTask = tasks.FirstOrDefault(t => t.taskId == delTaskAction.TaskId);
            }
            Application.Current.Dispatcher.Invoke(() => {
                if (tasks.Remove(delTask)) {
                    //这里不用异步会死锁  2018-1-30
                    Task.Run(() => {
                        Logger.Info($"任务 {delTaskAction.TaskId} 被指令删除");
                        App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                            Title = "通知",
                            Content = $"任务 {delTaskAction.TaskId} 已经被服务器删除"
                        }));
                        //更新数据库
                        SqliteHelper.DoAsync(ctx => {
                            ctx.SavePersist(new Persist("task_" + delTaskAction.MachineCode, JsonConvert.SerializeObject(tasks)));
                        });
                    });

                }
            });
        }

        /// <summary>
        /// 处理机台的接受到的指令
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">指令内容，目前指令只从 Mq 发出来</param>
        void whenCmdAccept(AppState state, IAction action) {
            var cmdAction = (MqActions.CmdAccept)action;
            var mqCmd = cmdAction.AppCmd;
            //里面 Action 指的是 MqCmdActions
            if (mqCmd.execWhere == AppActionsWhere.MqActions) {
                if (mqCmd.action == MqCmdActions.DEL_WORK_TASK) {
                    App.Store.Dispatch(new DMesActions.DelTask(cmdAction.MachineCode,
                        mqCmd.args?.ToString()));
                }
                //直接转发该条命令到 Redux
            } else if (cmdAction.AppCmd.execWhere == AppActionsWhere.ReduxActions) {
                //这个厉害了，因为很多地方用到了强转，所以反序列的时候需要知道数据的类型
                var type = YUtil.GetTypes(mqCmd.type, Assembly.GetExecutingAssembly());
                var json = JsonConvert.SerializeObject(mqCmd.args);
                dynamic data = JsonConvert.DeserializeObject(json, type[0]);
                App.Store.Dispatch(data);
            }
        }

        /// <summary>
        /// 栈板相关属性确认（只有 R 系列机台会用到）
        /// 现在已经不这么使用了，不用全局监听 PressedOk 可以在创建 FormView 的时候直接 Bind PressedOk 逻辑
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">FormView 按下了「确认」，此时 Form 里面的数据</param>
        void doFormViewPressedOk(AppState state, IAction action) {
            var sysAction = (SysActions.FormViewPressedOk)action;
            if (sysAction.Form != null && sysAction.Form is PalletConfirmForm form) {
                confirmPalletAxisNum(form);
            }
        }

        /// <summary>
        /// 确认栈板Rfid与轴数目的关系
        /// </summary>
        /// <param name="form">栈板相关数据</param>
        async void confirmPalletAxisNum(PalletConfirmForm form) {
            var machinieCode = form.MachineCode;
            if (!PalletDict.ContainsKey(machinieCode)) {
                return;
            }
            var pallet = new Pallet();
            pallet.AxisNum = form.AxisNum;
            pallet.Rfids = form.Rfid;
            pallet.WorkCode = form.WorkCode;
            //清空栈板轴数量
            PalletDict[machinieCode].AxisNum = 0;

            //保存栈板Rfid 和 轴数量之间的关系
            var filter = new FilterDefinitionBuilder<Pallet>().Where(s => s.Rfids == pallet.Rfids);
            var options = new UpdateOptions() { IsUpsert = true };
            var updater = Builders<Pallet>.Update.Set(s => s.AxisNum, pallet.AxisNum);
            MongoHelper.GetMongoService().GetDatabase(machinieCode).GetCollection<Pallet>("Pallets").UpdateOneAsync(filter, updater, options);

            var mqCall = new MqCall() {
                machineCode = machinieCode,
                callType = MqCallType.Forklift,
                callAction = MqCallAction.MovePallet,
                CallId = Guid.NewGuid().GetHashCode(),
                callArgs = pallet
            };
            Logger.Info("栈板绑定数据: " + JsonConvert.SerializeObject(mqCall));
            var callSuccess = await App.Store.Dispatch(mqEffects.CallSystem(new MqActions.CallSystem(machinieCode, mqCall)));
            if (callSuccess) {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "通知",
                    Content = "呼叫成功"
                }));
            } else {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "警告",
                    Content = "呼叫叉车失败"
                }));
            }
        }

        /// <summary>
        /// 清空机台所有任务
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">清空任务指令</param>
        void doClearSchTasks(AppState state, IAction action) {
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
        /// <param name="state">程序状态</param>
        /// <param name="action">完成轴号</param>
        void doCompleteSchAxis(AppState state, IAction action) {
            var dmesAction = (DMesActions.CompletedSchAxis)action;
            //完成一轴任务
            completeOneAxis(dmesAction.MachineCode, dmesAction.AxisCode, dmesAction.Status);
            //自动开始下一轴任务
            lock (SchTaskDoingLocks[dmesAction.MachineCode]) {
                var nextAxis = SchTaskDoingDict[dmesAction.MachineCode]?.MqSchTask?.axisParam?.FirstOrDefault(s => s.IsCompleted == false);
                Logger.Info("自动开始下一轴号：" + nextAxis);
                if (nextAxis != null) {
                    DMesActions.StartSchTaskAxis startAxis = new DMesActions.StartSchTaskAxis(dmesAction.MachineCode, nextAxis.axiscode, nextAxis.taskId);
                    doStartSchTaskAxis(state, startAxis, true);
                }
            }

        }

        /// <summary>
        /// 485通讯发生故障
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">485异常数据</param>
        void whenCom485SingleError(AppState state, IAction action) {
            if (HmiConfig.IsUpload485Err) {
                var alarmAction = (AlarmActions.Com485SingleError)action;
                var mqAlarm = createMqAlarmAnyway(alarmAction.MachineCode, alarmAction.CpmCode, alarmAction.CpmName,
                    $"ip {alarmAction.Ip} 485故障");
                //1小时记录一次485故障到文件
                Logger.Error($"ip {alarmAction.Ip}，参数：{alarmAction.CpmName} 485故障", 36000);
                //485故障暂时不考虑上报
                App.Store.Dispatch(new AlarmActions.GenerateOneAlarm(alarmAction.MachineCode, mqAlarm));
            }
        }

        /// <summary>
        /// 创建报警对象，无论是否有任务进行
        /// </summary>
        /// <param name="machineCode">报警机台</param>
        /// <param name="code">报警编码（一般为参数编码）</param>
        /// <param name="name">报警名</param>
        /// <param name="message">报警内容</param>
        /// <returns>标准的报警对象</returns>
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
                    workCode = SchTaskDoingDict[machineCode]?.WorkCode,
                    meter = meter,
                    time = YUtil.GetUtcTimestampMs(DateTime.Now),
                };
                return mqAlarm;
            }
        }

        /// <summary>
        /// 采集参数超过Plc设定的最值
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">根据读取 Plc 数据直接产生的报警</param>
        void whenCpmPlcAlarm(AppState state, IAction action) {
            var alarmAction = (AlarmActions.CpmPlcAlarmOccur)action;
            var meter = state.CpmState.NoteMeterDict[alarmAction.MachineCode];
            //记米小于等于0则不产生报警
            if (meter <= 0) {
                return;
            }
            MqAlarm mqAlarm = createMqAlarmAnyway(alarmAction.MachineCode, alarmAction.CpmCode, alarmAction.CpmName, alarmAction.Message);
            //Cpm 参数报警 5 分钟触发一次
            App.Store.Dispatch(new AlarmActions.GenerateOneAlarm(alarmAction.MachineCode, mqAlarm, 300));
        }

        /// <summary>
        /// 监听到扫描来料信息
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">来料数据</param>
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
        /// <param name="state">程序状态</param>
        /// <param name="action">接受到的速度值</param>
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
        /// <param name="state">程序状态</param>
        /// <param name="action">机台速度为0</param>
        void whenSpeedZeroAccept(AppState state, IAction action) {
            //var speedAction = (CpmActions.StateSpeedZeroAccept)action;
            //var machineCode = speedAction.MachineCode;
            ////速度为0的时候检查当前任务可否完成
            //checkAxisCanComplete(machineCode);
        }

        /// <summary>
        /// 检查当前任务可否完成，还是调试完毕
        /// </summary>
        /// <param name="machineCode">机台编码</param>
        private bool canAutoCompleteAxis(string machineCode) {
            Logger.Warn("尝试去检查任务能否自动完成...");
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                if (!taskDoing.IsStarted) {
                    return false;
                }
                // 完成率满足一定条件才行
                if (taskDoing.AxisCompleteRate >= 0.90) {
                    Logger.Warn("自动检查结果：没有完成。当前完成率: " + taskDoing.AxisCompleteRate);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 处理接收到的Rfid数据
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">Rfid 数据</param>
        void doRfidAccept(AppState state, IAction action) {
            try {
                var rfidAccpet = (DMesActions.RfidAccpet)action;
                if (rfidAccpet.RfidType == DMesActions.RfidType.EmpStartMachine ||
                    rfidAccpet.RfidType == DMesActions.RfidType.EmpEndMachine) {
                    var mqEmpRfid = (MqEmpRfid)rfidAccpet.MqData;
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "消息通知",
                        Content = $" {mqEmpRfid.name} 打{mqEmpRfid.type}卡成功, {rfidAccpet.MachineCode} 机台",
                        MinGapSec = 10
                    }));
                    //打了上机卡，清除未打上机卡警告
                    if (mqEmpRfid.type == MqRfidType.EmpStartMachine) {
                        App.Store.Dispatch(
                            new SysActions.DelMarqueeMessage(
                                SysActions.MARQUEE_PUNCH_START_MACHINE + rfidAccpet.MachineCode));
                    }
                } else if (rfidAccpet.RfidWhere == DMesActions.RfidWhere.FromMq) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "消息通知",
                        Content = $"手持机扫卡成功",
                        MinGapSec = 10
                    }));
                }
                lock (SchTaskDoingLocks[rfidAccpet.MachineCode]) {
                    if (!SchTaskDoingDict.ContainsKey(rfidAccpet.MachineCode)) {
                        return;
                    }
                    var doingTask = SchTaskDoingDict[rfidAccpet.MachineCode];
                    //放线卡
                    if (rfidAccpet.RfidType == DMesActions.RfidType.StartAxis) {
                        App.Store.Dispatch(
                            new SysActions.DelMarqueeMessage(
                                SysActions.MARQUEE_SCAN_START_AXIS_RFID + rfidAccpet.MachineCode));
                        //如果是带有栈板的机台，则只有一个放线盘，每当放线盘更改的时候都应该清空放线卡数据
                        if (PalletDict.ContainsKey(rfidAccpet.MachineCode)) {
                            if (!doingTask.StartAxisRfids.Contains(rfidAccpet.Rfids)) {
                                doingTask.StartAxisRfids.Clear();
                            }
                        }
                        rfidAccpet.RfidArr?.ForEach(rfid => doingTask.StartAxisRfids.Add(rfid));
                        //收线卡
                    } else if (rfidAccpet.RfidType == DMesActions.RfidType.EndAxis) {
                        App.Store.Dispatch(
                            new SysActions.DelMarqueeMessage(
                                SysActions.MARQUEE_SCAN_END_AXIS_RFID + rfidAccpet.MachineCode));
                        //收线盘只有一个
                        if (!doingTask.EndAxisRfids.Contains(rfidAccpet.Rfids)) {
                            doingTask.EndAxisRfids.Clear();
                            doingTask.EndAxisRfids.Add(rfidAccpet.Rfids);
                        }
                        if (PalletDict.ContainsKey(rfidAccpet.MachineCode)) {
                            //更新栈板数据
                            PalletDict[rfidAccpet.MachineCode].Rfids = string.Join(",", doingTask.EndAxisRfids);
                        }
                        //人员上机卡
                    } else if (rfidAccpet.RfidType == DMesActions.RfidType.EmpStartMachine) {
                        App.Store.Dispatch(
                            new SysActions.DelMarqueeMessage(
                                SysActions.MARQUEE_PUNCH_START_MACHINE + rfidAccpet.MachineCode));
                        var mqEmpRfid = (MqEmpRfid)rfidAccpet.MqData;
                        doingTask.EmpRfids.Add(rfidAccpet.Rfids);
                        //全局保存打卡信息
                        var isPrinted = MqEmpRfidDict[rfidAccpet.MachineCode]
                            .Exists(s => s.employeeCode == mqEmpRfid.employeeCode);
                        //如果没有打上机卡，则添加到全局保存
                        if (!isPrinted) {
                            MqEmpRfidDict[rfidAccpet.MachineCode].Add(mqEmpRfid);
                        }
                        //人员下机卡
                    } else if (rfidAccpet.RfidType == DMesActions.RfidType.EmpEndMachine) {
                        doingTask.EmpRfids.Remove(rfidAccpet.Rfids);
                        var mqEmpRfid = (MqEmpRfid)rfidAccpet.MqData;
                        var removeItem = MqEmpRfidDict[rfidAccpet.MachineCode]
                            .FirstOrDefault(s => s.employeeCode == mqEmpRfid.employeeCode);
                        //从全局打卡信息中移除
                        MqEmpRfidDict[rfidAccpet.MachineCode].Remove(removeItem);
                    }

                    //更新 参数 界面上面的 Rfid 值
                    var employees = string.Join(",",
                        (from c in MqEmpRfidDict[rfidAccpet.MachineCode] select c.name).ToList());
                    var startRfids = string.Join(",", doingTask.StartAxisRfids);
                    var endRfids = string.Join(",", doingTask.EndAxisRfids);
                    setCpmRfid(rfidAccpet.MachineCode, DefinedParamCode.EmpRfid, employees);
                    setCpmRfid(rfidAccpet.MachineCode, DefinedParamCode.StartAxisRfid, startRfids);
                    setCpmRfid(rfidAccpet.MachineCode, DefinedParamCode.EndAxisRfid, endRfids);
                }
            } catch (Exception e) {
                Logger.Error("Rfid异常：", e, 3600);
            }
        }

        /// <summary>
        /// 参数 界面上面的 Rfid 卡显示值
        /// </summary>
        /// <param name="machineCode">机台编码</param>
        /// <param name="cpmCode">Rfid 的参数编码</param>
        /// <param name="value">Rfid 数据</param>
        void setCpmRfid(string machineCode, int cpmCode, string value) {
            if (string.IsNullOrEmpty(value)) {
                value = "暂无";
            }
            if (App.Store.GetState().CpmState.OnlineCpmsDict[machineCode].TryGetValue(cpmCode, out var cpm)) {
                cpm.Value = value;
                cpm.ValueType = SmParamType.StrRfid;
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
        /// <param name="state">程序状态</param>
        /// <param name="action">任务数据</param>
        /// </summary>
        void whenSchTaskAccept(AppState state, IAction action) {
            var mqAction = (MqActions.SchTaskAccept)action;
            var machineCode = mqAction.MqSchTask.maccode;
            var allTasks = MqSchTasksDict[machineCode];
            var taskAccept = mqAction.MqSchTask;
            MqSchTask taskRemove = null;
            //容易卡死 UI ，这里让它运行在异步里面
            Task.Run(() => {
                lock (SchTaskDoingLocks[machineCode]) {
                    foreach (var task in allTasks) {
                        if (task.taskId == taskAccept.taskId) {
                            //任务已经开始了不能冲掉
                            if (SchTaskDoingDict[machineCode]?.MqSchTask?.taskId == taskAccept.taskId &&
                                SchTaskDoingDict[machineCode].IsStarted) {
                                Logger.Warn($"任务已经开始，无法替换 {task.taskId}", true);
                                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                                    Title = "通知",
                                    Content = $"任务已经开始，无法更换 {taskAccept.taskId}"
                                }));
                                return;
                            }
                            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                                Title = "通知",
                                Content = $"已更新任务 {taskAccept.taskId}"
                            }));
                            //要被冲掉的任务
                            taskRemove = task;
                            break;
                        }
                    }
                    //将任务添加到任务队列里面
                    //fix: 2018-01-04
                    // mqTasks被view引用了，所以用Ui线程来更新
                    Application.Current.Dispatcher.Invoke(() => {
                        allTasks.Remove(taskRemove);
                        allTasks.Add(taskAccept);
                    });
                    //有任务被顶掉了
                    if (taskRemove != null) {
                        App.Store.Dispatch(new MqActions.SchTaskReplaced(machineCode));
                    }
                }
                //更新任务缓存
                SqliteHelper.DoAsync(ctx => {
                    ctx.SavePersist(new Persist(@"task_" + machineCode, JsonConvert.SerializeObject(allTasks)));
                });

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
                        HashSet<string> allTaskIds = new HashSet<string>();
                        HashSet<MqSchTask> delTasks = new HashSet<MqSchTask>();
                        foreach (var task in tasks) {
                            //重复工单
                            if (!allTaskIds.Add(task.taskId)) {
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
        /// <param name="state">程序状态</param>
        /// <param name="action">火花机报警检查动作</param>
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
                    workCode = taskDoing?.WorkCode
                };
                return mqAlarm;
            }
        }


        /// <summary>
        /// 从Bom表中去出上下限，然后判断参数是否异常
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">待检查的 Bom 关键 Key</param>
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
                        var logDetail = $"任务 id={taskDoing.MqSchTaskId} 的Bom表上下限有误  {checkAlarm.MaxBomKey}: {maxObj},{checkAlarm.MinBomKey}:{minObj},{checkAlarm.StdBomKey}: {stdObj}";
                        //10分钟通知一次
                        App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                            Title = $"机台 {machineCode} 报警失败",
                            Content = $"工单 {taskDoing.WorkCode} Bom表上下限有误",
                            MinGapSec = 600,
                            LogDetail = logDetail
                        }));
                        Logger.Error(logDetail, 600);
                        return;
                    }
                }
                //根据标准值求最小值
                if (std.HasValue && max.HasValue) {
                    min = 2 * std - max;
                }
                //报警
                if (max.HasValue && min.HasValue) {
                    //上下限有误
                    if (max.Value <= min.Value) {
                        return;
                    }
                    var cpmVal = (float)checkAlarm.Cpm.Value;
                    if ((cpmVal > max && max > 0) || (cpmVal < min && min > 0)) {
                        MqAlarm mqAlarm = createMqAlarmMustInTask(machineCode, checkAlarm.Cpm.PickTimeStampMs, checkAlarm.Cpm.Name, AlarmType.CpmErr);
                        dispatchAlarmAction(machineCode, mqAlarm);
                    }
                } else {
                    Logger.Error($"未能从任务 Id={taskDoing.MqSchTaskId}的Bom表中求出上下限，Max: {max},Min {min},Std: {std}", 3600);
                }
            }
        }
        /// <summary>
        /// 记米相关处理
        /// <param name="state">程序状态</param>
        /// <param name="action">记米数据</param>
        /// </summary>
        void whenNoteMeterAccept(AppState state, IAction action) {
            var meterAction = (CpmActions.NoteMeterAccept)action;
            var machineCode = meterAction.MachineCode;
            var noteMeter = meterAction.Meter;
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                if (SchTaskDoingDict[machineCode].IsStarted) {
                    //记米数减小了，则去检查任务是否达完成条件
                    if (noteMeter < taskDoing.MeterWork && noteMeter < 30) {
                        if (canAutoCompleteAxis(machineCode)) {
                            App.Store.Dispatch(new DMesActions.CompletedSchAxis(machineCode, taskDoing?.MqSchAxis?.axiscode));
                        } else {
                            debugOneAxisEnd(machineCode, taskDoing?.MqSchAxis?.axiscode);
                            setTaskCompleteRate(taskDoing, noteMeter);
                        }
                        //任务已经完成
                    } else {
                        setTaskCompleteRate(taskDoing, noteMeter);
                    }
                }
            }
        }

        /// <summary>
        /// 设置任务的完成率
        /// </summary>
        /// <param name="taskDoing"></param>
        /// <param name="noteMeter"></param>
        void setTaskCompleteRate(SchTaskDoing taskDoing, float noteMeter) {
            taskDoing.MeterWork = noteMeter;
            var rate = noteMeter / taskDoing.MeterPlan;
            taskDoing.AxisCompleteRate = rate;
        }

        /// <summary>
        /// 手动开始轴任务
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">开始动作</param>
        void doStartSchTaskAxis(AppState state, IAction action) {
            doStartSchTaskAxis(state, action, false);
        }

        /// <summary>
        /// 根据轴号设置当前任务开始
        /// <param name="state">程序状态</param>
        /// <param name="action">开始轴任务的动作</param>
        /// <param name="isAutoStart">是否是自动开始任务</param>
        /// </summary>
        void doStartSchTaskAxis(AppState state, IAction action, bool isAutoStart) {
            var dmesAction = (DMesActions.StartSchTaskAxis)action;
            var machineCode = dmesAction.MachineCode;
            //准备工作
            if (!validTaskPrepare(machineCode)) {
                //
            }
            var axisCode = dmesAction.AxisCode;
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
                    notifyServerAxisStarted(machineCode, axisCode);
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
        /// <param name="machineCode">机台编码</param>
        /// <param name="axisCode">开始的轴号</param>
        /// </summary>
        void notifyServerAxisStarted(string machineCode, string axisCode) {
            MqUploadManu uManu = null;
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                uManu = new MqUploadManu() {
                    actualBeginTime = YUtil.GetUtcTimestampMs(taskDoing.StartTime),
                    actualEndTime = YUtil.GetUtcTimestampMs(taskDoing.EndTime),
                    axisName = axisCode,
                    macCode = machineCode,
                    axixLen = taskDoing.MeterWork,
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
                    avgOd = taskDoing.OdAvg,
                    maxOd = taskDoing.OdMax,
                    minOd = taskDoing.OdMin,
                    seqCode = taskDoing.MqSchAxis.seqcode,
                    status = "开始生产",
                };
            }
            Logger.Info("通知服务器开始任务：" + JsonConvert.SerializeObject(uManu));
            App.Store.Dispatch(mqEffects.UploadSchTaskManu(new MqActions.UploadSchTaskManu(HmiConfig.QueWebSrvPropSave, uManu)));
        }


        /// <summary>
        /// 检查任务准备工作是否做好
        /// <param name="machineCode">机台编码</param>
        /// </summary>
        bool validTaskPrepare(string machineCode) {
            bool isValid = true;
            lock (SchTaskDoingLocks[machineCode]) {
                var taskDoing = SchTaskDoingDict[machineCode];
                if (taskDoing.StartAxisRfids?.Count < 1) {
                    var message = $"请扫描 {machineCode} 的放线盘";
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "警告",
                        Content = message
                    }));
                    isValid = false;
                    App.Store.Dispatch(new SysActions.AddMarqueeMessage(SysActions.MARQUEE_SCAN_START_AXIS_RFID + machineCode, message));
                }
                if (taskDoing.EndAxisRfids?.Count < 1) {
                    var endAxisType = "收线盘";
                    var message = $"请扫描 {machineCode} 的 {endAxisType}";
                    if (GlobalConfig.PalletMachineCodes.Contains(machineCode)) {
                        message = message.Replace(endAxisType, "栈板");
                    }
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "警告",
                        Content = message
                    }));
                    isValid = false;
                    App.Store.Dispatch(new SysActions.AddMarqueeMessage(SysActions.MARQUEE_SCAN_END_AXIS_RFID + machineCode, message));
                }
                if (taskDoing.EmpRfids?.Count < 1) {
                    var message = $"请打 {machineCode} 的上机卡";
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "警告",
                        Content = message
                    }));

                    App.Store.Dispatch(new SysActions.AddMarqueeMessage(SysActions.MARQUEE_PUNCH_START_MACHINE + machineCode, message));
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
        /// <param name="taskDoing">运行的任务</param>
        /// <param name="st">接受到的任务</param>
        /// <param name="axis">开始的轴号</param>
        /// <param name="axisIndex">轴任务在任务列表的序号</param>
        void setSchTaskDoing([NotNull]SchTaskDoing taskDoing, [NotNull] MqSchTask st, [NotNull] MqTaskAxis axis, int axisIndex) {
            taskDoing.MqSchTask = st;
            taskDoing.MqSchTaskId = st.id;
            taskDoing.MqSchAxisIndex = axisIndex;
            taskDoing.MqSchAxis = axis;
            taskDoing.IsStarted = true;
            taskDoing.Step = st.step;
            taskDoing.WorkCode = axis.workcode;
            taskDoing.MeterPlan = axis.length;
            taskDoing.StartTime = DateTime.Now;
            taskDoing.CalcAvgSpeed = YUtil.CreateExecAvgFunc();
            taskDoing.CalcOdAvg = YUtil.CreateExecAvgFunc();
            taskDoing.TaskId = axis.taskId;
            axis.IsStarted = true;
            axis.State = MqSchTaskAxisState.Doing;
            axis.StartTime = DateTime.Now;
            if (taskDoing.StartElecPower <= 1) {
                taskDoing.StartElecPower = getElecPower(axis.maccode.ToUpper());
            }
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
        /// <param name="machineCode">机台编码</param>
        /// <param name="axisCode">调试的轴号</param>
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
        /// 完成某个 轴 任务
        /// <param name="machineCode">机台编码</param>
        /// <param name="axisCode">轴号</param>
        /// </summary>
        async void completeOneAxis(string machineCode, string axisCode, string reason) {
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
                    completeOneSchTask(machineCode, taskDoing.TaskId, taskDoing);
                }
                uManu = new MqUploadManu() {
                    actualBeginTime = YUtil.GetUtcTimestampMs(taskDoing.StartTime),
                    actualEndTime = YUtil.GetUtcTimestampMs(taskDoing.EndTime),
                    axisName = axisCode,
                    macCode = machineCode,
                    axixLen = taskDoing.MeterWork,
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
                    avgOd = taskDoing.OdAvg,
                    maxOd = taskDoing.OdMax,
                    minOd = taskDoing.OdMin,
                    seqCode = taskDoing.MqSchAxis.seqcode,
                    status = reason,
                };

                //重新初始化
                taskDoing.Init();
                //普通轴直接清空放线收线卡
                if (!PalletDict.ContainsKey(machineCode)) {
                    taskDoing.EndAxisRfids.Clear();
                    taskDoing.StartAxisRfids.Clear();
                    //清除「参数」界面的 Rfid 显示
                    setCpmRfid(machineCode, DefinedParamCode.StartAxisRfid, "");
                    setCpmRfid(machineCode, DefinedParamCode.EndAxisRfid, "");
                    //清除 「警告」界面的 Rfid 显示
                    App.Store.Dispatch(
                        new SysActions.DelMarqueeMessage(SysActions.MARQUEE_SCAN_START_AXIS_RFID + machineCode));
                    App.Store.Dispatch(
                        new SysActions.DelMarqueeMessage(SysActions.MARQUEE_SCAN_END_AXIS_RFID + machineCode));
                    //小轴用的是栈板，所以放线收线不能清空
                } else {
                    PalletDict[machineCode].AxisNum += 1;
                }
            }
            //更新缓存
            using (var ctx = SqliteHelper.CreateSqliteService()) {
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

        float getElecPower(String machineCode) {
            var cpmNameToCodeDict = MachineConfig.MachineDict[machineCode].CpmNameToCodeDict;
            var setting = GlobalConfig.MachineSettingDict[machineCode];
            //update:2018-4-13，添加总电能
            if (cpmNameToCodeDict.ContainsKey(setting.totalPower)) {
                if (App.Store.GetState().CpmState.OnlineCpmsDict[machineCode]
                    .TryGetValue(cpmNameToCodeDict[setting.totalPower], out var tp)) {
                    if (tp.ValueType == SmParamType.Signal) {
                        return tp.GetFloatVal();
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 完成某个工单
        /// </summary>
        /// <param name="machineCode">机台编码</param>
        /// <param name="taskId">任务关键字</param>
        void completeOneSchTask(string machineCode, string taskId, SchTaskDoing taskdoing) {
            var mqTasks = MqSchTasksDict[machineCode];
            var removeTask = mqTasks.FirstOrDefault(t => t.taskId == taskId);

            float elec = getElecPower(machineCode) - taskdoing.StartElecPower;
            var uploadElec = new MqUploadElec() {
                machinecode = machineCode,
                workcoder = removeTask.workcode,
                employees = string.Join(",", taskdoing.EmpRfids),
                elec = elec
            };
            //回传总电能
            App.Store.Dispatch(mqEffects.UploadElecPower(new MqActions.UploadElecPower(uploadElec)));
            taskdoing.StartElecPower = 0f;

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
