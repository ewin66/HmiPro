using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Config.Models {
    /// <summary>
    /// 机台固有设定，将机台的固有配置比如最高转速进行统一配置
    /// <author>ychost</author>
    /// <date>2018-01-09</date>
    /// </summary>
    public class MachineSetting {
        /// <summary>
        /// 机台编码
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Code { get; set; }
        //管理的模块Ip
        public string CpmIps { get; set; }
        //Hmi的Ip
        public string HmiIp { get; set; }
        //机台所属的工序编码
        public string SeqCode { get; set; }

        public string Remark { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        public MachineSetting() {
            UpdateTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 机台Oee的相关属性
    /// <author>ychost</author>
    /// <date>2017-01-09</date>
    /// </summary>
    public class MachineOeeSetting {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Code { get; set; }
        //Oee速度名称
        public string OeeSpeedCpmName { get; set; }
        //经验设定的最大速度
        public float? OeeSpeedMax1Setting { get; set; }
        //最大速度来自采集参数的名称
        public string OeeSpeedMax2CpmName { get; set; }
        //是否从Mq中读取最大值
        public string OeeSpeedMax3MqKey { get; set; }
        //Mq需要Hmi上传的速度
        public string MqNeedSpeedCpmName { get; set; }
        public string Remark { get; set; }
        public DateTime? UpdateTime { get; set; }

        public MachineOeeSetting() {
            UpdateTime = DateTime.Now;
        }
    }
}
