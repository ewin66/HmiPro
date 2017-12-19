using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using Reducto;

namespace HmiPro.Redux.Reducers {

    public static class MqReducer {
        public struct State {
            public bool IsListeningSchTask;
            public MqSchTask MqSchTask;
            public bool IsStartedUploadCpmsInterval;
        }

        public static SimpleReducer<MqReducer.State> Create() {
            return new SimpleReducer<State>().When<MqActiions.StartListenSchTaskSuccess>((state, action) => {
                if (state.IsListeningSchTask) {
                    throw new Exception("请勿重复监听排产任务消息队列");
                }
                state.IsListeningSchTask = true;
                state.MqSchTask = null;
                return state;
            }).When<MqActiions.StartListenSchTask>((state, action) => {
                state.IsListeningSchTask = false;
                return state;
            }).When<MqActiions.StartListenSchTaskFailed>((state, action) => {
                state.IsListeningSchTask = false;
                return state;
            }).When<MqActiions.SchTaskAccept>((state, action) => {
                state.MqSchTask = action.MqSchTask;
                return state;
            }).When<MqActiions.StartUploadCpmsInterval>((state, action) => {
                if (state.IsStartedUploadCpmsInterval) {
                    throw new Exception("请勿重复开启采集参数周期上传定时器");
                }
                state.IsStartedUploadCpmsInterval = true;
                return state;
            });
        }
    }
}
