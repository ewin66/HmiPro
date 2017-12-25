using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YCsharp.Util;

namespace YCsharp.Service {
    /// <summary>
    /// 日志服务，可输出到文件
    /// <todo>输出到数据库</todo>
    /// <date>2017-09-30</date>
    /// <author>ychost</author>
    /// </summary>
    public class LoggerService {

        /// <summary>
        /// 日志文件夹
        /// </summary>
        readonly string logFolder;

        /// <summary>
        /// 输出到控制台？,总开关
        /// </summary>
        public bool OutConsole = true;
        /// <summary>
        /// 输出到文件？,总开关
        /// </summary>
        public bool OutFile = true;

        public string DefaultLocation = "";

        private bool logPathIsExist = false;

        /// <summary>
        /// 构造函数需要日志文件夹
        /// </summary>
        /// <param name="logFolder"></param>
        public LoggerService(string logFolder) {
            this.logFolder = logFolder + @"\";
        }

        /// <summary>
        /// 常用调试信息
        /// </summary>
        /// <param name="message">输出内容</param>
        /// <param name="outFile">是否输出到温度，默认是</param>
        /// <param name="consoleColor">Console输出的颜色，默认白色</param>
        public void Info(string message, bool outFile = true, ConsoleColor consoleColor = ConsoleColor.White) {
            var lineNum = YUtil.GetCurCodeLineNum(2);
            Console.ForegroundColor = consoleColor;
            Info(DefaultLocation + $"[{lineNum}]行", message, outFile);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// 带自定义位置的调试信息
        /// </summary>
        /// <param name="location">位置标识</param>
        /// <param name="message">输出内容</param>
        /// <param name="outFile">是否输出到文件</param>
        public void Info(string location, string message, bool outFile) {
            var content = createLogContent(location, message, "info");
            consoleOut(content);
            if (outFile) {
                fileOut(content, "info");
            }
        }

        /// <summary>
        /// 系统通知消息，会存档
        /// </summary>
        /// <param name="message"></param>
        /// <param name="outFile"></param>
        /// <param name="consoleColor"></param>
        public void Notify(string message, bool outFile = true, ConsoleColor consoleColor = ConsoleColor.White) {
            var lineNum = YUtil.GetCurCodeLineNum(2);
            Console.ForegroundColor = consoleColor;
            Notify(DefaultLocation + $"[{lineNum}]行", message, outFile);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// 系统通知消息
        /// </summary>
        /// <param name="location"></param>
        /// <param name="message"></param>
        /// <param name="outFile"></param>
        public void Notify(string location, string message, bool outFile) {
            var content = createLogContent(location, message, "notify");
            consoleOut(content);
            if (outFile) {
                fileOut(content, "notify");
            }
        }

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="location"></param>
        /// <param name="message"></param>
        public void Error(string location, string message) {
            var content = createLogContent(location, message, "error");
            Console.ForegroundColor = ConsoleColor.Red;
            consoleOut(content);
            Console.ForegroundColor = ConsoleColor.White;
            fileOut(content, "error");
        }

        public void Error(string location, string message, Exception e) {
            Error(location, $"{message} 原因：{e.Message}");
        }

        /// <summary>
        /// 常用api
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        public void Error(string message, Exception e) {
            var lineNum = YUtil.GetCurCodeLineNum(2);
            Error(DefaultLocation + $"[{lineNum}]行", $"{message} 原因：{e}");
        }
        /// <summary>
        /// 常用api
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message) {
            var lineNum = YUtil.GetCurCodeLineNum(2);
            Error(DefaultLocation + $"[{lineNum}]行", message);
        }

        /// <summary>
        /// 输出调试信息，只能在控制台输出
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public void Debug(string message, ConsoleColor color = ConsoleColor.Green) {
            var content = createLogContent(DefaultLocation, message, "debug");
            Console.ForegroundColor = color;
            consoleOut(content);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="location"></param>
        /// <param name="message"></param>
        public void Warn(string location, string message) {
            Warn(location, message, true);
        }

        public void Warn(string message, bool outFile) {
            Warn(DefaultLocation, message, outFile);
        }

        public void Warn(string location, string message, bool outFile) {
            var content = createLogContent(location, message, "warn");
            Console.ForegroundColor = ConsoleColor.Yellow;
            consoleOut(content);
            Console.ForegroundColor = ConsoleColor.White;
            if (outFile) {
                fileOut(content, "warn");
            }
        }

        /// <summary>
        /// 常用api
        /// </summary>
        /// <param name="message"></param>
        public void Warn(string message) {
            var lineNum = YUtil.GetCurCodeLineNum(2);
            Warn(DefaultLocation + $"[{lineNum}]行", message);
        }
        /// <summary>
        /// 控制台输出
        /// </summary>
        /// <param name="content"></param>
        void consoleOut(string content) {
            if (OutConsole) {
                Console.Write(content);
            }
        }

        public static readonly object FileOutLock = new object();

        /// <summary>
        /// 文件输出
        /// </summary> 
        /// <param name="content"></param>
        /// <param name="mark"></param>
        void fileOut(string content, string mark) {
            lock (FileOutLock) {
                //总开关
                if (OutFile) {
                    var path = buildLogFilePath(mark);
                    //日志文件夹不存在则创建
                    if (!logPathIsExist) {
                        var file = new FileInfo(path);
                        var di = file.Directory;
                        if (!di.Exists) {
                            di.Create();
                        }
                        logPathIsExist = true;
                    }
                    //往文件输出日志
                    using (FileStream logFile = new FileStream(path, FileMode.OpenOrCreate,
                        FileAccess.Write, FileShare.Write)) {
                        logFile.Seek(0, SeekOrigin.End);
                        var bytes = Encoding.Default.GetBytes(content);
                        logFile.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }


        /// <summary>
        /// 获取日志文件路径
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private string buildLogFilePath(string prefix) {
            var time = DateTime.Now.ToString("yyyyMM");
            return logFolder + $"{prefix}-{time}.log.txt";
        }

        /// <summary>
        /// 生成日志内容
        /// </summary>
        /// <param name="location"></param>
        /// <param name="message"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        string createLogContent(string location, string message, string mark) {
            return $"{DateTime.Now} [{mark}]: 线程：[{Thread.CurrentThread.ManagedThreadId.ToString("00")}]  位置：{location}  信息：{message}\r\n";
        }

    }
}
