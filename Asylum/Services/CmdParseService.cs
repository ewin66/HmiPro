using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Asylum.Helpers;
using Asylum.Models;
using Newtonsoft.Json;
using YCsharp.Service;
using YCsharp.Util;

namespace Asylum.Services {
    /// <summary>
    /// 解析外部的命令
    /// </summary>
    public class CmdParseService {

        /// <summary>
        /// 命令执行者
        /// </summary>
        private readonly IDictionary<Type, Func<object, ExecRest>> executors;
        /// <summary>
        /// HmiPro.exe 进程名称
        /// </summary>
        private readonly string hmiProName;
        /// <summary>
        /// HmiPro.exe 路径
        /// </summary>
        private readonly string hmiProPath;
        /// <summary>
        /// 日志
        /// </summary>
        public readonly LoggerService Logger;

        /// <summary>
        /// 注入配置信息
        /// </summary>
        /// <param name="hmiProName"></param>
        /// <param name="hmiProPath"></param>
        public CmdParseService(string hmiProName, string hmiProPath) {
            UnityIocService.AssertIsFirstInject(GetType());
            this.hmiProName = hmiProName;
            this.hmiProPath = hmiProPath;
            Logger = LoggerHelper.Create(GetType().ToString());
            executors = new Dictionary<Type, Func<object, ExecRest>>();
            executors[typeof(CmdActions.StartHmiPro)] = startHmiPro;
            executors[typeof(CmdActions.CloseHmiPro)] = closeHmiPro;
        }



        /// <summary>
        /// 启动 Hmi Pro
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ExecRest startHmiPro(object data) {
            var start = (CmdActions.StartHmiPro)data;
            var rest = new ExecRest();
            try {
                rest.DebugMessage = YUtil.CheckProcessIsExist(hmiProName) ? "HmiPro 进程存在" : "HmiPro 进程不存在";
                //强制启动
                if (start.IsForced) {
                    YUtil.KillProcess(hmiProName);
                    YUtil.Exec(hmiProPath, start.StartArgs);rest.Message = "强制启动 HmiPro 成功";
                    //只有当进程不存在的时候才启动
                } else if (!YUtil.CheckProcessIsExist(hmiProName)) {
                    YUtil.Exec(hmiProPath, start.StartArgs);
                    rest.Message = "启动 HmiPro 成功";
                }
                rest.Code = ExecCode.Ok;
            } catch (Exception e) {
                rest.Message = "启动 HmiPro 失败";
                rest.Code = ExecCode.StartHmiProFailed;
                rest.DebugMessage = e.Message;
            }
            return rest;
        }


        /// <summary>
        /// 关闭 HmiPro
        /// 目前仅支持强制关闭
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ExecRest closeHmiPro(object data) {
            var close = (CmdActions.CloseHmiPro)data;
            var rest = new ExecRest();
            try {
                YUtil.KillProcess(hmiProName);
                rest.Message = "关闭 HmiPro 成功";
                rest.Code = ExecCode.Ok;
            } catch (Exception e) {
                rest.DebugMessage = e.Message;
                rest.Code = ExecCode.CloseHmiProFailed;
            }
            return rest;
        }

        /// <summary>
        /// 对外接口，解析 Cmd
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public ExecRest Exec(Cmd cmd) {
            Logger.Debug("[CmdParseService] 执行命令：" + cmd.Action);
            var rest = new ExecRest();
            var types = YUtil.GetTypes(cmd.Action, Assembly.GetExecutingAssembly());
            if (types.Count == 1) {
                var type = types[0];
                if (executors.TryGetValue(type, out var exec)) {
                    var data = cmd.Args != null
                        ? JsonConvert.DeserializeObject(JsonConvert.SerializeObject(cmd.Args), type)
                        : null;
                    try {
                        rest = exec(data);
                    } catch (Exception e) {
                        rest.DebugMessage = e.Message;
                        rest.Code = ExecCode.ExecFailed;
                        rest.Message = "执行解析逻辑异常";
                    }
                } else {
                    rest.Code = ExecCode.NotFoundAction;
                    rest.Message = "Action 对应的 Type 未注册";
                }
            } else if (types.Count > 1) {
                rest.Code = ExecCode.MapManyTypes;
                rest.Message = "Action 对应了多个 Types";
            } else {
                rest.Code = ExecCode.NotFoundType;
                rest.Message = "Action 未对应 Type";

            }
            return rest;
        }
    }
}
