using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// Mq传过来的人员卡
    /// <author>ychost</author>
    /// <date>201-1-20</date>
    /// </summary>
    public class MqEmpRfid {
        public string employeeCode { get; set; } // 生产部人员CODE ,用rfid代替
        public string type { get; set; } // 打卡类型（上班打卡、下班打卡、上机打卡、下机打卡）
        public string macCode { get; set; } // 机台编码
        //人名
        public string name { get; set; }

        public DateTime PrintTime { get; set; }
    }

    /// <summary>
    /// 线盘卡
    /// </summary>
    public class MqAxisRfid {
        //接收手持机端用
        /**
         * 接收手机传入的轴id
         */
        public string axis_id { get; set; }
        /**
         * 接收消息时间
         */
        public string date { get; set; }
        /**
         * 消息类型(axis_begin 代表上线   axis_end代表收线)
         */
        public string msg_type { get; set; }
        public string machine_id { get; set; }
        //发送给机台的参数
        //多个以,号隔开
        public string rfids { get; set; }
        //手持机扫描时间
        //取值：放线、收线
        public string msgType { get; set; }
        public string macCode { get; set; }

    }

    public static class MqRfidType {
        public static readonly string EmpStartWork = "上班";
        public static readonly string EmpEndWork = "下班";
        public static readonly string EmpStartMachine = "上机";
        public static readonly string EmpEndMachine = "下机";
        public static readonly string AxisStart = "放线";
        public static readonly string AxisEnd = "收线";
    }

}
