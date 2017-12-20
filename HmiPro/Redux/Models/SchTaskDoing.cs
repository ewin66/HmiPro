using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 当前任务
    /// </summary>
    public class SchTaskDoing
    {
        /// <summary>
        /// 工单任务
        /// </summary>
        public MqSchTask MqSchTask;
        /// <summary>
        /// 当前轴号任务
        /// </summary>
        public AxisParamItem MqSchAxis;
        /// <summary>
        /// 工单任务的id
        /// </summary>
        public int MqSchTaskId;
        /// <summary>
        /// 轴号在当前工单任务中的索引
        /// </summary>
        public int MqSchAxisIndex;
        /// <summary>
        /// 完成百分比
        /// </summary>
        public float CompletePercent;

        /// <summary>
        /// 是否开启任务
        /// </summary>
        public bool IsStarted;
    }
}
