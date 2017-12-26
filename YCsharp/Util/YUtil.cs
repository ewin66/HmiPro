using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace YCsharp.Util {
    /// <summary>
    /// 未分类的工具
    /// <date>2017-09-27</date>
    /// <author>ychost</author>
    /// </summary>
    public static partial class YUtil {

        /// <summary>
        /// 获取系统环境变量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetEnvValue(string name) {
            return Environment.GetEnvironmentVariable(name);
        }

        /// <summary>
        /// 设置环境变量值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public static void SetEnvVal(string name, string val) {
            Environment.SetEnvironmentVariable(name, val, EnvironmentVariableTarget.Machine);
        }

        /// <summary>
        /// 获取目前windows登录用户名字
        /// </summary>
        /// <returns></returns>
        public static string GetWindowsUserName() {
            return Environment.UserName;
        }

        /// <summary>
        /// 设置程序开机自启
        /// </summary>
        /// <param name="regKey">每个程序都有一个唯一的key，如果key相同会替换之前的自启程序</param>
        /// <param name="useAutoStart">是否使用自启</param>
        public static void SetAppAutoStart(string regKey, bool useAutoStart) {
            string curExePath = Assembly.GetCallingAssembly().Location;
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (useAutoStart) {
                rk2.SetValue(regKey, curExePath);
            } else {
                rk2.DeleteValue(regKey, false);
            }
            rk2.Close();
            rk.Close();
        }

        /// <summary>
        /// 获取随机数发生器
        /// 使用闭包，可达到在循环中调用的时候获取不同的随机数
        /// </summary>
        /// <returns></returns>
        public static Func<double> GetRandomGen() {
            int i = 0;
            return () => {
                Random r = new Random(int.Parse(DateTime.Now.ToString("HHmmssfff")) + i++);
                return r.NextDouble();
            };
        }

        /// <summary>
        /// 产生指定范围的随机整数
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Func<int> GetRandomIntGen(int begin, int end) {
            int i = 0;
            return () => {
                Random r = new Random(int.Parse(DateTime.Now.ToString("HHmmssfff")) + i++);
                return r.Next(begin, end);
            };
        }

   


        /// <summary>
        /// 控制台专用
        /// </summary>
        public static void ExitWithQ() {
            Console.WriteLine("输入Q即可退出");
            while (Console.ReadKey().KeyChar != 'q') {
                continue;
            }
        }

        /// <summary>
        /// 获取程序版本号
        /// 默认[弃用]：程序版本[自动生成] ---> 主版本.次版本.编译日期[距离2000年的天数].编译时间[距离0:0:0的秒数/2]
        /// 程序版本 ---> 主版本.次版本.编译次数.编译日期
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetAppVersion(Assembly assembly) {
            Version version = assembly.GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(version.Revision);
            string displayableVersion = $"{version} ({buildDate.ToShortDateString()})";
            return displayableVersion;
        }

        /// <summary>
        /// 创建一个dynamic对象，含该对象的字典
        /// 这样可以向js一样使用c#的对象
        /// </summary>
        /// <returns></returns>
        public static (dynamic, IDictionary<string, object>) CreateDynamic() {
            dynamic expando = new System.Dynamic.ExpandoObject();
            var expandoDict = (IDictionary<string, object>)expando;
            return (expando, expandoDict);
        }

     
    }
}
