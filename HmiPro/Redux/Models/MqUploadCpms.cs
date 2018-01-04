using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Model.Procotol.SmParam;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 给web端电子看板发送的模型
    /// <date>2017-07-20</date>
    /// <author>ychost</author>
    /// </summary>
    public class MqUploadCpms {

        public string machineCode { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public List<UploadParamInfo> paramInfoList { get; set; }

        /// <summary>
        /// 机台速度
        /// </summary>
        public object macSpeed { get; set; }

        /// <summary>
        /// 总电能
        /// </summary>
        public string totalPower { get; set; }
        /// <summary>
        /// 机台状态
        /// </summary>
        public string machineState { get; set; }

        /// <summary>
        /// 电流
        /// </summary>
        public string currentA { get; set; }
        public string currentB { get; set; }
        public string currentC { get; set; }
        /// <summary>
        /// 时间效率（开机率）
        /// </summary>
        public string TimeEff { get; set; }
        /// <summary>
        /// 速度效率
        /// </summary>
        public string SpeedEff { get; set; }
        /// <summary>
        /// 质量效率
        /// </summary>
        public string QualityEff { get; set; }
        /// <summary>
        /// 直径
        /// </summary>
        public string diameter { get; set; }

        public static class MachineState {
            public static readonly string Running = "运行";
            public static readonly string Closed = "关机";
            public static readonly string Repairing = "维修";
        }
    }




    /// <summary>
    /// 单个参数信息
    /// </summary>
    public class UploadParamInfo {
        /// <summary>
        /// 参数编码
        /// </summary>
        public string paramCode { get; set; }

        public string paramName { get; set; }


        public object paramValue { get; set; }

        public SmParamType valueType { get; set; }

        /// <summary>
        /// 这是timestamp
        /// 语言平台的限制统一用 unsigned int32
        /// 精确到毫秒
        /// </summary>
        public Int64 pickTimeStamp { get; set; }

        /// <summary>
        /// 是否异常
        /// </summary>
        public string isException { get; set; }
    }
}
