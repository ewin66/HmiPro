using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Daemon.Models;
using Newtonsoft.Json;
using YCsharp.Service;
using YCsharp.Util;
using Timer = System.Timers.Timer;

namespace Daemon {
    public partial class HmiDaemon : ServiceBase {

        private static readonly string logPath = "C:\\HmiPro\\Log\\Daemon\\";
        private static readonly string pipeName = "HmiDaemon";
        public readonly LoggerService Logger;
        private Timer keepHmiRunningTimer;
        private NamedPipeServerStream pipeServer;
        private readonly string hmiProcessName = "HmiPro";


        /// <summary>
        /// Hmi 程序往管道写入的周期
        /// </summary>
        private readonly int hmiWriteIntervalMs = 60000;

        public HmiDaemon() {
            InitializeComponent();
            Logger = new LoggerService(logPath) { DefaultLocation = "HmiDaemon" };
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args) {
            StartPipeServer();
            Logger.Info("--Hmi Daemon 启动完毕--");
        }

        /// <summary>
        /// 启动管道服务
        /// </summary>
        public void StartPipeServer() {
            try {
                pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                pipeServer.BeginWaitForConnection(waitForConnectionCallBack, pipeServer);
            } catch (Exception e) {
                Logger.Error("开启管道服务失败", e);
            }
        }

        /// <summary>
        /// 管道服务处理逻辑
        /// </summary>
        /// <param name="iar"></param>
        private void waitForConnectionCallBack(IAsyncResult iar) {
            try {
                NamedPipeServerStream server = (NamedPipeServerStream)iar.AsyncState;
                server.EndWaitForConnection(iar);
                byte[] buffer = new byte[65535];
                server.Read(buffer, 0, 65535);
                string json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                var rest = JsonConvert.DeserializeObject<PipeRest>(json);


                //一定要先端口连接
                server.Disconnect();
                //再连接
                server.BeginWaitForConnection(waitForConnectionCallBack, server);
            } catch (Exception e) {
                Logger.Error("管道错误", e);
            }
        }

        /// <summary>
        /// 关闭任务
        /// </summary>
        protected override void OnStop() {
            Logger.Info("--Hmi Daemon 停止--");
            YUtil.ClearTimeout(keepHmiRunningTimer);
            keepHmiRunningTimer = null;
        }

    }
}
