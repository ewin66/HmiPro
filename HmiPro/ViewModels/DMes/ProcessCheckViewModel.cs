using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.ViewModels.DMes.Tab;

namespace HmiPro.ViewModels.DMes {
    /// <summary>
    /// 制程质检
    /// <author>ychost</author>
    /// <date>2018-4-25</date>
    /// </summary>
    [POCOViewModel]
    public class ProcessCheckViewModel:BaseTab {
        public string SelectWorkCode { get; set; }

    }
}