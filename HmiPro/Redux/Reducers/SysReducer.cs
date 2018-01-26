using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using Reducto;
using YCsharp.Util;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// 系统功能状态维护
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
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

        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>(() => new State() { IsScreenLight = true, MarqueeMessagesDict = new SortedDictionary<string, string>() })
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
                 YUtil.OpenScreenByNirCmmd(AssetsHelper.GetAssets().ExeNirCmd);
                 state.IsScreenLight = true;
                 return state;
             }).When<SysActions.CloseScreen>((state, action) => {
                 YUtil.CloseScreenByNirCmd(AssetsHelper.GetAssets().ExeNirCmd);
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
                 ActiveMqHelper.GetActiveMqService().Close();
                 Application.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Send);
                 return state;
             }).When<SysActions.RestartApp>((state, action) => {
                 ActiveMqHelper.GetActiveMqService().Close();
                 Application.Current.Dispatcher.Invoke(() => {
                     //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                     YUtil.Exec(Application.ResourceAssembly.Location, " --wait " + action.WaitSec);
                     Application.Current.Shutdown();
                 });
                 return state;
             });
        }
    }
}
