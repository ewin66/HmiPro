using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Helpers;
using YCsharp.Service;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// 数据库相关操作
    /// <date>2017-12-20</date>
    /// <author>ychost</author>
    /// </summary>
    public class DbCore {
        public readonly LoggerService Logger;
        public DbCore() {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }
    }
}
