using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.ViewModels.Func {
    /// <summary>
    /// Mq呼叫模型
    /// <date>2017-12-26</date>
    /// <author>ychost</author>
    /// </summary>
    public class MqCall {
        /// <summary>
        /// 呼叫队Mq队列
        /// </summary>
        public string QueueName { get; set; }
        /// <summary>
        /// 呼叫Mq主题
        /// </summary>
        public string TopicName { get; set; }
        /// <summary>
        /// 呼叫的图片
        /// </summary>
        public string CallIcon { get; set; }
        /// <summary>
        /// 携带的参数
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 机台编码
        /// </summary>
        public string MachineCode { get; set; }
    }

    public enum MqCallType {
        //叉车
        Forklift,
        //质检
        QualityCheck,
        //维修
        Repair
    }
}
