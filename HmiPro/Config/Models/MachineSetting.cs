using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;

namespace HmiPro.Config.Models {

    /// <summary>
    /// 机台Oee的相关属性
    /// <author>ychost</author>
    /// <date>2017-01-09</date>
    /// </summary>
    public class MachineSetting {
        /// <summary>
        /// 机台编码
        /// </summary>
        public string Code { get; set; } = "";

        /// <summary>
        /// Oee速度参数
        /// </summary>
        public string OeeSpeed { get; set; } = "";

        /// <summary>
        /// Mq需要的速度参数
        /// </summary>
        public string MqNeedSpeed { get; set; } = "";
        /// <summary>
        /// Oee速度最大值，可能来自于设定值，也可能来自于Mq或者Plc，根据OeeSpeedType而定
        /// </summary>
        public object OeeSpeedMax { get; set; }
        /// <summary>
        /// 确定OeeSpeedMax的来源
        /// </summary>
        public OeeActions.CalcOeeSpeedType OeeSpeedType = OeeActions.CalcOeeSpeedType.Unknown;

        /// <summary>
        /// 机台状态速度，管理机台的开停机
        /// </summary>
        public string StateSpeed { get; set; } = "";

        /// <summary>
        /// 模块ip
        /// </summary>
        public string[] CpmModuleIps { get; set; } = new string[0];
        /// <summary>
        /// 回填参数
        /// </summary>
        public string[] DPms { get; set; } = new string[0];


        /// <summary>
        /// Hmi本身的Ip
        /// </summary>
        public string HmiIp { get; set; } = "";

        /// <summary>
        /// 记米参数
        /// </summary>
        public string NoteMeter { get; set; } = "";

        /// <summary>
        /// 火花值参数
        /// </summary>
        public string Spark { get; set; } = "";

        /// <summary>
        /// Od值
        /// </summary>
        public string Od { get; set; } = "";

        /// <summary>
        /// 总电能
        /// </summary>
        public string totalPower { get; set; } = "总电能";
        /// <summary>
        /// 放线盘的数量
        /// </summary>
        public int StartTrayNum { get; set; } = 1;
    }
}
