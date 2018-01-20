using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;
using YCsharp.Model.Procotol.SmParam;

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

        /// <summary>
        /// 采集参数值超过Plc的设定值
        /// </summary>
        public static readonly string CPM_PLC_ALARM_OCCUR = "[Alarm] Cpm Plc Alarm Occur";

        public static readonly string COM_485_SINGLE_ERROR = "[Alarm] Communication 485 Single Point Error";

        /// <summary>
        /// 485单点通讯状态异常
        /// </summary>
        public struct Com485SingleError : IAction {
            public string Type() => COM_485_SINGLE_ERROR;
            public string Ip;
            public string MachineCode;
            public int CpmCode;
            public string CpmName;

            public Com485SingleError(string machineCode, string ip, int cpmCode, string cpmName) {
                Ip = ip;
                MachineCode = machineCode;
                CpmCode = cpmCode;
                CpmName = cpmName;
            }
        }

        public struct CpmPlcAlarmOccur : IAction {
            public string Type() => CPM_PLC_ALARM_OCCUR;
            public string MachineCode;
            public string Message;
            public int CpmCode;
            public string CpmName;

            public CpmPlcAlarmOccur(string machineCode, string message, int cpmCode, string cpmName) {
                MachineCode = machineCode;
                Message = message;
                CpmCode = cpmCode;
                CpmName = cpmName;
            }
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
            public int MinGapSec;

            /// <summary>
            /// 上一次报警的时间点
            /// </summary>
            public static IDictionary<string, DateTime> LastGenerateTimeDict = new ConcurrentDictionary<string, DateTime>();


            public GenerateOneAlarm(string machineCode, MqAlarm alarm, int minGapSec = 0) {
                MachineCode = machineCode;
                MqAlarm = alarm;
                MinGapSec = minGapSec;
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
