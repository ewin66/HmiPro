using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Cores;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using Newtonsoft.Json;
using Reducto;
using YCsharp.Service;
using YCsharp.Util;
using Timer = System.Timers.Timer;

namespace HmiPro.Redux.Effects {
    /// <summary>
    /// 加载GlobalConfig、MachineConfig、xxHelper等等
    /// 将配置系统迁移到此处是为了能方便管理以及能够支持系统启动进度条
    /// 一定要先执行 LoadGlobalConfig 然后再执行 LoadMachineConfig
    /// <author>ychost</author>
    /// <date>2018-1-26</date>
    /// </summary>
    public class LoadEffects {
        /// <summary>
        /// 加载全局配置，GlobalConfig、各种Helper 等等
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<LoadActions.LoadGlobalConfig, bool> LoadGlobalConfig;
        /// <summary>
        /// 加载机台配置
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<LoadActions.LoadMachieConfig, bool> LoadMachineConfig;

        /// <summary>
        /// 日志
        /// </summary>
        public LoggerService Logger;

        /// <summary>
        ///初始化日志
        /// </summary>
        public LoadEffects() {
            Logger = new LoggerService(HmiConfig.LogFolder) { DefaultLocation = GetType().ToString() };
            initLoadGlobalConfig();
            initLoadMachineConfig();
        }

        /// <summary>
        /// 更新系统启动进度内容
        /// </summary>
        /// <param name="message"></param>
        /// <param name="percent"></param>
        /// <param name="sleepms"></param>
        void updateLoadingMessage(string message, double percent, int sleepms = 400) {
            App.Store.Dispatch(new SysActions.SetLoadingMessage(message, percent));
            if (!HmiConfig.IsDevUserEnv) {
                Thread.Sleep(sleepms);
            }
        }



        /// <summary>
        /// 加载 GlobalConfig 和 初始化 xxHelper
        /// </summary>
        void initLoadGlobalConfig() {
            LoadGlobalConfig = App.Store.asyncAction<LoadActions.LoadGlobalConfig, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() =>
                        globalConfigLoad()
                    );
                });
        }

        /// <summary>
        /// 加载 MachineConfig，主要是初始化 CpmInfo
        /// </summary>
        void initLoadMachineConfig() {
            LoadMachineConfig = App.Store.asyncAction<LoadActions.LoadMachieConfig, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    updateLoadingMessage("正在加载机台配置...", 0.4);
                    return await Task.Run(() => {
                        loadConfigByGlobal();
                        return true;
                    });
                });
        }

        /// <summary>
        /// LoggerHelper 和 Assets Helper 已经在 App.xaml.cs 中初始化了，所以这里不必要初始化了
        /// </summary>
        bool globalConfigLoad() {
            updateLoadingMessage("正在准备系统资源文件", 0.15);
            Thread.Sleep(CmdOptions.GlobalOptions.WaitSec * 1000);

            updateLoadingMessage("正在检查系统启动环境...", 0.17);
            if (processIsStarted()) {
                var message = "系统重复启动异常";
                App.Store.Dispatch(new SysActions.SetLoadingMessage(message, 0.18));
                shutdownAppAfterSec(10, 0.18, "重复启动系统异常");
                return false;
            }

            updateLoadingMessage("正在初始化异常配置...", 0.20);
            ExceptionHelper.Init();

            updateLoadingMessage("正在加载系统配置...", 0.23);
            GlobalConfig.Load(YUtil.GetAbsolutePath(".\\Profiles\\Global.xls"));

            updateLoadingMessage("正在初始化 ActiveMq...", 0.27);
            ActiveMqHelper.Init(HmiConfig.MqConn, HmiConfig.MqUserName, HmiConfig.MqUserPwd);

            updateLoadingMessage("正在初始化 MongoDb...", 0.30);
            MongoHelper.Init(HmiConfig.MongoConn);

            updateLoadingMessage("正在初始化 InfluxDb...", 0.33);
            InfluxDbHelper.Init($"http://{HmiConfig.InfluxDbIp}:8086", HmiConfig.InfluxCpmDbName);

            updateLoadingMessage("正在同步时间...", 0.35);
            syncTime(!HmiConfig.IsDevUserEnv);

            Logger.Debug("当前操作系统：" + YUtil.GetOsVersion());
            Logger.Debug("当前版本：" + YUtil.GetAppVersion(Assembly.GetExecutingAssembly()));
            Logger.Debug("是否为开发环境：" + HmiConfig.IsDevUserEnv);
            Logger.Debug("浮点精度：" + HmiConfig.MathRound);

            return true;
        }

        /// <summary>
        /// 通过 Global.xls中预定义的数据来加载配置
        /// </summary>
        void loadConfigByGlobal() {
            try {
                string hmiPath = "";
                if (!string.IsNullOrEmpty(CmdOptions.GlobalOptions.HmiName)) {
                    hmiPath = YUtil.GetAbsolutePath(CmdOptions.GlobalOptions.ConfigFolder + "\\Machines\\" + CmdOptions.GlobalOptions.HmiName + ".xls");
                }
                MachineConfig.LoadFromGlobal(hmiPath);
                App.StartupLog.HmiName = MachineConfig.HmiName;
                checkConfig();
                afterConfigLoaded();
            } catch (Exception e) {
                Logger.Error("程序配置出错", e);
                var message = "程序出错，请检查网络连接";
                updateLoadingMessage(message, 0.5, 0);
                shutdownAppAfterSec(10, 0.1, message);
            }
        }

        /// <summary>
        /// 配置文件加载成功之后执行的一些初始化
        /// </summary>
        async void afterConfigLoaded() {
            //== 初始化部分State
            updateLoadingMessage("正在初始化系统核心...", 0.5);
            App.Store.Dispatch(new ViewStoreActions.Init());
            App.Store.Dispatch(new CpmActions.Init());
            App.Store.Dispatch(new AlarmActions.Init());
            App.Store.Dispatch(new OeeActions.Init());
            App.Store.Dispatch(new DMesActions.Init());
            App.Store.Dispatch(new DpmActions.Init());

            var sysEffects = UnityIocService.ResolveDepend<SysEffects>();
            var cpmEffects = UnityIocService.ResolveDepend<CpmEffects>();
            var mqEffects = UnityIocService.ResolveDepend<MqEffects>();

            updateLoadingMessage("正在连接服务器...", 0.55);
            var task = Task.Run(() => {
                mqEffects.Start();
            });
            //更新连接服务器的进度
            double p = 0.55;
            bool isMqEffectsStarted = false;
            Timer updateTimer = null;
            updateTimer = YUtil.SetInterval(500, () => {
                p += 0.01;
                updateLoadingMessage("正在连接服务器...", p, 0);
                Logger.Debug("正在连接服务器..." + p.ToString("P1"));
                if (isMqEffectsStarted || p > 0.64) {
                    YUtil.ClearTimeout(updateTimer);
                }
            });
            isMqEffectsStarted = Task.WaitAll(new[] { task }, 10000);
            if (!isMqEffectsStarted) {
                updateLoadingMessage("连接 Mq 超时...", 0.6);
                App.Store.Dispatch(new SysActions.AddMarqueeMessage(SysActions.MARQUEE_CONECT_MQ_TIMEOUT, "Mq 连接超时"));
                restartAppAfterSec(10, 0.6, "连接 Mq 超时");
                return;
            }

            UnityIocService.ResolveDepend<DMesCore>().Init();
            UnityIocService.ResolveDepend<AlarmCore>().Init();
            UnityIocService.ResolveDepend<CpmCore>().Init();
            UnityIocService.ResolveDepend<OeeCore>().Init();
            UnityIocService.ResolveDepend<DpmCore>().Init();
            UnityIocService.ResolveDepend<HookCore>().Init();


            //Http 命令解析
            updateLoadingMessage("正在启动Http服务...", 0.7);
            var starHttpSystem = App.Store.Dispatch(sysEffects.StartHttpSystem(new SysActions.StartHttpSystem($"http://+:{HmiConfig.CmdHttpPort}/")));

            //参数采集服务
            updateLoadingMessage("正在启动参数采集服务...", 0.75);
            var startCpmServer = App.Store.Dispatch(cpmEffects.StartServer(new CpmActions.StartServer(HmiConfig.CpmTcpIp, HmiConfig.CpmTcpPort)));

            //监听排产和来料 
            updateLoadingMessage("正在启动监听排产服务...", 0.8);
            Dictionary<string, Task<bool>> startListenMqDict = new Dictionary<string, Task<bool>>();
            foreach (var pair in MachineConfig.MachineDict) {
                //监听排产任务
                var stQueueName = @"QUEUE_" + pair.Key;
                var stTask = App.Store.Dispatch(mqEffects.StartListenSchTask(new MqActions.StartListenSchTask(pair.Key, stQueueName)));
                startListenMqDict[stQueueName] = stTask;
                //监听来料
                var smQueueName = $@"JUDGE_MATER_{pair.Key}";
                var smTask = App.Store.Dispatch(mqEffects.StartListenScanMaterial(new MqActions.StartListenScanMaterial(pair.Key, smQueueName)));
                startListenMqDict[smQueueName] = smTask;
            }

            //监听人员打卡
            updateLoadingMessage("正在启动监听人员打卡...", 0.85);
            var empRfidTask = App.Store.Dispatch(mqEffects.StartListenEmpRfid(new MqActions.StartListenEmpRfid(MachineConfig.HmiName + "_employee", HmiConfig.TopicEmpRfid)));
            startListenMqDict["rfidEmpTask"] = empRfidTask;

            //监听轴号卡
            updateLoadingMessage("正在启动监听盘卡扫描...", 0.90);
            var axisRfidTask = App.Store.Dispatch(mqEffects.StartListenAxisRfid(new MqActions.StartListenAxisRfid(MachineConfig.HmiName + "_axis", HmiConfig.TopicListenHandSet)));
            startListenMqDict["rfidAxisTask"] = axisRfidTask;

            //监听机台命令
            updateLoadingMessage("正在启动监听机台命令...", 0.92);
            var cmdTask = App.Store.Dispatch(mqEffects.StartListenCmd(new MqActions.StartListenCmd(MachineConfig.HmiName + "_cmd", HmiConfig.TopicCmdReceived)));
            startListenMqDict["cmdTask"] = cmdTask;

            updateLoadingMessage("正在启动系统核心服务...", 0.95);
            var tasks = new List<Task<bool>>() { starHttpSystem, startCpmServer };
            tasks.AddRange(startListenMqDict.Values);
            //检查各项任务启动情况
            await Task.Run(() => {
                //等等所有任务完成
                var isStartedOk = Task.WaitAll(tasks.ToArray(), 30000);
                if (!isStartedOk) {
                    var message = "系统核心启动超时，请检查网络连接";
                    updateLoadingMessage(message, 0.95);
                    restartAppAfterSec(10, 0.95, "系统核心启动超时");
                    return;
                }
                //是否启动完成Cpm服务
                var isCpmServer = startCpmServer.Result;
                if (!isCpmServer) {
                    var message = "参数采集核心启动失败";
                    updateLoadingMessage(message, 0.95, 0);
                    restartAppAfterSec(10, 0.95, message);
                    return;
                }
                //是否启动完成Http解析系统
                var isHttpSystem = starHttpSystem.Result;
                if (!isHttpSystem) {
                    var message = "Http 核心启动失败";
                    updateLoadingMessage(message, 0.95, 0);
                    restartAppAfterSec(10, 0.95, message);
                    return;
                }
                //是否完成监听Mq
                foreach (var pair in startListenMqDict) {
                    var isStartListenMq = pair.Value.Result;
                    var mqKey = pair.Key.ToUpper();
                    if (!isStartListenMq) {
                        string failedMessage = string.Empty;
                        if (mqKey.Contains("QUEUE")) {
                            failedMessage = $"监听Mq 排产队列 {pair.Key} 失败";
                        } else if (mqKey.Contains("JUDGE_MATER")) {
                            failedMessage = $"监听Mq 扫描来料队列 {pair.Key} 失败";
                        } else if (mqKey.Contains("RFIDEMP")) {
                            failedMessage = $"监听mq 人员打卡 数据失败";
                        } else if (mqKey.Contains("RFIDAXIS")) {
                            failedMessage = $"监听Mq 线盘卡失败";
                        } else if (mqKey.Contains("CMD")) {
                            failedMessage = $"监听Mq 机台命令失败";
                        }
                        if (!string.IsNullOrEmpty(failedMessage)) {
                            updateLoadingMessage(failedMessage, 0.95, 0);
                            restartAppAfterSec(10, 0.95, failedMessage);
                            return;
                        }
                    }
                }
                if (HmiConfig.IsDevUserEnv) {
                    updateLoadingMessage("系统核心启动完毕，正在渲染界面...", 1, 0);
                    App.Store.Dispatch(new SysActions.AppInitCompleted());
                    return;
                }
                var percent = 0.95;
                YUtil.SetInterval(300, t => {
                    percent += 0.01;
                    updateLoadingMessage("系统核心启动完毕，正在渲染界面...", percent, 0);
                    if (t == 5 || percent >= 1) {
                        App.Store.Dispatch(new SysActions.AppInitCompleted());
                    }
                }, 5);
            });

            //update: 2018-3-28 
            // 将调度器最后启用，这些调度器需要依赖比较多，但本身不提供依赖
            updateLoadingMessage("正在启动调度器...", 0.98);
            await UnityIocService.ResolveDepend<SchCore>().Init();
        }
        /// <summary>
        /// 与服务器同步时间
        /// </summary>
        private void syncTime(bool canSync) {
            //非开发环境才同步时间
            if (canSync) {
                Task.Run(() => {
                    try {
                        //获取服务器时间
                        var ntpTime = YUtil.GetNtpTime(HmiConfig.NtpIp);
                        App.StartupLog.SyncServerTime = ntpTime;
                        //时间差超过10秒才同步时间
                        if (Math.Abs((DateTime.Now - ntpTime).TotalSeconds) > 10) {
                            YUtil.SetLoadTimeByDateTime(ntpTime);
                        }
                        Logger.Info($"同步时间成功: {ntpTime}");
                    } catch (Exception e) {
                        Logger.Error("获取服务器时间失败", e);
                    }
                });
            }
        }

        /// <summary>
        /// 检测进程是否存在，存在则显示已有的进程否则则关闭程序
        /// <href>https://www.cnblogs.com/zhili/p/OnlyInstance.html</href>
        /// </summary>
        static bool processIsStarted() {
            Process currentproc = Process.GetCurrentProcess();
            Process[] processcollection = Process.GetProcessesByName(currentproc.ProcessName.Replace(".vshost", string.Empty));
            if (processcollection.Length > 1) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 程序将在 totalSec 秒后自动关闭
        /// </summary>
        void shutdownAppAfterSec(int totalSec, double percent, string message = "程序启动超时") {
            try {
                updateAppStartupLog(message);
            } catch {
            }

            YUtil.SetInterval(1000, t => {
                var wait = totalSec - t;
                var waitMessage = $"{message}，将在 {wait} 秒后关闭";
                updateLoadingMessage(waitMessage, percent, 0);
                if (wait <= 0) {
                    App.Shutdown();
                }
            }, totalSec);
        }

        /// <summary>
        /// 程序将在 totalSec 之后重启
        /// </summary>
        /// <param name="totalSec"></param>
        /// <param name="percent"></param>
        /// <param name="message"></param>
        void restartAppAfterSec(int totalSec, double percent, string message = "程序启动超时") {
            try {
                var latestLog = getAppLatestStartupLog();
                //连续启动失败次数越多，等待启动时间越长
                if (latestLog?.ContinueFailedTimes > 0 && !HmiConfig.IsDevUserEnv) {
                    totalSec = latestLog.ContinueFailedTimes * 10;
                }
                updateAppStartupLog(message);
            } catch (Exception e) {
                Logger.Error("启动日志出问题", e);
            }
            //显示重启提示
            var waitMessage = $"{message}，将在 {totalSec} 秒后尝试重启";
            updateLoadingMessage(waitMessage, percent, 0);
            YUtil.SetInterval(1000, t => {
                var wait = totalSec - t;
                waitMessage = $"{message}，将在 {wait} 秒后尝试重启";
                updateLoadingMessage(waitMessage, percent, 0);
                if (wait <= 0) {
                    App.Restart();
                }
            }, totalSec);
        }

        /// <summary>
        /// 获取最近一次启动的日志
        /// </summary>
        /// <returns></returns>
        private StartupLog getAppLatestStartupLog() {
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                //Sqlite 目前不支持直接 LastOrDefault
                return ctx.StartupLogs.ToList().LastOrDefault();
            }
        }

        /// <summary>
        /// 更新启动日志
        /// </summary>
        /// <param name="startFailedReason">启动失败原因</param>
        private void updateAppStartupLog(string startFailedReason) {
            //设置 HmiName
            var localIp = YUtil.GetAllIps().FirstOrDefault(ip => ip.Contains("188."));
            var hmiName = !string.IsNullOrEmpty(MachineConfig.HmiName) ? MachineConfig.HmiName : localIp ?? "Unknowns";
            if (!string.IsNullOrEmpty(App.StartupLog.HmiName)) {
                App.StartupLog.HmiName = hmiName;
            }

            var lastLog = getAppLatestStartupLog();
            //增加启动失败次数
            if (lastLog != null) {
                App.StartupLog.ContinueFailedTimes = ++lastLog.ContinueFailedTimes;
            } else {
                App.StartupLog.ContinueFailedTimes = 1;
            }
            App.StartupLog.StartFailedReason = startFailedReason;
            Logger.Error(startFailedReason + " -->" + JsonConvert.SerializeObject(App.StartupLog));

            //保存到 Sqlite
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                ctx.StartupLogs.Add(App.StartupLog);
                ctx.SaveChanges();
            }
            //上传到 MongoDB
            var mongoClient = MongoHelper.GetMongoService();
            mongoClient.GetDatabase(MongoHelper.LogsDb).GetCollection<StartupLog>(MongoHelper.StartupLogsCollection).InsertOneAsync(App.StartupLog);
        }

        /// <summary>
        /// 检查配置
        /// </summary>
        void checkConfig() {

        }
    }
}
