using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 报警配置
    /// 某些情况只有最大值和最小值
    /// 某些情况为最大值和标准值，这时候最小值就为 2x标准值-最大值
    /// <date>2017-12-20</date>
    /// </summary>
    public class AlarmBomCheck {
        public Cpm Cpm;
        /// <summary>
        /// 最大值的Bom键
        /// </summary>
        public string MaxBomKey;
        /// <summary>
        /// 最小值的Bom键
        /// </summary>
        public string MinBomKey;
        /// <summary>
        /// 标准值的Bom键
        /// </summary>
        public string StdBomKey;
    }

}
