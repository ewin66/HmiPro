using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSLib.App.SimpleUpdater;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using Newtonsoft.Json;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Services {
    public class SysService {
        public Updater AppUpdater;
        public readonly LoggerService Logger;
        public HttpListener HttpListener;

        public IDictionary<string, Action<HttpListenerResponse>> HttpSystemCmdDict =
            new ConcurrentDictionary<string, Action<HttpListenerResponse>>();

        public SysService() {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            initCmdExecers();
        }

        public Task<bool> StartHttpSystem(SysActions.StartHttpSystem startHttpSystem) {
            return Task.Run(() => {
                HttpListener = new HttpListener();
                //Logger.Debug("http 监听地址 " + startHttpSystem.Url);
                HttpListener.Prefixes.Add(startHttpSystem.Url);
                return Task.Run(() => {
                    try {
                        HttpListener.Start();
                        HttpListener.BeginGetContext(processHttpContext, null);
                        return true;
                    } catch {
                        return false;
                    }
                });
            });

        }

        /// <summary>
        /// 指定命令的执行者
        /// </summary>
        private void initCmdExecers() {
            HttpSystemCmdDict["update-app"] = execUpdateApp;
            HttpSystemCmdDict["get-state"] = execGetState;
            HttpSystemCmdDict["clear-task"] = execClearSchTask;
            HttpSystemCmdDict["close-app"] = execCloseApp;
        }

        private void execCloseApp(HttpListenerResponse response) {
            var rest = new HttpSystemRest();
            rest.DebugMessage = "即将关闭程序";
            outResponse(response, rest);
            App.Store.Dispatch(new SysActions.ShutdownApp());

        }


        /// <summary>
        /// http相关处理
        /// </summary>
        /// <param name="ar"></param>
        private void processHttpContext(IAsyncResult ar) {
            var context = HttpListener.EndGetContext(ar);
            HttpListener.BeginGetContext(processHttpContext, null);
            var response = context.Response;
            response.AddHeader("Server", "Http System For HmiPro");
            var request = context.Request;
            var path = request.Url.LocalPath;
            if (path.StartsWith("/") || path.StartsWith("\\"))
                path = path.Substring(1);
            var visit = path.Split(new char[] { '/', '\\' }, 2);
            var cmd = "";
            if (visit.Length > 0) {
                cmd = visit[0].ToLower();
            }
            response.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            Logger.Info($"Http接受到命令：{cmd}", false);
            if (HttpSystemCmdDict.TryGetValue(cmd, out var exec)) {
                exec(response);
            } else {
                outResponse(response, new HttpSystemRest() { DebugMessage = $"未知命令：{cmd}" });
            }
        }

        /// <summary>
        /// 清空所管理机台的所有任务
        /// </summary>
        /// <param name="response"></param>
        private void execClearSchTask(HttpListenerResponse response) {
            App.Store.Dispatch(new DMesActions.ClearSchTasks(MachineConfig.MachineDict.Keys.ToArray()));
            outResponse(response, new HttpSystemRest() { Message = $"清空Hmi{MachineConfig.HmiName}任务成功" });
        }

        private void execGetState(HttpListenerResponse responnse) {
            var rest = new HttpSystemRest();
            rest.Data = AppState.ExectedActions;
            rest.Hmi = MachineConfig.HmiName;
            rest.Message = "获取程序状态成功";
            outResponse(responnse, rest);
        }


        /// <summary>
        /// 程序检查自动更新
        /// </summary>
        /// <param name="response"></param>
        private void execUpdateApp(HttpListenerResponse response) {
            bool hasUpdate = CheckUpdate();
            var rest = new HttpSystemRest();
            if (hasUpdate) {
                rest.Message = "检查到更新";
                rest.Code = 0;
            } else {
                rest.Message = "未能检查到更新";
                rest.Code = 0;
            }
            if (hasUpdate) {
                StartUpdate();
            }
            outResponse(response, rest);
        }

        /// <summary>
        /// 向http客户端返回数据
        /// </summary>
        /// <param name="response"></param>
        /// <param name="rest"></param>
        private void outResponse(HttpListenerResponse response, HttpSystemRest rest) {
            try {
                using (var stream = response.OutputStream) {
                    var result = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(rest));
                    stream.Write(result, 0, result.Length);
                }
            } catch (Exception e) {
                Logger.Error("Http 系统回复异常", e);
            }
        }
        /// <summary>
        /// 检查是否存在更新
        /// </summary>
        public bool CheckUpdate() {
            if (AppUpdater == null) {
                AppUpdater = Updater.CreateUpdaterInstance(HmiConfig.UpdateUrl, "update_c.xml");
            }
            var result = AppUpdater.CheckUpdateSync();
            return result.HasUpdate;
        }

        /// <summary>
        /// 执行更新
        /// </summary>
        public void StartUpdate() {
            //关闭监视进程
            YUtil.StopWinService(HmiConfig.DaemonServiceName);
            YUtil.KillProcess(HmiConfig.AsylumProcessName);
            AppUpdater.StartExternalUpdater();
        }
    }
}
