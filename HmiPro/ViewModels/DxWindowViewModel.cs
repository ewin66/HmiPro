using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.Redux.Sbuscribers;
using HmiPro.ViewModels.Sys;
using HmiPro.Views.Sys;
using Newtonsoft.Json;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.ViewModels {
    public class DxWindowViewModel : ViewModelBase {

        public readonly LoggerService Logger;

        public readonly StorePro<AppState> Store;


        public DxWindowViewModel() {
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            Store = UnityIocService.ResolveDepend<StorePro<AppState>>();
            Store.Subscribe(s => {
                if (s.Type == SysActions.SHOW_SETTING_VIEW) {
                    JumpAppSettingView("程序设置");
                }
            });

        }

        /// <summary>
        /// 页面加载事件命令
        /// </summary>
        ICommand onViewLoadedCommand; public ICommand OnViewLoadedCommand {
            get {
                if (onViewLoadedCommand == null)
                    onViewLoadedCommand = new DelegateCommand(OnViewLoaded);
                return onViewLoadedCommand;
            }
        }

        public virtual IDialogService DialogService {
            get { return GetService<IDialogService>(); }

        }

        /// <summary>
        /// 导航服务，注册在 MainWindows.xaml中
        /// </summary>
        public INavigationService NavigationService { get { return GetService<INavigationService>(); } }


        /// <summary>
        /// 导航函数
        /// </summary>
        /// <param name="target"></param>
        public void Navigate(string target) {
            NavigationService.Navigate(target, null, this);
        }

        /// <summary>
        /// 程序启动后跳转到主页
        /// </summary>
        public void OnViewLoaded() {
            Navigate("HomeView");
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                var setting = ctx.Settings.ToList().LastOrDefault();
                if (setting == null) {
                    Store.Dispatch(new SysActions.ShowSettingView());
                } else {
                    try {
                        MachineConfig.Load(setting.MachineXlsPath);
                        dispatchActions();
                    } catch (Exception e) {
                        Store.Dispatch(new SysActions.ShowSettingView());
                    }
                }
            }
        }

        void dispatchActions() {
            //== 初始化部分State
            App.Store.Dispatch(new CpmActions.Init());
            var sysEffects = UnityIocService.ResolveDepend<SysEffects>();
            var cpmEffects = UnityIocService.ResolveDepend<CpmEffects>();
            var mqEffects = UnityIocService.ResolveDepend<MqEffects>();
            var mockEffects = UnityIocService.ResolveDepend<MockEffects>();
            var cpmSubs = UnityIocService.ResolveDepend<CpmSubscribers>();
            Store.Dispatch(sysEffects.StartHttpSystem(new SysActions.StartHttpSystem($"http://+:{HmiConfig.CmdHttpPort}/")));
            Store.Dispatch(cpmEffects.StartServer(new CpmActions.StartServer(HmiConfig.CpmTcpIp, HmiConfig.CpmTcpPort)));
            //监听排产任务
            foreach (var pair in MachineConfig.MachineDict) {
                var queName = @"QUEUE_" + pair.Key;
                Store.Dispatch(mqEffects.StartListenSchTask(new MqActiions.StartListenSchTask(queName)));
            }
            Store.Dispatch(mqEffects.StartUploadCpmsInterval(new MqActiions.StartUploadCpmsInterval(HmiConfig.QueUpdateWebBoard, HmiConfig.UploadWebBoardInterval)));
            var task = YUtil.GetJsonObjectFromFile<MqSchTask>(AssetsHelper.GetAssets().MockMqSchTaskJson);
            //Store.Dispatch(mockEffects.MockSchTaskAccept(new MockActions.MockSchTaskAccpet(task)));
            cpmSubs.Init();

        }



        /// <summary>
        /// 跳转到程序设置界面
        /// 比如配置读取出错等等
        /// </summary>
        /// <param name="title"></param>
        public void JumpAppSettingView(string title) {
            Setting setting = null;
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                setting = ctx.Settings.OrderBy(s => s.Id).ToList().LastOrDefault();
                if (setting == null) {
                    setting = new Setting();
                }
            }

            var settingViewModel = SettingViewModel.Create(setting);
            UICommand okCommand = new UICommand() {
                Caption = "确定",
                IsCancel = false,
                IsDefault = false
            };
            UICommand cancelCommand = new UICommand() {
                Caption = "取消",
                IsCancel = true,
                IsDefault = true,
            };

            var resultCommand = DialogService.ShowDialog(new List<UICommand>() { okCommand, cancelCommand },
                title, nameof(SettingView), settingViewModel);

            if (resultCommand == okCommand) {
                try {
                    using (var ctx = SqliteHelper.CreateSqliteService()) {
                        ctx.Settings.Add(settingViewModel.Setting);
                        ctx.SaveChanges();
                    }
                    MessageBox.Show("配置成功，请重新启动软件", "配置成功", MessageBoxButton.OK, MessageBoxImage.None);
                    //广播消息出去
                    Store.Dispatch(new SysActions.ShutdownApp());

                    Application.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Send);
                } catch (Exception e) {
                    Logger.Error("动态配置有误", e);
                    MessageBox.Show(e.Message, "配置有误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else if (resultCommand == cancelCommand) {

            }
        }
    }
}
