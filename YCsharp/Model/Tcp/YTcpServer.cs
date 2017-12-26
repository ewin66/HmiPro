using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Service;

namespace YCsharp.Model.Tcp {

    /// <summary>
    /// Tcp 异步服务器
    /// <date>2017-09-07</date>
    /// <author>ychost</author>
    /// </summary>
    public class YTcpServer : IDisposable {

        #region Fields  

        /// <summary>  
        /// 服务器程序允许的最大客户端连接数  
        /// </summary>  
        private int maxClient;

        /// <summary>  
        /// 当前的连接的客户端数  
        /// </summary>  
        public int ClientCount { get; private set; }


        /// <summary>  
        /// 服务器使用的异步TcpListener  
        /// </summary>  
        private TcpListener listener;

        /// <summary>  
        /// 客户端会话列表  
        /// </summary>  
        private List<YTcpSrvClientState> clients;

        private bool disposed = false;

        #endregion

        #region Properties  

        /// <summary>  
        /// 服务器是否正在运行  
        /// </summary>  
        public bool IsRunning { get; private set; }

        /// <summary>  
        /// 监听的IP地址  
        /// </summary>  
        public IPAddress Address { get; private set; }

        /// <summary>  
        /// 监听的端口  
        /// </summary>  
        public int Port { get; private set; }

        /// <summary>  
        /// 通信使用的编码  
        /// </summary>  
        public Encoding Encoding { get; set; }

        public List<YTcpSrvClientState> Clicents => this.clients;

        #endregion

        #region 构造函数  

        public readonly LoggerService Logger;

        /// <summary>
        /// tcp服务需要ip和端口
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public YTcpServer(string ip, int port, LoggerService logger) {
            this.buildAsyncTCPServer(IPAddress.Parse(ip), port);
            Logger = logger;
        }


        /// <summary>  
        /// 异步TCP服务器  
        /// </summary>  
        /// <param name="localIPAddress">监听的IP地址</param>  
        /// <param name="listenPort">监听的端口</param>  
        private void buildAsyncTCPServer(IPAddress localIPAddress, int listenPort) {
            Address = localIPAddress;
            Port = listenPort;
            this.Encoding = Encoding.Default;
            clients = new List<YTcpSrvClientState>();
            listener = new TcpListener(Address, Port);
            listener.AllowNatTraversal(true);
        }

        #endregion

        #region Method  

        /// <summary>  
        /// 启动服务器   </summary>  
        public void Start() {
            if (!IsRunning) {
                IsRunning = true;
                listener.Start();

                listener.BeginAcceptTcpClient(
                    new AsyncCallback(HandleTcpClientAccepted), listener);
            }
        }


        /// <summary>  
        /// 启动服务器  
        /// </summary>  
        /// <param name="backlog">  
        /// 服务器所允许的挂起连接序列的最大长度  
        /// </param>  
        public void Start(int backlog) {
            if (!IsRunning) {
                IsRunning = true;
                listener.Start(backlog);
                listener.BeginAcceptTcpClient(
                    new AsyncCallback(HandleTcpClientAccepted), listener);
            }
        }

        /// <summary>  
        /// 停止服务器  
        /// </summary>  
        public void Stop() {
            if (IsRunning) {
                IsRunning = false;
                listener.Stop();
                //关闭所有客户端连接  
                CloseAllClient();
            }
        }

        /// <summary>  
        /// 处理客户端连接的函数  
        /// </summary>  
        /// <param name="ar"></param>  
        private void HandleTcpClientAccepted(IAsyncResult ar) {
            if (IsRunning) {

                try {
                    TcpClient client = listener.EndAcceptTcpClient(ar);
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    YTcpSrvClientState state
                        = new YTcpSrvClientState(client, buffer);
                    lock (clients) {
                        clients.Add(state);
                        RaiseClientConnected(state);
                    }


                    NetworkStream stream = state.NetworkStream;
                    //开始异步读取数据  
                    stream.BeginRead(state.Buffer, 0, state.Buffer.Length, HandleDataReceived, state);

                    listener.BeginAcceptTcpClient(
                        new AsyncCallback(HandleTcpClientAccepted), ar.AsyncState);
                } catch (Exception e) {

                }
            }
        }

        /// <summary>  
        /// 数据接受回调函数  
        /// </summary>  
        /// <param name="ar"></param>  
        private void HandleDataReceived(IAsyncResult ar) {
            if (IsRunning) {
                YTcpSrvClientState state = (YTcpSrvClientState)ar.AsyncState;
                NetworkStream stream = state.NetworkStream;
                int recv = 0;
                try {
                    recv = stream.EndRead(ar);
                } catch {
                    recv = 0;
                }

                if (recv == 0) {
                    // connection has been closed  
                    string ip = state.TcpClientIP;
                    clients.Remove(state);
                    //触发客户端连接断开事件  
                    RaiseClientDisconnected(state, ip);
                    return;
                }

                // received byte and trigger event notification  
                byte[] buff = new byte[recv];
                Buffer.BlockCopy(state.Buffer, 0, buff, 0, recv);
                state.BufferCount = recv;
                //触发数据收到事件  
                RaiseDataReceived(state);
                // continue listening for tcp datagram packets  
                try {
                    stream.BeginRead(state.Buffer, 0, state.Buffer.Length, HandleDataReceived, state);
                } catch (Exception e) {
                    Logger.Error("Tcp异常: ", e);
                }
            }
        }

        /// <summary>  
        /// 发送数据  
        /// </summary>  
        /// <param name="state">接收数据的客户端会话</param>  
        /// <param name="data">数据报文</param>  
        public void Send(YTcpSrvClientState state, byte[] data) {
            RaisePrepareSend(state);
            Send(state.TcpClient, data);
        }

        /// <summary>  
        /// 异步发送数据至指定的客户端  
        /// </summary>  
        /// <param name="client">客户端</param>  
        /// <param name="data">报文</param>  
        public void Send(TcpClient client, byte[] data) {
            if (!IsRunning)
                throw new InvalidProgramException("服务器未启动");

            if (client == null)
                throw new ArgumentNullException("client");

            if (data == null)
                throw new ArgumentNullException("data");
            client.GetStream().BeginWrite(data, 0, data.Length, SendDataEnd, client);
        }

        /// <summary>  
        /// 发送数据完成处理函数  
        /// </summary>  
        /// <param name="ar">目标客户端Socket</param>  
        private void SendDataEnd(IAsyncResult ar) {
            try {
                ((TcpClient)ar.AsyncState).GetStream().EndWrite(ar);
                RaiseCompletedSend(null);
            } catch (Exception e) {
                Console.WriteLine("[Tcp] 发送数据异常");
            }
        }

        #endregion

        #region 事件  

        /// <summary>  
        /// 与客户端的连接已建立事件  
        /// </summary>  
        public event EventHandler<YTcpSrvEventArgs> ClientConnected;

        /// <summary>  
        /// 与客户端的连接已断开事件  
        /// </summary>  
        public event EventHandler<YTcpSrvEventArgs> ClientDisconnected;


        /// <summary>  
        /// 触发客户端连接事件  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseClientConnected(YTcpSrvClientState state) {
            if (ClientConnected != null) {
                ClientConnected(this, new YTcpSrvEventArgs(state));
            }
        }

        /// <summary>  
        /// 触发客户端连接断开事件  
        /// </summary>  
        /// <param name="client"></param>  
        private void RaiseClientDisconnected(YTcpSrvClientState state, string ip) {
            if (ClientDisconnected != null) {
                ClientDisconnected(this, new YTcpSrvEventArgs(ip));
            }
        }

        /// <summary>  
        /// 接收到数据事件  
        /// </summary>  
        public event EventHandler<YTcpSrvEventArgs> DataReceived;

        private void RaiseDataReceived(YTcpSrvClientState state) {
            try {
                DataReceived?.Invoke(this, new YTcpSrvEventArgs(state));
            } catch (Exception e) {
                Logger.Error($"处理{state.TcpClientIP}的一包数据逻辑", e);
            }
        }

        /// <summary>  
        /// 发送数据前的事件  
        /// </summary>  
        public event EventHandler<YTcpSrvEventArgs> PrepareSend;

        /// <summary>  
        /// 触发发送数据前的事件  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaisePrepareSend(YTcpSrvClientState state) {
            if (PrepareSend != null) {
                PrepareSend(this, new YTcpSrvEventArgs(state));
            }
        }

        /// <summary>  
        /// 数据发送完毕事件  
        /// </summary>  
        public event EventHandler<YTcpSrvEventArgs> CompletedSend;

        /// <summary>  
        /// 触发数据发送完毕的事件  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseCompletedSend(YTcpSrvClientState state) {
            if (CompletedSend != null) {
                CompletedSend(this, new YTcpSrvEventArgs(state));
            }
        }

        /// <summary>  
        /// 网络错误事件  
        /// </summary>  
        public event EventHandler<YTcpSrvEventArgs> NetError;

        /// <summary>  
        /// 触发网络错误事件  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseNetError(YTcpSrvClientState state) {
            if (NetError != null) {
                NetError(this, new YTcpSrvEventArgs(state));
            }
        }

        /// <summary>  
        /// 异常事件  
        /// </summary>  
        public event EventHandler<YTcpSrvEventArgs> OtherException;

        /// <summary>  
        /// 触发异常事件  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseOtherException(YTcpSrvClientState state, string descrip) {
            if (OtherException != null) {
                OtherException(this, new YTcpSrvEventArgs(descrip, state));
            }
        }

        private void RaiseOtherException(YTcpSrvClientState state) {
            RaiseOtherException(state, "");
        }

        #endregion

        #region Close  

        /// <summary>  
        /// 关闭一个与客户端之间的会话  
        /// </summary>  
        /// <param name="state">需要关闭的客户端会话对象</param>  
        public void Close(YTcpSrvClientState state) {
            if (state != null) {
                state.Close();
                clients.Remove(state);
                ClientCount--;
                //TODO 触发关闭事件  
            }
        }

        /// <summary>  
        /// 关闭所有的客户端会话,与所有的客户端连接会断开  
        /// </summary>  
        public void CloseAllClient() {
            for (int i = 0; i < ClientCount; i++) {
                clients[i].Close();
            }
            ClientCount = 0;
            clients.Clear();
        }

        #endregion

        #region 释放  

        /// <summary>  
        /// Performs application-defined tasks associated with freeing,   
        /// releasing, or resetting unmanaged resources.  
        /// </summary>  
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>  
        /// Releases unmanaged and - optionally - managed resources  
        /// </summary>  
        /// <param name="disposing"><c>true</c> to release   
        /// both managed and unmanaged resources; <c>false</c>   
        /// to release only unmanaged resources.</param>  
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    try {
                        Stop();
                        if (listener != null) {
                            listener = null;
                        }
                    } catch (SocketException) {
                        //TODO  
                        RaiseOtherException(null);
                    }
                }
                disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// 客户端在线状态
    /// </summary>
    public enum YTcpSrvClientOnline {
        Oline = 0,
        Offline = 1,
        Other = 2
    }

    /// <summary>
    /// 客户端状态，含在线，ip，心跳时间等等
    /// </summary>
    public class YTcpSrvClientState {

        public List<byte> ModuleAddr;
        /// <summary>  
        /// 与客户端相关的TcpClient  
        /// </summary>  
        public TcpClient TcpClient { get; private set; }

        public string TcpClientIP => ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString();

        /// <summary>
        /// 客户端在线状态
        /// </summary>
        public YTcpSrvClientOnline SrvClientOnline { get; private set; }


        /// <summary>
        /// 最后心跳时间
        /// </summary>
        public DateTime LastHeartbeatTime { get; private set; }

        /// <summary>  
        /// 获取缓冲区  
        /// </summary>  
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Buffer的有效长度
        /// </summary>
        internal int BufferCount { get; set; }

        /// <summary>  
        /// 获取网络流  
        /// </summary>  
        internal NetworkStream NetworkStream => TcpClient.GetStream();

        public YTcpSrvClientState(TcpClient tcpClient, byte[] buffer) {
            this.LastHeartbeatTime = DateTime.Now;
            this.TcpClient = tcpClient ?? throw new ArgumentNullException("TcpClient");
            this.Buffer = buffer ?? throw new ArgumentNullException("Buffer");
            this.MarkClientOnline();
        }

        /// <summary>  
        /// 关闭  
        /// </summary>  
        public void Close() {
            //关闭数据的接受和发送  
            TcpClient.Close();
            Buffer = null;
        }

        /// <summary>
        /// 标记该客户端在线
        /// </summary>
        public void MarkClientOnline() {
            this.SrvClientOnline = YTcpSrvClientOnline.Oline;
            this.LastHeartbeatTime = DateTime.Now;
        }

        /// <summary>
        /// 标记客户端离线
        /// </summary>
        internal void MarkClientOffline() {
            this.SrvClientOnline = YTcpSrvClientOnline.Offline;
        }

    }

    public class YTcpSrvEventArgs : EventArgs {
        /// <summary>  
        /// 提示信息  
        /// </summary>  
        public string Msg;

        /// <summary>  
        /// 客户端状态封装类  
        /// </summary>  
        public YTcpSrvClientState State;


        /// <summary>  
        /// 是否已经处理过了  
        /// </summary>  
        public bool IsHandled { get; set; }

        public YTcpSrvEventArgs(string msg) {
            this.Msg = msg;
            IsHandled = false;
        }

        public YTcpSrvEventArgs(YTcpSrvClientState state) {
            this.State = state;
            IsHandled = false;
        }

        public YTcpSrvEventArgs(string msg, YTcpSrvClientState state) {
            this.Msg = msg;
            this.State = state;
            IsHandled = false;
        }

    }
}
