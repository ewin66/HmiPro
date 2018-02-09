using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Asylum.Config {
    /// <summary>
    /// 程序启动参数
    /// </summary>
    public class StartupArgs {
        /// <summary>
        /// HmiPro.exe 文件路径
        /// </summary>
        [Option(longName: "HmiPath", Default = "C:\\HmiPro\\Debug\\HmiPro.exe")]
        public string HmiProPath { get; set; }

        /// <summary>
        /// 是否开机自启
        /// </summary>
        [Option(longName: "autostart", Default = "True")]
        public string IsAutoStart { get; set; }
    }
}
