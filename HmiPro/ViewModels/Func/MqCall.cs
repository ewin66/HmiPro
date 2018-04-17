using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Annotations;

namespace HmiPro.ViewModels.Func {
    /// <summary>
    /// Mq呼叫模型
    /// <date>2017-12-26</date>
    /// <author>ychost</author>
    /// </summary>
    public class MqCall : INotifyPropertyChanged {
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
        /// <summary>
        /// 显示文本
        /// </summary>
        public string CallTxt { get; set; }
        /// <summary>
        /// 呼叫类型
        /// </summary>
        public MqCallType CallType { get; set; }

        private bool canCall = true;

        public bool CanCall {
            get { return canCall; }
            set {
                if (canCall != value) {
                    canCall = value;
                    OnPropertyChanged(nameof(CanCall));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum MqCallType {
        //叉车
        Forklift,
        //质检
        QualityCheck,
        //维修
        Repair,
        //维修完成
        RepairComplete
    }
}
