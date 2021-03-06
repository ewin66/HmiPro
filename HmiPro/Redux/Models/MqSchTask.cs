﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Annotations;
using YCsharp.Util;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 排产任务模型，从服务器反序列化而来
    /// <author>ychost</author>
    /// <date>2017-12-19</date>
    /// </summary>
    public class MqSchTask : INotifyPropertyChanged {
        /// <summary>
        /// 任务 Id 
        /// </summary>
        public string taskId { get; set; }
        public static readonly string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        /// <summary>
        /// 制程质检
        /// </summary>
        public List<MqProcessCheck> iqcList { get; set; }
        /// <summary>
        /// 当前任务序号
        /// </summary>
        public int CurPlanIndex { get; set; }
        /// <summary>
        /// 满载速度
        /// </summary>
        public float forceSpeed { get; set; }

        public Dictionary<string, object> userInputParams { get; set; }

        private string _workCode;
        /// <summary>
        /// 工单编码
        /// </summary>
        public string workcode {
            get => _workCode;
            set {
                if (_workCode != value) {
                    _workCode = value;
                    OnPropertyChanged(nameof(workcode));
                }

            }
        }
        /// <summary>
        /// 标准速度
        /// </summary>
        public double speed { get; set; }
        /// <summary>
        /// 工序编码
        /// </summary>
        public string seqcode { get; set; }
        /// <summary>
        /// 物料信息
        /// </summary>
        public List<PmtmsItem> pmtms { get; set; }

        private JavaTime _psTime;
        /// <summary>
        /// 计划开始时间
        /// </summary>
        public JavaTime pstime {
            get => _psTime;
            set {
                if (_psTime != value) {
                    _psTime = value;
                    OnPropertyChanged(nameof(pstime));
                    PsTimeStr = YUtil.UtcTimestampToLocalTime(_psTime.time).ToString(DateTimeFormat);
                    OnPropertyChanged(nameof(PsTimeStr));
                }
            }
        }
        public string PsTimeStr { get; private set; }


        /// <summary>
        /// 轴号信息
        /// </summary>
        public ObservableCollection<MqTaskAxis> axisParam { get; set; }
        /// <summary>
        /// 主操作手
        /// </summary>
        public string main_by { get; set; }
        /// <summary>
        /// 计划
        /// </summary>
        public string productstate { get; set; }
        /// <summary>
        /// 工单id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 副操作手
        /// </summary>
        public string vice_by { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string reaxistype { get; set; }
        /// <summary>
        /// bom 表信息
        /// </summary>
        public List<Dictionary<string, object>> bom { get; set; }
        /// <summary>
        /// 交货日期
        /// </summary>
        public JavaTime delidate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int schedule { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int useminute { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int priority { get; set; }
        /// <summary>
        /// 轴数量统计
        /// </summary>
        public int axiscount { get; set; }

        public JavaTime _pdTime;
        /// <summary>
        /// 计划结束时间
        /// </summary>
        public JavaTime pdtime {
            get => _pdTime;
            set {
                if (_pdTime != value) {
                    _pdTime = value;
                    OnPropertyChanged(nameof(pdtime));
                    PdTimeStr = YUtil.UtcTimestampToLocalTime(_pdTime.time).ToString(DateTimeFormat);
                    OnPropertyChanged(nameof(PdTimeStr));
                }
            }
        }

        public string PdTimeStr { get; set; }

      
        /// <summary>
        /// 
        /// </summary>
        public int step { get; set; }
        /// <summary>
        /// 机台编码
        /// </summary>
        public string maccode { get; set; }

        public float _compltedRate;

        /// <summary>
        /// 工单的完成率
        /// </summary>
        public float CompletedRate {
            get => _compltedRate;
            set {
                if (_compltedRate != value) {
                    _compltedRate = value;
                    OnPropertyChanged(nameof(CompletedRate));
                }
            }
        }

        public string remarks { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class PmtmsItem {

        /// <summary>
        /// 
        /// </summary>
        public string materil { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double matecount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string workcode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string seqcode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public JavaTime ptime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string maccode { get; set; }
    }

    public class JavaTime {
        /// <summary>
        /// 
        /// </summary>
        public int nanos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int minutes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int seconds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int hours { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int month { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int year { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int timezoneOffset { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int day { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int date { get; set; }
    }

    public class MqTaskAxis : INotifyPropertyChanged {
        private DateTime? _startTime;
        /// <summary>
        /// 轴任务开始时间
        /// </summary>
        public DateTime? StartTime {
            get => _startTime;
            set {
                if (_startTime != value) {
                    _startTime = value;
                    OnPropertyChanged(nameof(StartTime));
                    OnPropertyChanged(nameof(StartTimeStr));
                }
            }
        }

        public string StartTimeStr {
            get {
                if (IsStarted == false) {
                    return "/";
                }
                if (StartTime.HasValue) {
                    return StartTime.Value.ToString("MM-dd HH:mm");
                }
                return "/";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string product { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string workcode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string seqcode { get; set; }
        /// <summary>
        /// 红
        /// </summary>
        public string color { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float length { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int axiscount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string axiscode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int level { get; set; }

        public string Level {
            get {
                if (level != 0) {
                    return level.ToString();
                }
                return "/";
            }
            set {

            }
        }
        /// <summary>
        /// 任务 Id
        /// </summary>
        public string taskId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int step { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string maccode { get; set; }

        private float _completeRate;
        /// <summary>
        /// 完成率
        /// </summary>
        public float CompletedRate {
            get => _completeRate;
            set {
                if (_completeRate != value) {

                    _completeRate = value;
                    CompletedRatePercent = value * 100;
                    OnPropertyChanged(nameof(CompletedRatePercent));
                    OnPropertyChanged(nameof(CompletedRate));
                    OnPropertyChanged(nameof(CompletedRateStr));
                    //if (_completeRate > 0.9) {
                    //    CanCompleted = true;
                    //    OnPropertyChanged(nameof(CanCompleted));
                    //} else {
                    //    CanCompleted = false;
                    //    OnPropertyChanged(nameof(CanCompleted));
                    //}
                }
            }
        }

        public float CompletedRatePercent { get; set; }

        public string CompletedRateStr => (CompletedRate * 100).ToString("0.00") + "%";

        private bool _isCompleted;
        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsCompleted {
            get => _isCompleted;
            set {
                if (_isCompleted != value) {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                    CanCompleted = false;
                    OnPropertyChanged(nameof(CanCompleted));
                }
            }
        }

        private bool _isStarted;

        /// <summary>
        /// 是否启动
        /// </summary>
        public bool IsStarted {
            get { return _isStarted; }
            set {
                if (_isStarted != value) {
                    _isStarted = value;
                    OnPropertyChanged(nameof(IsStarted));
                    IsStoped = !_isStarted;
                    OnPropertyChanged(nameof(IsStoped));
                    CanCompleted = value;
                    OnPropertyChanged(nameof(CanCompleted));
                }
            }
        }

        public bool IsStoped { get; set; } = true;

        private string _state = MqSchTaskAxisState.WaitDoing;
        /// <summary>
        /// 状态
        /// </summary>
        public string State {
            get { return _state; }
            set {
                if (_state != value) {
                    _state = value;
                    OnPropertyChanged(nameof(State));
                }
            }
        }


        private bool _canStart = true;

        /// <summary>
        /// 能否开启任务
        /// </summary>
        public bool CanStart {
            get {
                if (IsCompleted) {
                    return false;
                }
                return _canStart;
            }
            set {
                if (_canStart != value) {
                    _canStart = value;
                    OnPropertyChanged(nameof(CanStart));
                }
            }
        }

        public bool CanCompleted { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class MqSchTaskAxisState {
        public static readonly string Doing = "正在生产...";
        public static readonly string Completed = "完成生产";
        public static readonly string WaitDoing = "等待生产";
    }

}
