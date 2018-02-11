using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asylum.Event;
using Asylum.Helpers;
using Newtonsoft.Json;
using YCsharp.Service;

namespace Asylum.Services {
    /// <summary>
    /// 管道服务，接受 Hmi 发过来的数据
    /// <author>ychost</author>
    /// <date>2018-2-10</date>
    /// </summary>
    public class PipeService {
        /// <summary>
        /// 管道服务
        /// </summary>
        private NamedPipeServerStream pipeServer;
        /// <summary>
        /// 日志
        /// </summary>
        public LoggerService Logger;
        /// <summary>
        /// 管道名称
        /// </summary>
        private readonly string pipeName;

        /// <summary>
        /// 注入管道名称
        /// </summary>
        /// <param name="pipeName"></param>
        public PipeService(string pipeName) {
            this.pipeName = pipeName;
            Logger = LoggerHelper.Create(GetType().ToString());
        }

        /// <summary>
        /// 启动管道服务
        /// </summary>
        public bool Start() {
            try {
                pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                pipeServer.BeginWaitForConnection(waitForConnectionCallBack, pipeServer);
                return true;
            } catch (Exception e) {
                Logger.Error("开启管道服务失败", e);
            }
            return false;
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
                App.EventStore.Dispatch(new PipeReceived() { Data = json });
                //一定要先端口连接
                server.Disconnect();
                //再连接
                server.BeginWaitForConnection(waitForConnectionCallBack, server);
            } catch (Exception e) {
                Logger.Error("管道数据处理错误", e);
                if (pipeServer.IsConnected) {
                    pipeServer.Disconnect();
                }
                pipeServer.BeginWaitForConnection(waitForConnectionCallBack, pipeServer);
            }
        }
    }
}
