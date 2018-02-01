using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 机台开机、停机态点
    /// <author>ychost</author>
    /// <date>2018-1-4</date>
    /// </summary>
    public class MachineState {
        public DateTime Time;
        public enum State {
            Start,
            Stop,
            Repair,
            RepairCompleted
        }
        public State StatePoint;
    }

    /// <summary>
    /// 调机状态点
    /// </summary>
    public class MachineDebugState {
        public enum State {
            Start,
            Stop
        }

        public DateTime Time;
        public State StatePoint;
        public float Meter;
    }


}
