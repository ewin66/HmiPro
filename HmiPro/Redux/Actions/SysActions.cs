using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
