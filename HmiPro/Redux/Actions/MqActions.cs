using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 对消息中间件的一些动作
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public static class MqActions {
        /// <summary>
        /// 开始监听排产任务
        /// </summary>
        public static readonly string START_LISTEN_SCH_TASK = "[Mq] Start Listen Schedule Task";
        public static readonly string START_LISTEN_SCH_TASK_SUCCESS = "[Mq] Start Listen Schedule Task Success";
        public static readonly string START_LISTEN_SCH_TASK_FAILED = "[Mq] Start Listen Schedule Task Failed";
        public static readonly string SCH_TASK_ACCEPT = "[Mq] Schedule Task Accept";
        public static readonly string SCH_TASK_REPLACED = "[Mq] Schedule Task Replaced";
        public static readonly string CMD_ACCEPT = "[Mq] Command Accpet";

        //监听来料信息
        public static readonly string SCAN_MATERIAL_ACCEPT = "[Mq] Scan Material Accept";
        public static readonly string START_LISTEN_SCAN_MATERIAL = "[Mq] Start Listen Scan Material";
        public static readonly string START_LISTEN_SCAN_MATERIAL_SUCCESS = "[Mq] Start Listen Scan Material Success";
        public static readonly string START_LISTEN_SCAN_MATERIAL_FAILED = "[Mq] Start Listen Scan Material Failed";

        //上传采集参数
        public static readonly string UPLOAD_CPMS = "[Mq] Upload Cpms To Mq";
        public static readonly string UPLOAD_CPMS_SUCCESS = "[Mq] Upload Cpms To Mq Success";
        public static readonly string UPLOAD_CPMS_FAILED = "[Mq] Upload Cpms To Mq Failed";
        public static readonly string START_UPLOAD_CPMS_INTERVAL = "[Mq] Start Upload Cpms Interval";

        //上传报警
        public static readonly string UPLOAD_ALARM_MQ = "[Mq] Upload Alarm To Mq";
        public static readonly string UPLOAD_ALARM_SUCCESS = "[Mq] Upload Alarm To Mq Success";
        public static readonly string UPLOAD_ALARM_FAILED = "[Mq] Upload Alarm To Mq Failed";

        //上传任务生产数据
        public static readonly string UPLOAD_SCH_TASK_MANU = "[Mq] Uplaod Schedule Task Manu Data";

        //监听人员卡
        public static readonly string START_LISTEN_EMP_RFID = "[Mq] Start Listen Employee Rfid";
        public static readonly string START_LISTEN_EMP_RFID_SUCCESS = "[Mq] Start Listen Employee Rfid Success";
        public static readonly string START_LISTEN_EMP_RFID_FAILED = "[Mq] Start Listen Employee Rfid Failed";

        //监听线轴卡
        public static readonly string START_LISTEN_AXIS_RFID = "[Mq] Start Listen Axis Rfid";
        public static readonly string START_LISTEN_AXIS_RFID_SUCCESS = "[Mq] Start Listen Axis Rfid Success";
        public static readonly string START_LISTEN_AXIS_RFID_FAILED = "[Mq] Start Listen Axis Rfid Failed";

        //监听命令
        public static readonly string START_LISTEN_CMD = "[Mq] Start Listen Command";
        public static readonly string START_LISTEN_CMD_SUCCESS = "[Mq] Start Listen Command Success";
        public static readonly string START_LISTEN_CMD_FAILED = "[Mq] Start Listen Command Failed";


        //上传回填参数
        public static readonly string UPLOAD_DPMS = "[Mq] Upload Dpms";
        public static readonly string UPLOAD_DPMS_SUCCESS = "[Mq] Upload Dpms Success";
        public static readonly string UPLOAD_DPMS_FAILED = "[Mq] Upload Dpms Failed";

        /// <summary>
        /// 呼叫系统，呼叫叉车、维修等等
        /// </summary>
        public static readonly string CALL_SYSTEM = "[Call] Call System";
        public static readonly string CALL_SYSTEM_SUCCESS = "[Call] Call System Success";
        public static readonly string CALL_SYSTEM_FAILED = "[Call] Call System Failed";


        public struct CmdAccept : IAction {
            public string Type() => CMD_ACCEPT;
            public string MachineCode;
            public MqCmd MqCmd;

            public CmdAccept(string machineCode, MqCmd mqCmd) {
                MachineCode = machineCode;
                MqCmd = mqCmd;
            }
        }

        public struct StartListenCmd : IAction {
            public string Type() => START_LISTEN_CMD;
            public string TopicName;

            public StartListenCmd(string topicName) {
                TopicName = topicName;
            }
        }

        public struct CallSystem : IAction {
            public string Type() => CALL_SYSTEM;
            public string MachineCode;
            public MqCall MqCall;

            public CallSystem(string machineCode, MqCall call) {
                MachineCode = machineCode;
                MqCall = call;
            }
        }



        public struct SchTaskReplaced : IAction {
            public string Type() => SCH_TASK_REPLACED;
            public string MachineCode;

            public SchTaskReplaced(string machineCode) {
                MachineCode = machineCode;
            }
        }


        public struct UploadDpms : IAction {
            public string Type() => UPLOAD_DPMS;
            public string MachineCode;
            public MqUploadDpm MqUploadDpms;

            public UploadDpms(string machineCode, MqUploadDpm mqUploadDpms) {
                MachineCode = machineCode;
                MqUploadDpms = mqUploadDpms;
            }
        }

        public struct StartListenAxisRfid : IAction {
            public string Type() => START_LISTEN_AXIS_RFID;
            public string TopicName;

            public StartListenAxisRfid(string topicName) {
                TopicName = topicName;
            }
        }


        public struct StartListenEmpRfid : IAction {
            public string Type() => START_LISTEN_EMP_RFID;
            public string TopicName;
            public StartListenEmpRfid(string topicName) {
                TopicName = topicName;
            }
        }



        public struct StartListenEmpRfidFailed : IAction {
            public string Type() => START_LISTEN_EMP_RFID_FAILED;
            public Exception Exp;

            public StartListenEmpRfidFailed(Exception exp) {
                Exp = exp;
            }
        }

        public struct StartListenEmpRfidSuccess : IAction {
            public string Type() => START_LISTEN_EMP_RFID_SUCCESS;
        }


        public struct UploadSchTaskManu : IAction {
            public string Type() => UPLOAD_SCH_TASK_MANU;
            public MqUploadManu MqUploadManu;
            public string QueueName;

            public UploadSchTaskManu(string queueName, MqUploadManu mqUploadManu) {
                QueueName = queueName;
                MqUploadManu = mqUploadManu;
            }
        }


        public struct StartListenSchTask : IAction {
            public string Type() => START_LISTEN_SCH_TASK;
            public string QueueName;
            public string MachineCode;

            public StartListenSchTask(string machineCode, string queueName) {
                QueueName = queueName;
                MachineCode = machineCode;
            }

        }

        public struct StartListenSchTaskSuccess : IAction {
            public string Type() => START_LISTEN_SCH_TASK_SUCCESS;
            public string MachineCode;

            public StartListenSchTaskSuccess(string machineCode) {
                MachineCode = machineCode;
            }
        }

        public struct StartListenSchTaskFailed : IAction {
            public string Type() => START_LISTEN_SCH_TASK_FAILED;
            public Exception Exp;
            public string MachineCode;
        }

        public struct SchTaskAccept : IAction {
            public string Type() => SCH_TASK_ACCEPT;
            public MqSchTask MqSchTask;

            public SchTaskAccept(MqSchTask task) {
                MqSchTask = task;
            }
        }

        public struct UploadCpms : IAction {
            public string Type() => UPLOAD_CPMS;
            public IDictionary<string, IDictionary<int, Cpm>> CpmsDict;
            public string QueueName;

            public UploadCpms(IDictionary<string, IDictionary<int, Cpm>> cpmsDict, string queueName) {
                CpmsDict = cpmsDict;
                QueueName = queueName;
            }
        }

        public struct UploadCpmsSuccess : IAction {
            public string Type() => UPLOAD_CPMS_SUCCESS;
        }

        public struct UploadCpmsFailed : IAction {
            public string Type() => UPLOAD_CPMS_FAILED;
            public Exception Exp;
        }

        public struct StartUploadCpmsInterval : IAction {
            public string Type() => START_UPLOAD_CPMS_INTERVAL;
            public string QueueName;
            public double Interval;

            public StartUploadCpmsInterval(string queueName, double interval) {
                QueueName = queueName;
                Interval = interval;
            }
        }

        public struct StartListenScanMaterial : IAction {
            public string Type() => START_LISTEN_SCAN_MATERIAL;
            public string MachineCode;
            public string QueueName;

            public StartListenScanMaterial(string machineCode, string queueName) {
                MachineCode = machineCode;
                QueueName = queueName;
            }
        }

        public struct StartListenScanMaterialSuccess : IAction {
            public string Type() => START_LISTEN_SCAN_MATERIAL_SUCCESS;
            public string MachineCode;

            public StartListenScanMaterialSuccess(string machineCode) {
                MachineCode = machineCode;
            }
        }

        public struct StartListenScanMaterialFailed : IAction {
            public string Type() => START_LISTEN_SCAN_MATERIAL_FAILED;
            public Exception Exp;
            public string MachineCode;
        }

        public struct ScanMaterialAccpet : IAction {
            public string Type() => SCAN_MATERIAL_ACCEPT;
            public MqScanMaterial ScanMaterial;
            public string MachineCode;

            public ScanMaterialAccpet(string machineCode, MqScanMaterial material) {
                ScanMaterial = material;
                MachineCode = machineCode;
            }
        }

        public struct UploadAlarmMq : IAction {
            public string Type() => UPLOAD_ALARM_MQ;
            public MqAlarm MqAlarm;
            public string QueueName;

            public UploadAlarmMq(string queueName, MqAlarm mqAlarm) {
                MqAlarm = mqAlarm;
                QueueName = queueName;
            }
        }

    }
}
