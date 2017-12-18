using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YCsharp.Model.Buffers;
using YCsharp.Model.Tcp;


namespace YCsharp.Model.Procotol.SmParam {
    /// <summary>
    /// 客户端管理
    /// </summary>
    public class SmClientManager {
        /// <summary>
        /// 所有的客户端
        /// </summary>
        public List<YTcpSrvClientState> TpClientStates { get; internal set; }

        /// <summary>
        /// 以ip为key的缓存
        /// </summary>
        public IDictionary<string, YDynamicBuffer> IPSessionBuffer { get; private set; }

        public SmClientManager() {
            this.TpClientStates = new List<YTcpSrvClientState>();
            this.IPSessionBuffer = new ConcurrentDictionary<string, YDynamicBuffer>();
        }


        /// <summary>
        /// 默认大小的缓冲区
        /// </summary>
        /// <returns></returns>
        public YDynamicBuffer DefaultBuffer() {
            //数据缓存区
            //todo  有时间换成redis
            return new YDynamicBuffer(1024 * 1024);
        }
    }
}
