using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YCsharp.Util {
    public static partial class YUtil {
        ///  
        /// 获取可用内存单位字节
        ///  
        public static long GetAvaliableMemoryByte() {
            long availablebytes = 0;
            ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
            foreach (ManagementObject mo in mos.GetInstances()) {
                if (mo["FreePhysicalMemory"] != null) {
                    availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                }
            }
            return availablebytes;
        }

        /// <summary>
        /// 获取本机所有的ip
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllIps() {
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            return ipadrlist.Select(ip => ip.ToString()).ToArray();
        }

        /// <summary>
        /// 关闭进程
        /// </summary>
        /// <param name="processName"></param>
        public static void KillProcess(string processName) {
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName(processName);
            foreach (System.Diagnostics.Process p in ps) {
                p.Kill();
            }
        }

        /// <summary>
        /// 检查某进程是否存在
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public static bool CheckProcessIsExist(string processName) {
            return System.Diagnostics.Process.GetProcessesByName(processName).Length > 0;
        }

        #region 任务栏显示/隐藏
        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        [DllImport("user32.dll")]
        public static extern int FindWindowEx(int parentHandle, int childAfter, string className, int windowTitle);

        [DllImport("user32.dll")]
        private static extern int GetDesktopWindow();

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;

        public static int Handle {
            get {
                return FindWindow("Shell_TrayWnd", "");
            }
        }

        public static int HandleOfStartButton {
            get {
                int handleOfDesktop = GetDesktopWindow();
                int handleOfStartButton = FindWindowEx(handleOfDesktop, 0, "button", 0);
                return handleOfStartButton;
            }
        }

        /// <summary>
        /// 显示任务栏
        /// </summary>
        public static void ShowTaskBar() {
            ShowWindow(Handle, SW_SHOW);
            ShowWindow(HandleOfStartButton, SW_SHOW);
        }

        /// <summary>
        /// 隐藏任务栏
        /// </summary>
        public static void HideTaskBar() {
            ShowWindow(Handle, SW_HIDE);
            ShowWindow(HandleOfStartButton, SW_HIDE);
        }
        #endregion
    }
}
