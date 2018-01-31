using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace YCsharp.Model.Procotol.SmParam {
    /// <summary>
    /// 电科智联协议加密
    /// <date>2017-09-08</date>
    /// <author>ychost</author>
    /// </summary>
    public static class SmEncrypt {

        private static List<byte> encrptyTable;

        /// <summary>
        /// 构造函数初始化
        /// </summary>
        static SmEncrypt() {
            if (encrptyTable == null) {
                encrptyTable = new List<byte>(256);
                for (int i = 0; i < 256; ++i) {
                    encrptyTable.Add((byte)i);
                }
                //更新一次
                UpdateEncriptyTable();
            }
        }

        /// <summary>
        /// 设置加密表
        /// </summary>
        /// <param name="enList"></param>
        public static void SetEncryptTable(List<byte> enList) {
            if (enList.Count != 256) {
                throw new Exception("加密表有误");
            }
            encrptyTable = enList;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data">待加密数据</param>
        /// <returns></returns>
        public static byte Encode(byte data) {
            return encrptyTable[data];
        }

        /// <summary>
        /// 批量加密数据
        /// </summary>
        /// <param name="dataArray"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] EncodeArray(byte[] dataArray, int offset, int count) {
            for (int i = offset; i < count + offset; i++) {
                dataArray[i] = Encode(dataArray[i]);
            }
            return dataArray;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">待解密数据</param>
        /// <returns></returns>
        public static byte Decode(byte data) {
            for (int i = 0; i < encrptyTable.Count; i++) {
                if (encrptyTable[i] == data) {
                    return (byte)i;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取加密表
        /// </summary>
        /// <returns></returns>
        public static List<byte> GetEncryptTable() {
            return encrptyTable;
        }


        /// <summary>
        /// 批量解码
        /// </summary>
        /// <param name="dataArray"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] DecodeArray(byte[] dataArray, int offset, int count) {
            for (int i = offset; i < offset + count; ++i) {
                dataArray[i] = Decode(dataArray[i]);
            }
            return dataArray;
        }

        /// <summary>
        /// 更新加密表,简单的洗牌
        /// todo 真爱生命，远离加密
        /// </summary>
        public static void UpdateEncriptyTable() {

            //encrptyTable.ShuffleDynamicEncrypt(0x68, 0x69, 0x0d);
        }
    }
}
