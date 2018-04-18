using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 打 Rfid 卡，上机、下机之类的
    /// <author>ychost</author>
    /// <date>2018-4-18</date>
    /// </summary>
    public class PrintCardWithRfid {
        /// <summary>
        /// 人员Rfid 卡
        /// </summary>
        public string rfid { get; set; }

        /// <summary>
        /// 上机打卡、下机打卡
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 机台编码
        /// </summary>
        public string macCode { get; set; }
    }
}
