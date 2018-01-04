using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Helpers;
using HmiPro.Mocks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Cores;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.ViewModels.DMes;
using HmiPro.ViewModels.Sys;
using Reducto;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.ViewModels {
    /// <summary>
    /// 程序入口页面
    /// <author>ychost</author>
    /// <date>2017-12-17</date>
    /// </summary>
    [POCOViewModel]
    public class HomeViewModel {
        public virtual Assets Assets { get; set; } = AssetsHelper.GetAssets();
        public virtual INavigationService NavigationSerivce => null;
        public static bool IsFirstEntry = true;
        public readonly LoggerService Logger;

        public HomeViewModel() {
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }

        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            if (!IsFirstEntry) {
                return;
            }
            IsFirstEntry = false;
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                var setting = ctx.Settings.ToList().LastOrDefault();
                if (setting == null) {
                    App.Store.Dispatch(new SysActions.ShowSettingView());
                } else {
                    try {
                        MachineConfig.Load(setting.MachineXlsPath);
                        checkConfig();
                        afterConfigLoaded();
                    } catch (Exception e) {
                        App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                            Title = "配置出错",
                            Content = e.Message
                        }));
                        Logger.Error($"程序配置有误", e);
                        App.Store.Dispatch(new SysActions.ShowSettingView());
                    }
                }
            }
        }

        async void afterConfigLoaded() {
            //== 初始化部分State
            App.Store.Dispatch(new CpmActions.Init());
            App.Store.Dispatch(new AlarmActions.Init());
            App.Store.Dispatch(new OeeActions.Init());
            App.Store.Dispatch(new DMesActions.Init());

            var sysEffects = UnityIocService.ResolveDepend<SysEffects>();
            var cpmEffects = UnityIocService.ResolveDepend<CpmEffects>();
            var mqEffects = UnityIocService.ResolveDepend<MqEffects>();
            var dbEffects = UnityIocService.ResolveDepend<DbEffects>();

            UnityIocService.ResolveDepend<DMesCore>().Init();
            UnityIocService.ResolveDepend<AlarmCore>().Init();
            UnityIocService.ResolveDepend<CpmCore>().Init();
            await UnityIocService.ResolveDepend<SchCore>().Init();
            foreach (var pair in MachineConfig.MachineDict) {
                if (pair.Value.OdAlarmType == AlarmActions.OdAlarmType.OdThresholdPlc) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = $"机台 {pair.Key} Od报警来源",
                        Content = "从Plc里面读取限值已报警"
                    }));
                }
            }

            if (Environment.UserName.ToLower().Contains("ychost")) {
                //dispatchMock();
            }

            var starHttpSystem = App.Store.Dispatch(sysEffects.StartHttpSystem(new SysActions.StartHttpSystem($"http://+:{HmiConfig.CmdHttpPort}/")));
            var startCpmServer = App.Store.Dispatch(cpmEffects.StartServer(new CpmActions.StartServer(HmiConfig.CpmTcpIp, HmiConfig.CpmTcpPort)));
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
            var empRfidTask = App.Store.Dispatch(mqEffects.StartListenEmpRfid(new MqActions.StartListenEmpRfid(HmiConfig.TopicEmpRfid)));
            startListenMqDict["rfidEmpTask"] = empRfidTask;

            //监听轴号卡
            var axisRfidTAsk =
                App.Store.Dispatch(
                    mqEffects.StartListenAxisRfid(new MqActions.StartListenAxisRfid(HmiConfig.TopicListenHandSet)));
            startListenMqDict["rfidAxisTask"] = axisRfidTAsk;
            var tasks = new List<Task<bool>>() { starHttpSystem, startCpmServer };
            tasks.AddRange(startListenMqDict.Values);
            await Task.Run(() => {
                //等等所有任务完成
                //一分钟超时
                var timeout = 60000 * 10;
                if (CmdOptions.GlobalOptions.MockVal) {
                    timeout = 3000;
                }
                var isTimeouted = Task.WaitAll(tasks.ToArray(), timeout);
                if (!isTimeouted) {
                    App.Store.Dispatch(new SysActions.SetTopMessage("启动超时，请检查网络连接", Visibility.Visible));
                    if (CmdOptions.GlobalOptions.MockVal) {
                        App.Store.Dispatch(new SysActions.AppInitCompleted());
                    }
                    return;
                }

                //是否启动完成Cpm服务
                var isCpmServer = startCpmServer.Result;
                if (!isCpmServer) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "启动失败",
                        Content = $"参数采集服务 {HmiConfig.CpmTcpIp}:{HmiConfig.CpmTcpPort} 启动失败，请检查"
                    }));
                }
                //是否启动完成Http解析系统
                var isHttpSystem = starHttpSystem.Result;
                if (!isHttpSystem) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "启动失败",
                        Content = $"Http {HmiConfig.CmdHttpPort} 端口服务启动失败，请检查"
                    }));
                }
                //是否完成监听Mq
                foreach (var pair in startListenMqDict) {
                    var isStartListenMq = pair.Value.Result;
                    if (pair.Key.ToUpper().Contains("QUEUE") && (!isStartListenMq)) {
                        App.Store.Dispatch(new SysActions.ShowNotification(
                            new SysNotificationMsg() { Title = "启动失败", Content = $"监听Mq 排产队列 {pair.Key} 失败，请检查" }));

                    } else if (pair.Key.ToUpper().Contains("JUDGE_MATER") && (!isStartListenMq)) {
                        App.Store.Dispatch(new SysActions.ShowNotification(
                            new SysNotificationMsg() { Title = "启动失败", Content = $"监听Mq 扫描来料队列 {pair.Key} 失败，请检查" }));

                    } else if (pair.Key.ToUpper().Contains("RFIDEMP") && (!isStartListenMq)) {
                        App.Store.Dispatch(new SysActions.ShowNotification(
                             new SysNotificationMsg() { Title = "启动失败", Content = $"监听Mq 人员打卡 数据失败，请检查" }));

                    } else if (pair.Key.ToUpper().Contains("RFIDAXIS") && (!isStartListenMq)) {
                        App.Store.Dispatch(new SysActions.ShowNotification(
                             new SysNotificationMsg() { Title = "启动失败", Content = $"监听Mq 线盘卡失败，请检查" }));
                    }
                }

                var version = YUtil.GetAppVersion(Assembly.GetExecutingAssembly());
                App.Store.Dispatch(
                    new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "系统启动完毕",
                        Content = $"版本:{version}"
                    }));
                //初始化完成
                App.Store.Dispatch(new SysActions.AppInitCompleted());
            });
        }

        /// <summary>
        /// 检查配置
        /// </summary>
        void checkConfig() {
            foreach (var pair in MachineConfig.MachineDict) {
                var machineCode = pair.Key;
                var machineConfig = pair.Value;
                if (!machineConfig.LogicToCpmDict.ContainsKey(CpmInfoLogic.OeeSpeed)) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "程序配置有误",
                        Content = $"机台 {machineCode} 未配置速度逻辑 {(int)CpmInfoLogic.OeeSpeed}",
                        Level = NotifyLevel.Error
                    }));
                }
                if (!machineConfig.LogicToCpmDict.ContainsKey(CpmInfoLogic.NoteMeter)) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "程序配置有误",
                        Content = $"机台 {machineCode} 未配置记米逻辑 {(int)CpmInfoLogic.NoteMeter}",
                        Level = NotifyLevel.Error
                    }));
                }
            }
        }


        /// <summary>
        /// 页面跳转命令
        /// </summary>
        /// <param name="viewName">页面名称，比如页面为HomeView.xaml，则名称为HomeView</param>
        [Command(Name = "NavigateCommand")]
        public void Navigate(string viewName) {
            if (viewName == "DMesCoreView") {
                var vm = DMesCoreViewModel.Create(MachineConfig.MachineDict.FirstOrDefault().Key);
                NavigatorViewModel.NavMachineCodeInDoing = vm.MachineCode;
                NavigationSerivce.Navigate("DMesCoreView", vm, null, this, true);
            } else {
                NavigationSerivce.Navigate(viewName, null, this, true);
            }
        }

        [Command(Name = "JumpAppSettingViewCommand")]
        public void JumpAppSetting() {
            App.Store.Dispatch(new SysActions.ShowSettingView());
        }
    }
}