using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// DMes系统相关核心指令
    /// <author>ychost</author>
    /// <date>2017-12-20</date>
    /// </summary>
    public static class DMesActions {
        /// <summary>
        /// 分配任务
        /// 这里的任务是对Mq收到的任务进行了集中处理了之后才分配的
        /// </summary>
        public static readonly string DMES_SCH_TASK_ASSIGN = "[DMes] Schedule Task Assign";

        //初始化一些字典等等
        public static readonly string INIT = "[DMes] Init";

        //开始某轴任务
        public static readonly string START_SCH_TASK_AXIS = "[DMes] Start Schedule Task Axis";
        public static readonly string START_SCH_TASK_AXIS_SUCCESS = "[DMes] Start Schedule Task Axis Success";
        public static readonly string START_SCH_TASK_AXIS_FAILED = "[DMes] Start Schedule Task Axis Failed";

        //完成某轴任务
        public static readonly string COMPLETED_SCH_AXIS = "[DMes] Completed Schedule Axis";
        public static readonly string COMPLETED_SCH_AXIS_SUCESS = "[DMes] Completed Schedule Axis Success";
        public static readonly string COMPLETED_SCH_AXIS_FAILED = "[DMes] Completed Schedule Axis Failed";

        //清空任务
        public static readonly string CLEAR_SCH_TASKS = "[DMes] Clear Sch Tasks";

        //删除某个任务集合
        public static readonly string DEL_TASK = "[DMes] Del Task";


        public struct DelTask : IAction {
            public string Type() => DEL_TASK;
            public string MachineCode;
            public string TaskId;

            public DelTask(string machineCode, string taskId) {
                MachineCode = machineCode;
                TaskId = taskId;
            }
        }

        //接受到Rfid可能来自Mq，可能来自底层参数
        public static readonly string RFID_ACCPET = "[Cpm] Rfid Accept";

        public enum RfidWhere {
            FromCpm,
            FromMq,
            Unknown
        }

        public enum RfidType {
            //放线轴
            StartAxis,
            //收线轴
            EndAxis,
            //人员上班打卡
            EmpStartWork,
            //人员上机打卡
            EmpStartMachine,
            //人员下班打卡
            EmpEndWork,
            //人员下机打卡
            EmpEndMachine,
            //未知
            Unknown,
        }

        /// <summary>
        /// 清空机台任务
        /// </summary>
        public struct ClearSchTasks : IAction {
            public string Type() => CLEAR_SCH_TASKS;
            public string[] Machines;

            public ClearSchTasks(params string[] machines) {
                Machines = machines;
            }
        }

        public struct RfidAccpet : IAction {
            public string Type() => RFID_ACCPET;
            public RfidWhere RfidWhere;
            public RfidType RfidType;
            public string MachineCode;
            public string Rfid;
            public object MqData;

            public RfidAccpet(string machineCode, string rfid, RfidWhere where, RfidType type, object mqData = null) {
                Rfid = rfid;
                MachineCode = machineCode;
                RfidWhere = where;
                RfidType = type;
                MqData = mqData;
            }
        }

        public struct StartAxisSuccess : IAction {
            public string Type() => START_SCH_TASK_AXIS_SUCCESS;
            public string MachineCode;
            public string AxisCode;

            public StartAxisSuccess(string machineCode, string axisCode) {
                MachineCode = machineCode;
                AxisCode = axisCode;
            }

        }

        public struct StartSchTaskAxis : IAction {
            public string Type() => START_SCH_TASK_AXIS;
            public string AxisCode;
            public string MachineCode;
            public string TaskId;

            public StartSchTaskAxis(string machineCode, string axisCode,string taskId) {
                MachineCode = machineCode;
                AxisCode = axisCode;
                TaskId = taskId;
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

        public struct CompletedSchAxis : IAction {
            public string Type() => COMPLETED_SCH_AXIS;
            public string MachineCode;
            public string AxisCode;

            public CompletedSchAxis(string machineCode, string axisCode) {
                MachineCode = machineCode;
                AxisCode = axisCode;
            }


        }
    }
}
