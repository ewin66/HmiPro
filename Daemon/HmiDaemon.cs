using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Daemon {
    public partial class HmiDaemon : ServiceBase {
        public HmiDaemon() {
            InitializeComponent();
        }
        private static readonly string logPath = "C:\\HmiPro\\Log\\daemon.txt";

        protected override void OnStart(string[] args) {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(logPath, true)) {
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "Hmi Daemon Start.");
            }

        }

        protected override void OnStop() {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(logPath, true)) {
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "Hmi Daemon Stop.");
            }
        }
    }
}
