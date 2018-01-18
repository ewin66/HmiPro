using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;
using YCsharp.Model.Procotol.SmParam;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 采集参数相关动作指令
    /// <author>ychost</author>
    /// </summary>
    public static class CpmActions {

        public static readonly string INIT = "[Cpm] Init";
        public static readonly string START_SERVER = "[Cpm] Start Server";
        public static readonly string START_SERVER_SUCCESS = "[Cpm] Start Server Success";
        public static readonly string START_SERVER_FAILED = "[Cpm] Start Server  Failed";
        public static readonly string CPMS_UPDATED_ALL = "[Cpm] Updated All";
        public static readonly string CPMS_UPDATED_DIFF = "[Cpm] Updated Difference";
        public static readonly string CPMS_IP_ACTIVED = "[Cpm] Ip Actived";
        public static readonly string STOP_SERVER = "[Cpm] Stop Server";
        public static readonly string STOP_SERVER_SUCCESS = "[Cpm] Stop Server Success";
        public static readonly string STOP_SERVER_FAILED = "[Cpm] Stop Server Failed";
        public static readonly string NOTE_METER_DIFF_ACCEPT = "[Cpm] Note Meter Diff Accept";
        public static readonly string NOTE_METER_ACCEPT = "[Cpm] Note Meter Accpet";
        public static readonly string STATE_SPEED_ACCEPT = "[Cpm] State Speed Accept";
        public static readonly string STATE_SPEED_DIFF_ACCEPT = "[Cpm] State Speed Diff Accept";
        public static readonly string OEE_SPEED_ACCEPT = "[Cpm] Oee Speed Accept";
        public static readonly string SPARK_DIFF_ACCEPT = "[Cpm] Spark Diff Accept";
        public static readonly string STATE_SPEED_DIFF_ZERO_ACCEPT = "[Cpm] State Speed Diff Zero Accept";
        public static readonly string STATE_SPEED_ZERRO_ACCEPT = "[Cpm] State Speed Zero Accept";
        public static readonly string OD_ACCPET = "[Cpm] Od Accept";
        public static readonly string UNREGISTERED_IP_ACTIVE = "[Cpm] Unregistered Ip Active";
        //485串口状态更新
        //单节点状态
        public static readonly string COM_485_SINGLE_STATUS_ACCEPT = "[Cpm] Communication 485 Single  Status Accept";

        public struct Com485SingleStatusAccept : IAction {
            public string Type() => COM_485_SINGLE_STATUS_ACCEPT;
            public string Ip;
            public string MachineCode;
            public SmSingleStatus Status;
            public int CpmCode;

            public Com485SingleStatusAccept(string machineCode, string ip, SmSingleStatus status, int cpmCode) {
                MachineCode = machineCode;
                Ip = ip;
                Status = status;
                CpmCode = cpmCode;
            }
        }

        public struct OdAccept : IAction {
            public string Type() => OD_ACCPET;
            public string MachineCode;
            public float Od;

            public OdAccept(string machineCode, float od) {
                MachineCode = machineCode;
                Od = od;
            }
        }


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

            public StartServer(string ip, int port) {
                Ip = ip;
                Port = port;
            }
        }

        public struct UnregIpActived : IAction {
            public string Type() => UNREGISTERED_IP_ACTIVE;
            public string Ip;

            public UnregIpActived(string ip) {
                this.Ip = ip;
            }
        }

        /// <summary>
        /// 某个ip有活动
        /// </summary>
        public struct CpmIpActivted : IAction {
            public string Type() => CPMS_IP_ACTIVED;
            public DateTime ActivedTime;
            public string Ip;
            public string MachineCode;
            public CpmIpActivted(string machineCode, string ip, DateTime time) {
                ActivedTime = time;
                Ip = ip;
                MachineCode = machineCode;
            }
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
            public string MachineCode;
            public string Type() {
                return CPMS_UPDATED_ALL;
            }

            public CpmUpdatedAll(string code, List<Cpm> cpms) {
                MachineCode = code;
                Cpms = cpms;
            }
        }

        /// <summary>
        /// 接受到的与上次对比有差异的参数
        /// </summary>
        public struct CpmUpdateDiff : IAction {
            public string Type() {
                return CPMS_UPDATED_DIFF;
            }

            public IDictionary<int, Cpm> CpmsDict;
            public string MachineCode;

            public CpmUpdateDiff(string code, IDictionary<int, Cpm> cpmsDict) {
                CpmsDict = cpmsDict;
                MachineCode = code;
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

        /// <summary>
        /// 记米变更
        /// </summary>
        public struct NoteMeterDiffAccept : IAction {
            public string Type() => NOTE_METER_DIFF_ACCEPT;
            public float Meter;
            public string MachineCode;

            public NoteMeterDiffAccept(string machineCode, float meter) {
                Meter = meter;
                MachineCode = machineCode;
            }
        }

        public struct NoteMeterAccept : IAction {
            public string Type() => NOTE_METER_ACCEPT;
            public float Meter;
            public string MachineCode;

            public NoteMeterAccept(string machineCode, float meter) {
                Meter = meter;
                MachineCode = machineCode;
            }
        }

        public struct SparkDiffAccept : IAction {
            public string Type() => SPARK_DIFF_ACCEPT;
            public string MachineCode;
            public float Spark;

            public SparkDiffAccept(string machineCode, float spark) {
                MachineCode = machineCode;
                Spark = spark;
            }
        }

        public struct StateSpeedAccept : IAction {
            public string Type() => STATE_SPEED_ACCEPT;
            public string MachineCode;
            public float Speed;

            public StateSpeedAccept(string machineCode, float speed) {
                MachineCode = machineCode;
                Speed = speed;
            }
        }

        public struct StateSpeedDiffAccpet : IAction {
            public string Type() => STATE_SPEED_DIFF_ACCEPT;
            public float Speed;
            public string MachineCode;

            public StateSpeedDiffAccpet(string machineCode, float speed) {
                MachineCode = machineCode;
                Speed = speed;
            }
        }

        public struct StateSpeedDiffZeroAccept : IAction {
            public string Type() => STATE_SPEED_DIFF_ZERO_ACCEPT;
            public string MachineCode;

            public StateSpeedDiffZeroAccept(string machineCode) {
                MachineCode = machineCode;
            }
        }

        public struct StateSpeedZeroAccept : IAction {
            public string Type() => STATE_SPEED_ZERRO_ACCEPT;
            public string MachineCode;

            public StateSpeedZeroAccept(string machineCode) {
                MachineCode = machineCode;
            }
        }

        public struct OeeSpeedAccept : IAction {
            public string Type() => OEE_SPEED_ACCEPT;
            public string MachineCode;
            public float Speed;

            public OeeSpeedAccept(string machineCode, float speed) {
                MachineCode = machineCode;
                Speed = speed;
            }
        }
    }
}
