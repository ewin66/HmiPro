using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpf.CodeView;
using HmiPro.Redux.Models;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 报警页面
    /// <author>ychost</author>
    /// <date>2017-12-20</date>
    /// </summary>
    public class AlarmTab : BaseTab {
        /// <summary>
        /// 报警显示的列表
        /// </summary>
        public ObservableCollection<MqAlarm> Alarms { get; set; }
        /// <summary>
        /// 保存用户操作数据，比如选中的第几个报警等等
        /// </summary>
        public DMesCoreViewStore ViewStore { get; set; }

        public void BindSource(string machinieCode, ObservableCollection<MqAlarm> alarms) {
            Alarms = alarms;
            ViewStore = App.Store.GetState().ViewStoreState.DMewCoreViewDict[machinieCode];
        }
    }
}
