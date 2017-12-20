using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 报警页面
    /// <date>2017-12-20</date>
    /// <author>ychost</author>
    /// </summary>
    public class AlarmTab : BaseTab {
        public ObservableCollection<MqAlarm> Alarms { get; set; }

        public void BindSource(ObservableCollection<MqAlarm> alarms) {
            Alarms = alarms;
        }
    }
}
