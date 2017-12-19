using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Config.Models;

namespace HmiPro.Redux.Models {
    public class Sys {

    }

    public class HttpSystemRest {
        public string Message { get; set; }
        public object Data { get; set; }
        public int Code { get; set; }
        public string Machine { get; set; }
        public int MachineId { get; set; }
        public string DebugMessage { get; set; }

        public HttpSystemRest() {
            Machine = MachineConfig.AllMachineName;
            Code = 0;
        }
    }
}
