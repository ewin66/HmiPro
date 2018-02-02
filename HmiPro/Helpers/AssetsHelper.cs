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
        /// <summary>
        /// 静态资源文件地址
        /// </summary>
        private static string assetsFolder;
        /// <summary>
        /// 全局唯一的静态资源文件对象
        /// </summary>
        private static Assets assets;

        /// <summary>
        /// 初始化全局唯一的静态资源文件对象
        /// </summary>
        /// <param name="folder">Assets 文件夹</param>
        public static void Init(string folder) {
            assetsFolder = folder;
            assets = new Assets(assetsFolder);
        }

        /// <summary>
        /// 获取全局的 Assets
        /// </summary>
        /// <returns>全局唯一的 Assets</returns>
        public static Assets GetAssets() {
            if (assets == null) {
                throw new Exception("请初始化 AssetsHelper.Init(folder)");
            }
            return assets;
        }
    }
}
