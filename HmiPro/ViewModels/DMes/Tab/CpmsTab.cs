using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        public IDictionary<int, Cpm> cpmsDict = new Dictionary<int, Cpm>();

        public void Init(Machine machine) {
            foreach (var cpmPair in machine.CodeToAllCpmDict) {
                if (cpmPair.Value.IsShow) {
                    var cpm = new Cpm() {
                        Name = cpmPair.Value.Name,
                        Code = cpmPair.Value.Code,
                        Unit = cpmPair.Value.Unit,
                        Value = "暂无"
                    };
                    cpmsDict[cpm.Code] = cpm;
                    Cpms.Add(cpm);
                }
            }
        }

        /// <summary>
        /// 实时更新显示参数
        /// </summary>
        public void Update(IDictionary<int, Cpm> onlineCpmDict) {
            foreach (var pair in onlineCpmDict) {
                if (cpmsDict.ContainsKey(pair.Key)) {
                    var cpm = cpmsDict[pair.Key];
                    if (pair.Value.ValueType == SmParamType.Signal) {
                        var val = (float)pair.Value.Value;
                        cpm.Value = val.ToString("0.##");
                    } else {
                        cpm.Value = pair.Value.Value;
                    }
                }
            }
        }
    }
}
