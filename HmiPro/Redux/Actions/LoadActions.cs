using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 加载配置文件命令
    /// <author>ychost</author>
    /// <date>2018-1-25</date>
    /// </summary>
    public static class LoadActions {
        public static readonly string LOAD_GLOBAL_CONFIG = "[Load] Load Global Config";

        public static readonly string LOAD_MACHINE_CONFIG = "[Load] Machine Config";


        public struct LoadGlobalConfig : IAction {
            public string Type() => LOAD_GLOBAL_CONFIG;
        }

        public struct LoadMachieConfig : IAction {
            public string Type() => LOAD_MACHINE_CONFIG;
        }
    }
}
