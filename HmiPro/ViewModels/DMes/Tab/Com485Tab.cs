using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpf.CodeView;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 485状态
    /// </summary>
    public class Com485Tab : BaseTab {

        public ObservableCollection<Com485SingleStatus> Com485Status { get; set; }
        public DMesCoreViewStore ViewStore { get; set; }

        public void BindSource(string machineCode, IList<Com485SingleStatus> status) {
            Com485Status = new ObservableCollection<Com485SingleStatus>();
            ViewStore = App.Store.GetState().ViewStoreState.DMewCoreViewDict[machineCode];
            foreach (var s in status) {
                Com485Status.Add(s);
            }
        }
    }
}
