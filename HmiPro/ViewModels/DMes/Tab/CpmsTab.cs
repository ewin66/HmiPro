using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Redux.Models;
using YCsharp.Model.Procotol.SmParam;

namespace HmiPro.ViewModels.DMes.Tab {


    /// <summary>
    /// 实时参数显示Tab页
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public class CpmsTab : BaseTab {
        public ObservableCollection<Cpm> Cpms { get; set; } = new ObservableCollection<Cpm>();

        /// <summary>
        /// 绑定实时参数的数据源，数据源的Cpm触发NotifyProperityChanged直接会更新界面
        /// </summary>
        public void BindSource(string machineCode, IDictionary<int, Cpm> sourceDict) {
            //保证显示顺序和配置顺序一致
            foreach (var pair in MachineConfig.MachineDict[machineCode].CodeToAllCpmDict) {
                Cpms.Add(sourceDict[pair.Key]);
            }

        }
    }
}
