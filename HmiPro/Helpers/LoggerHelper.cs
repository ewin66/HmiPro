using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using YCsharp.Service;

namespace HmiPro.Helpers {
    /// <summary>
    /// 日志服务创建辅助类
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public static class LoggerHelper {
        static string logFolder = null;
        public static void Init(string logFolder) {
            LoggerHelper.logFolder = logFolder;
        }
        public static LoggerService CreateLogger(string defaultLocation = "") {
            if (string.IsNullOrEmpty(logFolder)) {
                throw new Exception("请先初始化 LoggerHelper.Init(folder)");
            }
            return new LoggerService(logFolder) { DefaultLocation = defaultLocation.Split('_')[0] };
        }
    }
}
