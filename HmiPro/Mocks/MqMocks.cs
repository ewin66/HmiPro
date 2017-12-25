using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Helpers;
using HmiPro.Redux.Models;
using Newtonsoft.Json;
using YCsharp.Util;

namespace HmiPro.Mocks {
    /// <summary>
    /// Mq测试数据
    /// </summary>
    public static class MqMocks {
        public static MqScanMaterial CreateScanMaterial() {
            return YUtil.GetJsonObjectFromFile<MqScanMaterial>(AssetsHelper.GetAssets().MockMqScanMaterial);
        }
    }
}
