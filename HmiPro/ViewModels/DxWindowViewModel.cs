using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Cores;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.ViewModels.Sys;
using HmiPro.Views.Sys;
using Newtonsoft.Json;
using YCsharp.Service;
using FluentScheduler;
using HmiPro.Mocks;
using HmiPro.Redux.Services;
using YCsharp.Util;

namespace HmiPro.ViewModels {
    /// <summary>
    /// 程序窗体模型
    /// <date>2017-12-17</date>
    /// <author>ychost</author>
    /// </summary>
    public class DxWindowViewModel : ViewModelBase {
        public readonly LoggerService Logger;
        public readonly StorePro<AppState> Store;
        public readonly IDictionary<string, Action<AppState, IAction>> actionsExecDict = new Dictionary<string, Action<AppState, IAction>>();
        public static string CurTopMessage = "";
        /// <summary>
        /// 窗体顶部显示的提示信息，如：网络断开连接等等
        /// </summary>
        public string TopMessage {
            get => CurTopMessage;
            set {
                if (CurTopMessage != value) {
                    CurTopMessage = value;
                    RaisePropertiesChanged(nameof(TopMessage));
                }
            }
        }

        private Visibility topMessageVisibility = Visibility.Collapsed;
        public Visibility TopMessageVisibility {
            get => topMessageVisibility;
            set {
                if (topMessageVisibility != value) {
                    topMessageVisibility = value;
                    RaisePropertiesChanged(nameof(TopMessageVisibility));
                }
            }
        }



        public DxWindowViewModel() {
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            Store = UnityIocService.ResolveDepend<StorePro<AppState>>();
            actionsExecDict[SysActions.SHOW_NOTIFICATION] = doShowNotification;
            actionsExecDict[SysActions.SHOW_SETTING_VIEW] = doShowSettingView;
            actionsExecDict[OeeActions.UPDATE_OEE_PARTIAL_VALUE] = whenOeeUpdated;
            actionsExecDict[SysActions.SET_TOP_MESSAGE] = doSetTopMessage;
            actionsExecDict[SysActions.APP_INIT_COMPLETED] = whenAppInitCompleted;

            Store.Subscribe(actionsExecDict);
            //每一分钟检查一次与服务器的连接
            Task.Run(() => {
                YUtil.SetInterval(60000, () => {
                    checkNetwork(HmiConfig.InfluxDbIp);
                });
            });
        }

        /// <summary>
        /// 程序初始化完成
        /// 包括配置文件初始化成功
        /// Mq消息监听成功
        /// Cpm服务启动成功
        /// Http服务启动成功
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenAppInitCompleted(AppState state, IAction action) {
            if (CmdOptions.GlobalOptions.MockVal) {
                foreach (var pair in MachineConfig.MachineDict) {
                    var machineCode = pair.Key;
                    //Mocks.MockDispatchers.DispatchMockMqEmpRfid(machineCode);
                    YUtil.SetTimeout(3000, () => {
                        Mocks.MockDispatchers.DispatchMockAlarm(33);
                    });

                    //YUtil.SetTimeout(6000, () => {
                    //    Mocks.MockDispatchers.DispatchMqMockScanMaterial(machineCode);
                    //});

                    //YUtil.SetTimeout(7000, () => {
                    //    Mocks.MockDispatchers.DispatchMockMqEmpRfid(machineCode, MqRfidType.EmpStartMachine);
                    //    YUtil.SetTimeout(15000, () => {
                    //        Mocks.MockDispatchers.DispatchMockMqEmpRfid(machineCode, MqRfidType.EmpEndMachine);
                    //    });
                    //});

                    YUtil.SetTimeout(3000, () => {
                        for (int i = 0; i < 5; i++) {
                            MockDispatchers.DispatchMockSchTask(machineCode, i);
                        }
                    });
                }
            }

            //启动完毕则检查更新
            if (!HmiConfig.IsDevUserEnv) {
                Task.Run(() => {
                    var sysService = UnityIocService.ResolveDepend<SysService>();
                    if (sysService.CheckUpdate()) {
                        sysService.StartUpdate();
                    }
                });
            }
        }

        /// <summary>
        /// 检查与某个ip的连接状况，并显示在window顶部
        /// </summary>
        /// <param name="ip"></param>
        void checkNetwork(string ip) {
            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(ip, 1000);
            if (reply.Status != IPStatus.Success) {
                Store.Dispatch(new SysActions.SetTopMessage(
                    $"与服务器 {ip} 连接断开，请联系管理员 {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}", Visibility.Visible));
            } else {
                if (TopMessage.Contains("连接断开")) {
                    Store.Dispatch(new SysActions.SetTopMessage("", Visibility.Collapsed));
                }
            }
        }


        /// <summary>
        /// 设置window顶部错误，消息，一般用来显示网络状况问题
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void doSetTopMessage(AppState state, IAction action) {
            var sysActionn = (SysActions.SetTopMessage)action;
            TopMessage = sysActionn.Message;
            TopMessageVisibility = sysActionn.Visibility;
        }

        /// <summary>
        /// 打印计算的Oee
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenOeeUpdated(AppState state, IAction action) {
            var oeeAction = (OeeActions.UpdateOeePartialValue)action;
            //Logger.Debug($@"Oee 时间效率 {oeeAction.TimeEff ?? 1}, 速度效率：{oeeAction.SpeedEff ?? 1}，质量效率：{oeeAction.QualityEff ?? 1}", ConsoleColor.Yellow);
        }

        /// <summary>
        /// 显示设置界面
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void doShowSettingView(AppState state, IAction action) {
            JumpAppSettingView("程序设置");
        }

        /// <summary>
        /// 显示通知消息
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void doShowNotification(AppState state, IAction action) {
            var msg = ((SysActions.ShowNotification)action).Message;
            //两次相同通知时间间隔秒数>=MinGapSec 才能显示
            //默认都显示
            var key = "Title: " + msg.Title + " Content: " + msg.Content;
            if (msg.MinGapSec.HasValue) {
                if (SysNotificationMsg.NotifyTimeDict.TryGetValue(key, out var lastTime)) {
                    if ((DateTime.Now - lastTime).TotalSeconds < msg.MinGapSec.Value) {
                        return;
                    }
                }
            }
            SysNotificationMsg.NotifyTimeDict[key] = DateTime.Now;
            //保存消息日志
            var logDetail = "Title: " + msg.Title + "\t Content: " + msg.Content;
            if (!string.IsNullOrEmpty(msg.LogDetail)) {
                logDetail = msg.LogDetail;
            }
            Logger.Notify(logDetail);
            DispatcherService.BeginInvoke(() => {
                INotification notification = NotifyNotificationService.CreatePredefinedNotification(msg.Title, msg.Content, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //Hmi没有播放声音设备
                if (HmiConfig.IsDevUserEnv) {
                    SystemSounds.Exclamation.Play();
                }
                notification.ShowAsync();
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

        public virtual IDispatcherService DispatcherService => GetService<IDispatcherService>();
        public virtual INotificationService NotifyNotificationService => GetService<INotificationService>();
        /// <summary>
        /// 导航服务，注册在 MainWindows.xaml中
        /// </summary>
        public INavigationService NavigationService { get { return GetService<INavigationService>(); } }


        /// <summary>
        /// 导航函数
        /// </summary>
        /// <param name="target"></param>
        public void Navigate(string target) {
            NavigationService.Navigate(target, null, this, true);
        }

        /// <summary>
        /// 程序启动后跳转到主页
        /// </summary>
        public void OnViewLoaded() {
            Navigate("HomeView");
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
