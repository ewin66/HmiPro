using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
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
                axisCode = "GD2017122701ls01",
                code = code,
                CpmName = "直径",
                message = "直径值超过Plc设定的最大值",
                time = YUtil.GetUtcTimestampMs(DateTime.Now),
                meter = 1103.3f,
                workCode = "GD2017122701",
                machineCode = MachineConfig.MachineDict.First().Key
            };
        }
    }
}
