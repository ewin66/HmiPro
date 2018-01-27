using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using DevExpress.Xpf.Core;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Helpers;
using HmiPro.Redux;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.Views.Dx;
using MongoDB.Bson.IO;
using Reducto;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// 解析命令、初始化 HmiConfig、初始化 Redux、打印所有的 Action、初始化 LoggerHelper、AssetsHelper、SqliteHelper 
    /// <date>2017-12-17</date>
    /// <author>ychost</author>
    /// </summary>
    public partial class App : Application {
        /// <summary>
        /// 设置程序启动时间
        /// </summary>
        public App() {
            AppState.ExectedActions["[App] Started"] = DateTime.Now;
        }
        public static LoggerService Logger;
        public static StorePro<AppState> Store;
        /// <summary>
        /// 减缓这些消除的日志输出频率 
        /// 因为这些消息的原始频率太高了
        /// </summary>
        public static List<string> MuffleLogActions = new List<string>()   {
            CpmActions.CPMS_UPDATED_ALL,
            CpmActions.CPMS_UPDATED_DIFF,
            CpmActions.CPMS_IP_ACTIVED,
            CpmActions.STATE_SPEED_ACCEPT,
            CpmActions.OD_ACCPET,
            CpmActions.NOTE_METER_ACCEPT,
            DbActions.UPLOAD_CPMS_INFLUXDB,
            DbActions.UPLOAD_CPMS_INFLUXDB_SUCCESS,
            AlarmActions.CHECK_CPM_BOM_ALARM,
        };
        public static IDictionary<string, Mufflog> MuffleLogDict = new ConcurrentDictionary<string, Mufflog>();

        /// <summary>
        /// 对程序进行配置
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            ReduxIoc.Init();
            Store = UnityIocService.ResolveDepend<StorePro<AppState>>();

            //异步初始化，直接进入 DxWindow
            Task.Run(() => {
                hmiConfigInit(e);
                initSubscribe();
                //通知 DxWindow 初始化完毕
                Store.Dispatch(new SysActions.AppXamlInited(e));
            });
        }

        /// <summary>
        /// 程序初始化完毕之后才订阅打印日志
        /// </summary>
        void initSubscribe() {
            Logger = LoggerHelper.CreateLogger("App");
            //减缓频率的日志输出初始化
            MuffleLogActions.ForEach(a => {
                MuffleLogDict[a] = new Mufflog();
            });
            Store.Subscribe(logDebugActions);
        }


        /// <summary>
        /// 输出Action动作
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void logDebugActions(AppState state, IAction action) {
            if (action.Type() != null) {
                ConsoleColor color = ConsoleColor.Green;
                if (action.Type().Contains("[Mq]")) {
                    color = ConsoleColor.Yellow;
                } else if (action.Type().Contains("[Cpm")) {
                    color = ConsoleColor.DarkYellow;
                } else if (action.Type().Contains("[Db]")) {
                    color = ConsoleColor.Magenta;
                } else if (action.Type().Contains("[Alarm]")) {
                    color = ConsoleColor.Red;
                } else if (action.Type().Contains("[Sys]")) {
                    color = ConsoleColor.Green;
                }
                if (action.Type().Contains("Failed")) {
                    color = ConsoleColor.Red;
                }
                //需要减缓频率的消息，没隔 MinGapSec 秒输出一次
                if (MuffleLogDict.TryGetValue(action.Type(), out var muffle)) {
                    muffle.Freq += 1;
                    if ((DateTime.Now - muffle.LastLogTime).TotalSeconds >= muffle.MinGapSec) {
                        Logger.Debug($"Redux Muffle Action: {action.Type()}  Occur [{muffle.Freq}] Times In {muffle.MinGapSec} Seconds");
                        muffle.LastLogTime = DateTime.Now;
                        muffle.Freq = 0;
                    }
                } else {
                    //普通动作直接输出
                    Logger.Debug("Redux Current Action: " + action.Type(), color);
                }
                AppState.ExectedActions[action.Type()] = DateTime.Now;
            }
        }

        /// <summary>
        /// 处理命令
        /// </summary>
        /// <param name="e"></param>
        void hmiConfigInit(StartupEventArgs e) {
            updateLoadingMessage("正在解析命令...", 0.01);
            Parser.Default.ParseArguments<CmdOptions>(e.Args).WithParsed(opt => {
                opt.HmiName = opt.HmiName.ToUpper();
                opt.ProfilesFolder = YUtil.GetAbsolutePath(opt.ProfilesFolder);
                var configFolder = opt.ProfilesFolder + "\\" + opt.Mode;
                opt.ConfigFolder = configFolder;
                var assetsFolder = opt.ProfilesFolder + @"\Assets";

                if (bool.Parse(opt.ShowConsole)) {
                    ConsoleHelper.Show();
                }
                Console.WriteLine("是否开机自启动: -" + opt.AutoSatrt);
                Console.WriteLine("配置文件夹：-" + opt.ProfilesFolder);
                Console.WriteLine("显示 Splash -" + opt.ShowSplash);
                Console.WriteLine("当前运行模式：-" + opt.Mode);

                updateLoadingMessage("配置开机自启...", 0.02);
                YUtil.SetAppAutoStart(GetType().ToString(), bool.Parse(opt.AutoSatrt));

                updateLoadingMessage("初始化Hmi配置...", 0.03);
                var configFile = configFolder + $@"\Hmi.Config.{opt.Config}.json";
                HmiConfig.Load(configFile);
                Console.WriteLine($"初始化配置文件: -{configFile}");
                Console.WriteLine("是否启用Mock数据：-" + bool.Parse(opt.Mock));

                updateLoadingMessage("初始化工艺字典...", 0.04);
                HmiConfig.InitCraftBomZhsDict(assetsFolder + @"\Dicts\工艺Bom.xls");

                updateLoadingMessage("初始化资源文件...", 0.05);
                AssetsHelper.Init(YUtil.GetAbsolutePath(assetsFolder));

                updateLoadingMessage("初始化日志服务...", 0.06);
                LoggerHelper.Init(YUtil.GetAbsolutePath(HmiConfig.LogFolder));

                updateLoadingMessage("初始化 Sqlite...", 0.08);
                HmiConfig.SqlitePath = YUtil.GetAbsolutePath(opt.SqlitePath);
                SqliteHelper.Init(YUtil.GetAbsolutePath(HmiConfig.SqlitePath));
                //保留启动参数
                CmdOptions.GlobalOptions = opt;
            }).WithNotParsed(err =>
                throw new Exception("启动参数异常" + err)
            );

            //记录程序崩溃日志
            AppDomain.CurrentDomain.UnhandledException += (s, ue) => {
                var message = $"程序崩溃：{ue.ExceptionObject}\r\n当前可用内存：{YUtil.GetAvaliableMemoryByte() / 1000000} M\r\nApp.Store: {Newtonsoft.Json.JsonConvert.SerializeObject(App.Store)}";
                //将错误日志写入mongoDb
                Logger.ErrorWithDb(message, MachineConfig.HmiName);
                if (!HmiConfig.IsDevUserEnv) {
                    //重启软件
                    ActiveMqHelper.GetActiveMqService().Close();
                    Application.Current.Dispatcher.Invoke(() => {
                        YUtil.Exec(Application.ResourceAssembly.Location, "");
                        Application.Current.Shutdown();
                    });
                }
            };
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


    }
    /// <summary>
    /// 对频率较高的日志的打印频率进行抑制
    /// </summary>
    public class Mufflog {
        /// <summary>
        /// 在最小时间间隔内出现的次数
        /// </summary>
        public int Freq;
        /// <summary>
        /// 上次打印时间
        /// </summary>
        public DateTime LastLogTime = DateTime.MinValue;
        /// <summary>
        /// 最小打印时间间隔
        /// </summary>
        public int MinGapSec = 10;
    }
}
