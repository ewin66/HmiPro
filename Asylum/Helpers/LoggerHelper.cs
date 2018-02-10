using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Service;

namespace Asylum.Helpers {
    /// <summary>
    /// 日志服务创建
    /// <author>ychost</author>
    /// <date>2018-2-9</date>
    /// </summary>
    public class LoggerHelper {

        public static readonly string LogFolder = @"C:\Asylum\Logs\";

        /// <summary>
        /// 创建日志服务
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static LoggerService Create(string location = "") {
            return new LoggerService(LogFolder) { DefaultLocation = location };
        }
    }
}
