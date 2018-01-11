using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Config.Models;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// Http系统RestApi外层包裹
    /// <author>ychost</author>
    /// <date>2017-12-20</date>
    /// </summary>
    public class HttpSystemRest {
        public string Message { get; set; }
        public object Data { get; set; }
        public int Code { get; set; }
        public string Hmi { get; set; }
        public string DebugMessage { get; set; }

        public HttpSystemRest() {
            Hmi = MachineConfig.HmiName;
            Code = 0;
        }
    }
}
