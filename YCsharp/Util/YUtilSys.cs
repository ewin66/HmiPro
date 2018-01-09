using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
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
    }
}
