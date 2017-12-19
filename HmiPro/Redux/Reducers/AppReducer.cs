using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Reducers {

    public struct AppState {
        public string Type;
        public CpmReducer.State CpmState;
        public SysReducer.State SysState;
        public MqReducer.State MqState;
        /// <summary>
        /// 保存所有执行的动作，
        /// Actions:ExectedTime
        /// </summary>
        public static IDictionary<string, DateTime> ExectedActions = new ConcurrentDictionary<string, DateTime>();
    }
}
