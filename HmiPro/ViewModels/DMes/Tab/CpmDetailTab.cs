using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Redux.Models;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 采集参数的酷炫详情界面
    /// <author>ychost</author>
    /// <date>2018-1-29</date>
    /// </summary>
    public class CpmDetailTab : BaseTab {
        /// <summary>
        /// 保存用户操作属性
        /// </summary>
        public CpmDetailViewStore ViewStore { get; set; }

        /// <summary>
        /// 在线参数列表
        /// </summary>
        public ObservableCollection<Cpm> OnlineCpms { get; set; }

        /// <summary>
        /// 绑定绘图参数
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="sourceDict"></param>
        public void BindSource(string machineCode, IDictionary<int, Cpm> sourceDict) {
            OnlineCpms = new ObservableCollection<Cpm>();
            ViewStore = App.Store.GetState().ViewStoreState.CpmDetailViewDict[machineCode];
            //保证显示顺序和配置顺序一致
            foreach (var pair in MachineConfig.MachineDict[machineCode].CodeToAllCpmDict) {
                //忽略掉一些不需要绘图的参数
                //比如自定义的 Oee、Rfid、火花值、转义参数等等
                var cpmInfo = pair.Value;
                if (cpmInfo.MethodName.HasValue && cpmInfo.MethodName.Value == CpmInfoMethodName.Escape || cpmInfo.Code < 500) {
                    continue;
                }
                string[] filters = { "火花", "米", "P", "I", "D", "方向", "长度", "报警", "系数", "率", "距离", "选择", "模", "RFID", "OEE","卡","设","比","最", };
                var beFilted = false;
                if (cpmInfo.Code >= 500) {
                    foreach (var str in filters) {
                        if (cpmInfo.Name.ToUpper().Contains(str)) {
                            beFilted = true;
                            break;
                        }
                    }
                }
                if (!beFilted) {
                    OnlineCpms.Add(sourceDict[pair.Key]);
                }
            }
        }
    }
}
