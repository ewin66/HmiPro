using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging.Simple;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// Oee相关动作
    /// </summary>
    public static class OeeActions {
        public static readonly string START_CALC_OEE_TIMER = "[Oee] Start Calc Oee Timer";
        public static readonly string NOTIFY_OEE_CACLED = "[Oee] New Oee Calced";
        public static readonly string INIT = "[Oee] Init";

        public struct Init : IAction {
            public string Type() => INIT;
        }

        public struct StartCalcOeeTimer : IAction {
            public string Type() => START_CALC_OEE_TIMER;
            public double Interval;

            public StartCalcOeeTimer(double interval) {
                Interval = interval;
            }
        }

        public struct NotifyOeeCacled : IAction {
            public string Type() => NOTIFY_OEE_CACLED;
            /// <summary>
            /// 时间效率
            /// </summary>
            public float? TimeEff;
            /// <summary>
            /// 正品率
            /// </summary>
            public float? QualityEff;
            /// <summary>
            /// 速度效率
            /// </summary>
            public float? SpeedEff;
            /// <summary>
            /// Oee值
            /// </summary>
            public float? Oee;

            public string MachineCode;

            public NotifyOeeCacled(string machineCode, float? timeEff , float? qualityEff , float? speedEff ) {
                MachineCode = machineCode;
                TimeEff = timeEff;
                SpeedEff = speedEff;
                QualityEff = qualityEff;
                Oee = TimeEff * qualityEff * speedEff;
            }
        }
    }
}
