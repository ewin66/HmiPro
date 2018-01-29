using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Redux.Models;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 采集参数的酷炫详情界面
    /// <author>ychost</author>
    /// <date>2018-1-29</date>
    /// </summary>
    public class CpmDetailTab : BaseTab {
        /// <summary>
        /// 
        /// </summary>
        public CpmDetailViewStore ViewStore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<Cpm> OnlineCpms { get; set; }


        public void BindSource(string machineCode, IDictionary<int, Cpm> sourceDict) {
            OnlineCpms = new ObservableCollection<Cpm>();
            ViewStore = App.Store.GetState().ViewStoreState.CpmDetailsiewDict[machineCode];
            //保证显示顺序和配置顺序一致
            foreach (var pair in MachineConfig.MachineDict[machineCode].CodeToAllCpmDict) {
                OnlineCpms.Add(sourceDict[pair.Key]);
            }
        }
    }
}
