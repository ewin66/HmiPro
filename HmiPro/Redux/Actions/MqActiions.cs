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
    public static class MqActiions {
        /// <summary>
        /// 开始监听排产任务
        /// </summary>
        public static readonly string START_LISTEN_SCH_TASK = "[Mq] Start Listen Schedule Task";
        public static readonly string START_LISTEN_SCH_TASK_SUCCESS = "[Mq] Start Listen Schedule Task Success";
        public static readonly string START_LISTEN_SCH_TASK_FAILED = "[Mq] Start Listen Schedule Task Failed";
        public static readonly string SCH_TASK_ACCEPT = "[Mq] Schedule Task Accept";

        //监听来料信息
        public static readonly string SCAN_MATERIAL_ACCEPT = "[Mq] Scan Material Accept";
        public static readonly string START_LISTEN_SCAN_MATERIAL = "[Mq] Start Scan Material";
        public static readonly string START_LISTEN_SCAN_MATERIAL_SUCCESS = "[Mq] Start Scan Material Success";
        public static readonly string START_LISTEN_SCAN_MATERIAL_FAILED = "[Mq] Start Scan Material Failed";

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

            public ScanMaterialAccpet(MqScanMaterial material) {
                ScanMaterial = material;
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
