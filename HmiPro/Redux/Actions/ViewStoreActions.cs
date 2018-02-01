using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 保存视图状态
    /// <author>ychost</author>
    /// <date>201-1-15</date>
    /// </summary>
    public static class ViewStoreActions {
        //初始化
        public static readonly string INIT = "[ViewStore] Init";

        //改变导航机台
        public static readonly string CHANGE_DMES_SELECTED_MACHINE_CODE = "[ViewStore] Change DMes Selected Machine Code";

        public struct ChangeDMesSelectedMachineCode : IAction {
            public string Type() => CHANGE_DMES_SELECTED_MACHINE_CODE;
            public string MachineCode;

            public ChangeDMesSelectedMachineCode(string machineCode) {
                MachineCode = machineCode;
            }

        }

        public struct Init : IAction {
            public string Type() => INIT;
        }
    }
}
