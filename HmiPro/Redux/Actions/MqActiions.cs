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
        //上传采集参数
        public static readonly string UPLOAD_CPMS = "[Mq] Upload Cpms";
        public static readonly string UPLOAD_CPMS_SUCCESS = "[Mq] Upload Cpms Success";
        public static readonly string UPLOAD_CPMS_FAILED = "[Mq] Upload Cpms Failed";
        public static readonly string START_UPLOAD_CPMS_INTERVAL = "[Mq] Upload Cpms Interval";

        public struct StartListenSchTask : IAction {
            public string Type() => START_LISTEN_SCH_TASK;
            public string QueueName;

            public StartListenSchTask(string queueName) {
                QueueName = queueName;
            }

        }

        public struct StartListenSchTaskSuccess : IAction {
            public string Type() => START_LISTEN_SCH_TASK_SUCCESS;
        }

        public struct StartListenSchTaskFailed : IAction {
            public string Type() => START_LISTEN_SCH_TASK_FAILED;
            public Exception Exp;
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
    }
}
