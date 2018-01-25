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
        /// <summary>
        /// 程序的资源路径，主要使用了里面的图片资源
        /// </summary>
        public virtual Assets Assets { get; set; } = AssetsHelper.GetAssets();
        /// <summary>
        /// HomeView 是由 DxWindow 导航而来，所以这里的 NavigationService 可以直接使用
        /// </summary>
        public virtual INavigationService NavigationSerivce => null;
        /// <summary>
        /// 日志
        /// </summary>
        public readonly LoggerService Logger;
        /// <summary>
        /// 「设置」菜单目前在生产环境是隐藏了的，现在采用的是约定的方式，所以隐藏「设置」，但是开发环境还是打开了的，方便配置
        /// </summary>
        public Visibility SettingViewVisibility { get; set; } = Visibility.Collapsed;
        /// <summary>
        /// UI 线程调度器
        /// </summary>
        public virtual IDispatcherService DispatcherService => null;

        /// <summary>
        /// 决定「设置」菜单是否显示
        /// </summary>
        public HomeViewModel() {
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            if (HmiConfig.IsDevUserEnv) {
                SettingViewVisibility = Visibility.Visible;
            }
        }

        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {

        }

        /// <summary>
        /// 页面跳转命令
        /// </summary>
        /// <param name="viewName">页面名称，比如页面为HomeView.xaml，则名称为HomeView</param>
        [Command(Name = "NavigateCommand")]
        public void Navigate(string viewName) {
            if (viewName == "DMesCoreView") {
                var vm = DMesCoreViewModel.Create(App.Store.GetState().ViewStoreState.NavView.DMesSelectedMachineCode);
                NavigationSerivce.Navigate("DMesCoreView", vm, null, this, true);
            } else {
                NavigationSerivce.Navigate(viewName, null, this, true);
            }
        }

        /// <summary>
        /// 显示「设置」Modal 界面
        /// </summary>
        [Command(Name = "JumpAppSettingViewCommand")]
        public void JumpAppSetting() {
            App.Store.Dispatch(new SysActions.ShowSettingView());
        }
    }
}