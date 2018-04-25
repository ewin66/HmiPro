using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.ViewModels.DMes.Form.ProcessQi {
    /// <summary>
    /// 拉丝工序
    /// <author>ychhost</author>
    /// <date>2018-4-22</date>
    /// </summary>
    public class ProcessLs {
        public float 直径 { get; set; }
        public float 延伸率 { get; set; }
        public PLs表面观感 表面观感 { get; set; }
        public string 抗张强度 { get; set; }
        public List<PLs表面观感> 测试项目 { get; set; } = new List<PLs表面观感>();
    }

    public enum PLs表面观感 {
        氧化,
        有斑点,
        有毛刺
    }
}
