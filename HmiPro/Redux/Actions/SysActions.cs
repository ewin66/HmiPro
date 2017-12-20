using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Xpf.WindowsUI;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 系统功能相关动作，配置都存在sqlite中的
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public static class SysActions {
        public static readonly string SHOW_SETTING_VIEW = "[Sys] Show Setting View";
        public static readonly string SHUTDOWN_APP = "[Sys] Shutdown App";
        public static readonly string START_HTTP_SYSTEM = "[Sys] Start Http System";
        public static readonly string START_HTTP_SYSTEM_SUCCESS = "[Sys] Start Http System Success";
        public static readonly string START_HTTP_SYSTEM_FAILED = "[Sys] Start Http System Failed";
        public static readonly string HTTP_SYSTEM_INVOKE = "[Sys] Http System Invoke";
        public static readonly string FIND_UPDATED_VERSION = "[Sys] Find Updated Version";
        public static readonly string NOT_FIND_UPDATED_VERSION = "[Sys] Not Find Updated Version";
        public static readonly string HMI_CONFIG_INITED = "[Sys] Hmi Config Has Configed";
        public static readonly string CLOSE_SCREEN = "[Sys] Close Screen";
        public static readonly string OPEN_SCREEN = "[Sys] Open Screen";
        public static readonly string START_CLOSE_SCREEN_TIMER = "[Sys] Start Turn Off Screen Timer";
        public static readonly string STOP_CLOSE_SCREEN_TIMER = "[Sys] Stop Turn Off Screen Timer";
        public static readonly string SHOW_NOTIFICATION = "[Sys] Show Notification";

        public struct ShowNotification : IAction {
            public string Type() => SHOW_NOTIFICATION;
            public SysNotificationMsg Message;

            public ShowNotification(SysNotificationMsg msg) {
                Message = msg;
            }
        }


        public struct StartCloseScreenTimer : IAction {
            public string Type() => START_CLOSE_SCREEN_TIMER;
            public double Interval;

            public StartCloseScreenTimer(double interval) {
                Interval = interval;

            }
        }

        public struct StopCloseScreenTimer : IAction {
            public string Type() => STOP_CLOSE_SCREEN_TIMER;
        }

        public struct ShowSettingView : IAction {
            public string Type() => SHOW_SETTING_VIEW;
        }

        public struct ShutdownApp : IAction {
            public string Type() => SHUTDOWN_APP;
        }

        public struct StartHttpSystem : IAction {
            public string Type() {
                return START_HTTP_SYSTEM;
            }

            public StartHttpSystem(string url) {
                Url = url;
            }

            public string Url;
        }

        public struct StartHttpSystemSuccess : IAction {
            public string Type() => START_HTTP_SYSTEM_SUCCESS;
        }

        public struct StartHttpSystemFailed : IAction {
            public string Type() => START_HTTP_SYSTEM_FAILED;
            public Exception e;
        }

        public struct FindUpdatedVersion : IAction {
            public string Type() => FIND_UPDATED_VERSION;
        }

        public struct NotFindUpdatedVersion : IAction {
            public string Type() => NOT_FIND_UPDATED_VERSION;
        }

        public struct HttpSystemInvoke : IAction {
            public string Type() => HTTP_SYSTEM_INVOKE;
            public string Cmd;
        }

        public struct HmiConfigInited : IAction {
            public string Type() => HMI_CONFIG_INITED;
        }

        public struct OpenScreen : IAction {
            public string Type() => OPEN_SCREEN;
        }
        public struct CloseScreen : IAction {
            public string Type() => CLOSE_SCREEN;
        }
    }
}
