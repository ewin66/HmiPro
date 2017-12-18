using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCsharp.Extension {
    /// <summary>
    /// List的一些扩展
    /// <date>2017-09-08</date>
    /// <author>ychost</author>
    /// </summary>
    public static class ListExt {
        private static readonly Random random = new Random();

        /// <summary>
        /// 对动态加密表进行洗牌
        /// 因为有些字符不能被加密
        /// </summary>
        /// <param name="list"></param>
        public static void ShuffleDynamicEncrypt(this IList<byte> list, params byte[] noEncryptBytes) {
            if (list.Count > 256) {
                throw new Exception("字节洗牌长度不能超过256");
            }
            int n = list.Count;
            while (n > 1) {
                n--;
                byte k = (byte)random.Next(n + 1);
                byte value = list[k];
                if (!noEncryptBytes.Contains(k) && !noEncryptBytes.Contains((byte)n)) {
                    list[k] = list[n];
                    list[n] = value;
                }
            }
        }
    }
}
