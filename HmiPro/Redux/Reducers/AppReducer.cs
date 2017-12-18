using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Reducers {

    public struct AppState {
        public string Type;
        public CpmReducer.CpmState CpmState;
        public SysReducer.SysState SysState;
    }
}
