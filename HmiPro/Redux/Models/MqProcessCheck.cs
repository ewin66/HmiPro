
using System;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 制程质检
    /// <author>ychost</author>
    /// <date>2018-4-25</date>
    /// </summary>
    public class MqProcessCheck {
        public string seqCode { get; set; } // 工序编号
        public string seqName { get; set; } // 工序名称
        public string proGgxh { get; set; } // 规格型号
        public string detectionItem { get; set; } // 检测项
        public string produceCod { get; set; } // 检测标准
        public string produceType { get; set; } // 格式 input:select
        public string selectParam { get; set; } // select的参数 以";"分隔
        public string unit { get; set; } // 单位
        public string[] selectParamArr { get; set; } // select参数数组
    }

    /// <summary>
    /// 制程质检 回传
    /// <author>ychost</author>
    /// <date>2018-4-25</date>
    /// </summary>
    public class MqProcessCheckResult {
        public string workCode; // 工单
        public string macCode; // 机台编号
        public string classType; // 班次
        public string seqCode;// 工序编码
        public string seqName; // 工序名称
        public string proGgxh; // 规格型号
        public string detectionItem; // 检测项
        public string produceCod; // 检测标准
        public string produceResult; // 检测实际值
        public string pass; // 是否合格 合格:不合格
        public string unit; // 单位
    }
}
