using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace HmiPro.Config {
    /// <summary>
    /// 终端命令配置解析
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    [Verb("start")]
    public class CmdOptions {
        [Option(longName: "console", Default = "true", HelpText = "使用console终端")]
        public string ShowConsole { get; set; }
        [Option(longName: "splash", Default = "true", HelpText = "显示 splash 画面")]
        public string ShowSplash { get; set; }
        [Option(longName: "autostart", Default = "false", HelpText = "开机自启")]
        public string AutoSatrt { get; set; }
        [Option(longName: "config", Default = @".\Profiles\Prod\", HelpText = "指定配置文件夹")]
        public string ConfigFolder { get; set; }
        [Option(longName: "sqlite", Default = @"C:\HmiPro\Store.db", HelpText = "Sqlite 文件地址")]
        public string SqlitePath { get; set; }
    }

}
