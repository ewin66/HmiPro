﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.ViewModels.Sys;

namespace HmiPro.ViewModels.DMes.Form {
    /// <summary>
    /// 确认打下机卡
    /// <author>ychost</author>
    ///<date>2018-4-18</date>
    /// </summary>
    public class ConfirmEndMachine : BaseForm {
        public ConfirmEndMachine(string message) {
            this.Message = message;
        }
        [Display(Name = "信息")]
        public string Message { get; private set; } = "确认打下机卡？";

    }
}
