using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 每一轴洛轴后回传给服务器的数据
    /// <author>ychost</author>
    /// <date>2017-12-21</date>
    /// </summary>
    public class MqUploadManu {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }

        public string courseCode { get; set; } //工单编号
                                               //机台编码
        public string macCode { get; set; }
        //轴名称
        public string axisName { get; set; }
        //工序编码
        public string seqCode { get; set; }
        //收线时间
        public Int64 sxTime { get; set; }
        //完成状态(正常结束；异常结束；) 
        public string status { get; set; }
        public int step { get; set; }

        //放线轴rfid,多个以英文逗号隔开
        public string rfids_begin { get; set; }

        public Int64 actualBeginTime { get; set; } //实际生产开始时间   从准备时间开始算起
        public Int64 actualEndTime { get; set; }  //实际生产结束时间   从落轴停机开始算起

        //实际物料配送到位时间  叉车配料或人手推。 这个字段以后需要叉车或手持机告诉你。现在可以随意给个值
        public Int64 acutalDispatchTime { get; set; }

        //收线rfid. 收线只有一个轴
        public string rfid_end { get; set; }

        //人员RFID
        public String empRfid { get; set; }
        //生产长度
        public double axixLen { get; set; }
        //调试时间
        public long testTime { get; set; }
        //调试长度
        public float testLen { get; set; }
        //平均速度
        public float speed { get; set; }
        //产品规格型号
        public String proGgxh { get; set; }
        //落轴：yes，非落轴：no
        public string mqType { get; set; }
    }
}
