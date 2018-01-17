using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YCsharp.Util {
    /// <summary>
    /// 对电脑的操作部分，关机，注销，关闭显示器等等
    /// 都是调用其它exe文件
    /// </summary>
    public static partial class YUtil {
        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);
        [DllImport("user32")]
        public static extern void LockWorkStation();


        /// <summary>
        /// 关机
        /// </summary>
        public static void ShutDown() {
            try {
                System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-s -t 00");
                System.Diagnostics.Process.Start(startinfo);
            } catch { }
        }
        /// <summary>
        /// 重启
        /// </summary>
        public static void Restart() {
            try {
                System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-r -t 00");
                System.Diagnostics.Process.Start(startinfo);
            } catch { }
        }
        /// <summary>
        /// 注销
        /// </summary>
        public static void LogOff() {
            try {
                ExitWindowsEx(0, 0);
            } catch { }
        }
        /// <summary>
        /// 锁屏
        /// </summary>
        public static void LockPC() {
            try {
                LockWorkStation();
            } catch { }
        }



        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="exePath">接受命令的可执行文件</param>
        /// <param name="cmd">命令</param>
        public static void Exec(string exePath, string cmd) {
            try {
                System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo(exePath, cmd);
                System.Diagnostics.Process.Start(startinfo);
            } catch {
                Console.WriteLine("执行命令 " + exePath + " " + cmd + " 异常");
            }
        }
        /// <summary>
        /// 显示虚拟键盘
        /// </summary>
        public static void CallOskAsync() {
            Task.Run(() => {
                Exec(@"osk.exe", "");
            });
        }
        /// <summary>
        /// 通过 NirCmd调用的方式关闭显示器
        /// </summary>
        /// <param name="nirCmdPath"></param>
        public static void CloseScreenByNirCmd(string nirCmdPath) {
            if (YUtil.GetOsVersion().Contains(Windows10)) {
                Console.WriteLine("暂不支持win10的关闭屏幕操作");
                return;
            }
            var task = Task.Run(() => {
                Exec(nirCmdPath, "monitor off");
            });
            //1秒超时，之前有卡死的bug，目前这样修复
            task.Wait(1000);
        }

        /// <summary>
        /// 通过NirCmd调用的方式打开显示器
        /// </summary>
        /// <param name="nirCmdPath"></param>
        public static void OpenScreenByNirCmmd(string nirCmdPath) {
            if (YUtil.GetOsVersion().Contains(Windows10)) {
                Console.WriteLine("暂不支持win10的开启屏幕操作");
                return;
            }
            var task = Task.Run(() => {
                Exec(nirCmdPath, "monitor on");
            });
            //1秒超时，之前有卡死的bug，目前这样修复
            task.Wait(1000);
        }
    }
}
