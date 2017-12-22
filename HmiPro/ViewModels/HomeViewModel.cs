using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
using Reducto;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.ViewModels {
    [POCOViewModel]
    public class HomeViewModel {
        public virtual Assets Assets { get; set; } = AssetsHelper.GetAssets();
        public virtual INavigationService NavigationSerivce => null;
        public static bool IsFirstEntry = true;

        public HomeViewModel() {
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
            UnityIocService.ResolveDepend<DMesCore>().Init();
            UnityIocService.ResolveDepend<AlarmCore>().Init();
            UnityIocService.ResolveDepend<CpmCore>().Init();
            var id = 0;
            //派发三个测试任务
            YUtil.SetInterval(10000, () => {
                Task.Run(() => {
                    dispatchMockSchTask((++id));
                    dispatchMockAlarm(++id);
                });
            }, 1)();
            await UnityIocService.ResolveDepend<SchCore>().Init();
            //启动Http解析系统
            var isHttpSystem = await App.Store.Dispatch(sysEffects.StartHttpSystem(new SysActions.StartHttpSystem($"http://+:{HmiConfig.CmdHttpPort}/")));
            if (!isHttpSystem) {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() { Title = "启动失败", Content = $"Http {HmiConfig.CmdHttpPort} 端口服务启动失败，请检查" }));
            }
            //启动Cpm采集服务
            var isCpmServer = await App.Store.Dispatch(cpmEffects.StartServer(new CpmActions.StartServer(HmiConfig.CpmTcpIp, HmiConfig.CpmTcpPort)));
            if (!isCpmServer) {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() { Title = "启动失败", Content = $"参数采集服务 {HmiConfig.CpmTcpIp}:{HmiConfig.CpmTcpPort} 启动失败，请检查" }));
            }
            foreach (var pair in MachineConfig.MachineDict) {
                //监听排产任务
                var stQueueName = @"QUEUE_" + pair.Key;
                var isSchTask = await App.Store.Dispatch(mqEffects.StartListenSchTask(new MqActiions.StartListenSchTask(stQueueName, pair.Key)));
                if (!isSchTask) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() { Title = "启动失败", Content = $"监听Mq排产队列 {stQueueName} 失败，请检查" }));
                }
                //监听扫描物料信息
                var smQueueName = $@"JUDGE_MATER_{pair.Key}";
                var isScanMaterial = await App.Store.Dispatch(mqEffects.StartListenScanMaterial(new MqActiions.StartListenScanMaterial(pair.Key, smQueueName)));
                if (!isScanMaterial) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() { Title = "启动失败", Content = $"监听Mq扫描来料队列 {smQueueName} 失败，请检查" }));
                }
            }
            var version = YUtil.GetAppVersion(Assembly.GetExecutingAssembly());
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() { Title = "系统启动完毕", Content = $"版本:{version}" }));
        }

        /// <summary>
        /// 检查配置
        /// </summary>
        void checkConfig() {
            foreach (var pair in MachineConfig.MachineDict) {
                var machineCode = pair.Key;
                var machineConfig = pair.Value;
                if (!machineConfig.LogicToCpmDict.ContainsKey(CpmInfoLogic.Speed)) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "程序配置有误",
                        Content = $"机台 {machineCode} 未配置速度逻辑 {(int)CpmInfoLogic.Speed}",
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
        /// 派发模拟排产任务
        /// </summary>
        /// <param name="id">任务id</param>
        void dispatchMockSchTask(int id = 0) {
            var mockEffects = UnityIocService.ResolveDepend<MockEffects>();
            var task = YUtil.GetJsonObjectFromFile<MqSchTask>(AssetsHelper.GetAssets().MockMqSchTaskJson);
            task.id = id;
            App.Store.Dispatch(mockEffects.MockSchTaskAccept(new MockActions.MockSchTaskAccpet(task)));
        }


        void dispatchMockAlarm(int code) {
            foreach (var pair in MachineConfig.MachineDict) {
                var machineCode = pair.Key;
                App.Store.Dispatch(new AlarmActions.GenerateOneAlarm(machineCode, AlarmMocks.CreateOneAlarm(code)));
            }
        }

        /// <summary>
        /// 页面跳转命令
        /// </summary>
        /// <param name="viewName">页面名称，比如页面为HomeView.xaml，则名称为HomeView</param>
        [Command(Name = "NavigateCommand")]
        public void Navigate(string viewName) {
            NavigationSerivce.Navigate(viewName);
        }

        [Command(Name = "JumpAppSettingViewCommand")]
        public void JumpAppSetting() {
            App.Store.Dispatch(new SysActions.ShowSettingView());
        }
    }
}