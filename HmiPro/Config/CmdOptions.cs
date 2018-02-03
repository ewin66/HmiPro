using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;

namespace HmiPro.Config {
    /// <summary>
    /// 终端命令配置解析
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    [Verb("start")]
    public class CmdOptions {
        /// <summary>
        /// --console true 显示终端
        /// </summary>
        [Option(longName: "console", Default = "true", HelpText = "使用console终端")]
        public string ShowConsole { get; set; }
        /// <summary>
        /// --splash true 显示启动 Splash
        /// </summary>
        [Option(longName: "splash", Default = "true", HelpText = "显示 splash 画面")]
        public string ShowSplash { get; set; }
        /// <summary>
        /// --autostart true 开机自启
        /// </summary>
        [Option(longName: "autostart", Default = "true", HelpText = "开机自启")]
        public string AutoSatrt { get; set; }
        /// <summary>
        /// --sqlite c:\HmiPro\Store.db sqlite文件路径
        /// </summary>
        [Option(longName: "sqlite", Default = @"C:\HmiPro\Store.db", HelpText = "Sqlite 文件地址")]
        public string SqlitePath { get; set; }
        /// <summary>
        /// --profiles .\Profiles\ 配置主文件夹
        /// </summary>
        [Option(longName: "profiles", Default = @".\Profiles", HelpText = "配置文件夹")]
        public string ProfilesFolder { get; set; }
        /// <summary>
        /// --mode dev Machines 文件夹路径
        /// </summary>
        [Option(longName: "mode", Default = @"Prod", HelpText = "Dev或者Prod模式，会自动寻找Profiles文件夹下面的Mode文件夹")]
        public string Mode { get; set; }
        /// <summary>
        /// --mock true 启用模拟数据
        /// </summary>
        [Option(longName: "mock", Default = "false", HelpText = "是否启用模拟数据")]
        public string Mock { get; set; }
        /// <summary>
        /// --config shop 使用 hmi.config.shop.json 配置文件
        /// </summary>
        [Option(longName: "config", Default = "Shop", HelpText = "指定 Hmi.Config.[value].json")]
        public string Config { get; set; }
        /// <summary>
        /// --hmi de_df 指定程序的机台位 DE、DF
        /// </summary>
        [Option(longName: "hmi", Default = "", HelpText = "指定Hmi名称如：DE_DF")]
        public string HmiName { get; set; }
        /// <summary>
        /// --wait 5 程序延迟启动 5 秒
        /// </summary>
        [Option(longName: "wait", Default = "0", HelpText = "程序延迟启动，单位 秒")]
        public string Wait { get; set; }

        /// <summary>
        /// 程序延迟启动秒数
        /// </summary>
        public int WaitSec => int.Parse(Wait);
        /// <summary>
        /// 是否启用模拟数据
        /// </summary>
        public bool MockVal => bool.Parse(Mock);
        /// <summary>
        /// 由 Config 参数默认解析的文件夹
        /// </summary>
        public string ConfigFolder;
        /// <summary>
        /// 解析后的命令
        /// </summary>
        public static CmdOptions GlobalOptions;
        /// <summary>
        /// 启动原始参数
        /// </summary>
        public static StartupEventArgs StartupEventArgs;
    }


}
