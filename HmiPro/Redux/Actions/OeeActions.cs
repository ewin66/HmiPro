using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging.Simple;
using HmiPro.Config;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// Oee相关动作
    /// <author>ychost</author>
    /// <date>2017-12-25</date>
    /// </summary>
    public static class OeeActions {
        [Obsolete("现在是每次收到参数就更新 Oee，不需要定时器了")]
        public static readonly string START_CALC_OEE_TIMER = "[Oee] Start Calc Oee Timer";
        /// 更新Oee的部分参数
        public static readonly string UPDATE_OEE_PARTIAL_VALUE = "[Oee] Update Oee Parital Value";

        //初始化
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

        public enum CalcOeeSpeedType {
            //Plc的设定最大值
            MaxSpeedPlc = 1,
            //Mq设定最大值
            MaxSpeedMq = 2,
            //经验设定的最大值
            MaxSpeedSetting = 3,
            //未知
            Unknown = -1,
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
                if (TimeEff.HasValue) {
                    TimeEff = (float)Math.Round(TimeEff.Value, HmiConfig.MathRound);
                }
                if (SpeedEff.HasValue) {
                    SpeedEff = (float)Math.Round(SpeedEff.Value, HmiConfig.MathRound);
                }
                if (QualityEff.HasValue) {
                    QualityEff = (float)Math.Round(QualityEff.Value, HmiConfig.MathRound);
                }
            }
        }
    }
}
