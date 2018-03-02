using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WindowsInput;
using DevExpress.Xpf.Core;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Properties;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Services;
using HmiPro.Views.Dx;
using Reducto;
using YCsharp.Util;
using Application = System.Windows.Application;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// 系统功能状态维护
    /// <author>ychost</author>
    /// <date>2017-12-19</date>
    /// </summary>
    public static class SysReducer {
        /// <summary>
        /// 
        /// </summary>
        public struct State {
            /// <summary>
            /// Http系统是否已经启动
            /// </summary>
            public bool HttpSystemIsStarted;
            /// <summary>
            /// 屏幕是否亮着
            /// </summary>
            public bool IsScreenLight;
            /// <summary>
            /// 是否启动周期息屏
            /// </summary>
            public bool IsStartOpenScreenTimer;
            /// <summary>
            /// 系统通知消息
            /// </summary>
            public SysNotificationMsg NotificationMsg;

            /// <summary>
            /// 用来表示消息的Id，就能在 Add 之后 在其它地方获到 Remove 需要的 Message
            /// </summary>
            public IDictionary<string, string> MarqueeMessagesDict { get; set; }

            public LoadingWindow LoadingWindow;
        }



        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>(() => new State() { IsScreenLight = true, MarqueeMessagesDict = new SortedDictionary<string, string>(), LoadingWindow = new LoadingWindow() { Topmost = true, WindowStartupLocation = WindowStartupLocation.CenterOwner } })
             .When<SysActions.StartHttpSystemSuccess>((state, action) => {
                 state.HttpSystemIsStarted = true;
                 return state;
             }).When<SysActions.StartHttpSystemFailed>((state, action) => {
                 state.HttpSystemIsStarted = false;
                 return state;
             }).When<SysActions.StartHttpSystem>((state, action) => {
                 state.HttpSystemIsStarted = false;
                 return state;
             }).When<SysActions.OpenScreen>((state, action) => {
                 YUtil.OpenScreen(AssetsHelper.GetAssets().ExeNirCmd);
                 state.IsScreenLight = true;
                 return state;
             }).When<SysActions.CloseScreen>((state, action) => {
                 YUtil.CloseScreen(AssetsHelper.GetAssets().ExeNirCmd);
                 state.IsScreenLight = false;
                 return state;
             }).When<SysActions.StartCloseScreenTimer>((state, action) => {
                 if (state.IsStartOpenScreenTimer) {
                     throw new Exception("请勿重复启动息屏定时器");
                 }
                 state.IsStartOpenScreenTimer = true;
                 return state;
             }).When<SysActions.StopCloseScreenTimer>((state, action) => {
                 state.IsStartOpenScreenTimer = false;
                 return state;
             }).When<SysActions.ShowNotification>((state, action) => {
                 state.NotificationMsg = action.Message;
                 return state;
             }).When<SysActions.ShutdownApp>((state, action) => {
                 App.Shutdown();
                 return state;
             }).When<SysActions.RestartApp>((state, action) => {
                 ActiveMqHelper.GetActiveMqService().Close();
                 App.Restart();
                 return state;
             }).When<SysActions.HideDesktop>((state, action) => {
                 YUtil.Exec(AssetsHelper.GetAssets().ExeNirCmd, "win hide class progman ");
                 return state;
             }).When<SysActions.ShowDesktop>((state, action) => {
                 YUtil.Exec(AssetsHelper.GetAssets().ExeNirCmd, "win show class progman ");
                 return state;
             }).When<SysActions.ReturnDesktop>((state, action) => {
                 //通过快捷键的方式来显示桌面
                 //http://inputsimulator.codeplex.com/
                 InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_D);
                 return state;
             }).When<SysActions.HideTaskBar>((state, action) => {
                 YUtil.HideTaskBar();
                 return state;
             }).When<SysActions.ShowTaskBar>((state, action) => {
                 YUtil.ShowTaskBar();
                 return state;
             }).When<SysActions.CloseLoadingSplash>((state, action) => {
                 Application.Current.Dispatcher.Invoke(() => {
                     try {
                         //DXSplashScreen.Close();
                         //state.LoadingWindow.Hide();
                     } catch {
                         Console.WriteLine("隐藏加载框失败");
                     }
                 });
                 return state;
             }).When<SysActions.ShowLoadingSplash>((state, action) => {
                 Application.Current.Dispatcher.Invoke(() => {
                     try {
                         //DXSplashScreen.Show<LoadingWindow>();
                         //state.LoadingWindow.Show();
                     } catch {
                         Console.WriteLine(Resources.Show_Loading_View_Failed);
                     }
                 });
                 return state;
             });
        }
    }
}
