using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCsharp.Util {
    /// <summary>
    /// 扩展类，扩展 String 等类型
    /// <author>ychost</author>
    /// <date>2018-1-29</date>
    /// </summary>
    public static partial class YUtil {
        /// <summary>
        /// 用 restStr 来代替 string 中超过 maxChars 的部分
        /// </summary>
        /// <returns></returns>
        public static string Truncate(this string value, int maxChars, string restStr = "...") {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + restStr;
        }

    }
}
