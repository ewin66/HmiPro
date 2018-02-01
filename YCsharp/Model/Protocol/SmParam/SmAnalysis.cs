using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YCsharp.Model.Buffers;
using YCsharp.Service;

namespace YCsharp.Model.Procotol.SmParam {
    /// <summary>
    /// 电科智联协议解析，使用时应传入 DynamicBuffer 作为协议数据缓存
    /// <date>2017-09-08</date>
    /// <author>ychost</author>
    /// </summary>
    public class SmAnalysis : IDisposable {
        private YDynamicBuffer socketBuffer;
        public readonly LoggerService Logger;
        public SmAnalysis(YDynamicBuffer buffer, LoggerService logger) {
            socketBuffer = buffer;
            Logger = logger;
        }

        /// <summary>
        /// 将buffer里面的数据流入解码栈进行解码操作，
        /// 这是对外的接口
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<SmModel> ThroughAnalysisStack(byte[] buffer, int offset, int count) {
            List<SmModel> dataList = new List<SmModel>();
            //从流中提取完整的包
            var packageDictionary = processBuffer2(buffer, offset, count);
            List<byte[]> paramPackages = packageDictionary[SmPackageType.ParamPackage];
            List<byte[]> heartbeatPackages = packageDictionary[SmPackageType.HeartbeatPackage];
            List<byte[]> replayCmdPackages = packageDictionary[SmPackageType.ClientReplyCmd];
            //采集参数包解码
            paramPackages.ForEach(p => {
                try {
                    SmModel data = processParamPackage(p, 0);
                    dataList.Add(data);
                    //打印错误包数据
                } catch (Exception e) {
                    var gap = 3600 * 24;
                    StringBuilder strB = new StringBuilder();
                    for (int i = 0; i < buffer.Length; i++) {
                        strB.Append("" + buffer[i].ToString("X2"));
                    }
                    Logger.Error("参数解码错误: bffer \r\n " + strB.ToString(), e, gap);
                }
            });
            //心跳包解码
            heartbeatPackages.ForEach(p => {
                SmModel data = processDecodeHeartbeatPackage(p, 0);
                dataList.Add(data);
            });
            //命令回复包
            replayCmdPackages.ForEach(p => {
                SmModel data = processDecodeClientReplyCmdPackage(p, 0);
                dataList.Add(data);
            });

            //删除起始帧之前的帧
            //减小内存开销
            socketBuffer.ClearUntilByte(0, (byte)SmFrame.Start);
            return dataList;
        }

        /// <summary>
        /// 缓存数据==>可用的包
        /// 对ProcessBuffer函数进行了极大的优化
        /// 目前效率 250次循环 61ms 50个单元单元测试包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private Dictionary<SmPackageType, List<byte[]>> processBuffer2(byte[] buffer, int offset, int count) {
            //解密
            buffer = SmEncrypt.DecodeArray(buffer, offset, count);

            Dictionary<SmPackageType, List<byte[]>> packageDictionary = new Dictionary<SmPackageType, List<byte[]>>();
            try {
                //fixed: 2018-01-04
                // 某些机台这里会抛异常
                socketBuffer.WriteBuffer(buffer, offset, count);
            } catch (Exception e) {
                //Console.WriteLine("DynamicBuffer WriteBuffer 异常" + e.Message);
                Logger.Error($"DynamicBuffer WriteBuffer 异常: DataCount: {socketBuffer.GetDataCount()},BufferSize: {socketBuffer.BufferSize}", e);
                //清空缓存
                socketBuffer.Clear();
            }
            List<byte[]> normalPackages = new List<byte[]>();
            List<byte[]> heartbeatPackages = new List<byte[]>();
            List<byte[]> clientReplyCmdPackages = new List<byte[]>();
            SmPackageType smPackageType = SmPackageType.ErrorPackage;
            int startFrameCount = 0;
            for (int i = offset; i < offset + socketBuffer.GetDataCount(); ++i) {
                //检验帧头
                if (socketBuffer[i] == (byte)SmFrame.Start) {
                    ++startFrameCount;
                    //找到长度帧
                    byte[] datalenBytes = { socketBuffer[i + (int)SmIndex.TotalLenStart], socketBuffer[(int)SmIndex.TotalLenStart + 1 + i] };
                    datalenBytes = datalenBytes.Reverse().ToArray();
                    //数据长度（该位后面（不包括结束符,CRC）的长度）
                    Int16 propLen = BitConverter.ToInt16(datalenBytes, 0);
                    //整个包长度
                    int packageLen =
                        +(int)SmIndex.TotalLenStart
                        + (int)SmIndex.TotalLenCount
                        + propLen
                        + 3;
                    int index = i + packageLen - 1;
                    //加上长度是否为结束帧
                    if (propLen > 0 && index < socketBuffer.GetDataCount() &&
                        socketBuffer[index] == (byte)SmFrame.End) {
                        //获取包类型
                        smPackageType = SmPackage.GetPackageType(socketBuffer.Buffer, i, packageLen);
                        if (smPackageType != SmPackageType.ErrorPackage) {
                            byte[] bytes = copyPackageAndClearOrigin2(ref i, packageLen);
                            if (smPackageType == SmPackageType.ParamPackage) {
                                normalPackages.Add(bytes);
                            } else if (smPackageType == SmPackageType.HeartbeatPackage) {
                                heartbeatPackages.Add(bytes);
                            } else if (smPackageType == SmPackageType.ClientReplyCmd) {
                                clientReplyCmdPackages.Add(bytes);
                            }
                        }
                    } else {
                        ////缓存中出现了超过1000个字节
                        ////全删掉
                        if (startFrameCount >= 1000) {
                            //socketBuffer.Clear(0, i);
                            Console.WriteLine("缓存中超过1000个坏包");
                        }
                    }
                }
            }
            //存包
            packageDictionary[SmPackageType.ParamPackage] = normalPackages;
            packageDictionary[SmPackageType.HeartbeatPackage] = heartbeatPackages;
            packageDictionary[SmPackageType.ClientReplyCmd] = clientReplyCmdPackages;
            return packageDictionary;
        }

        /// <summary>
        /// 提取包处理的公共部分
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private byte[] copyPackageAndClearOrigin2(ref int offset, int count) {
            byte[] bytes = new byte[count];
            Buffer.BlockCopy(socketBuffer.Buffer, offset, bytes, 0, count);
            socketBuffer.Clear(offset, count);
            --offset;
            return bytes;
        }

        /// <summary>
        /// 包解码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private SmModel processParamPackage(byte[] buffer, int offset) {
            List<byte> byList = buffer.ToList();
            byList = byList.GetRange(offset, byList.Count);
            SmModel emSocket = processPackage(buffer, offset, SmPackageType.ParamPackage);

            //第一个数据长度起始位置
            //为传感器地址
            int cursor = (int)SmIndex.PropLength + offset;
            emSocket.SmParams = new List<SmParam>();
            while (cursor < (byList.Count - 3)) {
                SmParam emSocketNormal = new SmParam();
                //属性域长度
                var propLen = byList[cursor];
                //数据域长度
                var dataLen = propLen - 4;
                //传感器地址
                var sensorAddrBytes = byList.GetRange(cursor + 1, 2).ToArray().Reverse().ToArray();
                var sensorAddr = BitConverter.ToInt16(sensorAddrBytes, 0);
                emSocketNormal.ParamCode = sensorAddr;
                //类型
                var type = byList[cursor + 3];
                emSocketNormal.DataType = type;
                //小数位
                var floatPlace = byList[cursor + 4];
                emSocketNormal.FloatPlace = floatPlace;
                //数据
                emSocketNormal.Data = byList.GetRange(cursor + 5, dataLen).ToArray();
                //指向下一个属性长度位置
                emSocket.SmParams.Add(emSocketNormal);
                cursor += propLen + 1;
            }
            return emSocket;
        }

        /// <summary>
        /// 心跳包进行处理
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private SmModel processDecodeHeartbeatPackage(byte[] buffer, int offset) {
            var emSocket = processPackage(buffer, offset, SmPackageType.HeartbeatPackage);
            return emSocket;
        }

        /// <summary>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private SmModel processDecodeClientReplyCmdPackage(byte[] buffer, int offset) {
            var emSocket = processPackage(buffer, offset, SmPackageType.ClientReplyCmd);
            return emSocket;
        }

        /// <summary>
        /// 所有包都能提取的信息
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private SmModel processPackage(byte[] buffer, int offset, SmPackageType type) {
            List<byte> byList = buffer.ToList();
            byList = byList.GetRange(offset, byList.Count);
            SmModel emSocket = new SmModel(type);
            //解码
            emSocket.ModuleAddr = byList.GetRange(SmTool.GetSocketIndex(SmIndex.MachineAddrStart), (int)SmIndex.MacineAddrCount);
            emSocket.Cmd = byList[SmTool.GetSocketIndex(SmIndex.Cmd)];
            emSocket.AimType = byList[SmTool.GetSocketIndex(SmIndex.AimType)];
            return emSocket;
        }
        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose() {
            socketBuffer = null;
        }
    }

    /// <summary>
    /// 基础工具类
    /// <date>2017-07-14</date>
    /// <author>ychost</author>
    /// </summary>
    internal static class SmTool {
        public static int GetSocketIndex(SmIndex index, int offset = 0) {
            return (int)index + offset;
        }
    }
}
