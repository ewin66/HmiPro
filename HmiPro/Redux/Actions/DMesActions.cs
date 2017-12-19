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
