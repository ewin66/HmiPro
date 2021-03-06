﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.ViewModels.Sys;

namespace HmiPro.ViewModels.DMes.Form {
    /// <summary>
    /// 确认栈板上面轴的数量
    ///<author>ychost</author>
    ///<date>2018-1-24</date>
    /// </summary>
    public class PalletConfirmForm : BaseForm {
        public PalletConfirmForm(string machineCode, string rfid, int axisNum, string workcode) {
            MachineCode = machineCode;
            Rfid = rfid;
            AxisNum = axisNum;
            WorkCode = workcode;
        }
        [Display(Name = "轴数")]
        public int AxisNum { get; set; }

        [Display(Name = "机台")] public string MachineCode { get; }
        [Display(Name = "Rfid")] public string Rfid { get; }
        [Display(Name = "工单")] public string WorkCode { get; }
    }
}
