using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using Newtonsoft.Json;
using YCsharp.Service;

namespace HmiPro.Redux.Effects {
    public class PipeEffects {
        /// <summary>
        /// 往命名管道里面异步写入数据
        /// <author>ychost</author>
        /// <date>2018-1-22</date>
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<PipeActions.WriteRest, bool> WriteString;

        public readonly LoggerService Logger;
        public PipeEffects() {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            initWriteStringAsync();
        }


        private void asyncSend(IAsyncResult iar) {
            try {
                using (var pipeStream = (NamedPipeClientStream)iar.AsyncState) {
                    pipeStream.EndWrite(iar);
                    pipeStream.Flush();
                    App.Store.Dispatch(new SimpleAction(PipeActions.WRITE_STRING_SUCCESS));
                    Logger.Info("写入管道成功", true, ConsoleColor.White, 36000);
                }
            } catch (Exception e) {
                Logger.Error("往管道写入数据失败", e);
                App.Store.Dispatch(new SimpleAction(PipeActions.WRITE_STRING_FAILED, e));
            }
        }
        private void initWriteStringAsync() {
            WriteString = App.Store.asyncAction<PipeActions.WriteRest, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() => {
                        try {
                            NamedPipeClientStream pipeStream = new NamedPipeClientStream(instance.PipeServerName, instance.PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                            pipeStream.Connect(3000);
                            var str = JsonConvert.SerializeObject(instance.RestData);
                            byte[] buffer = Encoding.UTF8.GetBytes(str);
                            pipeStream.BeginWrite(buffer, 0, buffer.Length, asyncSend, pipeStream);
                            return true;
                        } catch (Exception e) {
                            App.Store.Dispatch(new SimpleAction(PipeActions.WRITE_STRING_FAILED, e));
                            Logger.Error("往管道写入数据失败", e);
                        }
                        return false;
                    });
                });
        }
    }
}
