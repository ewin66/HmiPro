using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;
using YCsharp.Util;

namespace HmiPro.Mocks {
    /// <summary>
    /// 模拟数据
    /// </summary>
    public static class AlarmMocks {
        public static MqAlarm CreateOneAlarm(int code = -1) {
            return new MqAlarm() {
                alarmType = AlarmType.OtherErr,
                axisCode = "Mock AxisCode",
                code = code,
                CpmName = "Mock Cpm",
                message = "测试报警",
                time = YUtil.GetUtcTimestampMs(DateTime.Now),
                meter = 11.3f,
                workCode = "Mock Work Code",
            };
        }
    }
}
