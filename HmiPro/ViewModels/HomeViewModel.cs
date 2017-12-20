using System;
using System.Linq;
using System.Reflection;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Config;
using HmiPro.Helpers;
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
            var sysEffects = UnityIocService.ResolveDepend<SysEffects>();
            var cpmEffects = UnityIocService.ResolveDepend<CpmEffects>();
            var mqEffects = UnityIocService.ResolveDepend<MqEffects>();
            UnityIocService.ResolveDepend<DMesCore>().Init();
            await UnityIocService.ResolveDepend<SchCore>().Init();
            UnityIocService.ResolveDepend<CpmCore>().Init();

            //启动Http解析系统
            await App.Store.Dispatch(sysEffects.StartHttpSystem(new SysActions.StartHttpSystem($"http://+:{HmiConfig.CmdHttpPort}/")));
            //启动Cpm采集服务
            await App.Store.Dispatch(cpmEffects.StartServer(new CpmActions.StartServer(HmiConfig.CpmTcpIp, HmiConfig.CpmTcpPort)));
            foreach (var pair in MachineConfig.MachineDict) {
                //监听排产任务
                var stQueueName = @"QUEUE_" + pair.Key;
                await App.Store.Dispatch(mqEffects.StartListenSchTask(new MqActiions.StartListenSchTask(stQueueName, pair.Key)));
                //监听扫描物料信息
                var smQueueName = $@"JUDGE_MATER_{pair.Key}";
                await App.Store.Dispatch(mqEffects.StartListenScanMaterial(new MqActiions.StartListenScanMaterial(pair.Key, smQueueName)));
            }
            var version = YUtil.GetAppVersion(Assembly.GetExecutingAssembly());
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() { Title = "系统启动完毕", Content = $"版本:{version}" }));
        }

        void dispatchMockSchTask() {
            var mockEffects = UnityIocService.ResolveDepend<MockEffects>();
            var task = YUtil.GetJsonObjectFromFile<MqSchTask>(AssetsHelper.GetAssets().MockMqSchTaskJson);
            App.Store.Dispatch(mockEffects.MockSchTaskAccept(new MockActions.MockSchTaskAccpet(task)));
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