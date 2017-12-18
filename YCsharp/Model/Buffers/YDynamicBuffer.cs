using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCsharp.Model.Buffers {
    /// <summary>
    /// 动态缓存，提升性能
    /// 处理协议缓存
    /// <date>2017-09-07</date>
    /// <author>ychost</author>
    /// </summary>
    public class YDynamicBuffer {
        public Byte[] Buffer { get; set; } //存放内存的数组
        public int DataCount { get; set; } //写入数据大小
        public int BufferSize { get; set; } //Buffer的上限


        public YDynamicBuffer(int bufferSize) {
            DataCount = 0;
            this.BufferSize = bufferSize;
            Buffer = new byte[bufferSize];

        }


        public int GetDataCount() //获得当前写入的字节数
        {
            return DataCount;
        }

        public int GetReserveCount() //获得剩余的字节数
        {
            return Buffer.Length - DataCount;
        }

        /// <summary>
        /// 清除指定的数据
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Clear(int offset, int count) {
            if (count + offset >= BufferSize) {
                DataCount = 0;
            } else {
                //移位
                for (int i = offset; i < DataCount - count + offset; ++i) {
                    Buffer[i] = Buffer[count + i];
                }
                DataCount = DataCount - count;
            }
        }



        /// <summary>
        /// 清空offset到指定字节的数据
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="by"></param>
        public void ClearUntilByte(int offset, byte by) {
            int count = 0;
            if (offset < DataCount) {
                for (int i = offset; i < DataCount; ++i) {
                    if (Buffer[i] == by) { break; } else { count++; }
                }
            }
            this.Clear(offset, count);
        }


        /// <summary>
        /// 写入buffer，常用
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void WriteBuffer(byte[] buffer, int offset, int count) {
            if (GetReserveCount() >= count) //缓冲区空间够，不需要申请
            {
                Array.Copy(buffer, offset, Buffer, DataCount, count);
                DataCount = DataCount + count;
            } else //缓冲区空间不够，需要申请更大的内存，并进行移位
            {
                int totalSize = Buffer.Length + count - GetReserveCount(); //总大小-空余大小
                byte[] tmpBuffer = new byte[totalSize];
                Array.Copy(Buffer, 0, tmpBuffer, 0, DataCount); //复制以前的数据
                Array.Copy(buffer, offset, tmpBuffer, DataCount, count); //复制新写入的数据
                DataCount = DataCount + count;
                Buffer = tmpBuffer; //替换
            }
        }

        /// <summary>
        /// 获取缓存，有效的缓存
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer() {
            return this.Buffer.ToList().GetRange(0, GetDataCount()).ToArray();
        }

        public void WriteBuffer(byte[] buffer) {
            WriteBuffer(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 写入短整形
        /// </summary>
        /// <param name="value"></param>
        /// <param name="convert"></param>
        public void WriteShort(short value, bool convert) {
            if (convert) {
                value = System.Net.IPAddress.HostToNetworkOrder(value); //NET是小头结构，网络字节是大头结构，需要客户端和服务器约定好
            }
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }
        /// <summary>
        /// 写入整形
        /// </summary>
        /// <param name="value"></param>
        /// <param name="convert"></param>
        public void WriteInt(int value, bool convert) {
            if (convert) {
                value = System.Net.IPAddress.HostToNetworkOrder(value); //NET是小头结构，网络字节是大头结构，需要客户端和服务器约定好
            }
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }
        /// <summary>
        /// 写入长整形
        /// </summary>
        /// <param name="value"></param>
        /// <param name="convert"></param>
        public void WriteLong(long value, bool convert) {
            if (convert) {
                value = System.Net.IPAddress.HostToNetworkOrder(value); //NET是小头结构，网络字节是大头结构，需要客户端和服务器约定好
            }
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }

        /// <summary>
        /// 以utf8写入字符串
        /// </summary>
        /// <param name="value"></param>
        public void WriteString(string value) //文本全部转成UTF8，UTF8兼容性好
        {
            byte[] tmpBuffer = Encoding.UTF8.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }

        /// <summary>
        /// 添加一个字节数据
        /// </summary>
        /// <param name="value"></param>
        public void Add(byte value) {
            this.Buffer[this.DataCount++] = value;
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte this[int index] => this.Buffer[index];
    }
}
