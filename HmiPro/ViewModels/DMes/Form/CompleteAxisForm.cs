using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.ViewModels.Sys;

namespace HmiPro.ViewModels.DMes.Form {
    /// <summary>
    /// 确认完成一轴，手动点完成
    /// <author>ychost</author>
    /// <date>2018-4-16</date>
    /// </summary>
    public class CompleteAxisForm : BaseForm {
        /// <summary>
        /// 完成原因
        /// </summary>
        [Display(Name = "结束状态")]
        public CompleteStatus CompleteStatus { get; set; }

    }

    /// <summary>
    /// 完成一轴的原因
    /// </summary>
    public enum CompleteStatus {
        [Display(Name = "正常结束")]
        Exception,
        [Display(Name = "异常结束")]
        Normal,
    }
}
