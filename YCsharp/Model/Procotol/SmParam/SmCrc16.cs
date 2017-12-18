using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace YCsharp.Model.Procotol.SmParam {
    /// <summary>
    /// 电科智联协议Crc16计算
    /// <date>2017-09-08</date>
    /// <author>ychost</author>
    /// </summary>
    public static class SmCrc16 {
        private static readonly int[] crcTables = new[] {
            0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401, 0xA001, 0x6C00, 0x7800, 0xB401, 0x5000,
            0x9C01, 0x8801, 0x4400,
        };
        /// <summary>
        /// crc计算,移植与C语言
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int CrcCalc(byte[] buffer, int offset, int count) {
            int crc;
            int i;
            int bufferByte;
            int hi;
            int li;
            crc = 0xFFFF;
            for (i = offset; i < offset + count; i++) {
                bufferByte = buffer[i];
                crc = crcTables[(bufferByte ^ crc) & 15] ^ (crc >> 4);
                crc = crcTables[((bufferByte >> 4) ^ crc) & 15] ^ (crc >> 4);
            }
            hi = crc % 256;
            li = crc / 256;
            crc = (hi << 8) | li;
            return crc;
        }
    }
}
