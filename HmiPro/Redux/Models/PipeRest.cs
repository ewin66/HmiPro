﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 往管道里面写入数据的格式
    /// <author>ychost</author>
    /// <date>2018-1-22</date>
    /// </summary>
    public class PipeRest {
        /// <summary>
        /// 写入时间
        /// </summary>
        public DateTime WriteTime { get; set; }
        /// <summary>
        /// 管道数据类型
        /// </summary>
        public PipeDataType DataType { get; set; }

        /// <summary>
        /// 管道数据
        /// </summary>
        public object Data;

        public PipeRest() {
            WriteTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 管道数据类型
    /// </summary>
    public enum PipeDataType {
        HeartBeat = 0,
        Event = 1,
    }

    /// <summary>
    /// 当 DataType == PipeDataType.Event 的时候 Data 的取值类型
    /// </summary>
    public class PipeEventData {
        /// <summary>
        /// 执行 Daemon 的事件
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// 执行 Daemon 的事件参数
        /// </summary>
        public string EventArgs { get; set; }
    }
}
