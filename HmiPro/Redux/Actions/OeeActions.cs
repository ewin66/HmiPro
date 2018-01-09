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
        /// <summary>
        /// 更新Oee的部分参数
        /// </summary>
        public static readonly string UPDATE_OEE_PARTIAL_VALUE = "[Oee] Update Oee Parital Value";

        public static readonly string CALC_OEE = "[Oee] Calc Oee";
        public static readonly string INIT = "[Oee] Init";

        public struct Init : IAction {
            public string Type() => INIT;
        }


        public struct CalcOee : IAction {
            public string Type() => CALC_OEE;
            public string MachineCode;

            public CalcOee(string machineCode) {
                MachineCode = machineCode;
            }
        }

        public struct StartCalcOeeTimer : IAction {
            public string Type() => START_CALC_OEE_TIMER;
            public double Interval;

            public StartCalcOeeTimer(double interval) {
                Interval = interval;
            }
        }

        public enum CalcOeeSpeedType {
            //Plc的设定最大值
            MaxSpeedPlc = 1,
            //Mq设定最大值
            MaxSpeedMq = 2,
            //未知
            Unknown = 3,
        }

        public struct UpdateOeePartialValue : IAction {
            public string Type() => UPDATE_OEE_PARTIAL_VALUE;
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

            public UpdateOeePartialValue(string machineCode, float? timeEff, float? speedEff, float? qualityEff) {
                MachineCode = machineCode;
                TimeEff = timeEff;
                SpeedEff = speedEff;
                QualityEff = qualityEff;
                Oee = TimeEff * qualityEff * speedEff;
            }
        }
    }
}
