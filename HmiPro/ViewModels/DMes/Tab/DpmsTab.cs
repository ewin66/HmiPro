using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 用户回填参数
    /// <author>ychost</author>
    /// <date>2018-1-17</date>
    /// </summary>
    public class DpmsTab : BaseTab {
        public ObservableCollection<Dpm> Dpms { get; set; }

        public void BindSource(ObservableCollection<Dpm> dpms) {
            Dpms = dpms;
        }
    }
}
