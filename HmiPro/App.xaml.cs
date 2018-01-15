using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
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
            //移植日志输出初始化
            MuffleLogActions.ForEach(action => {
                MuffleLogDict[action] = new Mufflog();
            });
            //配置程序未处理异常提示
            ExceptionHelper.Init();
            //配置解析外部命令
            configInit(e);
            //加载全局配置
            GlobalConfig.Load(YUtil.GetAbsolutePath(".\\Profiles\\Global.xls"));
            //配置日志路径
            LoggerHelper.Init(HmiConfig.LogFolder);
            //配置Sqlite路径
            SqliteHelper.Init(HmiConfig.SqlitePath);
            //配置ActiveMq
            ActiveMqHelper.Init(HmiConfig.MqConn, HmiConfig.MqUserName, HmiConfig.MqUserPwd);
            //配置MongoDb
            MongoHelper.Init(HmiConfig.MongoConn);
            //配置InfluxDb保存实时数据
            InfluxDbHelper.Init($"http://{HmiConfig.InfluxDbIp}:8086", HmiConfig.InfluxCpmDbName);
            //配置Redux
            ReduxIoc.Init();
            //初始化全局的Store
            Store = UnityIocService.ResolveDepend<StorePro<AppState>>();
            //初始化全局的日志
            Logger = LoggerHelper.CreateLogger("App");
            //打印Redux系统的动作
            Store.Subscribe(logDebugActions);
            //同步时间
            bool canSync = !HmiConfig.IsDevUserEnv;
            syncTime(canSync);
            Logger.Debug("当前操作系统：" + YUtil.GetOsVersion());
            Logger.Debug("当前版本：" + YUtil.GetAppVersion(Assembly.GetExecutingAssembly()));
            Logger.Debug("是否为开发环境：" + HmiConfig.IsDevUserEnv);
            Logger.Debug("浮点精度："+HmiConfig.MathRound);

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
        void configInit(StartupEventArgs e) {
            Parser.Default.ParseArguments<CmdOptions>(e.Args).WithParsed(opt => {
                opt.ProfilesFolder = YUtil.GetAbsolutePath(opt.ProfilesFolder);
                var configFolder = opt.ProfilesFolder + "\\" + opt.Mode;
                opt.ConfigFolder = configFolder;
                var assetsFolder = opt.ProfilesFolder + @"\Assets";
                //开机自启
                YUtil.SetAppAutoStart(GetType().ToString(), bool.Parse(opt.AutoSatrt));
                //显示Console框
                if (bool.Parse(opt.ShowConsole)) {
                    ConsoleHelper.Show();
                }
                Console.WriteLine("是否开机自启动: -" + opt.AutoSatrt);
                Console.WriteLine("配置文件夹：-" + opt.ProfilesFolder);
                //启动画面
                if (bool.Parse(opt.ShowSplash)) {
                    DXSplashScreen.Show<SplashScreenView>();
                    DXSplashScreen.SetState(SplashState.Default);
                }
                //全局配置文件
                var configFile = configFolder + $@"\Hmi.Config.{opt.Config}.json";
                HmiConfig.Load(configFile);
                Console.WriteLine($"初始化配置文件: -{configFile}");
                Console.WriteLine("是否启用Mock数据：-" + bool.Parse(opt.Mock));
                //配置静态资源文件
                HmiConfig.SqlitePath = YUtil.GetAbsolutePath(opt.SqlitePath);
                HmiConfig.InitCraftBomZhsDict(assetsFolder + @"\Dicts\工艺Bom.xls");
                Console.WriteLine("当前运行模式：-" + opt.Mode);
                AssetsHelper.Init(YUtil.GetAbsolutePath(assetsFolder));
                //设置全局配置
                CmdOptions.GlobalOptions = opt;
            }).WithNotParsed(err => {
                throw new Exception("参数异常" + e);
            });

            //记录程序崩溃日志
            AppDomain.CurrentDomain.UnhandledException += (s, ue) => {
                var message = $"程序崩溃：{ue.ExceptionObject}\r\n当前可用内存：{YUtil.GetAvaliableMemoryByte() / 1000000} M\r\nApp.Store: {Newtonsoft.Json.JsonConvert.SerializeObject(App.Store)}";
                //将错误日志写入mongoDb
                Logger.ErrorWithDb(message, MachineConfig.HmiName);
            };
        }

        /// <summary>
        /// 与服务器同步时间
        /// </summary>
        private void syncTime(bool canSync) {
            //非开发环境才同步时间
            if (canSync) {
                try {
                    //获取服务器时间
                    var ntpTime = YUtil.GetNtpTime(HmiConfig.NtpIp);
                    //时间差超过10秒才同步时间
                    if (Math.Abs((DateTime.Now - ntpTime).TotalSeconds) > 10) {
                        YUtil.SetLoadTimeByDateTime(ntpTime);
                    }
                    Logger.Info($"同步时间成功: {ntpTime}");
                } catch (Exception e) {
                    Logger.Error("获取服务器时间失败", e);
                }
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
