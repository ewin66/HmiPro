using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 
    /// </summary>
    public static class ViewStoreActions {
        public static readonly string INIT = "[ViewStore] Init";

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
