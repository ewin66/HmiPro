using System;
using System.Collections.Generic;
using Asylum.Config;
using Asylum.Event;
using Asylum.Helpers;
using Asylum.Models;
using Newtonsoft.Json;
using YCsharp.Event.Models;
using YCsharp.Service;
using YCsharp.Util;
using Console = Colorful.Console;
namespace Asylum.Services {
    /// <summary>
    /// 处理所有派发的事件
    /// Asylum 的核心服务
    /// <author>ychost</author>
    /// <date>2018-2-10</date>
    /// </summary>
    public class AsylumService {
        /// <summary>
        /// 事件执行者
        /// </summary>
        public IDictionary<Type, EventHandler<YEventArgs>> eventHandlers;
        /// <summary>
        /// 日志
        /// </summary>
        public LoggerService Logger;
        /// <summary>
        /// HmiPro 最后活动的时间
        /// </summary>
        public DateTime HmiLastActiveTime;
        /// <summary>
        /// 防止多次注入
        /// </summary>
        public AsylumService() {
            UnityIocService.AssertIsFirstInject(GetType());
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init() {
            eventHandlers = new Dictionary<Type, EventHandler<YEventArgs>>();
            eventHandlers[typeof(PipeReceived)] = whenPipeReceived;
            App.EventStore.Subscribe(eventHandlers);
            //HmiPro 软件保活
            YUtil.SetInterval(60000, keepHmiAlive);
            Logger = LoggerHelper.Create(GetType().ToString());
        }

        /// <summary>
        /// 接受到管道数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void whenPipeReceived(object sender, YEventArgs args) {
            var json = (args.Payload as PipeReceived).Data;
            Cmd cmd = null;
            try {
                cmd = JsonConvert.DeserializeObject<Cmd>(json);
                if (cmd.SendTime.HasValue) {
                    HmiLastActiveTime = YUtil.UtcTimestampToLocalTime(cmd.SendTime.Value);
                    Logger.Debug("HmiPro 活动时间：" + HmiLastActiveTime);
                }
            } catch (Exception e) {
                Logger.Error("反序列化管道数据失败，数据：" + json, e);
                return;
            }
            cmd.Where = CmdWhere.FromPipe;
        }

        /// <summary>
        /// HmiPro 软件保活
        /// </summary>
        void keepHmiAlive() {
            Logger.Debug("HmiPro 保活");
            if ((DateTime.Now - HmiLastActiveTime).TotalMinutes > 3) {
                HmiLastActiveTime = DateTime.Now;
                var isExist = YUtil.CheckProcessIsExist(GlobalConfig.HmiProcessName);
                Logger.Error("HmiPro 已经无响应了，进程是否存在：" + isExist + "，最后活动时间：" + HmiLastActiveTime);
                YUtil.KillProcess(GlobalConfig.HmiProcessName);
                string startupArgs = string.Empty;
                if (GlobalConfig.IsDevEnv) {
                    startupArgs = @"--console false --autostart false --splash false --config office --hmi DE_DF --mock true";
                }
                Logger.Debug("HmiPro.exe 路径："+GlobalConfig.StartupArgs.HmiProPath +" "+startupArgs);
                YUtil.Exec(GlobalConfig.StartupArgs.HmiProPath, startupArgs);
            }
        }
    }
}
