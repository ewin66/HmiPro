using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 采集参数相关动作指令
    /// </summary>
    public static class CpmActions {

        public static readonly string INIT = "[Cpm] Init";
        public static readonly string START_SERVER = "[Cpm] Start Server";
        public static readonly string START_SERVER_SUCCESS = "[Cpm] Start Server Success";
        public static readonly string START_SERVER_FAILED = "[Cpm] Start Failed";
        public static readonly string CPMS_UPDATED_ALL = "[Cpm] Updated All";
        public static readonly string CPMS_UPDATED_DIFF = "[Cpm] UPdated Different";
        public static readonly string CPMS_IP_ACTIVED = "[Cpm] Ip Actived";
        public static readonly string STOP_SERVER = "[Cpm] Stop Server";
        public static readonly string STOP_SERVER_SUCCESS = "[Cpm] Stop Server Success";
        public static readonly string STOP_SERVER_FAILED = "[Cpm] Stop Server Failed";

        /// <summary>
        /// 初始化
        /// </summary>
        public struct Init : IAction {
            public string Type() {
                return INIT;
            }
        }

        /// <summary>
        /// 启动参数采集服务
        /// </summary>
        public struct StartServer : IAction {
            public string Type() {
                return START_SERVER;
            }
            public string Ip;
            public int Port;
        }

        /// <summary>
        /// 启动成功
        /// </summary>
        public struct StartServerSuccess : IAction {
            public string Type() {
                return START_SERVER_SUCCESS;
            }
        }

        /// <summary>
        /// 启动失败含失败的异常
        /// </summary>
        public struct StartServerFailed : IAction {
            public Exception Exception;
            public string Type() {
                return START_SERVER_FAILED;
            }
        }


        /// <summary>
        /// 接受到的所有新参数
        /// </summary>
        public struct CpmUpdatedAll : IAction {
            public List<Cpm> Cpms;
            public string Type() {
                return CPMS_UPDATED_ALL;
            }
        }

        /// <summary>
        /// 接受到的与上次对比有差异的参数
        /// </summary>
        public struct CpmUpdateDiff : IAction {
            public string Type() {
                return CPMS_UPDATED_DIFF;
            }
        }

        /// <summary>
        /// 停止参数采集服务
        /// </summary>
        public struct StopServer : IAction {
            public string Type() {
                return STOP_SERVER;
            }
        }

        /// <summary>
        /// 停止参数采集服务成功
        /// </summary>
        public struct StopServerSuccess : IAction {
            public string Type() {
                return STOP_SERVER_SUCCESS;
            }
        }

        /// <summary>
        /// 停止参数采集服务失败
        /// </summary>
        public struct StopServerFailed : IAction {
            public string Type() {
                return STOP_SERVER_FAILED;
            }
        }

    }
}
