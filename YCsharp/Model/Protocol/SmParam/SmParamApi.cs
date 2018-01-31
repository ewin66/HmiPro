using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCsharp.Model.Procotol.SmParam {
    /// <summary>
    /// 电科智联协议对外的api
    /// <date>2017-10-06</date>
    /// <author>ychost</author>
    /// </summary>
    public static class SmParamApi {
        /// <summary>
        /// 创建协议包
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="sendData">发送数据</param>
        /// <param name="aimType">类型</param>
        /// <param name="moduleAddr">模块地址</param>
        /// <returns></returns>
        public static byte[] BuildParamPackage(
            byte cmd,
            List<SmSend> sendData,
            byte aimType = (byte)SmFrame.IndustryAimType,
            List<byte> moduleAddr = null) {
            if (moduleAddr == null) {
                moduleAddr = new List<byte>(8) { 0, 0, 0, 0, 0, 0, 0, 0 };
            }
            return SmPackage.BuildParamPackage(moduleAddr, cmd, sendData, aimType);
        }


        /// <summary>
        /// 构建报警命令报警包
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="operate">可设置打开或者关闭报警</param>
        /// <param name="aimType"></param>
        /// <returns></returns>
        public static byte[] BuildAlarmPackage(List<byte> addr, SmAction operate,
            byte aimType = (byte)SmFrame.IndustryAimType) {
            return SmPackage.BuildAlarmPackage(addr, operate, aimType);
        }

        /// <summary>
        /// 获取包类型
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static SmPackageType GetPackageType(byte[] buffer, int offset, int count) {
            return SmPackage.GetPackageType(buffer, offset, count);
        }

        /// <summary>
        /// 检查包是否满足协议
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool AsserIsPackage(byte[] buffer, int offset, int count) {
            return SmPackage.AsserIsPackage(buffer, offset, count);
        }
    }
}
