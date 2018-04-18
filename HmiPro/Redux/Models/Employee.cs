using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 操作员工
    /// <author>ychost</author>
    /// <date>2018-4-17</date>
    /// </summary>
    public class Employee {
        /// <summary>
        /// 人员关联的 Rfid
        /// </summary>
        public string Rfid { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 打卡机台
        /// </summary>
        public string MachineCode { get; set; }
        /// <summary>
        /// 招聘
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// 打卡时间
        /// </summary>
        public DateTime PrintCardTime { get; set; }
    }
}
