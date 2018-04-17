using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 呼叫 Web 服务器
    /// <author>ychost</author>
    /// <date>2018-1-25</date>
    /// </summary>
    public class MqCall {
        /// <summary>
        /// 机台编码
        /// </summary>
        public string machineCode { get; set; }
        /// <summary>
        /// 呼叫类型
        /// </summary>
        public string callType { get; set; }
        /// <summary>
        /// 呼叫动作
        /// </summary>
        public string callAction { get; set; }
        /// <summary>
        /// 呼叫参数，暂时没有使用
        /// </summary>
        public object callArgs{ get; set; }
        /// <summary>
        /// 呼叫Id
        /// </summary>
        public int CallId { get; set; }
    }

    /// <summary>
    /// 呼叫动作
    /// </summary>
    public static class MqCallAction {
        public static readonly string MovePallet = "叉走栈板";
    }

    /// <summary>
    /// 呼叫类型
    /// </summary>
    public static class MqCallType {
        public static readonly string Forklift = "叉车";
        public static readonly string Repair = "维修";
        public static readonly string RepairComplete = "维修完成";
    }

}
