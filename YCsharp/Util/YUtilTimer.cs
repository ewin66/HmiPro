using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace YCsharp.Util {
    /// <summary>
    /// 时间工具
    /// <date>2017-09-27</date>
    /// <author>ychost</author>
    /// </summary>
    public static partial class YUtil {
        /// <summary>
        /// 实现类似于js的setTimetout
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        public static Timer SetTimeout(double interval, Action action) {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e) {
                ClearTimeout(timer);
                action();
            };
            RecoveryTimeout(timer);
            return timer;
        }

        /// <summary>
        /// 实现类似js的setInterval,单位毫秒
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        public static Timer SetInterval(double interval, Action action) {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e) {
                action();
            };
            RecoveryTimeout(timer);
            return timer;
        }

        /// <summary>
        /// action中传递这是第几次执行
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Timer SetInterval(double interval, Action<int> action) {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            int i = 0;
            timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e) {
                action(++i);
            };

            return timer;
        }

        /// <summary>
        /// 实现类似js的setInterval,单位毫秒
        /// 采用的是闭包
        /// <example>
        ///  SetInterval(1000,()=>{},3)();
        /// </example>
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="cycleTimes">循环次数 -1 位无穷</param>
        public static Func<Timer> SetInterval(double interval, Action action, int cycleTimes) {
            if (cycleTimes == -1) {
                return () => SetInterval(interval, action);
            }
            Timer timer = new Timer(interval);
            if (cycleTimes > 0) {
                return () => {
                    if (cycleTimes > 0) {
                        timer.Elapsed += (s, e) => {
                            action();
                            if (--cycleTimes == 0) {
                                ClearTimeout(timer);
                            }
                        };
                        RecoveryTimeout(timer);
                    }
                    return timer;
                };
            }
            return () => timer;
        }

        /// <summary>
        /// 清除记时
        /// </summary>
        /// <param name="timer"></param>
        public static void ClearTimeout(Timer timer) {
            if (timer != null) {
                timer.Enabled = false;
            }
        }

        /// <summary>
        /// 恢复记时
        /// </summary>
        /// <param name="timer"></param>
        public static void RecoveryTimeout(Timer timer) {
            if (timer != null) {
                timer.Enabled = true;
            }
        }


        /// <summary>
        /// 时间戳，毫秒级别
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Int64 GetUtcTimestampMs(DateTime time) {
            //return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((time.ToUniversalTime() - epoch).TotalMilliseconds);
        }


        /// <summary>
        /// utc 时间戳转本地时间
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static DateTime UtcTimestampToLocalTime(long ms) {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddMilliseconds(ms);
            return dt;
        }

        /// <summary>
        /// 时间戳转 utc 时间
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static DateTime TimestampToUtcTime(long ms) {
            DateTime startTime = new DateTime(1970, 1, 1);
            DateTime dt = startTime.AddMilliseconds(ms);
            return dt;
        }


        /// <summary>
        /// 时间戳，秒级别
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Int64 GetTimeStampSec(DateTime time) {
            return GetUtcTimestampMs(time) / 1000;
        }

        /// <summary>
        /// 设置系统时间dll入口
        /// </summary>
        /// <param name="sysTime"></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);

        /// <summary>
        /// 通过字符串设置系统时间
        /// </summary>
        /// <param name="timestr"></param>
        /// <returns></returns>
        public static bool SetLocalTimeByStr(string timestr) {
            DateTime dt = Convert.ToDateTime(timestr);
            return SetLoadTimeByDateTime(dt);
        }

        /// <summary>
        /// 设置系统时间可能会抛出异常
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool SetLoadTimeByDateTime(DateTime dt) {
            bool flag = false;
            SystemTime sysTime = new SystemTime();
            sysTime.wYear = Convert.ToUInt16(dt.Year);
            sysTime.wMonth = Convert.ToUInt16(dt.Month);
            sysTime.wDay = Convert.ToUInt16(dt.Day);
            sysTime.wHour = Convert.ToUInt16(dt.Hour);
            sysTime.wMinute = Convert.ToUInt16(dt.Minute);
            sysTime.wSecond = Convert.ToUInt16(dt.Second);
            flag = SetLocalTime(ref sysTime);
            return flag;
        }


        /// <summary>
        /// 获取当天星期几
        /// </summary>
        /// <returns></returns>
        public static string GetCurWeekDay() {
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(DateTime.Now.DayOfWeek);
        }




        /// <summary>
        /// 获取当前时间，日期，星期几
        /// </summary>
        /// <returns></returns>
        public static string GetCurLocalTimeAndWeekDay() {
            return DateTime.Now.ToLocalTime() + " " + GetCurWeekDay();
        }

        /// <summary>
        /// 系统时间结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMiliseconds;
        }

        /// <summary>
        /// 获取Ntp服务器时间
        /// </summary>
        /// <param name="ntpServer"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static DateTime GetNtpTime(string ntpServer, int port = 123) {

            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            //var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var addresses = new IPAddress[] { IPAddress.Parse(ntpServer) };

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], port);
            //NTP uses UDP
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Connect(ipEndPoint);

            //Stops code hang if NTP is blocked
            socket.ReceiveTimeout = 3000;

            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            //Convert From big-endian to little-endian
            intPart = swapEndianness(intPart);
            fractPart = swapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToLocalTime();
        }

        // stackoverflow.com/a/3294698/162671
        static uint swapEndianness(ulong x) {
            return (uint)(((x & 0x000000ff) << 24) +
                          ((x & 0x0000ff00) << 8) +
                          ((x & 0x00ff0000) >> 8) +
                          ((x & 0xff000000) >> 24));
        }

        /// <summary>
        /// 销毁线程
        /// </summary>
        /// <param name="task"></param>
        public static void DisposeTask(Task task) {
            task?.Dispose();
        }

        /// <summary>
        /// 获取工作时间点，比如早8点，晚20点对应当前的公历时间
        /// </summary>
        /// <param name="whiteHour">白班时间点</param>
        /// <param name="darkHour">晚班时间点</param>
        /// <returns></returns>
        public static DateTime GetWorkTime(int whiteHour, int darkHour) {
            if (whiteHour > darkHour) {
                throw new Exception("白班时间点不能大于晚班时间点");
            }
            var now = DateTime.Now;
            DateTime startTime;
            if (now.Hour >= whiteHour && now.Hour < darkHour) {
                //白班开始时间为早上8点
                startTime = new DateTime(now.Year, now.Month, now.Day, whiteHour, 0, 0, 0);
            } else {
                //夜班开始时间为晚上20点
                //如果是凌晨则时间为昨天的20点
                if (now.Hour < whiteHour) {
                    now = now.AddDays(-1);
                }
                startTime = new DateTime(now.Year, now.Month, now.Day, darkHour, 0, 0, 0);
            }
            return startTime;
        }

        /// <summary>
        /// 获取启东的上班时间
        /// </summary>
        /// <returns></returns>
        public static DateTime GetKeystoneWorkTime() {
            return GetWorkTime(8, 20);
        }
    }
}
