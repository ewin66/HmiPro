using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YCsharp.Util {
    /// <summary>
    /// 对电脑的操作部分，关机，注销，关闭显示器等等
    /// 都是调用其它exe文件
    /// </summary>
    public static partial class YUtil {

        #region 关机、重启、注销、锁屏

        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [DllImport("user32")]
        public static extern void LockWorkStation();


        /// <summary>
        /// 关机
        /// </summary>
        public static void ShutdownPc() {
            try {
                ProcessStartInfo startinfo = new ProcessStartInfo("shutdown.exe", "-s -t 00");
                Process.Start(startinfo);
            } catch {
            }
        }

        /// <summary>
        /// 重启
        /// </summary>
        public static void RestartPC() {
            try {
                ProcessStartInfo startinfo = new ProcessStartInfo("shutdown.exe", "-r -t 00");
                Process.Start(startinfo);
            } catch {
            }
        }

        /// <summary>
        /// 注销
        /// </summary>
        public static void LogOff() {
            try {
                ExitWindowsEx(0, 0);
            } catch {
            }
        }

        /// <summary>
        /// 锁屏
        /// </summary>
        public static void LockPC() {
            try {
                LockWorkStation();
            } catch {
            }
        }
        #endregion

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="exePath">接受命令的可执行文件</param>
        /// <param name="cmd">命令</param>
        public static void Exec(string exePath, string cmd, ProcessWindowStyle winStyle = ProcessWindowStyle.Normal) {
            try {
                ProcessStartInfo startinfo = new ProcessStartInfo(exePath, cmd);
                startinfo.WindowStyle = winStyle;
                Process.Start(startinfo);
            } catch {
                Console.WriteLine("执行命令 " + exePath + " " + cmd + " 异常");
            }
        }

        #region 弹出 windows 自带的软件
        /// <summary>
        /// 显示虚拟键盘
        /// </summary>
        public static void CallOskAsync() {
            Task.Run(() => {
                Exec(@"osk.exe", "");
            });
        }

        /// <summary>
        /// 弹出任务管理器
        /// </summary>
        public static void CallTaskMgrAsync() {
            Task.Run(() => {
                Exec("TaskMgr.exe", "");
            });
        }
        #endregion

        #region  显示/隐藏桌面
        public static void HideDesktop(string nirCmdPath) {
            Task.Run(() => {
                Exec(nirCmdPath, "win hide class progman");
            }).Wait(1000);
        }

        public static void ShowDesktop(string nirCmdPath) {
            Task.Run(() => {
                Exec(nirCmdPath, "win show class progman");
            }).Wait(1000);
        }
        #endregion

        #region 显示器开启/熄灭
        /// <summary>
        /// 通过 NirCmd调用的方式关闭显示器
        /// 暂时不支持 Win10
        /// </summary>
        /// <param name="nirCmdPath"></param>
        public static void CloseScreen(string nirCmdPath) {
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
        /// 暂时不支持 Win10
        /// </summary>
        /// <param name="nirCmdPath"></param>
        public static void OpenScreen(string nirCmdPath) {
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

        #endregion

        #region Windows Service 辅助
        /// <summary>
        /// 获取 Windows 服务状态
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static ServiceControllerStatus GetWinServiceStatus(string serviceName) {
            var service = ServiceController.GetServices();
            for (int i = 0; i < service.Length; i++) {
                if (service[i].ServiceName.ToUpper().Equals(serviceName.ToUpper())) {
                    return service[i].Status;
                }
            }
            return ServiceControllerStatus.Stopped;
        }

        /// <summary>
        /// 启动 Windows 服务
        /// </summary>
        /// <param name="serviceName"></param>
        public static void StartWinService(string serviceName) {
            if (!CheckServiceIsExist(serviceName)) {
                return;
            }
            using (ServiceController control = new ServiceController(serviceName)) {
                if (control.Status == ServiceControllerStatus.Stopped) {
                    control.Start();
                }
            }
        }

        /// <summary>
        /// 停止 Windows 服务
        /// </summary>
        /// <param name="serviceName"></param>
        public static void StopWinService(string serviceName) {
            if (!CheckServiceIsExist(serviceName)) {
                return;
            }
            using (ServiceController control = new ServiceController(serviceName)) {
                if (control.Status == System.ServiceProcess.ServiceControllerStatus.Running) {
                    control.Stop();
                }
            }
        }

        /// <summary>
        /// 获取 .net 目录
        /// </summary>
        /// <returns></returns>
        public static string GetDotNetFolder() {
            var frameworkName = "Framework" + (Environment.Is64BitOperatingSystem ? "x64" : "");
            return $@"C:\Windows\Microsoft.NET\{frameworkName}\v4.0.30319\";
        }


        /// <summary>
        /// 安装 Windows 服务
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="serviceName"></param>
        public static void InstallWinService(string servicePath, string serviceName) {
            var installUtil = $@"{GetDotNetFolder()}\InstallUtil.exe";
            //服务已经安装了
            if (CheckServiceIsExist(serviceName)) {
                return;
            }
            //执行安装操作
            YUtil.Exec(installUtil, servicePath);
        }

        /// <summary>
        /// 卸载 Windows 服务
        /// </summary>
        /// <param name="serviceName"></param>
        public static void UninstallWinService(string serviceName) {
            var installUtil = $@"{GetDotNetFolder()}\InstallUtil.exe";
            if (!CheckServiceIsExist(serviceName)) {
                return;
            }
            YUtil.Exec(installUtil, "/u " + serviceName);
        }

        /// <summary>
        /// 检查 Windows 服务是否存在
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool CheckServiceIsExist(string serviceName) {
            var service = ServiceController.GetServices();
            for (int i = 0; i < service.Length; i++) {
                //服务已经安装了，则忽略此次安装
                if (service[i].ServiceName.ToUpper().Equals(serviceName.ToUpper())) {
                    return true;
                }
            }
            return false;
        }
        #endregion


    }
}
