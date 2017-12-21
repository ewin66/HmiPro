using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Annotations;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 当前任务
    /// </summary>
    public class SchTaskDoing:INotifyPropertyChanged {
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
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime;
        /// <summary>
        /// 放线轴
        /// </summary>
        public HashSet<string> StartRfids;
        /// <summary>
        /// 收线Rfid，可能有多个
        /// </summary>
        public HashSet<string> EndRfids = new HashSet<string>();
        /// <summary>
        /// 当前生产长度
        /// </summary>
        public float Meter;
        /// <summary>
        /// 工单
        /// </summary>
        public string WorkCode;
        /// <summary>
        /// 操作人员
        /// </summary>
        public HashSet<string> EmpRfids = new HashSet<string>();

        /// <summary>
        /// 当前为工序的第几步，从工单里面可以获取
        /// </summary>
        public int Step;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
