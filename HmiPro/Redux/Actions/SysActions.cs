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

        public struct ShowSettingView : IAction {
            public string Type() => SHOW_SETTING_VIEW;
        }

        public struct ShutdownApp : IAction {
            public string Type() => SHUTDOWN_APP;
        }
    }
}
