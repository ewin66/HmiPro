using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// <author>ychost</author>
    /// <date>2017-12-20</date>
    /// </summary>
    public struct SysNotificationMsg {
        /// <summary>
        /// 提示信息第一行
        /// </summary>
        public string Title;
        /// <summary>
        /// 提示信息第二行
        /// </summary>
        public string Content;
        /// <summary>
        /// 最小间隔秒数
        /// 同一个通知的提醒时间-上次提醒时间 > MinGapSec ? ShowNotify : Not ShowNotify
        /// 如果 MinGapSec==null : ShowNotify
        /// </summary>
        public int? MinGapSec;
        /// <summary>
        /// 写入Logger.Notify的数据
        /// 默认为 Title + Content
        /// </summary>
        public string LogDetail;
        /// <summary>
        /// 用来配合 MingGapSec
        /// </summary>
        public static readonly IDictionary<string, DateTime> NotifyTimeDict = new ConcurrentDictionary<string, DateTime>();
        /// <summary>
        /// 通知级别，备用
        /// </summary>
        public NotifyLevel Level;

    }

    /// <summary>
    /// 通知级别
    /// </summary>
    public enum NotifyLevel {
        Info,
        Warn,
        Error,
    }

}
