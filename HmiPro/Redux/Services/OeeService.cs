using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Services {
    /// <summary>
    /// <date>2017-12-20</date>
    /// <author>ychost</author>
    /// </summary>
    public class OeeService {
        public OeeService() {
            UnityIocService.AssertIsFirstInject(GetType());
        }

        /// <summary>
        /// 获取机台开机运行时间
        /// </summary>
        /// <returns></returns>
        public double GetMachineRunTimeSec(IList<MachineState> machineStates, float currentSpeed) {
            double runTimeSec = -1;
            //开工时间
            var workTime = YUtil.GetWorkTime(8, 20);
            //只有一个状态的情况
            if (machineStates.Count == 1) {
                if (machineStates[0].StatePoint == MachineState.State.Start) {
                    runTimeSec = (DateTime.Now - machineStates[0].Time).TotalSeconds;
                } else {
                    runTimeSec = (machineStates[0].Time - workTime).TotalSeconds;
                }
                //多个状态的情况
            } else if (machineStates.Count > 1) {
                for (var i = 0; i < machineStates.Count - 1; i += 1) {
                    var preeState = machineStates[i];
                    var nextState = machineStates[i + 1];
                    if (preeState.StatePoint == MachineState.State.Start && nextState.StatePoint == MachineState.State.Stop) {
                        runTimeSec += (nextState.Time - preeState.Time).TotalSeconds;
                    }
                }
                //开机时间为开工之前
                if (machineStates[0].StatePoint == MachineState.State.Stop) {
                    runTimeSec += (machineStates[0].Time - workTime).TotalSeconds;
                }
                //当前正在运转
                if (machineStates.Last().StatePoint == MachineState.State.Start) {
                    runTimeSec += (DateTime.Now - machineStates[0].Time).TotalSeconds;
                }
                //没有保留的历史状态
            } else if (machineStates.Count == 0) {
                //机台当前正在运转，则认为从上班时间到现在未停过机
                if (currentSpeed > 0) {
                    runTimeSec = (DateTime.Now - workTime).TotalSeconds;
                    //机台未运转，则认为从上班时间到现在未开过机
                } else {
                    runTimeSec = 0;
                }
            }
            return runTimeSec;
        }

        /// <summary>
        ///<todo>获取机台调机时间</todo> 
        /// </summary>
        /// <returns></returns>
        public double GetMachineDebugTimeSec() {
            return 0;
        }

    }
}
