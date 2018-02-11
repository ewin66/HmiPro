using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Asylum.Config;
using Asylum.Event;
using Asylum.Helpers;
using Asylum.Services;
using CommandLine;
using YCsharp.Event;
using YCsharp.Event.Models;
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
    public class App {
        /// <summary>
        /// 程序消息总线
        /// </summary>
        public static YEventStore EventStore;
        /// <summary>
        /// 全局可使用的日志
        /// </summary>
        public static LoggerService Logger;
        /// <summary>
        /// 初始化日志
        /// </summary>
        static App() {
            Logger = LoggerHelper.Create("Asylum");
            EventStore = new YEventStore();
        }

        /// <summary>
        /// 程序入口
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args) {
            Console.WriteAscii("Asylum");
            parseStartupArgs(args);
            Logger.Info("启动中...");
            var task = Init();
            Task.WaitAll(new Task[] { task });
            //启动失败
            if (!task.Result) {
                Logger.Error("启动失败");
                return;
            }
            YUtil.ExitWithQ();
        }

        /// <summary>
        /// 解析启动参数
        /// </summary>
        /// <param name="args"></param>
        static void parseStartupArgs(string[] args) {
            Parser.Default.ParseArguments<StartupArgs>(args).WithParsed(opt => {
                Logger.Debug("当前操作系统：-" + YUtil.GetOsVersion());
                Logger.Debug("当前版本：-" + YUtil.GetAppVersion(Assembly.GetExecutingAssembly()));

                bool autoStart = bool.Parse(opt.IsAutoStart);
                YUtil.SetAppAutoStart("Asylum", autoStart);
                Logger.Debug("是否开机自启动：-" + autoStart);

                Logger.Debug("HmiPro.exe 路径：-" + opt.HmiProPath);

                GlobalConfig.StartupArgs = opt;
            }).WithNotParsed(err => {
                Logger.Debug("启动命令解析错误");
                throw new Exception("启动命令解析错误");
            });
        }

        /// <summary>
        /// 初始化
        /// </summary>
        static async Task<bool> Init() {
            var cmdParse = new CmdParseService("HmiPro", GlobalConfig.StartupArgs.HmiProPath);
            UnityIocService.RegisterGlobalDepend(cmdParse);
            UnityIocService.RegisterGlobalDepend<HttpParse>();
            UnityIocService.RegisterGlobalDepend<AsylumService>();

            UnityIocService.ResolveDepend<AsylumService>().Init();
            if (!await UnityIocService.ResolveDepend<HttpParse>().Start("http://+:9988/")) {
                Logger.Error("Http 服务启动失败");
                return false;
            }
            var pipeService = new PipeService("Asylum");
            UnityIocService.RegisterGlobalDepend(pipeService);
            return pipeService.Start();
        }
    }
}
