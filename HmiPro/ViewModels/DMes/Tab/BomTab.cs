using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 排产Bom页数据
    /// <date>2017-12-23</date>
    /// <author>ychost</author>
    /// </summary>
    [Obsolete("请使用SchTaskBomViewModel")]
    public class BomTab : BaseTab {
      public  List<Dictionary<string, object>> Boms { get; set; }

        public void BindSource(List<Dictionary<string, object>> boms) {
            Boms = boms;
        }
    }
}
