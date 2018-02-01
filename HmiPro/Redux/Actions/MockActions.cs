using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 模拟动作
    /// <author>ychost</author>
    /// <date>2018-01-11</date>
    /// </summary>
    public static class MockActions {
        public static readonly string MOCK_SCH_TASK_ACCEPT = "[Mock] Schedule Task Accept";
        public struct MockSchTaskAccpet : IAction {
            public string Type() => MOCK_SCH_TASK_ACCEPT;
            public MqSchTask SchTask;

            public MockSchTaskAccpet(MqSchTask task) {
                SchTask = task;
            }
        }
    }
}
