using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 报警指令
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public static class AlarmActions {
        //检查Cpm
        public static readonly string CHECK_CPM = "[Alarm] Check Cpm";
        //打开报警灯
        public static readonly string OPEN_ALARM_LIGHTS = "[Alarm] Open Alarm Lights";
        //关闭报警灯
        public static readonly string CLOSE_ALARM_LIGHTS = "[Alarm] Close Alarm Lights";

        public struct CheckCpm : IAction {
            public string Type() => CHECK_CPM;
            public Cpm Cpm;

            public CheckCpm(Cpm cpm) {
                Cpm = cpm;
            }
        }

        public struct OpenAlarmLights : IAction {
            public string Type() => OPEN_ALARM_LIGHTS;
            public string MachineCode;

            public OpenAlarmLights(string machineCode) {
                MachineCode = machineCode;
            }
        }


        public struct CloseAlarmLights : IAction {
            public string Type() => OPEN_ALARM_LIGHTS;
            public string MachineCode;

            public CloseAlarmLights(string machineCode) {
                MachineCode = machineCode;
            }
        }

    }
}
