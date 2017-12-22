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
    /// <date>2017-12-20</date>
    /// <author>ychost</author>
    /// </summary>
    public class AlarmTab : BaseTab {
        public virtual ObservableCollection<MqAlarm> Alarms { get; set; } = new ObservableCollection<MqAlarm>();

        public void Init(ObservableCollection<MqAlarm> alarms) {
            Alarms.AddRange(alarms);
        }
    }
}
