using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config.Models;
using HmiPro.ViewModels.Sys;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 制程质检
    /// <author>ychost</author>
    /// <date>2018-4-20</date>
    /// </summary>
    public class ProcessQiTab : BaseTab {
        public object Form { get; set; }


        public void BindSource(object form) {
            this.Form = form;
        }
    }
}
