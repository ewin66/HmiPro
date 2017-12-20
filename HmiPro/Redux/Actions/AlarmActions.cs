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
        //根据Bom来检查参数是否异常
        public static readonly string CHECK_CPM_BOM_ALARM = "[Alarm] Check Cpm Bom Alarm";
        //打开报警灯
        public static readonly string OPEN_ALARM_LIGHTS = "[Alarm] Open Alarm Lights";
        //关闭报警灯
        public static readonly string CLOSE_ALARM_LIGHTS = "[Alarm] Close Alarm Lights";
        //通知报警
        public static readonly string NOTIFY_ALARM = "[Alarm] Notify Alarm";

        public static readonly string INIT = "[Alarm] Init";


        public struct CheckCpmBomAlarm : IAction {
            public string Type() => CHECK_CPM_BOM_ALARM;
            public AlarmBomCheck AlarmBomCheck;
            public string MachineCode;

            public CheckCpmBomAlarm(string machineCode, AlarmBomCheck alarmBomCheck) {
                MachineCode = machineCode;
                AlarmBomCheck = alarmBomCheck;
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

        public struct NotifyAlarm : IAction {
            public string Type() => NOTIFY_ALARM;
            public MqAlarm MqAlarm;
            public string MachineCode;

            public NotifyAlarm(string machineCode, MqAlarm mqAlarm) {
                MachineCode = machineCode;
                MqAlarm = mqAlarm;
            }
        }

        public struct Init : IAction {
            public string Type() => INIT;
        }



    }
}
