using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using NeoSmart.AsyncLock;
using YCsharp.Model.Procotol.SmParam;
using YCsharp.Model.Tcp;
using YCsharp.Util;


namespace YCsharp.Model.Procotol {
    /// <summary>
    /// 电科智联协议在tcp上面的封装
    /// 对外的接口
    /// <date>2017-09-10</date>
    /// <author>ychost</author>
    /// </summary>
    public class YSmParamTcp {
        private readonly YTcpServer tcpServer;
        public readonly SmClientManager SmClientManager;
        //ip,数据
        public Action<string, List<SmModel>> OnDataReceivedAction;

        //保存每个报警ip对应的客户端信息
        public readonly IDictionary<string, YTcpSrvClientState> ActionClientDict;

        //由于并不是每次发送命令都成功，所以这里做一个命令缓冲区，定时执行扫描任务
        //执行成功则置位
        public readonly IDictionary<string, SmAction> ActionCache;
        public readonly AsyncLock ActionLock = new AsyncLock();

        //上述缓存的扫描定时器
        public Timer ScanActionTimer;

        //ip连接
        public Action<string> OnConnectedAction;

        //ip断连
        public Action<string> OnDisConnectedAction;

        public bool isRunning { private set; get; } = false;


        public YSmParamTcp(string ip, int port) {
            tcpServer = new YTcpServer(ip, port);
            SmClientManager = new SmClientManager();
            tcpServer.DataReceived += onDataReceived;
            tcpServer.ClientConnected += onConnected;
            tcpServer.ClientDisconnected += onDisConnected;
            ActionClientDict = new ConcurrentDictionary<string, YTcpSrvClientState>();
            ActionCache = new ConcurrentDictionary<string, SmAction>();
        }

        /// <summary>
        /// 启动，软停止和硬停止都可用
        /// </summary>
        public void Start() {
            if (!isRunning) {
                isRunning = true;
                tcpServer.Start();
                //每两秒扫描一次命令缓冲区
                ScanActionTimer = YUtil.SetInterval(2000, () => {
                    using (ActionLock.Lock()) {
                        //自动执行任务
                        var canClear = true;
                        foreach (var pair in ActionCache) {
                            if (pair.Value != SmAction.NoAction && ActionClientDict.ContainsKey(pair.Key)) {
                                try {
                                    var state = ActionClientDict[pair.Key];
                                    tcpServer.Send(state, SmParamApi.BuildAlarmPackage(state.ModuleAddr, pair.Value));
                                    Console.WriteLine($"发送命令 {Enum.GetName(typeof(SmAction), pair.Value)} 成功 {pair.Key}");
                                    ActionCache[pair.Key] = SmAction.NoAction;
                                } catch {
                                    canClear = false;
                                    Console.WriteLine($"发送命令 {Enum.GetName(typeof(SmAction), pair.Value)} 异常 {pair.Key}");
                                }
                            }
                        }
                        if (canClear) {
                            YUtil.ClearTimeout(ScanActionTimer);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 逻辑上的停止，推荐使用
        /// </summary>
        public void StopSoft() {
            isRunning = false;
            //停止对命令缓冲的扫描
            YUtil.ClearTimeout(ScanActionTimer);
            ScanActionTimer = null;
        }

        /// <summary>
        /// 关闭Tcp从而停止，不推荐使用
        /// </summary>
        public void StopHard() {
            isRunning = false;
            tcpServer.Stop();
            //停止对命令缓冲的扫描
            YUtil.ClearTimeout(ScanActionTimer);
            ScanActionTimer = null;
        }
        /// <summary>
        /// 接受到数据并处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onDataReceived(object sender, YTcpSrvEventArgs e) {
            if (!isRunning) {
                return;
            }

            var ip = e.State.TcpClientIP;
            if (e.State.BufferCount >= e.State.Buffer.Length) {
                e.State.BufferCount = 0;
            }
            var reBuffer = e.State.Buffer;
            var reCount = e.State.BufferCount;

            //取得ip对应的缓存区
            if (!SmClientManager.IPSessionBuffer.ContainsKey(ip)) {
                SmClientManager.IPSessionBuffer[ip] = SmClientManager.DefaultBuffer();
            }

            List<SmModel> smModels = new List<SmModel>();

            //解析套接字数据
            using (var analysis = new SmAnalysis(SmClientManager.IPSessionBuffer[ip])) {
                smModels = analysis.ThroughAnalysisStack(reBuffer, 0, reCount);
                if (smModels?.Count == 0) {
                    Console.WriteLine($"{ip} 包解析失败");
                }
                smModels.ForEach(sm => {
                    //按协议应给客户端回复
                    if (sm.PackageType == SmPackageType.ParamPackage || sm.PackageType == SmPackageType.HeartbeatPackage) {
                        var replayPkg = SmParamApi.BuildParamPackage((byte)(sm.Cmd + 0x80), null, 2, sm.ModuleAddr);
                        try {
                            tcpServer.Send(e.State.TcpClient, replayPkg);
                        } catch {
                            Console.WriteLine($"回复客户端：{ip} 异常");
                        }
                    }
                });
                //设置模块地址
                if (smModels.Count > 0 && e.State.ModuleAddr == null) {
                    e.State.ModuleAddr = smModels[0].ModuleAddr;
                }
            }
            if (smModels.Count > 0) {
                OnDataReceivedAction?.Invoke(ip, smModels);
            }
        }


        /// <summary>
        /// 向某个命令客户端发送报警指令
        /// </summary>
        /// <param name="ip"></param>
        public void OpenAlarm(string ip) {
            SendAction(ip, SmAction.AlarmOpen);
        }

        /// <summary>
        /// 向某个命令客户端发送关闭报警
        /// </summary>
        /// <param name="ip"></param>
        public void CloseAlarm(string ip) {
            SendAction(ip, SmAction.AlarmClose);
        }

        /// <summary>
        /// 向某个ip发送命令，这里的ip必须是底层可以执行动作的ip
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="action"></param>
        public void SendAction(string ip, SmAction action) {
            using (ActionLock.Lock()) {
                if (ActionClientDict.TryGetValue(ip, out var state)) {
                    try {
                        tcpServer.Send(state, SmParamApi.BuildAlarmPackage(state.ModuleAddr, action));
                        Console.WriteLine($"发送命令 {Enum.GetName(typeof(SmAction), action)} 成功 {ip}");
                        ActionCache[ip] = SmAction.NoAction;
                    } catch {
                        ActionCache[ip] = action;
                        YUtil.RecoveryTimeout(ScanActionTimer);
                    }
                }
            }
        }

        /// <summary>
        /// 客户端连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onConnected(object sender, YTcpSrvEventArgs e) {
            if (!isRunning) {
                return;
            }
            //管理可报警ip客户端
            //以100结尾的ip都可以收命令
            if (e.State.TcpClientIP.EndsWith("100")) {
                ActionClientDict[e.State.TcpClientIP] = e.State;
            }
            SmClientManager.TpClientStates = ((YTcpServer)sender).Clicents;
            OnConnectedAction?.Invoke(e.State.TcpClientIP);
        }

        /// <summary>
        /// 客户端断连
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onDisConnected(object sender, YTcpSrvEventArgs e) {
            if (!isRunning) {
                return;
            }
            using (ActionLock.Lock()) {
                try {
                    if (ActionClientDict.ContainsKey(e.State.TcpClientIP)) {
                        ActionClientDict.Remove(e.State.TcpClientIP);
                    }
                } catch (Exception ex) {
                    Console.WriteLine("移除命令客户端异常" + ex);
                }
            }
            OnDisConnectedAction?.Invoke(e.State.TcpClientIP);
        }
    }
}
