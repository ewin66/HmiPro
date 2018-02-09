using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Asylum.Config;
using Asylum.Services;
using CommandLine;
using YCsharp.Service;
using YCsharp.Util;
using Console = Colorful.Console;


namespace Asylum {
    /// <summary>
    /// HmiPro 的监控、诊断、修复程序
    /// 主要实现对 Hmi 的启动、关闭控制等
    /// <author>ychost</author>
    /// <date>2018-2-9</date>
    /// </summary>
    public class Program {
        static void Main(string[] args) {
            parseStartupArgs(args);
            Console.WriteAscii("Asylum");
            Console.WriteLine("启动中...");
            Init();
            YUtil.ExitWithQ();
        }

        /// <summary>
        /// 解析启动参数
        /// </summary>
        /// <param name="args"></param>
        static void parseStartupArgs(string[] args) {
            Parser.Default.ParseArguments<StartupArgs>(args).WithParsed(opt => {
                Console.WriteLine("当前版本："+YUtil.GetAppVersion(Assembly.GetExecutingAssembly()));

                bool autoStart = bool.Parse(opt.IsAutoStart);
                YUtil.SetAppAutoStart("Asylum", autoStart);
                Console.WriteLine("是否开机自启动：-" + autoStart);

                Console.WriteLine("HmiPro.exe 路径：" + opt.HmiProPath);

                GlobalConfig.StartupArgs = opt;
            }).WithNotParsed(err => {
                Console.WriteLine("启动命令解析错误");
                throw new Exception("启动命令解析错误");
            });
        }

        /// <summary>
        /// 初始化
        /// </summary>
        static void Init() {
            var cmdParse = new CmdParseService("HmiPro", GlobalConfig.StartupArgs.HmiProPath);
            UnityIocService.RegisterGlobalDepend(cmdParse);
            UnityIocService.RegisterGlobalDepend<HttpParse>();
            UnityIocService.ResolveDepend<HttpParse>().Start("http://+:9988/");
        }
    }
}
