using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 上传报警
    /// <date>2017-12-13</date>
    /// <author>ychost</author>
    /// </summary>
    public class MqAlarm {
        /// <summary>
        /// 机台编码
        /// </summary>
        public string machineCode { get; set; }
        /// <summary>
        /// 报警内容
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 轴号编码
        /// </summary>
        public string axisCode { get; set; }
        /// <summary>
        /// 工单编码
        /// </summary>
        public string workCode { get; set; }
        /// <summary>
        /// utc时间戳，毫秒级别
        /// </summary>
        public long time { get; set; }
        /// <summary>
        /// 报警对应的米数
        /// </summary>
        public float meter { get; set; }
        /// <summary>
        /// 操作人员
        /// </summary>
        public HashSet<string> employees { get; set; }
        /// <summary>
        /// 放线处rfid
        /// </summary>
        public HashSet<string> startRfids { get; set; }
        /// <summary>
        /// 报警类型
        /// </summary>
        public string alarmType { get; set; }

        /// <summary>
        /// 报警编码，一般为CpmCode
        /// </summary>
        public int code { get; set; }

    }

    public static class AlarmType {
        public static readonly string OdErr = "直径超限";
        public static readonly string SparkErr = "火花报警";
        public static readonly string PounchErr = "打卡错误";
        public static readonly string ClearErr = "清零错误";
        public static readonly string OtherErr = "其它异常";
    }
}
