using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>(() => new State() { IsScreenLight = true })
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
                 YUtil.OpenScreen();
                 state.IsScreenLight = true;
                 return state;
             }).When<SysActions.CloseScreen>((state, action) => {
                 YUtil.CloseScreen();
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
             });
        }
    }
}
