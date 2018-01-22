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
            if (processIsStarted()) {
                MessageBox.Show("程序已经启动，请勿重复启动", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                Current.Shutdown(0);
                return;
            }
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
            syncTime(!HmiConfig.IsDevUserEnv);
            //启用守护进程
            //if (!HmiConfig.IsDevUserEnv) {
            //    startDaemonAndBuildPipe();
            //}
            Logger.Debug("当前操作系统：" + YUtil.GetOsVersion());
            Logger.Debug("当前版本：" + YUtil.GetAppVersion(Assembly.GetExecutingAssembly()));
            Logger.Debug("是否为开发环境：" + HmiConfig.IsDevUserEnv);
            Logger.Debug("浮点精度：" + HmiConfig.MathRound);
        }

        /// <summary>
        /// 启动守护进程
        /// </summary>
        void startDaemonAndBuildPipe() {
            if (!YUtil.CheckServiceIsExist(HmiConfig.DaemonName)) {
                Logger.Debug("安装守护进程...");
                YUtil.InstallWinService(YUtil.GetAbsolutePath(@".\daemon\Debug\Daemon.exe"), HmiConfig.DaemonName);
                Logger.Debug("安装守护进程完毕");
                return;
            } else {
                if (YUtil.GetWinServiceStatus(HmiConfig.DaemonName) != ServiceControllerStatus.Running) {
                    Logger.Debug("启动守护进程..");
                    YUtil.StartWinService(HmiConfig.DaemonName);
                    Logger.Debug("启动守护进程成功");
                } else {
                    Logger.Debug("守护进程已经启动");
                }
            }
            var pipeEffects = UnityIocService.ResolveDepend<PipeEffects>();
            //往管道里面发送心跳
            YUtil.SetInterval(HmiConfig.PipeHeartbeatMs, () => {
                var rest = new PipeRest(){DataType = PipeDataType.HeartBeat};
                PipeActions.WriteRest writeData = new PipeActions.WriteRest(rest, "HmiDaemon");
                App.Store.Dispatch(pipeEffects.WriteString(writeData));
            });
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
                //直接重启电脑
                YUtil.RestartPC();
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


        #region 如果用户多次启动程序则只显示第一次启动的程序，保证只有一个实例程序
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
        /// 找到某个窗口与给出的类别名和窗口名相同窗口
        /// 非托管定义为：http://msdn.microsoft.com/en-us/library/windows/desktop/ms633499(v=vs.85).aspx
        /// </summary>
        /// <param name="lpClassName">类别名</param>
        /// <param name="lpWindowName">窗口名</param>
        /// <returns>成功找到返回窗口句柄,否则返回null</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 切换到窗口并把窗口设入前台,类似 SetForegroundWindow方法的功能
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="fAltTab">True代表窗口正在通过Alt/Ctrl +Tab被切换</param>
        [DllImport("user32.dll ", SetLastError = true)]
        static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        ///// <summary>
        /////  设置窗口的显示状态
        /////  Win32 函数定义为：http://msdn.microsoft.com/en-us/library/windows/desktop/ms633548(v=vs.85).aspx
        ///// </summary>
        ///// <param name="hWnd">窗口句柄</param>
        ///// <param name="cmdShow">指示窗口如何被显示</param>
        ///// <returns>如果窗体之前是可见，返回值为非零；如果窗体之前被隐藏，返回值为零</returns>
        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        public const int SW_RESTORE = 9;
        public static IntPtr formhwnd;
        #endregion
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
