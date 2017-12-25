using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Annotations;
using YCsharp.Util;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 上传报警
    /// <date>2017-12-13</date>
    /// <author>ychost</author>
    /// </summary>
    public class MqAlarm : MongoDoc, INotifyPropertyChanged {
        /// <summary>
        /// 机台编码
        /// </summary>
        public string machineCode { get; set; }

        private string _message;
        /// <summary>
        /// 报警内容
        /// </summary>
        public string message {
            get => _message;
            set {
                if (_message != value) {
                    _message = value;
                    OnPropertyChanged(nameof(message));
                }
            }

        }

        private string _axisCode;
        /// <summary>
        /// 轴号编码
        /// </summary>
        public string axisCode {
            get => _axisCode;
            set {
                if (_axisCode != value) {
                    _axisCode = value;
                    OnPropertyChanged(nameof(axisCode));
                }
            }
        }
        /// <summary>
        /// 工单编码
        /// </summary>
        public string workCode { get; set; }

        private long _time;
        /// <summary>
        /// utc时间戳，毫秒级别
        /// </summary>
        public long time {
            get => _time;
            set {
                if (_time != value) {
                    _time = value;
                    TimeStr = YUtil.UtcTimestampToLocalTime(_time).ToString("yyyy-MM-dd HH:mm:ss");
                    TimeStrHms = YUtil.UtcTimestampToLocalTime(_time).ToString("HH:mm:ss");
                    OnPropertyChanged(nameof(TimeStrHms));
                    OnPropertyChanged(nameof(TimeStr));
                }
            }
        }
        public string TimeStr { get; set; }
        public string TimeStrHms { get; set; }

        private float _meter;
        /// <summary>
        /// 报警对应的米数
        /// </summary>
        public float meter {
            get => _meter;
            set {
                if (_meter != value) {
                    _meter = value;
                    OnPropertyChanged(nameof(meter));
                }
            }
        }
        /// <summary>
        /// 操作人员
        /// </summary>
        public HashSet<string> employees { get; set; }
        /// <summary>
        /// 放线处rfid
        /// </summary>
        public HashSet<string> startRfids { get; set; }

        public HashSet<string> endRfids { get; set; }

        private string _alarmType;
        /// <summary>
        /// 报警类型
        /// </summary>
        public string alarmType {
            get => _alarmType;
            set {
                if (_alarmType != value) {
                    _alarmType = value;
                    OnPropertyChanged(nameof(alarmType));
                }
            }
        }

        /// <summary>
        /// 报警编码，如果两个报警编码一致，只取最新的那个
        /// </summary>
        public int code { get; set; }

        private string _cpmName;
        /// <summary>
        /// 参数名字
        /// </summary>
        public string CpmName {
            get => _cpmName;
            set {
                if (_cpmName != value) {
                    _cpmName = value;
                    OnPropertyChanged(nameof(CpmName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class AlarmType {
        public static readonly string OdErr = "直径超限";
        public static readonly string SparkErr = "火花报警";
        public static readonly string PounchErr = "打卡错误";
        public static readonly string ClearErr = "清零错误";
        public static readonly string OtherErr = "其它异常";
    }
}
