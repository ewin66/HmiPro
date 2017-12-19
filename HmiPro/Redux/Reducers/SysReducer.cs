using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using Reducto;

namespace HmiPro.Redux.Reducers {
    public static class SysReducer {
        public struct State {
            public bool HttpSystemIsStarted;
        }

        public static SimpleReducer<State> Create() {
            return new SimpleReducer<State>().When<SysActions.ShowSettingView>((state, action) => {
                return state;
            }).When<SysActions.StartHttpSystemSuccess>((state, action) => {
                state.HttpSystemIsStarted = true;
                return state;
            }).When<SysActions.StartHttpSystemFailed>((state, action) => {
                state.HttpSystemIsStarted = false;
                return state;
            }).When<SysActions.StartHttpSystem>((state, action) => {
                state.HttpSystemIsStarted = false;
                return state;
            });
        }
    }
}
