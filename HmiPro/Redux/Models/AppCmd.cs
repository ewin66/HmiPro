﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// Mq 向机台发送的指令
    /// <author>ychost</author>
    /// <date>2018-1-27</date>
    /// </summary>
    public class AppCmd {
        /// <summary>
        /// 机台编码
        /// </summary>
        public string machineCode { get; set; }
        /// <summary>
        /// 动作
        /// </summary>
        public string action { get; set; }
        /// <summary>
        /// 命令 参数
        /// action 不同，该值类型也不同
        /// </summary>
        public object args { get; set; }
        /// <summary>
        /// 发送时间，毫秒戳
        /// </summary>
        public long? sendTime { get; set; }
        /// <summary>
        /// 指定该任务的执行时间，毫秒戳
        /// 可以为null，为 null 则表示立即执行
        /// </summary>
        public long? execTime { get; set; }
        /// <summary>
        /// action 派遣的地方
        /// </summary>
        public AppActionsWhere execWhere { get; set; } = AppActionsWhere.MqActions;
        /// <summary>
        /// 类型转发到 Redux 需要 args 的类型
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 命令是从哪里发出来的
        /// </summary>
        public AppCmdFrom cmdFrom { get; set; } = AppCmdFrom.FromMq;

    }

    /// <summary>
    /// 命令来源
    /// </summary>
    public enum AppCmdFrom {
        FromMq = 1,
        FromHttp = 2,
        FromPipe = 3,
        FromTcp = 4
    }


    /// <summary>
    /// action 参数是发送到 Redux 还是使用 MqCmdActions
    /// </summary>
    public enum AppActionsWhere {
        MqActions = 0,
        ReduxActions = 1,
    }

    /// <summary>
    /// MqCmd.action 的取值
    /// </summary>
    public static class MqCmdActions {
        /// <summary>
        /// 删除工单，则 args 为工单号
        /// </summary>
        public static readonly string DEL_WORK_TASK = "del_work_task";
    }


}
