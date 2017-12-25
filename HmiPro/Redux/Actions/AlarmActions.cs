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

        public static readonly string GENERATE_ONE_ALARM = "[Alarm] Generate An Alarm";

        //历史报警集合发生更改
        public static readonly string UPDATE_HISTORY_ALARMS = "[Alarm] Update History Alarms";

        public static readonly string INIT = "[Alarm] Init";

        public enum OdAlarmType {
            //从Plc中读取最大最小值
            OdThresholdPlc = 1,
            Unknown = 2
        }

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
            /// <summary>
            /// 亮灯时间，毫秒数，时间之后关闭报警灯
            /// </summary>
            public int LightMs;

            public OpenAlarmLights(string machineCode, int lightMs = 10000) {
                MachineCode = machineCode;
                LightMs = lightMs;
            }
        }


        public struct CloseAlarmLights : IAction {
            public string Type() => CLOSE_ALARM_LIGHTS;
            public string MachineCode;

            public CloseAlarmLights(string machineCode) {
                MachineCode = machineCode;
            }
        }



        public struct Init : IAction {
            public string Type() => INIT;
        }

        public struct GenerateOneAlarm : IAction {
            public string Type() => GENERATE_ONE_ALARM;
            public string MachineCode;
            public MqAlarm MqAlarm;

            public GenerateOneAlarm(string machineCode, MqAlarm alarm) {
                MachineCode = machineCode;
                MqAlarm = alarm;
            }
        }


        public struct UpdateHistoryAlarms : IAction {
            public string Type() => UPDATE_HISTORY_ALARMS;
            public UpdateAction UpdateAction;
            public string MachineCode;
            public MqAlarm MqAlarmAdd;
            public MqAlarm MqAlarmRemove;


            public UpdateHistoryAlarms(string machineCode, UpdateAction action, MqAlarm alarmAdd, MqAlarm mqAlarmRemove) {
                MachineCode = machineCode;
                UpdateAction = action;
                MqAlarmAdd = alarmAdd;
                MqAlarmRemove = mqAlarmRemove;
            }

        }

        public enum UpdateAction {
            Add,
            Remove,
            Change
        }

    }
}
