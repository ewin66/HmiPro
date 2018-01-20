using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace HmiDaemon {
    static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main() {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new DaemonService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
