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
    public class SchTaskDoing : INotifyPropertyChanged {
        /// <summary>
        /// 工单任务
        /// </summary>
        public MqSchTask MqSchTask;
        /// <summary>
        /// 当前轴号任务
        /// </summary>
        public MqTaskAxis MqSchAxis;
        /// <summary>
        /// 工单任务的id
        /// </summary>
        public int MqSchTaskId;
        /// <summary>
        /// 轴号在当前工单任务中的索引
        /// </summary>
        public int MqSchAxisIndex;

        /// <summary>
        /// 当轴百分比
        /// </summary>
        private float axisCompleteRate;

        /// <summary>
        /// 完成百分比
        /// </summary>
        public float AxisCompleteRate {
            get => axisCompleteRate;
            set {
                if (axisCompleteRate != value) {
                    axisCompleteRate = value;
                    if (MqSchAxis != null) {
                        MqSchAxis.CompletedRate = value;
                    }
                }
            }
        }
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
        public HashSet<string> StartAxisRfids = new HashSet<string>();
        /// <summary>
        /// 收线Rfid，可能有多个
        /// </summary>
        public HashSet<string> EndAxisRfids = new HashSet<string>();
        /// <summary>
        /// 计划生产长度
        /// </summary>
        public float MeterPlan;
        /// <summary>
        /// 调试长度
        /// </summary>
        public float MeterDebug;
        /// <summary>
        /// 调试时间段，毫秒级别
        /// </summary>
        public long DebugTimestampMs;
        /// <summary>
        /// 实际生产长度
        /// </summary>
        public float MeterWork;
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
        /// <summary>
        /// 计算平均速度
        /// </summary>
        public Func<double, double> CalcAvgSpeed;
        /// <summary>
        /// 一轴的平均速度
        /// </summary>
        public float SpeedAvg;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SchTaskDoing() {
            Init();
        }

        public void Init() {
            //MqSchTask = null;
            MqSchAxis = null;
            IsStarted = false;
            AxisCompleteRate = 0;
            WorkCode = null;
            Step = -1;
            CalcAvgSpeed = null;
            SpeedAvg = -1;
            MeterPlan = -1;
            MeterDebug = -1;
            MeterWork = -1;
            DebugTimestampMs = 0;
            MqSchAxisIndex = -1;
            //StartAxisRfids.Clear();
            //EndAxisRfids.Clear();
            //EmpRfids.Clear();
        }

        public void Clear() {
            Init();
            MqSchTask = null;
        }
    }
}
