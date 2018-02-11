using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Asylum.Helpers;
using Asylum.Models;
using Newtonsoft.Json;
using YCsharp.Service;

namespace Asylum.Services {
    /// <summary>
    /// Http 数据解析
    /// <author>ychost</author>
    /// <date>2018-2-9</date>
    /// </summary>
    public class HttpParse {
        /// <summary>
        /// 命令真正触发的地方
        /// </summary>
        private readonly CmdParseService cmdPrase;
        /// <summary>
        /// Http 服务核心
        /// </summary>
        public HttpListener HttpListener;
        /// <summary>
        /// 日志
        /// </summary>
        public LoggerService Logger;

        /// <summary>
        /// 代理注入
        /// </summary>
        /// <param name="cmdParse"></param>
        public HttpParse(CmdParseService cmdParse) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.cmdPrase = cmdParse;
            Logger = LoggerHelper.Create(GetType().ToString());
        }


        /// <summary>
        /// http相关处理
        /// </summary>
        /// <param name="ar"></param>
        private void processHttpContext(IAsyncResult ar) {
            Logger.Info("[HttpParse] 接受到数据");
            var context = HttpListener.EndGetContext(ar);
            HttpListener.BeginGetContext(processHttpContext, null);
            var response = context.Response;
            var request = context.Request;
            response.AddHeader("Asylum", "IntelliManu Pro Asylum");
            response.ContentType = "text/plain;charset=UTF-8";
            if (request.ContentType != null) {
                Logger.Debug("[HttpParse] Request 类型: " + request.ContentType);
            }
            ExecRest rest = new ExecRest();
            var postData = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
            try {
                var cmd = JsonConvert.DeserializeObject<Cmd>(postData);
                cmd.Where = CmdWhere.FromHttp;
                rest = cmdPrase.Exec(cmd);
            } catch (Exception e) {
                rest.Code = ExecCode.FormatError;
                rest.DebugMessage = e.Message;
                rest.Message = "提交格式错误，不满足 Cmd 的格式";
                Logger.Error("解析命令出错：" + postData, e);
            }
            using (var stream = response.OutputStream) {
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(rest));
                stream.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// 启动 Http 服务
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Task<bool> Start(string url) {
            Logger.Debug("[HttpParse] 启动中...");
            return Task.Run(() => {
                HttpListener = new HttpListener();
                HttpListener.Prefixes.Add(url);
                return Task.Run(() => {
                    try {
                        HttpListener.Start();
                        HttpListener.BeginGetContext(processHttpContext, null);
                        Logger.Debug("[HttpParse] 启动成功");
                        return true;
                    } catch {
                        Logger.Error("[HttpParse] 启动失败");
                        return false;
                    }
                });
            });

        }
    }
}
