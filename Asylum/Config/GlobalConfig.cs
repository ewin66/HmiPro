using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Util;

namespace Asylum.Config {
    /// <summary>
    /// 一些全局的配置
    /// <author>ychost</author>
    /// <date>2018-2-9</date>
    /// </summary>
    public static class GlobalConfig {
        public static StartupArgs StartupArgs;

        public static readonly string HmiProcessName = "HmiPro";

        public static bool IsDevEnv => YUtil.GetWindowsUserName().ToLower().Contains("ychost");

    }
}
