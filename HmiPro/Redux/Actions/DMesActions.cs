using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// DMes系统相关核心指令
    /// </summary>
    public static class DMesActions {
        /// <summary>
        /// 分配任务
        /// 这里的任务是对Mq收到的任务进行了集中处理了之后才分配的
        /// </summary>
        public static readonly string DMES_SCH_TASK_ASSIGN = "[DMes] Schedule Task Assign";

        public static readonly string INIT = "[DMes] Init";

        public static readonly string START_SCH_TASK_AXIS = "[DMes] Start Schedule Task Axis";
        public static readonly string START_SCH_TASK_AXIS_SUCCESS = "[DMes] Start Schedule Task Axis Success";
        public static readonly string START_SCH_TASK_AXIS_FAILED = "[DMes] Start Schedule Task Axis Failed";


        public struct StartSchTaskAxis : IAction {
            public string Type() => START_SCH_TASK_AXIS;
            public string AxisCode;
            public string MachineCode;

            public StartSchTaskAxis(string machineCode, string axisCode) {
                MachineCode = machineCode;
                AxisCode = axisCode;
            }
        }

        public struct Init : IAction {
            public string Type() => INIT;
        }

        public struct DMesSchTaskAssign : IAction {
            public string Type() => DMES_SCH_TASK_ASSIGN;
            public MqSchTask SchTask;
            public string MachineCode;

            public DMesSchTaskAssign(string machineCode, MqSchTask task) {
                SchTask = task;
                MachineCode = machineCode;
            }
        }
    }
}
