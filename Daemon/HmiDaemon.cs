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
using System.Threading.Tasks;
using System.Timers;
using Daemon.Models;
using Newtonsoft.Json;
using YCsharp.Service;
using YCsharp.Util;

namespace Daemon {
    public partial class HmiDaemon : ServiceBase {

        private static readonly string logPath = "C:\\HmiPro\\Log\\Daemon\\";
        private static readonly string pipeName = "HmiDaemon";
        private NamedPipeServerStream pipeServer;
        private readonly int pipeInBufferSize = 4096;
        private readonly int pipeOutBufferSize = 65535;
        public readonly LoggerService Logger;
        private readonly Encoding encoding = Encoding.UTF8;
        private DateTime hmiLastRunTime = DateTime.MinValue;
        private Timer keepHmiRunningTimer;
        /// <summary>
        /// Hmi 程序往管道写入的周期
        /// </summary>
        private readonly TimeSpan hmiWriteInterval = TimeSpan.FromMilliseconds(60000);

        public HmiDaemon() {
            InitializeComponent();
            Logger = new LoggerService(logPath) { DefaultLocation = "HmiDaemon" };
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args) {
            pipeServer = new NamedPipeServerStream
            (
                pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                pipeInBufferSize,
                pipeOutBufferSize
            );
            pipeServer.BeginWaitForConnection(waitForConnectionCallback, pipeServer);
            keepHmiRunningTimer = YUtil.SetInterval(hmiWriteInterval.TotalMilliseconds * 4, keepHmiRunning);
            Logger.Info("Hmi Daemon 启动完毕");
        }
        /// <summary>
        /// 读取管道内容回调
        /// </summary>
        /// <param name="ar"></param>
        private void waitForConnectionCallback(IAsyncResult ar) {
            var server = (NamedPipeServerStream)ar.AsyncState;
            server.EndWaitForConnection(ar);
            var data = new byte[pipeInBufferSize];
            var count = server.Read(data, 0, pipeInBufferSize);
            if (count > 0) {
                // 通信双方可以约定好传输内容的形式，例子中我们传输简单文本信息。
                string json = encoding.GetString(data, 0, count);
                var rest = JsonConvert.DeserializeObject<PipeRest>(json);
                hmiLastRunTime = rest.WriteTime;
                //心跳
                if (rest.DataType == PipeDataType.HeartBeat) {

                }
                Logger.Info("Hmi 最后运行时间：" + rest.WriteTime);

            }
            server.BeginWaitForConnection(waitForConnectionCallback, pipeServer);
        }

        /// <summary>
        /// 关闭任务
        /// </summary>
        protected override void OnStop() {
            pipeServer.Dispose();
            Logger.Info("Hmi Daemon 停止");
            YUtil.ClearTimeout(keepHmiRunningTimer);
            keepHmiRunningTimer = null;
        }

        /// <summary>
        /// 保证Hmi程序正常运行
        /// 如果检查到Hmi挂了，则重新启动其程序
        /// </summary>
        private void keepHmiRunning() {
            //超过4次待在没收到消息
            //则认为程序已死
            if ((DateTime.Now - hmiLastRunTime) > hmiWriteInterval.Add(hmiWriteInterval).Add(hmiWriteInterval).Add(hmiWriteInterval)) {
                Logger.Error("Hmi 程序已经挂掉，准备重启程序");
            }
        }


    }
}
