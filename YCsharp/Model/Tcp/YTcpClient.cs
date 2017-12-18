using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YCsharp.Model.Tcp {

    /// <summary>
    /// 连接服务器状态
    /// </summary>
    public enum YTcpClientState {
        //等待连接
        WaitConnecting = 0,
        //已连接
        Connected = 1,
        //丢失连接
        DisConnect = 2,
        //错误
        Error = 3,
        //发送数据成功
        SendSuccess = 4,
        //发送数据失败
        SendFaild = 5,
        //读取数据成功
        ReadSuccess = 6,
        //读取数据失败
        ReadFaild = 7,
        //关闭
        Close = 8
    }


    /// <summary>
    /// tcp客户端封装
    /// <date>2017-09-28</date>
    /// <author>ychost</author>
    /// </summary>
    public class YTcpClient : IDisposable {
        private TcpClient tcpClient;
        public Action<YTcpClientState> OnStateChanged;
        public event Action<byte[]> OnDataReceived;

        private string ip;
        private int port;

        /// <summary>
        /// 当发送数据失败，自动重连
        /// </summary>
        public bool AutoReConnectWhenSendFaild;

        private YTcpClientState clientState;
        public YTcpClientState ClientState {
            get => this.clientState;
            private set {
                this.clientState = value;
                this.OnStateChanged?.Invoke(value);
            }
        }


        public YTcpClient() {
            this.tcpClient = new TcpClient();
        }
        public YTcpClient(string ip, int port) {
            this.tcpClient = new TcpClient();
            this.ip = ip;
            this.port = port;
        }

        /// <summary>
        /// 设置要连接的服务器的信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void ConnectToServer(string ip, int port) {
            this.ip = ip;
            this.port = port;
            this.ReConnect();
        }

        /// <summary>
        /// 连接服务器回调
        /// </summary>
        /// <param name="ar"></param>
        private void connectCallback(IAsyncResult ar) {
            TcpClient t = (TcpClient)ar.AsyncState;
            try {
                if (t.Connected) {
                    t.EndConnect(ar); //函数运行到这里就说明连接成功
                    this.ClientState = YTcpClientState.Connected;
                    this.beginReadBytes();
                } else {
                    this.ClientState = YTcpClientState.DisConnect;
                }
            } catch (Exception e) {
                this.ClientState = YTcpClientState.DisConnect;
            }
        }

        /// <summary>
        /// 发送数据给服务器
        /// </summary>
        /// <param name="bytes"></param>
        public void SendBytes(byte[] bytes) {
            try {
                tcpClient.GetStream().BeginWrite(bytes, 0, bytes.Length, new AsyncCallback(sendBytesCallback),
                    null);
            } catch (Exception e) {
                if (this.AutoReConnectWhenSendFaild) {
                    this.ReConnect();
                }
                this.ClientState = YTcpClientState.DisConnect;
            }

        }

        /// <summary>
        /// 读取服务器发来的数据
        /// </summary>
        private void beginReadBytes() {
            if (tcpClient == null || !tcpClient.Connected) {
                return;
            }
            //接收字符串
            try {
                lock (tcpClient) {
                    StateObject state = new StateObject();
                    state.TcpClient = tcpClient;
                    var stream = tcpClient.GetStream();
                    if (stream.CanRead) {
                        stream.BeginRead(state.Buffer, 0, state.BufferSize, new AsyncCallback(readCallback),
                            state); //异步接受服务器回报的字符串
                    } else {
                        Console.WriteLine("无法从流中获取数据");
                    }
                }
            } catch (Exception e) {
                this.OnStateChanged?.Invoke(YTcpClientState.ReadFaild);
            }

        }

        /// <summary>
        /// 从流中读取数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void readCallback(IAsyncResult ar) {
            try {
                var state = (StateObject)ar.AsyncState;
                if (state.TcpClient == null || (!state.TcpClient.Connected)) {
                    return;
                }
                lock (tcpClient) {
                    NetworkStream stream = tcpClient.GetStream();
                    var byteLen = stream.EndRead(ar);
                    if (byteLen > 0) {
                        byte[] buffer = new byte[byteLen];
                        Array.Copy(state.Buffer, 0, buffer, 0, byteLen);
                        OnDataReceived?.Invoke(buffer);
                        stream.BeginRead(state.Buffer, 0, state.BufferSize, new AsyncCallback(readCallback),
                            state); //异步接受服务器回报的字符串
                    } else {
                        stream.Close();
                        state.TcpClient.Close();
                    }

                }
            } catch {
                this.Close();
            }
        }

        /// <summary>
        /// 发送数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void sendBytesCallback(IAsyncResult ar) {
            ClientState = YTcpClientState.SendSuccess;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose() {
            this.Close();
            ((IDisposable)tcpClient)?.Dispose();
        }

        /// <summary>
        /// 开始连接
        /// </summary>
        public void BeginConnect() {
            this.ClientState = YTcpClientState.WaitConnecting;
            tcpClient.BeginConnect(ip, port, new AsyncCallback(connectCallback), tcpClient);
        }

        /// <summary>
        /// 重连
        /// </summary>
        public void ReConnect() {
            this.Close();
            this.ClientState = YTcpClientState.WaitConnecting;
            this.tcpClient = new TcpClient();
            tcpClient.BeginConnect(ip, port, new AsyncCallback(connectCallback), tcpClient);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close() {
            this.tcpClient?.Close();
            this.tcpClient = null;
            this.ClientState = YTcpClientState.Close;
            GC.Collect();
        }
    }

    internal class StateObject {
        public TcpClient TcpClient { get; set; }
        public int BufferSize { get; set; } = 2048 * 1000;
        public byte[] Buffer { get; set; }

        public StateObject() {
            Buffer = new byte[BufferSize];
        }
    }
}