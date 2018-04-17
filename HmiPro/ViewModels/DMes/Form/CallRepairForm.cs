using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.ViewModels.Sys;

namespace HmiPro.ViewModels.DMes.Form {
    /// <summary>
    /// <author>ychost</author>
    /// <date>2018-4-17</date>
    /// </summary>
    public class CallRepairForm : BaseForm {
        [Display(Name = "故障原因")]
        public CallRepairType RepairType { get; set; }
    }

    public enum CallRepairType {
        [Display(Name = "电气故障")]
        Electron,
        [Display(Name = "机械故障")]
        Machine
    }
}
