using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Services;
using Reducto;

namespace HmiPro.Redux.Reducers {
    /// <summary>
    /// cpm相关状态处理逻辑
    /// <date>2017-12-17</date>
    /// <author>ychost</author>
    /// </summary>
    public static class CpmReducer {

        /// <summary>
        /// 采集参数存储的状态和数据
        /// </summary>
        public struct CpmState {
            /// <summary>
            /// 接受到更新的所有参数
            /// </summary>
            public List<Cpm> UpdatedCpmsAll;
            /// <summary>
            /// 接受到更新的差异参数
            /// </summary>
            public List<Cpm> UpdatedCpmsDiff;

            public string MachineCode;

        }


        public static SimpleReducer<CpmState> Create() {
            return new SimpleReducer<CpmState>().When<CpmActions.Init>((state, action) => {
                state.UpdatedCpmsAll?.Clear();
                state.UpdatedCpmsDiff?.Clear();
                state.UpdatedCpmsAll = new List<Cpm>();
                state.UpdatedCpmsDiff = new List<Cpm>();
                return state;
            })
            .When<CpmActions.StartServerSuccess>((state, action) => {
                state.UpdatedCpmsAll.Clear();
                state.UpdatedCpmsDiff.Clear();
                return state;
            }).When<CpmActions.StartServerFailed>((state, action) => {
                state.UpdatedCpmsAll.Clear();
                state.UpdatedCpmsDiff.Clear();
                return state;
            }).When<CpmActions.CpmUpdateDiff>((state, action) => {
                state.UpdatedCpmsDiff = action.Cpms;
                state.MachineCode = action.MachineCode;
                return state;
            }).When<CpmActions.CpmUpdatedAll>((state, action) => {
                state.UpdatedCpmsAll = action.Cpms;
                state.MachineCode = action.MachineCode;
                return state;
            });
        }
    }
}
