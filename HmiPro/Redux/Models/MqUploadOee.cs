using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 实时上传Oee参数
    /// <date>2017-12-21</date>
    /// <author>ychost</author>
    /// </summary>
    public class MqUploadOee {
        /** 机台id */
        public int machineId { get; set; }

        /** 机台code */
        public string machineCode { get; set; }

        /** 工单编号 */
        public string courseCode { get; set; }

        /** 开机时间 */
        public long start { get; set; }

        /** 关机时间 */
        public long end { get; set; }

        /** 开机分钟数 */
        public int startMin { get; set; }

        /** 停机分钟数 */
        public int endMin { get; set; }

        /** 不良品 */
        public int rejects { get; set; }

        /** 零碎品 */
        public int bitsPieces { get; set; }

        /** 过量耗用品 */
        public int overdoes { get; set; }

        /** 所属步骤 */
        public int step { get; set; }
    }
}
