using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Service;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// 保存视图状态
    /// </summary>
    public class ViewStoreCore {
        public ViewStoreCore() {
            UnityIocService.AssertIsFirstInject(GetType());
        }
    }
}
