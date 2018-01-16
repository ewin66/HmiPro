using System;
using System.Collections.Generic;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Reducers;
using HmiPro.ViewModels.DMes.Tab;
using YCsharp.Util;

namespace HmiPro.ViewModels.DMes {
    [POCOViewModel]
    public class CraftBomViewModel : BaseTab {
        public virtual string MachineCode { get; set; }
        public virtual string WorkCode { get; set; }
        public virtual INavigationService NavigationSerivce => null;
        public virtual IList<Dictionary<string, object>> Boms { get; set; } = new List<Dictionary<string, object>>();


        /// <summary>
        /// 必须要无参数构造函数，不然导航会出错
        /// </summary>
        public CraftBomViewModel() {

        }

        public CraftBomViewModel(string machineCode, string workCode, IList<Dictionary<string, object>> boms) {
            MachineCode = machineCode;
            WorkCode = workCode;
            Boms = makeZhsBom(boms);
        }

        /// <summary>
        /// 汉化bom
        /// 只显示汉化字典里面有的数据
        /// </summary>
        private IList<Dictionary<string, object>> makeZhsBom(IList<Dictionary<string, object>> boms) {
            IList<Dictionary<string, object>> zhsBoms = new List<Dictionary<string, object>>();
            foreach (var bom in boms) {
                var zshBom = new Dictionary<string, object>();
                foreach (var pair in bom) {
                    //bom字典里面是下划线，而变量名是骆驼峰
                    var key = YUtil.CamelToUnderScore(pair.Key);
                    if (HmiConfig.CraftBomZhsDict.TryGetValue(key, out var zhs)) {
                        zshBom[zhs] = pair.Value;
                    }
                }
                zhsBoms.Add(zshBom);
            }
            return zhsBoms;
        }

        /// <summary>
        /// 清空Bom
        /// </summary>
        public void Clear() {
            Application.Current.Dispatcher.Invoke(() => {
                Boms.Clear();
            });
        }

        /// <summary>
        /// 自动导入了 INotifyPropertyChanged
        /// 用Virutal就行
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="workcode"></param>
        /// <param name="bom"></param>
        /// <returns></returns>
        public static CraftBomViewModel Create(string machineCode, string workcode, IList<Dictionary<string, object>> bom) {
            return ViewModelSource.Create(() => new CraftBomViewModel(machineCode, workcode, bom));
        }

    }
}