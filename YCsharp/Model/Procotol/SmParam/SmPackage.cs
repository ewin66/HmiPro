using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace YCsharp.Model.Procotol.SmParam {
    /// <summary>
    /// 电科智联协议包，二进制包，协议规则
    /// <date>2017-09-08</date>
    /// <author>ychost</author>
    /// </summary>
    public static class SmPackage {
        /// <summary>
        /// 判断缓存中的数据是什么包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        internal static SmPackageType GetPackageType(byte[] buffer, int offset, int count) {
            try {
                if (AsserIsPackage(buffer, offset, count)) {
                    byte cmdFrame = buffer[SmTool.GetSocketIndex(SmIndex.Cmd, offset)];
                    if (cmdFrame == (byte)SmCmdFrame.Param) {
                        return SmPackageType.ParamPackage;
                    } else if (cmdFrame == (byte)SmCmdFrame.Heartbeat) {
                        return SmPackageType.HeartbeatPackage;
                    } else if (cmdFrame == (byte)SmCmdFrame.ClientReplyCmd) {
                        return SmPackageType.ClientReplyCmd;
                    }
                }
            } catch {

            }
            return SmPackageType.ErrorPackage;
        }

        /// <summary>
        /// 判断是否为一普通个包，因为没有转义所以包内容可能和帧头，帧尾重复
        /// 一定要先运行AssertIsPackage
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        internal static bool AssertIsNormalPackage(byte[] buffer, int offset, int count, bool isPackage = false) {
            return AssertPackageType(buffer, offset, count, SmPackageType.ParamPackage, isPackage);
        }

        /// <summary>
        /// 判断包类型，这是个辅助方法
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="type"></param>
        /// <param name="isPackage"></param>
        /// <returns></returns>
        internal static bool AssertPackageType(byte[] buffer, int offset, int count, SmPackageType type, bool isPackage) {
            if (!isPackage) {
                if (!AsserIsPackage(buffer, offset, count)) {
                    return false;
                }
            }
            byte cmdFrame = buffer[SmTool.GetSocketIndex(SmIndex.Cmd, offset)];
            return cmdFrame == (byte)type;
        }


        /// <summary>
        /// 是否为客户端回复的命令包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="isPackage"></param>
        /// <returns></returns>
        internal static bool AssertIsClientReplyCmdPackage(byte[] buffer, int offset, int count, bool isPackage = false) {
            return AssertPackageType(buffer, offset, count, SmPackageType.ClientReplyCmd, isPackage);
        }

        /// <summary>
        /// 所有包协议一致，通过协议来检验是否为一个包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool AsserIsPackage(byte[] buffer, int offset, int count) {
            //帧头，帧尾，长度判断
            byte startFrame = buffer[offset];
            int endFrame = buffer[count + offset - 1];
            if (count < (int)SmSup.MinLength
                || (startFrame != (byte)SmFrame.Start)
                || (endFrame != (byte)SmFrame.End)) {
                return false;
            }
            List<byte> byList = buffer.ToList();
            //固定帧校验
            byte fixedFrame = byList[SmTool.GetSocketIndex(SmIndex.Fixed, offset)];
            if (fixedFrame != (byte)SmFrame.Fixed) {
                return false;
            }

            //crc校验
            int crsFrameStart = offset + count - 3;
            byte[] crcBytes = byList.GetRange(crsFrameStart, 2).ToArray().Reverse().ToArray();
            int crc = BitConverter.ToUInt16(crcBytes, 0);
            var crcCalc = SmCrc16.CrcCalc(buffer, offset, count - 3);
            if (crc != crcCalc) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检验是否为心跳包
        /// 一定要先运行AsserIsPackage
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        internal static bool AssertIsHeartbeatPackage(byte[] buffer, int offset, int count, bool isPackage = false) {
            if (!isPackage) {
                if (!AsserIsPackage(buffer, offset, count)) {
                    return false;
                }
            }
            byte cmdFrame = buffer[SmTool.GetSocketIndex(SmIndex.Cmd, offset)];
            if (cmdFrame == (byte)SmCmdFrame.Heartbeat) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 构建报警命令包
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="operate">可设置打开或者关闭报警</param>
        /// <param name="aimType"></param>
        /// <returns></returns>
        internal static byte[] BuildAlarmPackage(List<byte> addr, SmAction operate,
            byte aimType = (byte)SmFrame.IndustryAimType) {
            byte[] sendBytes = new byte[] { (byte)operate };
            return BuildActionPackage(addr, (byte)SmActionFrame.Alarm, sendBytes.ToList(), aimType);
        }


        /// <summary>
        /// 构建一个命令包
        /// </summary>
        /// <param name="addr">机台地址</param>
        /// <param name="cmd">命令</param>
        /// <param name="data">数据</param>
        /// <param name="aimType">类型，默认：工业类型</param>
        /// <returns></returns>
        internal static byte[] BuildActionPackage(List<byte> addr, byte cmd, List<byte> data, byte aimType = (byte)SmReplyFrame.IndustryAimType) {
            if (addr.Count != (int)SmReplyIndex.MachineAddrCount) {
                throw new Exception("module addr 长度不对");
            }
            //data可为空,即无数据
            if (data == null) {
                data = new List<byte>();
            }

            int packageLen = (int)SmReplySup.ExceptDataLength + data.Count;
            byte[] package = new byte[packageLen];
            //地址
            package[(int)SmReplyIndex.Start] = (byte)SmReplyFrame.Start;
            for (int i = 0; i < (int)SmReplyIndex.MachineAddrCount; ++i) {
                package[i + (int)SmReplyIndex.MachineAddrStart] = addr[i];
            }
            //固定帧
            package[(int)SmReplyIndex.Fixed] = (byte)SmReplyFrame.Fixed;
            //类型帧
            package[(int)SmReplyIndex.AimType] = aimType;
            //命令为接受的命令
            package[(int)SmReplyIndex.Cmd] = (byte)(cmd);
            //数据长度，高位在前
            byte[] dataLenBytes = BitConverter.GetBytes(data.Count);
            package[(int)SmReplyIndex.DataLengthStart] = dataLenBytes[1];
            package[(int)SmReplyIndex.DataLengthStart + 1] = dataLenBytes[0];
            //crc校验,高位在前
            var cursor = (int)SmReplyIndex.DataLengthStart + 2;
            //构建数据包
            if (data.Count > 0) {
                for (var i = 0; i < data.Count; i++) {
                    cursor += i;
                    package[cursor] = data[i];
                }
                cursor += 1;
            }
            //crc校验
            Int16 crc = (Int16)SmCrc16.CrcCalc(package, 0, packageLen - 3);
            byte[] crcBytes = BitConverter.GetBytes(crc);
            package[cursor++] = crcBytes[1];
            package[cursor++] = crcBytes[0];

            //结束帧
            package[cursor] = (byte)SmReplyFrame.End;
            //加密
            //package = SmEncrypt.EncodeArray(package, 0, package.Length);
            return package;

        }

        /// <summary>
        /// 构建发送的参数包，数据发生器会使用到
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="cmd"></param>
        /// <param name="sendData"></param>
        /// <param name="aimType"></param>
        /// <returns></returns>
        internal static byte[] BuildParamPackage(List<byte> addr, byte cmd, List<SmSend> sendData,
            byte aimType = (byte)SmFrame.IndustryAimType) {
            List<byte> package = new List<byte>();
            //起始
            package.Add((byte)SmFrame.Start);
            //地址
            package.AddRange(addr);
            //固定帧
            package.Add((byte)SmFrame.Fixed);
            //类型
            package.Add((byte)aimType);
            //命令
            package.Add(cmd);
            //总长度
            Int16 dataTotalLen = 0;
            //数据区域
            List<byte> dataArea = new List<byte>();
            sendData?.ForEach(data => {
                dataTotalLen += (Int16)data.ToPackageBytes().Length;
                dataArea.AddRange(data.ToPackageBytes());

            });
            //数据域总长度
            package.AddRange(BitConverter.GetBytes(dataTotalLen).Reverse());
            //数据域
            package.AddRange(dataArea);

            //crc校验
            var crc = BitConverter.GetBytes((Int16)SmCrc16.CrcCalc(package.ToArray(), 0, package.Count)).Reverse();
            package.AddRange(crc);
            //结束位
            package.Add((byte)SmFrame.End);
            return package.ToArray();
        }

        /// <summary>
        /// 构建动态加密包，此包要分三次发给嵌入式
        /// </summary>
        /// <returns></returns>
        public static List<byte[]> BuildEncryptPackages(List<byte> moduleAddr, bool isUpdateEncryptTable = false, byte aimType = (byte)SmEncryptFrame.AimType) {
            List<byte[]> packages = new List<byte[]>();
            if (moduleAddr.Count != (int)SmEncryptIndex.ModuleAddrCount) {
                throw new Exception("加密包module addr 长度不对");
            }
            //更新表
            if (isUpdateEncryptTable) {
                SmEncrypt.UpdateEncriptyTable();
            }
            List<byte> tables = SmEncrypt.GetEncryptTable();
            //对加密表进行切分
            //0-84
            List<byte> firstTable = tables.GetRange(0, 85);
            //85 - 169
            List<byte> secondTable = tables.GetRange(85, 85);
            //170-255
            List<byte> thirdTable = tables.GetRange(170, 86);
            //切分好的表添加到一个表中好遍历
            List<List<byte>> tableList = new List<List<byte>>();
            tableList.Add(firstTable);
            tableList.Add(secondTable);
            tableList.Add(thirdTable);
            int completedLen = 0;
            //将切分的包构建成package
            tableList.ForEach(data => {
                byte[] encryptPackage = buildEncryptPackage(data, moduleAddr, completedLen, aimType);
                completedLen += data.Count;
                packages.Add(encryptPackage);
            });
            return packages;
        }

        /// <summary>
        /// 构建加密表包，单个包
        /// </summary>
        /// <param name="data"></param>
        /// <param name="machineAddr"></param>
        /// <param name="completedLen"></param>
        /// <param name="aimType"></param>
        /// <returns></returns>
        private static byte[] buildEncryptPackage(List<byte> data, List<byte> machineAddr, int completedLen, byte aimType = (byte)SmEncryptFrame.AimType) {
            //加密包总长度
            byte[] encryptPackage = new byte[(int)SmEncryptSup.ExceptDataPackageLen + data.Count];
            encryptPackage[(int)SmEncryptIndex.Start] = (byte)SmEncryptFrame.Start;
            //构建machineAddr
            for (int i = 0; i < machineAddr.Count; ++i) {
                encryptPackage[i + 1] = machineAddr[i];
            }
            encryptPackage[(int)SmEncryptIndex.Fixed] = (byte)SmEncryptFrame.Fixed;
            encryptPackage[(int)SmEncryptIndex.AimType] = aimType;
            encryptPackage[(int)SmEncryptIndex.Cmd] = (byte)SmEncryptFrame.Cmd;

            //数据域长度+1字节（已完成长度）
            int dataLen = data.Count + 1;
            byte[] lenBytes = BitConverter.GetBytes(dataLen);
            encryptPackage[(int)SmEncryptIndex.DataLenStart] = lenBytes[1];
            encryptPackage[(int)SmEncryptIndex.DataLenStart + 1] = lenBytes[0];

            //已完成字节
            encryptPackage[(int)SmEncryptIndex.CompleteByte] = (byte)completedLen;
            //数据域
            for (int i = 0; i < data.Count; ++i) {
                encryptPackage[(int)SmEncryptIndex.DataStart + i] = data[i];
            }
            //crc校验
            Int16 crc = (Int16)SmCrc16.CrcCalc(encryptPackage, 0, (int)SmEncryptIndex.CompleteByte + data.Count + 1);
            byte[] crcBytes = BitConverter.GetBytes((Int16)crc).Reverse().ToArray();
            encryptPackage[encryptPackage.Length - 3] = crcBytes[0];
            encryptPackage[encryptPackage.Length - 2] = crcBytes[1];
            //结束帧
            encryptPackage[encryptPackage.Length - 1] = (byte)SmEncryptFrame.End;
            return encryptPackage;
        }
    }
}
