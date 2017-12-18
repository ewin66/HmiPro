using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;

namespace HmiPro.Helpers {
    /// <summary>
    /// 静态文件辅助类
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public static class AssetsHelper {
        private static string assetsFolder;

        public static void Init(string folder) {
            assetsFolder = folder;
            assets = new Assets(assetsFolder);
        }

        private static Assets assets;

        public static Assets GetAssets() {
            if (assets == null) {
                throw new Exception("请初始化 AssetsHelper.Init(folder)");
            }
            return assets;
        }
    }
}
