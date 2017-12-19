using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 排产任务模型，从服务器反序列化而来
    /// <author>ychost</author>
    /// <date>2017-12-19</date>
    /// </summary>
    public class MqSchTask {
        /// <summary>
        /// 当前任务序号
        /// </summary>
        public int CurPlanIndex { get; set; }
        /// <summary>
        /// 工单编码
        /// </summary>
        public string workcode { get; set; }
        /// <summary>
        /// 标准速度
        /// </summary>
        public double speed { get; set; }
        /// <summary>
        /// 工序编码
        /// </summary>
        public string seqcode { get; set; }
        /// <summary>
        /// 物料信息
        /// </summary>
        public List<PmtmsItem> pmtms { get; set; }
        /// <summary>
        /// 计划开始时间
        /// </summary>
        public MesTime pstime { get; set; }
        /// <summary>
        /// 轴号信息
        /// </summary>
        public List<AxisParamItem> axisParam { get; set; }
        /// <summary>
        /// 主操作手
        /// </summary>
        public string main_by { get; set; }
        /// <summary>
        /// 计划
        /// </summary>
        public string productstate { get; set; }
        /// <summary>
        /// 工单id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 副操作手
        /// </summary>
        public string vice_by { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string reaxistype { get; set; }
        /// <summary>
        /// bom 表信息
        /// </summary>
        public List<Dictionary<string, object>> bom { get; set; }
        /// <summary>
        /// 交货日期
        /// </summary>
        public MesTime delidate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int schedule { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int useminute { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int priority { get; set; }
        /// <summary>
        /// 轴数量统计
        /// </summary>
        public int axiscount { get; set; }
        /// <summary>
        /// 计划结束时间
        /// </summary>
        public MesTime pdtime { get; set; }
        /// <summary>
        /// 实际开始时间
        /// </summary>
        public string fdtime { get; set; }
        /// <summary>
        /// 实际结束时间
        /// </summary>
        public string fstime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int step { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int fspeed { get; set; }
        /// <summary>
        /// 机台编码
        /// </summary>
        public string maccode { get; set; }
    }
    public class PmtmsItem {
        /// <summary>
        /// 
        /// </summary>
        public string materil { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double matecount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string workcode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string seqcode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public MesTime ptime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string maccode { get; set; }
    }

    public class MesTime {
        /// <summary>
        /// 
        /// </summary>
        public int nanos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int minutes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int seconds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int hours { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int month { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int year { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int timezoneOffset { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int day { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int date { get; set; }
    }

    public class AxisParamItem {
        /// <summary>
        /// 
        /// </summary>
        public string product { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string workcode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string seqcode { get; set; }
        /// <summary>
        /// 红
        /// </summary>
        public string color { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int length { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int axiscount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string axiscode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int step { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string maccode { get; set; }
    }



}
