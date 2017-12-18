using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using Reducto;

namespace HmiPro.Redux.Reducers {
    public struct SysReducer {
        public struct SysState {

        }

        public static SimpleReducer<SysState> Create() {
            return new SimpleReducer<SysState>().When<SysActions.ShowSettingView>((state, action) => {
                return state;
            });
        }
    }
}
