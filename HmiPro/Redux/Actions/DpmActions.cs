using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 回填参数动作
    /// <author>ychost</author>
    /// <date>2018-1-17</date>
    /// </summary>
    public static class DpmActions {
        public static readonly string INIT = "[Dpm] Init";
        public static readonly string SUBMIT = "[Dpm] Submit ";
        public static readonly string SUBMIT_SUCCESS = "[Dpm] Submit Success ";
        public static readonly string SUBMIT_FAILED = "[Dpm] Submit Failed";

        public struct Init : IAction {
            public string Type() => INIT;
        }

        public struct Submit : IAction {
            public string Type() => SUBMIT;
            public string MachineCode;
            public IList<Dpm> Dpms;

            public Submit(string machineCode, IList<Dpm> dpms) {
                MachineCode = machineCode;
                Dpms = dpms;
            }
        }

    }
}
