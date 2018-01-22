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
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<PipeActions.WriteRest, bool> WriteString;

        public readonly LoggerService Logger;
        public PipeEffects() {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            initWriteStringAsync();
        }

        private void initWriteStringAsync() {
            WriteString = App.Store.asyncAction<PipeActions.WriteRest, bool>(
                async (dispatch, getState, instance) => {
                    return await Task.Run(() => {
                        dispatch(instance);
                        try {
                            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(instance.PipeServerName, instance.PipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None)) {
                                Logger.Info("等待管道连接");
                                //
                                pipeClient.Connect(3000);
                                using (StreamWriter sw = new StreamWriter(pipeClient)) {
                                    var str = JsonConvert.SerializeObject(instance.RestData);
                                    var utf8Bytes = Encoding.UTF8.GetBytes(str);
                                    var utf8Str = Encoding.UTF8.GetString(utf8Bytes);
                                    sw.WriteLine(utf8Str);
                                    sw.Flush();
                                }
                            }
                            App.Store.Dispatch(new SimpleAction(PipeActions.WRITE_STRING_SUCCESS));
                            Logger.Info("写入管道成功");
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
