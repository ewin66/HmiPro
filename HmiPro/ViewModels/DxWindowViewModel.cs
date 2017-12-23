using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
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
using YCsharp.Util;

namespace HmiPro.ViewModels {
    public class DxWindowViewModel : ViewModelBase {

        public readonly LoggerService Logger;

        public readonly StorePro<AppState> Store;
        public readonly IDictionary<string, Action<AppState, IAction>> actionsExecDict = new Dictionary<string, Action<AppState, IAction>>();


        public DxWindowViewModel() {
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            Store = UnityIocService.ResolveDepend<StorePro<AppState>>();
            actionsExecDict[SysActions.SHOW_NOTIFICATION] = doShowNotification;
            actionsExecDict[SysActions.SHOW_SETTING_VIEW] = doShowSettingView;
            actionsExecDict[OeeActions.UPDATE_OEE_PARTIAL_VALUE] = whenOeeUpdated;
            Store.Subscribe((state, aciton) => {
                if (actionsExecDict.TryGetValue(aciton.Type(), out var exec)) {
                    exec(state, aciton);
                }
            });
        }


        /// <summary>
        /// 打印计算的Oee
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenOeeUpdated(AppState state, IAction action) {
            var oeeAction = (OeeActions.UpdateOeePartialValue)action;
            Logger.Debug($@"Oee 时间效率 {oeeAction.TimeEff ?? -1},
                            速度效率：{oeeAction.SpeedEff ?? -1}，
                            质量效率：{oeeAction.QualityEff ?? -1}", ConsoleColor.Yellow);
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
            bool canNotify = true;
            if (msg.MinGapSec.HasValue) {
                var key = "Title: " + msg.Title + "\t Content: " + msg.Content;
                if (SysNotificationMsg.NotifyTimeDict.TryGetValue(key, out var lastTime)) {
                    if ((DateTime.Now - lastTime).TotalSeconds >= msg.MinGapSec.Value) {
                        canNotify = true;
                    } else {
                        canNotify = false;
                    }

                } else {
                    SysNotificationMsg.NotifyTimeDict[key] = DateTime.Now;
                    canNotify = true;
                }
            }
            if (canNotify) {
                //保存消息日志
                var logDetail = "Title: " + msg.Title + "\t Content: " + msg.Content;
                if (!string.IsNullOrEmpty(msg.LogDetail)) {
                    logDetail = msg.LogDetail;
                }
                Logger.Notify(logDetail);
                DispatcherService.BeginInvoke(() => {
                    INotification notification =
                        NotifyNotificationService.CreatePredefinedNotification(msg.Title, msg.Content,
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    SystemSounds.Exclamation.Play();
                    notification.ShowAsync();
                });
            }
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
            NavigationService.Navigate(target, null, this);
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
