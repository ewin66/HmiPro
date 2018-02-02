using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Redux.Models;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Helpers {
    /// <summary>
    /// 日志服务创建辅助类
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public static class LoggerHelper {
        /// <summary>
        /// 日志文件夹
        /// </summary>
        static string logFolder = null;

        /// <summary>
        /// 初始化日志文件夹
        /// </summary>
        /// <param name="logFolder">文件夹</param>
        public static void Init(string logFolder) {
            LoggerHelper.logFolder = logFolder;
        }

        /// <summary>
        /// 创建一个日志记录对象
        /// </summary>
        /// <param name="defaultLocation">一般为GetType().ToString() 主要为了定位日志所在的类</param>
        /// <returns>新生成的日志对象</returns>
        public static LoggerService CreateLogger(string defaultLocation = "") {
            if (string.IsNullOrEmpty(logFolder)) {
                throw new Exception("请先初始化 LoggerHelper.Init(folder)");
            }
            return new LoggerService(logFolder) { DefaultLocation = defaultLocation.Split('_')[0] };
        }
    }
}
