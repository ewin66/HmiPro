using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 程序启动日志
    /// <author>yhost</author>
    /// <date>2018-2-3</date>
    /// </summary>
    public class StartupLog:MongoDoc {
        /// <summary>
        /// 自增 Id
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }
        /// <summary>
        /// 程序启动的参数
        /// </summary>
        public string StartArgs { get; set; }
        /// <summary>
        /// 程序的版本
        /// </summary>
        public string AppVersion { get; set; }
        /// <summary>
        /// 启动时间点（本地时间）
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 同步的时间（服务器时间）
        /// </summary>
        public DateTime SyncServerTime { get; set; }
        /// <summary>
        /// 是否启动成功
        /// </summary>
        public bool IsStartSuccess { get; set; }
        /// <summary>
        /// 启动耗时多少 Ms
        /// </summary>
        public long StartDurationMs { get; set; }
        /// <summary>
        /// 启动失败的原因
        /// </summary>
        public string StartFailedReason { get; set; }
        /// <summary>
        /// 连续启动失败的次数
        /// </summary>
        public int ContinueFailedTimes { get; set; }
    }
}
